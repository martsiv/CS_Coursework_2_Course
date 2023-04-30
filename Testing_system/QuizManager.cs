using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
#pragma warning disable SYSLIB0011 

namespace Quiz
{
    [Serializable]
    internal class QuizManager //розбити на partial 
    {
        #region Basic data
        private Dictionary<string, User> _users;
        private SortedDictionary<string, List<Question>> _questions;
        private List<QuizResult> _quizResult;
        private KeyValuePair<string, string> adminData;
        #endregion

        #region ctors
        public QuizManager(Dictionary<string, User> users, SortedDictionary<string, List<Question>> questions, List<QuizResult> quizResult)
        {
            _users = users;
            _questions = questions;
            _quizResult = quizResult;
        }
        public QuizManager()
        {
            _users = new Dictionary<string, User>();
            _questions = new SortedDictionary<string, List<Question>>();
            _quizResult = new List<QuizResult>();
        }
        #endregion

        #region Own methods
        public void Run()
        {
            if (!CheckAdminDataFile())
            {
                Console.WriteLine("This is your first login, you need to enter an administrator login and password");
                CreateAdminData();
            }
            Load();
            while (true)
            {
                Console.Clear();
                Console.WriteLine
                ("1 - Log in\n" +
                "2 - Register\n" +
                "3 - Exit");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                        Login();
                        break;
                    case ConsoleKey.D2:
                        Register();
                        Save();
                        break;
                    case ConsoleKey.D3:
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            }
        }
        private void Login()
        {
            Console.Clear();
            Console.Write("\nEnter your login: ");
            string login = Console.ReadLine();
            Console.Write("Enter your password: ");
            string password = EncryptMD5(Console.ReadLine());

            if (login == adminData.Key && password == adminData.Value)
                AdminPanel();
            if (!_users.ContainsKey(login))
            {
                Console.WriteLine("No such user found");
                return;
            }
            User user = _users[login];
            if (user.HashPassword == password)
                UserPanel(user);
            else { Console.WriteLine("Wrong password!"); }
        }
        private void Register()
        {
            Console.Clear();
            KeyValuePair<string, string> loginPassword = EnterCorrectLoginAndEncryptedPassword();
            DateTime birth = EnterCorrectBirth();
            _users.Add(loginPassword.Key, new User(loginPassword.Key, loginPassword.Value, birth));    //Add new user
        }

        #region Utility
        public static int LevenshteinDistance(string string1, string string2)
        {
            if (string1 == null) throw new ArgumentNullException("string1");
            if (string2 == null) throw new ArgumentNullException("string2");
            int diff;
            int[,] m = new int[string1.Length + 1, string2.Length + 1];

            for (int i = 0; i <= string1.Length; i++) { m[i, 0] = i; }
            for (int j = 0; j <= string2.Length; j++) { m[0, j] = j; }

            for (int i = 1; i <= string1.Length; i++)
            {
                for (int j = 1; j <= string2.Length; j++)
                {
                    diff = (string1[i - 1] == string2[j - 1]) ? 0 : 1;

                    m[i, j] = Math.Min(Math.Min(m[i - 1, j] + 1, m[i, j - 1] + 1), m[i - 1, j - 1] + diff);
                }
            }
            return m[string1.Length, string2.Length];
        }
        private int GetNumber(int leftRange, int rightRange)
        {
            int? result = 0;
            do
                result = int.Parse(Console.ReadLine());
            while (result == null || result < leftRange || rightRange < result);
            return result.Value;
        }
        private KeyValuePair<string, string> EnterCorrectLoginAndEncryptedPassword()
        {
            string login;
            while (true)
            {
                Console.Write("\nEnter your login: ");
                login = Console.ReadLine().ToLower();
                Regex regex = new Regex("^[a-z0-9]+$");
                // check whether the string corresponds to a valid format
                if (!regex.IsMatch(login) || _users.ContainsKey(login))
                    Console.WriteLine("Invalid login");
                else break;
            }
            string password;
            while (true)
            {
                Console.Write("Enter your password: ");
                password = Console.ReadLine();
                if (password == null || password.Any(char.IsWhiteSpace) || !password.Any(char.IsDigit) || !password.Any(char.IsLetter) || password.Length < 2 || password.Length > 20)
                    Console.WriteLine("Invalid password");
                else break;
            }
            return new KeyValuePair<string, string>(login, EncryptMD5(password));
        }
        private DateTime EnterCorrectBirth()
        {
            DateTime birth;
            while (true)
            {
                Console.Write("Enter your date of birth: ");
                birth = DateTime.Parse(Console.ReadLine());
                if ((DateTime.Now.Year - 5) < birth.Year || birth.Year < (DateTime.Now.Year - 120))
                    Console.WriteLine("Invalid birth date");
                else break;
            }
            return birth;
        }
        private string EncryptMD5(string password)
        {
            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            byte[] hashedBytes = MD5.Create().ComputeHash(passwordBytes);
            string hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedPassword;
        }
        #endregion Utility

