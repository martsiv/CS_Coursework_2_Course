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
        public QuizResult RunQuiz(string userName)
        {
            Console.Clear();
            StartupInformation();   //Початкове повідомлення, загальна інформація про умови проходження
            // Отримуємо час початку тесту
            DateTime startTime = DateTime.Now;

            // Запускаємо тест
            int progress = 1;
            int correctAnswersCount = 0;
            foreach (var question in Questions)
            {
                Console.Clear();
                ShowInformationInQuiz(userName, correctAnswersCount, progress, question);
                //Зчитуємо варіанти відповідей посимвольно
                Console.Write("Your answer(s): ");
                string[] selectedAnswers = Console.ReadLine()?.Split(" ");
                List<int> selectedIndexes = new List<int>();
                foreach (string answer in selectedAnswers)
                {
                    if (int.TryParse(answer, out int index))
                    {
                        selectedIndexes.Add(index - 1);
                    }
                }
                //Перевіряємо чи ВСІ відповіді співпадають
                if (selectedIndexes.Count == question.CorrectAnswers.Count &&
                    question.CorrectAnswers.All(selectedIndexes.Contains))
                {
                    ++correctAnswersCount;
                }
                ++progress;
            }

            // Отримуємо час закінчення тесту
            DateTime endTime = DateTime.Now;

            // Створюємо і повертаємо об'єкт результату тестування
            int questionsCount = Questions.Count;
            return new QuizResult(userName, Topic, Questions.Count, correctAnswersCount, startTime, endTime);

        }
        private void StartupInformation()
        {
            Console.WriteLine($"{new string('-', 40)}\nYou need to answer {Questions.Count} questions on {Topic}.\n" +
                $"There may be several correct answers. If you want to select multiple options,\n" +
                $"write them with a Space and then press Enter. If you do not indicate all correct answers or indicate more,\n" +
                $"then the question will not be counted as correctly completed. \nPress any key to start.\n{new string('-', 40)}");
            Console.ReadKey();
        }
        private void ShowInformationInQuiz(string user, int correctAnswersCount, int progress, Question question)
        {
            Console.WriteLine($"=========================== {Topic} quiz ===========================");
            Console.WriteLine($"User: {user}. Progress: {progress}/{Questions.Count}. Result: {correctAnswersCount}\n");
            Console.WriteLine(question.Text);
            for (int i = 0; i < question.Options.Count; i++)
                Console.WriteLine($"{i + 1}. {question.Options[i]}");
        }

    }
}
