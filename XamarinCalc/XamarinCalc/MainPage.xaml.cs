using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace XamarinCalc
{
    public partial class MainPage : ContentPage
    {
        private List<string> workingEquation = new List<string>();
        private string workingNumber = "";

        // this defines our order of operations. tweak this for alternate universes
        private Dictionary<string, int> precedenceTable = new Dictionary<string, int>()
        {
            { "(", 5 },
            { ")", 4 },
            { "/", 3 },
            { "*", 2 },
            { "+", 1 },
            { "-", 1 },
        };

        public MainPage()
        {
            InitializeComponent();
        }

        void pushOperator(string op)
        {
            if (workingNumber.Length > 0)
            {
                workingEquation.Add(workingNumber);
                workingNumber = "";
            }
            workingEquation.Add(op);
            refreshDisplay();
        }

        void pushBracket(string bracket)
        {
            if (workingNumber.Length > 0)
            {
                workingEquation.Add(workingNumber);
                workingNumber = "";
            }
            workingEquation.Add(bracket);
            refreshDisplay();
        }

        void refreshDisplay()
        {
            LblOutput.Text = string.Join(" ", workingEquation) + " " + workingNumber;
        }

        void Clear_Clicked(object sender, EventArgs e)
        {
            workingEquation.Clear();
            workingNumber = "";
            refreshDisplay();
        }

        bool isOperator(string token)
        {
            switch (token)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                    return true;
                default: return false;
            }
        }

        bool isNumber(string token)
        {
            return !isOperator(token) && !isLeftBracket(token) && !isRightBracket(token);
        }

        bool precedenceGreater(string lhs, string rhs)
        {
            return precedenceTable[lhs] > precedenceTable[rhs];
        }

        List<string> shuntingAlgorithm(List<string> tokens)
        {

            Stack<string> stack = new Stack<string>();
            List<string> queue = new List<string>();

            foreach (string token in tokens)
            {
                if (isNumber(token))
                {
                    queue.Add(token);
                }
                else if (isOperator(token))
                {
                    while (stack.Count > 0 && !isLeftBracket(stack.Peek()) && !isRightBracket(stack.Peek()) && precedenceGreater(stack.Peek(), token))
                    {
                        queue.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
                else if (isLeftBracket(token))
                {
                    stack.Push(token);
                }
                else if (isRightBracket(token))
                {
                    while (stack.Count > 0 && !isLeftBracket(stack.Peek()))
                    {
                        queue.Add(stack.Pop());
                    }
                    stack.Pop(); // pop the left bracket and discard
                }
            }

            while (stack.Count > 0)
            {
                queue.Add(stack.Pop());
            }

            return queue;
        }

        float reversePolish(List<string> tokens)
        {
            Stack<string> stack = new Stack<string>();

            foreach (string token in tokens)
            {
                if (isNumber(token))
                {
                    stack.Push(token);
                }
                else if (isOperator(token))
                {
                    var rhs = float.Parse(stack.Pop());
                    var lhs = float.Parse(stack.Pop());

                    float result = 0;

                    switch (token)
                    {
                        case "/":
                            result = lhs / rhs;
                            break;
                        case "*":
                            result = lhs * rhs;
                            break;
                        case "+":
                            result = lhs + rhs;
                            break;
                        case "-":
                            result = lhs - rhs;
                            break;
                    }

                    stack.Push(result.ToString());
                }
            }

            // the result should be the only item left on the stack
            return (stack.Count > 0) 
                ? float.Parse(stack.Pop()) 
                : float.NaN;
        }

        void Equals_Clicked(object sender, EventArgs e)
        {
            if (workingNumber.Length > 0)
            {
                workingEquation.Add(workingNumber);
                workingNumber = "";
            }

            try
            {
                List<string> converted = shuntingAlgorithm(workingEquation);
                float result = reversePolish(converted);

                workingEquation.Clear();
                LblOutput.Text = result.ToString();
            } catch (Exception error)
            {
                workingEquation.Clear();
                workingNumber = "";
                LblOutput.Text = "ERROR: " + error.Message;
            }
        }

        void Operator_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            pushOperator(button.Text);
        }

        bool isLeftBracket(string val)
        {
            return val == "(";
        }

        bool isRightBracket(string val)
        {
            return val == ")";
        }

        void Bracket_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string bracket = button.Text;

            if (isLeftBracket(bracket))
            {
                pushBracket(bracket);
            }
            else if (isRightBracket(bracket))
            {
                int leftBracketCount = workingEquation.FindAll(isLeftBracket).Count;
                int rightBracketCount = workingEquation.FindAll(isRightBracket).Count;

                if (leftBracketCount > rightBracketCount)
                {
                    pushBracket(bracket);
                }
            }
        }

        void Decimal_Clicked(object sender, EventArgs e)
        {
            if (workingNumber.Length > 0)
            {
                if (!workingNumber.Contains("."))
                {
                    workingNumber += ".";
                }
            } else
            {
                workingNumber = "0.";
            }
            refreshDisplay();
        }

        void Number_Clicked(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            workingNumber += btn.Text;
            refreshDisplay();
        }
    }
}
