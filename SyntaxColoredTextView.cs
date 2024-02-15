using System.Text.RegularExpressions;
using Terminal.Gui;

namespace tuiedit
{
    /// <summary>
    /// Class derived from TextView which supports syntax colouring
    /// </summary>
    /// <seealso cref="Terminal.Gui.TextView" />
    internal class SyntaxColoredTextView : TextView
    {
        #region Fields and properties

        private HashSet<string> keywords = new HashSet<string>(
            StringComparer.CurrentCultureIgnoreCase
        );
        private Terminal.Gui.Attribute blue;
        private Terminal.Gui.Attribute white;
        private Terminal.Gui.Attribute magenta;
        private Terminal.Gui.Attribute yellow;
        private Terminal.Gui.Attribute green;
        private Terminal.Gui.Attribute gray;
        private Terminal.Gui.Attribute brown;
        private Terminal.Gui.Attribute red;

        Regex numerical = new Regex(@"-?\d+(\.\d+)?");
        Regex hexidecimal = new Regex(@"0x[0-9A-Fa-f]+");
        Regex arithmatic = new Regex(@"[+\-*/%{}()<>;\[\]]");
        Regex relational = new Regex(@"[<>!]?=");
        Regex logical = new Regex(@"&|\|\||!");
        Regex directive1 = new Regex(
            //@"^#[^\S\r\n]+.*$"
            //@"#\s*(include|define|ifdef|ifndef|else|endif|if|elif|undef|pragma|error|line)\b"
            @"#"
        );
        Regex directive2 = new Regex(
            @"\b(include|define|ifdef|ifndef|else|endif|if|elif|undef|pragma|error|line)\b"
        );
        Regex singleLineComments = new Regex(@"\/\/[^\\r\\n]*");

        #endregion

        #region Class Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxColoredTextView"/> class.
        /// </summary>
        public SyntaxColoredTextView()
            : base()
        {
            keywords.Add("auto");
            keywords.Add("break");
            keywords.Add("case");
            keywords.Add("char");
            keywords.Add("const");
            keywords.Add("continue");
            keywords.Add("default");
            keywords.Add("do");
            keywords.Add("double");
            keywords.Add("else");
            keywords.Add("enum");
            keywords.Add("extern");
            keywords.Add("float");
            keywords.Add("for");
            keywords.Add("goto");
            keywords.Add("if");
            keywords.Add("int");
            keywords.Add("long");
            keywords.Add("register");
            keywords.Add("return");
            keywords.Add("short");
            keywords.Add("signed");
            keywords.Add("sizeof");
            keywords.Add("static");
            keywords.Add("struct");
            keywords.Add("switch");
            keywords.Add("typedef");
            keywords.Add("union");
            keywords.Add("unsigned");
            keywords.Add("void");
            keywords.Add("volatile");
            keywords.Add("while");
            //keywords.Add("include");
            //keywords.Add("define");

            Autocomplete.AllSuggestions = keywords.ToList();

            magenta = Driver.MakeAttribute(Color.Magenta, Color.Black);
            blue = Driver.MakeAttribute(Color.Cyan, Color.Black);
            gray = Driver.MakeAttribute(Color.Gray, Color.Black);
            white = Driver.MakeAttribute(Color.White, Color.Black);
            yellow = Driver.MakeAttribute(Color.BrightYellow, Color.Black);
            green = Driver.MakeAttribute(Color.BrightGreen, Color.Black);
            red = Driver.MakeAttribute(Color.BrightRed, Color.Black);
            brown = Driver.MakeAttribute(Color.Brown, Color.Black);
        }

        #endregion

        #region Base Class Overrides

        protected override void SetNormalColor()
        {
            Driver.SetAttribute(white);
        }

        // TODO: Syntax Coloring - Handle trailing comments
        // TODO: Syntax Coloring - Handle multi-line comments
        // TODO: General - Add code comments & tidy up

        protected override void SetNormalColor(List<System.Rune> line, int idx)
        {
            // Retrieve current line as a string & the current word ...
            var lineAsString = LineAsString(line);
            var word = IdxToWord(line, idx);

            // Is current word a numeric value ? ...
            if (IsNumericValue(word))
            {
                // Numeric values in RED ...
                Driver.SetAttribute(red);
                return;
            }

            // Is current word a C comment ? ...
            if (IsCComment(lineAsString, word))
            {
                // C comments in GREEN ...
                Driver.SetAttribute(green);
                return;
            }

            // Is current word a C directive ? ...
            if (IsCDirective(lineAsString, word))
            {
                // C directives in BROWN ...
                Driver.SetAttribute(brown);
                return;
            }

            // Is current word a C operator ? ...
            if (IsCOperator(word))
            {
                // C operators in MAGENTA ...
                Driver.SetAttribute(magenta);
                return;
            }

            // Is current word a C keyword ? ...
            if (IsCKeyword(word))
            {
                // C keywords in BLUE ...
                Driver.SetAttribute(blue);
                return;
            }

            // Default to white text ...
            Driver.SetAttribute(white);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the specified word is a numeric value.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>
        ///   <c>true</c> if the specified word is a numeric value; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNumericValue(string word)
        {
            if (hexidecimal.IsMatch(word))
            {
                return true;
            }

            double output;
            return double.TryParse(word, out output);

            //if (regexNumeric.IsMatch(word))
            //{
            //    return true;
            //}
            //
            // return false;
        }

        /// <summary>
        /// Determines whether the specified word is a C operator.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>
        ///   <c>true</c> if specified word is a c operator; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCOperator(string word)
        {
            if (arithmatic.IsMatch(word))
            {
                return true;
            }

            if (relational.IsMatch(word))
            {
                return true;
            }

            if (logical.IsMatch(word))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified line/word is a C comment.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="word">The word.</param>
        /// <returns>
        ///   <c>true</c> if the specified line/word is a C comment; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCComment(string line, string word)
        {
            if (line.StartsWith("//")
            // && singleLineComments.IsMatch(word)
            )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified line/word is a C directive.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="word">The word.</param>
        /// <returns>
        ///   <c>true</c> if the specified line/word is a C directive; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCDirective(string line, string word)
        {
            if (directive1.IsMatch(word))
            {
                return true;
            }

            if (line.StartsWith("#") && directive2.IsMatch(word))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified word is a C keyword.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>
        ///   <c>true</c> if the specified word is a C keyword; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCKeyword(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            return keywords.Contains(word, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Converts the specified line into a string
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        private string LineAsString(List<System.Rune> line)
        {
            return new string(line.Select(r => (char)r).ToArray()).Trim();
        }

        /// <summary>
        /// Returns the containing word from the specified line and index.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="idx">The index.</param>
        /// <returns>The containing word as a string value</returns>
        private string IdxToWord(List<System.Rune> line, int idx)
        {
            var words = Regex.Split(new string(line.Select(r => (char)r).ToArray()), "\\b");
            // var words = Regex.Split(new string(line.Select(r => (char)r).ToArray()), " ");

            var count = 0;
            string current = null;

            foreach (var word in words)
            {
                current = word;
                count += word.Length;
                if (count > idx)
                {
                    break;
                }
            }

            return current?.Trim();
        }

        #endregion
    }
}
