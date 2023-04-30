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
        public string Topic { get; set; }               // Розділ
        public string Text { get; set; }                // Текст питання
        public List<string> Options { get; set; }       // Варіанти відповідей
        public List<int> CorrectAnswers { get; set; }   // Індекси правильних відповідей
        

        #endregion
        //===========================================
        #region ctors
        public Question(string topic, string text, List<string> options, List<int> correctAnswers)
        {
            Topic = topic;
            Text = text;
            Options = options;
            CorrectAnswers = correctAnswers;
        }
        public Question()
        {
            Options = new List<string>();
            CorrectAnswers = new List<int>();
        }
        #endregion
        //===========================================
        #region Inherited methods
        public override string ToString() => Text;

        public override bool Equals(object? obj) => Equals(obj as Question);
        public bool Equals(Question? other)
        {
            return other is not null &&
                   Topic == other.Topic &&
                   Options == other.Options &&
                   Text == other.Text;
        }
        public override int GetHashCode() 
            => HashCode.Combine(Topic, Text, Options, CorrectAnswers);
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
