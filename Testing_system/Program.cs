namespace Quiz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            QuizManager quizManager = new QuizManager();
            quizManager.Run();
            Console.WriteLine("End");
        }
    }
}