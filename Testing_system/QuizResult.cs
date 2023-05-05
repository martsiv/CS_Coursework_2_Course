using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quiz
{
    [Serializable]
    internal class QuizResult : IComparable<QuizResult>
    {
        public string UserName { get; }
        public string QuizSection { get; }
        public int NumberOfQuestions { get; }
        public int NumberOfCorrectAnswers { get; }
        public float ResultInPercent => NumberOfCorrectAnswers * 100F / NumberOfQuestions;
        public DateTime TimeOfStart { get; }
        public DateTime TimeOfEnd { get; }
        public QuizResult(string userName, string quizSection, int numberOfQuestions, int numberOfCorrectAnswers, DateTime timeOfStart, DateTime timeOfEnd)
        {
            UserName = userName;
            QuizSection = quizSection;
            NumberOfQuestions = numberOfQuestions;
            NumberOfCorrectAnswers = numberOfCorrectAnswers;
            TimeOfStart = timeOfStart;
            TimeOfEnd = timeOfEnd;
        }

        public int CompareTo(QuizResult? other)
        {
            if (other == null)
                return 1;
            //Порівнюємо за розділом
            int result  = QuizSection.CompareTo(other.QuizSection);
            

            if (result == 0)
            {
                // Якщо розділи співпали, порівнюємо за результатом
                result = ResultInPercent == other.ResultInPercent ? 0 : ResultInPercent < other.ResultInPercent ? 1 : -1;
            }
    
            return result;
        }

        public bool Equals(string? other)
        {
            return UserName.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is string other)
                return Equals(other);

            if (obj is QuizResult otherClass)
                return Equals(otherClass.UserName);

            return false;
        }

        public override string? ToString()
        {
            return $"Topic - {QuizSection}. Result {NumberOfCorrectAnswers}/{NumberOfQuestions} {ResultInPercent}%\n" +
                        $"User:\t\t{UserName}\n" +
                        $"Start test:\t{TimeOfStart}\n" +
                        $"End test: \t{TimeOfEnd}";
        }
    }
}
