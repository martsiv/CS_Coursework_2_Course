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
    internal class QuizManager
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
                        Console.WriteLine("Good bye!");
                        return;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
            }
        }
        private void Save()
        {
            SaveUsers();
            SaveQuestions();
            SaveResult();
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
        private void Load()
        {
            LoadUsers();
            LoadQuestions();
            LoadResult();
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
        //private bool IsValidLogin(string login)
        //{
        //    // regular expression that defines the valid format for the login
        //    Regex regex = new Regex("^[a-z0-9]+$");
        //    // check whether the string corresponds to a valid format
        //    if (regex.IsMatch(login) && !_users.ContainsKey(login))
        //        return true;
        //    else
        //        return false;
        //}
        //private bool IsValidPasswordr(string password)
        //{
        //    if (password == null || password.Any(char.IsWhiteSpace) || !password.Any(char.IsDigit) || !password.Any(char.IsLetter) || password.Length < 2 || password.Length > 20)
        //        return false;
        //    return true;
        //}
        //private bool IsValidBirthDate(DateTime birth)
        //{
        //    if ((DateTime.Now.Year - 5) < birth.Year || birth.Year < (DateTime.Now.Year - 120))
        //    {
        //        //Console.WriteLine("You must be between 5 and 120 years old");
        //        return false;
        //    }
        //    return true;
        //}

        private string EncryptMD5(string password)
        {
            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            byte[] hashedBytes = MD5.Create().ComputeHash(passwordBytes);
            string hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hashedPassword;
        }
        private void UserPanel(User user)
        {
            Console.Clear();
            Console.WriteLine("============== Users control panel ==============");
            Console.WriteLine("Escape to exit");
            if (Console.ReadKey().Key == ConsoleKey.Escape)
                return;
        }
        private void AdminPanel()
        {
            Console.Clear();
            Console.WriteLine("============== Admin control panel ==============");
            Console.WriteLine("Escape to exit");
            if (Console.ReadKey().Key == ConsoleKey.Escape)
                return;
        }
        #endregion

        #region Inherited methods

        #endregion

    }
}
