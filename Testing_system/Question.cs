using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    [Serializable]
    internal class Question : IEquatable<Question?>
    {
        #region Basic data
        public string Text { get; set; } // Текст питання
        public List<string> Options { get; set; } // Варіанти відповідей
        public List<int> CorrectAnswers { get; set; } // Індекси правильних відповідей
        

        #endregion
        //===========================================
        #region ctors
        public Question(string text, List<string> options, List<int> correctAnswers)
        {
            Text = text;
            Options = options;
            CorrectAnswers = correctAnswers;
        }
     
        #endregion
        //===========================================
        #region Inherited methods
        public override string ToString() => Text;

        public override bool Equals(object? obj) => Equals(obj as Question);
        public bool Equals(Question? other)
        {
            return other is not null &&
                   Text == other.Text &&
                   EqualityComparer<List<string>>.Default.Equals(Options, other.Options) &&
                   EqualityComparer<List<int>>.Default.Equals(CorrectAnswers, other.CorrectAnswers);
        }
        public override int GetHashCode() 
            => HashCode.Combine(Text, Options, CorrectAnswers);
        public static bool operator ==(Question? left, Question? right) 
            => EqualityComparer<Question>.Default.Equals(left, right);
        public static bool operator !=(Question? left, Question? right) 
            => !(left == right);


        #endregion
        //===========================================
        #region Own methods

        #endregion
    }
}
