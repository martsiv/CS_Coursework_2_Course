using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    [Serializable]
    internal class Quiz
    {
        #region Basic data
        public string Topic { get; set; }
        public List<Question> Questions { get; set; }

        #endregion
        //===========================================
        #region ctors
        public Quiz()
        {
            Questions = new List<Question>();
            Topic = string.Empty;
        }

        public Quiz(string topic, List<Question> questions)
        {
            Topic = topic;
            Questions = questions;
        }
  
        #endregion
        //===========================================
        #region Inherited methods
        public QuizResult RunQuiz(string userName)
        {
            // Отримуємо час початку тесту
            DateTime startTime = DateTime.Now;

            // Запускаємо тест
            int correctAnswersCount = 0;
            foreach (var question in Questions)
            {
                Console.WriteLine(question.Text);
                for (int i = 0; i < question.Options.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {question.Options[i]}");
                }

                Console.Write("Your answer(s): ");
                string[] selectedAnswers = Console.ReadLine()?.Split(",");
                List<int> selectedIndexes = new List<int>();
                foreach (string answer in selectedAnswers)
                {
                    if (int.TryParse(answer, out int index))
                    {
                        selectedIndexes.Add(index - 1);
                    }
                }

                if (selectedIndexes.Count == question.CorrectAnswers.Count &&
                    question.CorrectAnswers.All(selectedIndexes.Contains))
                {
                    correctAnswersCount++;
                }
            }

            // Отримуємо час закінчення тесту
            DateTime endTime = DateTime.Now;

            // Створюємо і повертаємо об'єкт результату тестування
            int questionsCount = Questions.Count;
            return new QuizResult(userName, Topic, Questions.Count, correctAnswersCount, startTime, endTime);

        }

        #endregion
        //===========================================
        #region Own methods

        #endregion

    }
}
