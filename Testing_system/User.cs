using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    [Serializable]
    internal class User : IEquatable<User?>
    {
        #region Basic data
        public string Login { get; set; }
        public string HashPassword { get; set; }
        public DateTime Birthday { get; set; }
        #endregion
        //===========================================
        #region ctors
        public User(string login, string hashPassword, DateTime birthday)
        {
            Login = login;
            HashPassword = hashPassword;
            Birthday = birthday;
        }
        #endregion
        //===========================================
        #region Own methods

        #endregion
        //===========================================
        #region Inherited methods
        public override string ToString() => Login;
        public override bool Equals(object? obj)
            => Equals(obj as User);
        public bool Equals(User? other)
        {
            return other is not null &&
                   _login == other._login &&
                   birthday.Equals(other.birthday);
        }
        public override int GetHashCode()
            => HashCode.Combine(Login, Birthday);
        public static bool operator ==(User? left, User? right)
            => EqualityComparer<User>.Default.Equals(left, right);
        public static bool operator !=(User? left, User? right)
            => !(left == right);

        #endregion
        //===========================================
    }
}
