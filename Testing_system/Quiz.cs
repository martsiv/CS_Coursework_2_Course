using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    internal class Quiz
    {
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
    }
}