        #region Filework
        private void CreateAdminData()
        {
            adminData = EnterCorrectLoginAndEncryptedPassword();

            BinaryFormatter formatter = new BinaryFormatter();

            using (Stream fs = File.Create("admin.bin"))
            {
                formatter.Serialize(fs, adminData);
            }
        }
        private bool CheckAdminDataFile()
        {
            if (File.Exists("admin.bin"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (Stream fs = File.OpenRead("admin.bin"))
                {
                    adminData = (KeyValuePair<string, string>)formatter.Deserialize(fs);
                }
                return true;
            }
            return false;
        }
        private void Save()
        {
            SaveUsers();
            SaveQuestions();
            SaveResult();
        }
        private void Load()
        {
            LoadUsers();
            LoadQuestions();
            LoadResult();
        }
        private void SaveUsers()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream fs = File.Create("users.bin"))
            {
                formatter.Serialize(fs, _users);
            }
        }
        private void SaveQuestions()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream fs = File.Create("questions.bin"))
            {
                formatter.Serialize(fs, _questions);
            }
        }
        private void SaveResult()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream fs = File.Create("result.bin"))
            {
                formatter.Serialize(fs, _quizResult);
            }
        }
        private void LoadUsers()
        {
            if (File.Exists("users.bin"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (Stream fs = File.OpenRead("users.bin"))
                {
                    _users = (Dictionary<string, User>)formatter.Deserialize(fs);
                }
            }
        }
        private void LoadQuestions()
        {
            if (File.Exists("questions.bin"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (Stream fs = File.OpenRead("questions.bin"))
                {
                    _questions = (SortedDictionary<string, List<Question>>)formatter.Deserialize(fs);
                }
            }
        }
        private void LoadResult()
        {
            if (File.Exists("result.bin"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (Stream fs = File.OpenRead("result.bin"))
                {
                    _quizResult = (List<QuizResult>)formatter.Deserialize(fs);
                }
            }
        }

        #endregion Filework

        #region User panel
        private void UserPanel(User user)
        {
            Console.Clear();
            Console.WriteLine("============== Users control panel ==============");
            Console.WriteLine("Escape to exit");
            if (Console.ReadKey().Key == ConsoleKey.Escape)
                return;
        }

        #endregion User panel

        #region Admin panel
        private void AdminPanel()
        {
            Console.Clear();
            Console.WriteLine("============== Admin control panel ==============");

            while (true)
            {
                Console.Clear();
                Console.WriteLine
                ("1 - Show all questions\n" +
                "2 - Add question\n" +
                "3 - Delete question\n" +
                "4 - Modify question\n" +
                "5 - Delete topic\n" +
                "9 - Exit");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                        ShowAllQuestions();
                        break;
                    case ConsoleKey.D2:
                        AddQuestion();
                        break;
                    case ConsoleKey.D3:
                        DeleteQuestion();
                        break;
                    case ConsoleKey.D4:
                        ModifyQuestion();
                        break;
                    case ConsoleKey.D5:
                        DeleteTopic();
                        break;
                    case ConsoleKey.D9:
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            }
        }

        #region Question control
        
        private void ShowTopicsList()
        {
            Console.WriteLine("====== All topics ======");
            foreach (var item in _questions)
                Console.WriteLine($"\t- {item.Key}");
        }
        private void ShowAllQuestions(string topic = null)
        {
            Console.WriteLine("=========== All question ============");
            foreach (var item in _questions)
            {
                if (topic != null && topic == item.Key)     //Якщо ключ не задано, то показує все. Інакше, лише певний розділ
                {
                    Console.WriteLine(item.Key);
                    foreach (var item1 in item.Value)
                    {
                        Console.WriteLine(item1.Text);
                        for (int i = 0; i < item1.Options.Count; i++)
                        {
                            foreach (var item2 in item1.CorrectAnswers)
                                if (i == item2)
                                    Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{i + 1} - {item1.Options[i]}");
                            Console.ResetColor();
                        }
                    }
                }
            }
            Console.WriteLine("Press eny button co continue");
            Console.ReadKey();
        }
        private void ShowOptions(List<string> options, List<int> correctAnswers, bool isAdmin = true)    //Відображення варіантів відповідей з підсвіченням вірних відповідей
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (correctAnswers.Contains(i) && isAdmin == true)
                    Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{i + 1} - {options[i]}");
                Console.ResetColor();
            }
        }
        private List<string> GetOptions()       //Запросити ввід варіантів відповідей
        {
            List<string> options = new List<string>();
            ConsoleKey key = ConsoleKey.Y;
            while (key == ConsoleKey.Y)     //Перевірка: Варіантів може бути 2+
            {
                for (int i = 0; i < options.Count; i++)
                    Console.WriteLine($"{i + 1} - {options[i]}");
                Console.Write($"Enter an answer option #{options.Count + 1}: ");
                options.Add(Console.ReadLine());
                if (2 <= options.Count)
                {
                    Console.WriteLine("Would you like to add another answer option? (Y, N)");
                    key = Console.ReadKey().Key;
                }
            }
            return options;
        }
        private List<int> GetCorrectAnswers(List<string> options)       //Запросити ліст правильних відповідей відповідно до наданих варіантів
        {
            List<int> correctAnswers = new List<int>();
            while (correctAnswers.Count < options.Count)
            {
                ShowOptions(options, correctAnswers);
                if (0 < correctAnswers.Count)
                {
                    Console.WriteLine("Would you like to add another correct answer? (Y, N)");
                    if (ConsoleKey.Y != Console.ReadKey().Key)
                        break;
                }
                correctAnswers.Add(ReadCorrectAnswer(options.Count, correctAnswers) - 1);
            }
            return correctAnswers;
        }
        private int ReadCorrectAnswer(int maxOption, List<int> correctAnswers)  //Запросити ввід вірної відповіді відповідно до наданих варіантів відповідей
        {
            int? answer = null;
            Console.WriteLine("Enter a number of correct answer: ");
            answer = int.Parse(Console.ReadLine());
            if (answer == null)
                Console.WriteLine("You must enter a number");
            else if (answer <= 0 || answer > maxOption)
                Console.WriteLine($"Rejected! The correct answer must be in the range from 1 to {maxOption}");
            else if (correctAnswers.Contains(answer.Value - 1))
                Console.WriteLine($"Rejected! Such a number of the correct answer has already been added");
            return answer.Value;
        }
        private void AddQuestion()
        {
            Console.Write("Enter the quiz topic: ");                //Додати розділ
            string topic = FindExistingTopic(Console.ReadLine());
            Console.Write("Enter a question description: ");        //Додати текст запитання
            string text = Console.ReadLine();
            List<string> options = GetOptions();                    //Додати варіанти відповідей

            List<int> correctAnswers = GetCorrectAnswers(options);  //Додати номери вірних відповідей

            Question newQuestion = new Question(topic, text, options, correctAnswers);  //Створити Питання, відповідно до заповнених даних
            if (!_questions.ContainsKey(topic))                                         //Перевірити чи такий розділ існує 
                _questions.Add(topic, new List<Question> { newQuestion });              //Якщо існує, то просто додати питання до списку, якщо ні - створити розділ
            else if (!_questions[topic].Contains(newQuestion))                          //Перевірити питання чи таке вже існує
                _questions[topic].Add(newQuestion);                                     //Якщо не існує, можемо додавати
            else
            {
                Console.WriteLine("There is already such a question in this section");  //Якщо існує, не додавати, вивести повідомлення
                Console.WriteLine("Press eny button co continue");
                Console.ReadKey();
            }
            Save();                                                                     //Зберегти загальну базу даних
        }
        private void DeleteQuestion()
        {
            Question question = FindQuestionByKeyword();
            if (question == null)
                return;
            if (_questions[question.Topic].Remove(question))
            {
                Console.WriteLine("Success!");
                Save();
            }
            else Console.WriteLine("Failed to delete question");
            Console.WriteLine("Press eny button co continue");
            Console.ReadKey();
        }
        public Question FindQuestionByKeyword()             //Шукає конкретне питання
        {
            if (_questions.Count == 0)  //Якщо список питань відсутній
            {
                Console.WriteLine("There are no questions");
                return null;
            }
            ShowTopicsList();
            Console.WriteLine("Enter topic to search or press enter to exit:");
            string topicInput = Console.ReadLine();
            if (string.IsNullOrEmpty(topicInput))
            {
                return null;
            }

            if (!_questions.TryGetValue(topicInput, out List<Question> questions))
            {
                Console.WriteLine($"Topic '{topicInput}' not found.");
                return null;
            }

            Console.WriteLine($"Found {questions.Count} questions in topic '{topicInput}'.");
            
            Console.WriteLine("Enter search keyword:");
            string keyword = Console.ReadLine();

            foreach (var question in questions)
            {
                if (question.Text.ToLower().Contains(keyword.ToLower()))
                {
                    Console.WriteLine($"Found a question with matching keyword in topic '{topicInput}':");
                    Console.WriteLine(question.Text);
                    Console.WriteLine("Is this the question you are looking for? (y/n)");
                    string answer = Console.ReadLine();
                    if (answer.ToLower() == "y")
                    {
                        return question;
                    }
                }
            }

            Console.WriteLine("No matching questions found.");
            return null;
        }
        private void ModifyQuestion()
        {
            Question question = FindQuestionByKeyword();
            if (question == null)
                return;
            while (true)
            {
                Console.WriteLine("You can change the description, options and correct answers. Click to change:\n" +
                "1 - Description\n" +
                "2 - Options and Correct answers\n" +
                "3 - Correct answers\n" +
                "9 - Return");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:                     //Змінює опис
                        question.Text = Console.ReadLine();
                        break;
                    case ConsoleKey.D2:                     //Змінює варіанти відповідей, та нові вірні відповіді
                        question.Options = GetOptions();
                        question.CorrectAnswers = GetCorrectAnswers(question.Options);
                        break;
                    case ConsoleKey.D3:                     //Змінює лише вірні відповіді
                        question.CorrectAnswers = GetCorrectAnswers(question.Options);
                        break;
                    case ConsoleKey.D9:
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            }
        }
        private string FindExistingTopic(string newTopic)   //При додаванні, виконує пошук схожого за назвою розділу і пропонує обрати його
        {
            foreach (var topic in _questions.Keys)
            {
                if (newTopic.Equals(topic))
                {
                    return topic;
                }
                else if (LevenshteinDistance(newTopic, topic) == 1)
                {
                    // якщо відстань Левенштейна між topic і newTopic дорівнює 1, то вони майже однакові
                    // можна запропонувати використати існуючий topic
                    Console.WriteLine($"\"{newTopic}\" is almost identical to \"{topic}\". Use \"{topic}\" instead?");
                    Console.WriteLine("1 - yes\n" +
                        "2 - no");
                    if (Console.ReadKey().Key == ConsoleKey.D1)
                        return topic;
                }
            }
            return newTopic;
        }
        private void DeleteTopic()
        {
            ShowTopicsList();
            Console.Write("Enter the name of the desired topic or Enter to exit: ");
            string topicInput = Console.ReadLine();
            if (string.IsNullOrEmpty(topicInput))
                return;
            if (_questions.ContainsKey(topicInput))
            {
                Console.WriteLine($"Are you sure you want to delete the {topicInput} topic? (Y, N)");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    _questions.Remove(topicInput);
                    Save();
                }
            }
        }
        #endregion

        #endregion Admin panel
        #endregion Own methods

        #region Inherited methods

        #endregion Inherited methods

    }
}
