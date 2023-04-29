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
        public string QuizSection { get; set; }
        private List<Question> _questions;
        public List<Question> Questions
        {
            get => _questions;
            private set
            {
                if (value.Count < 2)
                    throw new ArgumentException("There should be more than 2 questions");
                _questions = value;
            }
        }
        #endregion
        //===========================================
        #region ctors
        public Quiz()
        {
            _questions = new List<Question>();

        }
        #endregion
        //===========================================
        #region Inherited methods

        #endregion
        //===========================================
        #region Own methods

        #endregion

    }
}
