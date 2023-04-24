using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz
{
    internal class Question
    {
        public string Description { get; set; }
        private List<string> _answerOptions;
        public List<string> AnswerOptions 
        {
            //get => answerOptions ??= new List<string>();
            get => _answerOptions;
            private set
            {
                if (value.Count < 2) 
                    throw new ArgumentException("There should be more than 2 answer options");
                _answerOptions = value;
            }
        }
        private List<int> _answersCorrect;
        private List<int> AnswersCorrect 
        { 
            get => (0 < _answerOptions.Count ? _answersCorrect : null); 
            set
            {
                if (AnswerOptions.Count <= value.Count)
                    throw new ArgumentException("There should be fewer correct answers than total answers");
                else if (value.Count < 1)
                    throw new ArgumentException("There must be one or more correct answers");
                _answersCorrect = value; 
            }
        }
    }
}
