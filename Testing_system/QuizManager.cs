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
using System.Runtime.Serialization;
using System.Xml;
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
        private KeyValuePair<string, string> EnterCorrectLoginAndEncryptedPassword()
        {
            string login = CreateLogin();
            Console.Write("Enter the password: ");
            string password = CreatePassword();
            return new KeyValuePair<string, string>(login, EncryptMD5(password));
        }

        private string CreateLogin()
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
            return login;
        }
        private string CreatePassword()
        {
            string password;
            while (true)
            {
                //Console.Write("Enter the password: ");
                password = Console.ReadLine();
                if (password == null || password.Any(char.IsWhiteSpace) || !password.Any(char.IsDigit) || !password.Any(char.IsLetter) || password.Length < 2 || password.Length > 20)
                    Console.WriteLine("Invalid password");
                else break;
            }
            return password;
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
            Console.WriteLine("Success!");
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
            SaveUsersXML();
            SaveUsers();
            SaveQuestionsXML();
            SaveQuestions();
            SaveResultXML();
            SaveResult();
        }
        private void SaveXML()
        {
            SaveUsersXML();
            SaveQuestionsXML();
            SaveResultXML();
        }
        private void Load()
        {
            LoadUsersXML();
            LoadQuestionsXML();
            LoadResultXML();
        }

        private void LoadXML()
        {
            LoadQuestionsXML();
            LoadResultXML();
        }
        private void SaveUsers()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream fs = File.Create("users.bin"))
            {
                formatter.Serialize(fs, _users);
            }
        }

        private void SaveUsersXML()
        {
            FileStream writer = new FileStream("users.xml", FileMode.Create);
            DataContractSerializer ser =
                new DataContractSerializer(typeof(Dictionary<string, User>));
            ser.WriteObject(writer, _users);
            writer.Close();
        }
        private void SaveQuestions()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream fs = File.Create("questions.bin"))
            {
                formatter.Serialize(fs, _questions);
            }
        }

        private void SaveQuestionsXML()
        {
            FileStream writer = new FileStream("questions.xml", FileMode.Create);
            DataContractSerializer ser =
                new DataContractSerializer(typeof(SortedDictionary<string, List<Question>>));
            ser.WriteObject(writer, _questions);
            writer.Close();
        }
        private void SaveResult()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (Stream fs = File.Create("result.bin"))
            {
                formatter.Serialize(fs, _quizResult);
            }
        }

        private void SaveResultXML()
        {
            FileStream writer = new FileStream("result.xml", FileMode.Create);
            DataContractSerializer ser =
                new DataContractSerializer(typeof(List<QuizResult>));
            ser.WriteObject(writer, _quizResult);
            writer.Close();
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

        private void LoadUsersXML()
        {
            Console.WriteLine("Deserializing an instance of the object.");
            FileStream fs = new FileStream("users.xml",
            FileMode.Open);
            XmlDictionaryReader reader =
                XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(Dictionary<string, User>));

            // Deserialize the data and read it from the instance.
            _users = (Dictionary<string, User>)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();
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

        private void LoadQuestionsXML()
        {
            Console.WriteLine("Deserializing an instance of the object.");
            FileStream fs = new FileStream("questions.xml",
            FileMode.Open);
            XmlDictionaryReader reader =
                XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(SortedDictionary<string, List<Question>>));

            // Deserialize the data and read it from the instance.
            _questions = (SortedDictionary<string, List<Question>>)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();
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

        private void LoadResultXML()
        {
            Console.WriteLine("Deserializing an instance of the object.");
            FileStream fs = new FileStream("result.xml",
            FileMode.Open);
            XmlDictionaryReader reader =
                XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
            DataContractSerializer ser = new DataContractSerializer(typeof(List<QuizResult>));

            // Deserialize the data and read it from the instance.
            _quizResult = (List<QuizResult>)ser.ReadObject(reader, true);
            reader.Close();
            fs.Close();
        }

        #endregion FileWork

        #region User panel
        private void UserPanel(User user)
        {
            Console.Clear();
            Console.WriteLine("============== Users control panel ==============");
            while (true)
            {
                Console.Clear();
                Console.WriteLine
                ("1 - Start new Quiz\n" +
                "2 - Show my stats\n" +
                "3 - Show TOP-20\n" +
                "4 - Chenge password\n" +
                "5 - Change my birth date\n" +
                "9 - Exit");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D1:
                        StartNewQuiz(user.Login);
                        break;
                    case ConsoleKey.D2:
                        ShowUsersStat(user.Login);
                        break;
                    case ConsoleKey.D3:
                        ShowTOP20();
                        break;
                    case ConsoleKey.D4:
                        ChangePassword(user);
                        break;
                    case ConsoleKey.D5:
                        user.Birthday = EnterCorrectBirth();
                        break;
                    case ConsoleKey.D9:
                        Save();
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            }
        }
        private void StartNewQuiz(string userName)
        {
            while (true)
            {
                //Тут ми формуємо квіз і всі атрибути для нього
                Console.WriteLine("Enter the topic or Enter to return");
                ShowTopicsList();
                string topic = Console.ReadLine();
                if (string.IsNullOrEmpty(topic))
                    return;
                if (!_questions.ContainsKey(topic))
                {
                    Console.WriteLine("Invalid topic name!");
                    continue;
                }
                if (_questions[topic].Count < 20)
                {
                    Console.WriteLine("Unfortunately, the topic does not contain an adequate number of questions in order to form a test from this topic");
                    continue;
                }
                Quiz quiz = new Quiz(topic, CreateQuestionListForQuiz(topic));
                //Сформували квіз

                //Проходження тесту
                QuizResult result = quiz.RunQuiz(userName);
                //Збереження результатів
                _quizResult.Add(result);
                Save();
                ShowUsersStat(userName);
                return;
            }
        }
        private List<Question> CreateQuestionListForQuiz(string topic, int num = 20)  //Рандомить ліст питань (20шт by default) під квіз
        {
            // Отримуємо всі питання, пов'язані з даною темою
            List<Question> questions = new List<Question>(_questions[topic]);

            // Створюємо об'єкт генератора випадкових чисел
            Random random = new Random();

            // Вибираємо випадкові 20 питань зі списку
            List<Question> selectedQuestions = new List<Question>();
            for (int i = 0; i < num; i++)
            {
                int index = random.Next(questions.Count);
                selectedQuestions.Add(questions[index]);
                questions.RemoveAt(index);
            }

            return selectedQuestions;
        }
        private void ShowUsersStat(string userName)
        {
            Console.WriteLine($"========= Statistics of the user {userName} =========");
            int counter = 0;
            foreach (var item in _quizResult)
            {
                if (item.UserName == userName)
                    Console.WriteLine($"{counter++}\t{item.QuizSection}  {item.NumberOfCorrectAnswers}/{item.NumberOfQuestions} {item.ResultInPercent}%\n" +
                        $"Start test:\t{item.TimeOfStart}\n" +
                        $"End test: \t{item.TimeOfEnd}");
            }
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
        private void ShowTOP20()
        {
            Console.WriteLine($"========= Statistics TOP-20 =========");
            if (_quizResult.Count == 0)
                return;
            //В QuizResult CompareTo сортування йде по розділах, а далі за результатом
            _quizResult.Sort();
            string topic = _quizResult[0].QuizSection;
            Console.WriteLine(topic);
            for (int i = 0, j = 0; i < _quizResult.Count; i++, j++)
            {
                if (topic != _quizResult[i].QuizSection)
                {
                    j = 0;
                    topic = _quizResult[i].QuizSection;
                    Console.WriteLine(topic);
                }
                if (j < 20)
                {
                    Console.WriteLine($"{j + 1} - {_quizResult[i].UserName}. " +
                        $"Answered {_quizResult[i].NumberOfCorrectAnswers}/{_quizResult[i].NumberOfQuestions} questions, " +
                        $"percentage of correct answers {_quizResult[i].ResultInPercent}%");
                }
            }
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }
        private void ChangePassword(User user)
        {
            Console.Write("Enter your old password: ");
            string pass = Console.ReadLine();
            if (string.IsNullOrEmpty(pass)) return;
            if (EncryptMD5(pass) == user.HashPassword)
            {
                Console.Write("Enter your new password: ");
                string newPassword = Console.ReadLine();
                Console.Write("Repeat your new password: ");
                if (Console.ReadLine() == newPassword)
                {
                    user.HashPassword = EncryptMD5(newPassword);
                    Console.WriteLine("Success!");
                    Console.ReadKey();
                    return;
                }
            }
            Console.WriteLine("Unsuccessfully!");
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
                        Save();
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
                if (topic == null || topic == item.Key)     //Якщо ключ не задано, то показує все. Інакше, лише певний розділ
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
                string answerTmp;
                do answerTmp = Console.ReadLine();      //Перевіряє, щоб не було null або пустого рядка
                while (string.IsNullOrWhiteSpace(answerTmp));
                options.Add(answerTmp);
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
            ShowOptions(options, correctAnswers);
            while (correctAnswers.Count < options.Count - 1)
            {
                if (0 < correctAnswers.Count)
                {
                    Console.WriteLine("Would you like to add another correct answer? (Y, N)");
                    if (ConsoleKey.Y != Console.ReadKey().Key)
                        break;
                }
                correctAnswers.Add(ReadCorrectAnswer(options.Count, correctAnswers) - 1);
                ShowOptions(options, correctAnswers);
            }
            return correctAnswers;
        }
        private int ReadCorrectAnswer(int maxOption, List<int> correctAnswers)  //Запросити ввід вірної відповіді відповідно до наданих варіантів відповідей
        {
            int answer;
            string tmp;
            while (true)
            {
                Console.WriteLine("Enter a number of correct answer: ");
                do
                {
                    tmp = Console.ReadLine();
                }
                while (!int.TryParse(tmp, out answer));

                if (answer <= 0 || answer > maxOption)
                    Console.WriteLine($"Rejected! The correct answer must be in the range from 1 to {maxOption}");
                else if (correctAnswers.Contains(answer - 1))
                    Console.WriteLine($"Rejected! Such a number of the correct answer has already been added");
                else
                    break;
            }
            return answer;
        }
        private void AddQuestion()
        {
            Console.Write("Enter the quiz topic: ");                //Додати розділ
            string topic = FindExistingTopic(Console.ReadLine());
            if (string.IsNullOrEmpty(topic))
                return;
            Console.Write("Enter a question description: ");        //Додати текст запитання
            string text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
                return;
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
            string topicInput = question.Topic;
            if (_questions[question.Topic].Remove(question))
                Console.WriteLine("Success!");
            else Console.WriteLine("Failed to delete question");
            if (_questions[topicInput].Count == 0)      //Якщо в розділі не залишилось питань, запропонує видалити весь топік
            {
                Console.WriteLine($"There are no more questions left in the {topicInput} topic. You can delete this topic");
                DeleteTopic(topicInput);
            }
            Save();
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
            ShowTopicsList();               //Показати список розділів
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

            ShowAllQuestions(topicInput);       //Показати всі питання в топіку

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
                        string text = Console.ReadLine();
                        if (string.IsNullOrEmpty(text))
                            break;
                        question.Text = text;
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
            if (string.IsNullOrEmpty(newTopic))
                return newTopic;
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
        private void DeleteTopic(string topicInput = null)
        {
            if (string.IsNullOrEmpty(topicInput))       //Якщо аргумент пустий, запросити ввід вручну, якщо ні, перейти до кроку 2
            {
                ShowTopicsList();
                Console.Write("Enter the name of the desired topic or Enter to exit: ");
                topicInput = Console.ReadLine();
                if (string.IsNullOrEmpty(topicInput))
                    return;
            }
            if (_questions.ContainsKey(topicInput))     //крок 2. Знайти топік і запросити підтвердження видалення
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
