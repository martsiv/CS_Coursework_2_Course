using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    internal class User
    {
        #region Basic data
        private string _login;
		public string Login
		{
			get => _login;
			set 
			{
				if (string.IsNullOrWhiteSpace(value) && value.Length >= 3)
					throw new ArgumentException("Login must be no shorter than 3 characters and no spaces");
				_login = value; 
			}
		}
		private string _password;
		public string Password
		{
			private get => _password;
			set
			{
                if (string.IsNullOrWhiteSpace(value) 
					&& value.Length >= 3 
					&& value.All(c => char.IsLetter(c) 
					|| char.IsDigit(c)))
                    throw new ArgumentException("The password must be no shorter than 4 characters, must contain only numbers and letters");
                _password = value;
			}
		}
		private DateOnly birthday;
		public DateOnly Birthday
		{
			get => birthday; 
			set 
			{
				if (DateTime.Now.Year + 4 < value.Year)
					throw new ArgumentException("Date of birth is not correct, age must be older than 4 years");
                birthday = value;
			}
		}
        #endregion
        //===========================================
        #region ctors
        public User(string login, string password, DateOnly birthday)
        {
			try
			{
				Login = login;
				Password = password;
				Birthday = birthday;
			}
			catch 
            {
			
			}
        }
        #endregion
        //===========================================
        #region Methods

        #endregion
        //===========================================
    }
}
