using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    internal class QuizSystem
    {
        private SortedSet<User> users;
        private Dictionary<Quiz, List<Quiz>> quizzes;
        private List<UserResult> userResult;
        private List<QuizResult> quizResult;
    }
}
