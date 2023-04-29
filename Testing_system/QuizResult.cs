using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    [Serializable]
    internal class QuizResult : IComparable<QuizResult>
    {

        public string UserName { get; }
        public string QuizSection { get; }
        public int Result { get; }
        public TimeSpan Time { get; }
        public QuizResult(string userName, string quizSection, int result, TimeSpan time)
        {
            UserName = userName;
            QuizSection = quizSection;
            Result = result;
            Time = time;
        }

        public int CompareTo(QuizResult? other)
        {
            if (other == null)
                return 1;
            return QuizSection.CompareTo(other.QuizSection);
        }
    }
}
