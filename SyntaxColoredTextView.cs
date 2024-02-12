using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Terminal.Gui;

namespace tuiedit
{
    internal class SyntaxColoredTextView : TextView
    {
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

        Regex regexNumeric = new Regex(@"-?\d+(\.\d+)?");
        Regex regexHex = new Regex(@"0x[0-9A-Fa-f]+");
        Regex regexArith = new Regex(@"[+\-*/%{}()<>;\[\]]");
        Regex regexRel = new Regex(@"[<>!]?=");
        Regex regexLog = new Regex(@"&&|\|\||!");
        Regex regexDirective1 = new Regex(
            //@"^#[^\S\r\n]+.*$"
            //@"#\s*(include|define|ifdef|ifndef|else|endif|if|elif|undef|pragma|error|line)\b"
            @"#"
        );
        Regex regexDirective2 = new Regex(
            @"\b(include|define|ifdef|ifndef|else|endif|if|elif|undef|pragma|error|line)\b"
        );

        public SyntaxColoredTextView()
            : base()
        {
            Init();
        }

        public void Init()
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

        protected override void SetNormalColor()
        {
            Driver.SetAttribute(white);
        }

        // TODO: Syntax Coloring - Tidy up handling of directives
        // TODO: Syntax Coloring - Add colouring of comments
        // TODO: General - Add code comments & tidy up

        protected override void SetNormalColor(List<System.Rune> line, int idx)
        {
            var word = IdxToWord(line, idx);

            if (IsNumeric(word))
            {
                Driver.SetAttribute(red);
                return;
            }

            if (IsInDirective(word))
            {
                Driver.SetAttribute(brown);
                return;
            }

            if (IsInCOperator(word))
            {
                Driver.SetAttribute(magenta);
                return;
            }

            if (IsKeyword(word))
            {
                Driver.SetAttribute(blue);
                return;
            }

            Driver.SetAttribute(white);
        }

        private bool IsNumeric(string word)
        {
            if (regexHex.IsMatch(word))
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

        private bool IsInCOperator(string word)
        {
            if (regexArith.IsMatch(word))
            {
                return true;
            }

            if (regexRel.IsMatch(word))
            {
                return true;
            }

            if (regexLog.IsMatch(word))
            {
                return true;
            }

            return false;
        }

        private bool IsInDirective(string word)
        {
            if (regexDirective1.IsMatch(word))
            {
                return true;
            }

            if (regexDirective2.IsMatch(word))
            {
                return true;
            }

            return false;
        }

        private bool IsKeyword(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return false;
            }

            return keywords.Contains(word, StringComparer.CurrentCultureIgnoreCase);
        }

        private string IdxToWord(List<System.Rune> line, int idx)
        {
            var words = Regex.Split(new string(line.Select(r => (char)r).ToArray()), "\\b");
            // var words = Regex.Split(new string(line.Select(r => (char)r).ToArray()), " ");

            int count = 0;
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
    }
}
