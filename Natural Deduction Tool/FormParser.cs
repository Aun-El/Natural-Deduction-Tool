using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    class FormParser
    {
        private string content;
        private int cursor;
        private int length;

        /// <summary>
        /// Transforms the input into an IFormula and returns it.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static IFormula ParseFormula(string s)
        {
            FormParser parser = new FormParser(s);
            return parser.ParseFormula();
        }

        private FormParser(string s)
        {
            content = s;
            cursor = 0;
            length = s.Length;
        }

        private void SkipSpaces()
        {
            while (cursor < length && char.IsWhiteSpace(content[cursor]))
                cursor++;
        }

        private IFormula ParseFormula()
        {
            IFormula i = ParseIff();
            SkipSpaces();
            if (cursor < length)
                throw new Exception($"Extra input on position {cursor} ({content[cursor]})");
            return i;
        }

        private IFormula ParseFactor()
        {
            SkipSpaces();
            if (cursor < length && content[cursor] == '(')
            {
                cursor++;
                IFormula resultaat = ParseIff();
                SkipSpaces();
                if (content[cursor] != ')') throw new Exception("sluithaakje ontbreekt op positie " + cursor);
                cursor++;
                return resultaat;
            }
            else if (cursor < length && ((content[cursor] == '-' && content[cursor + 1] != '>') || content[cursor] == '!' || content[cursor] == '~'))
            {
                cursor++;
                IFormula n = ParseFactor();
                return MakeNegation(n);
            }
            else
            {
                int t = 1;
                while (cursor + t < length && Char.IsLetterOrDigit(content, cursor + t))
                    t++;
                string s = content.Substring(cursor, t);
                cursor += t;
                return MakeProposition(s);
            }
        }

        private IFormula ParseTerm()
        {
            IFormula f = ParseFactor();
            SkipSpaces();
            if (cursor < length - 1 && (content[cursor] == '/' && content[cursor + 1] == '\\' || content[cursor] == '&' && content[cursor + 1] == '&'))
            {
                cursor += 2;
                IFormula t = ParseTerm();
                return MakeConjunction(f, t);
            }
            return f;
        }

        private IFormula ParseExpression()
        {
            IFormula t = ParseTerm();
            SkipSpaces();
            if (cursor < length - 1 && (content[cursor] == '\\' && content[cursor + 1] == '/' || content[cursor] == '|' && content[cursor + 1] == '|'))
            {
                cursor += 2;
                IFormula e = ParseIff();
                return MakeDisjunction(t, e);
            }
            return t;
        }

        private IFormula ParseIff()
        {
            IFormula i = ParseImpl();
            SkipSpaces();
            if (cursor < length - 1 && content[cursor] == '<' && content[cursor + 1] == '-' && content[cursor + 2] == '>')
            {
                cursor += 3;
                IFormula e = ParseIff();
                return MakeIff(i, e);
            }
            return i;
        }

        private IFormula ParseImpl()
        {
            IFormula e = ParseExpression();
            SkipSpaces();
            if (cursor < length - 1 && content[cursor] == '-' && content[cursor + 1] == '>')
            {
                cursor += 2;
                IFormula i = ParseIff();
                return MakeImplication(e, i);
            }
            return e;
        }

        static IFormula MakeProposition(string variabele)
        {
            PropVar prop = new PropVar(variabele);
            return prop;
        }

        static IFormula MakeNegation(IFormula formule)
        {
            Negation neg = new Negation(formule);
            return neg;
        }

        static IFormula MakeConjunction(IFormula left, IFormula right)
        {
            Conjunction conj = new Conjunction(left, right);
            return conj;
        }

        static IFormula MakeDisjunction(IFormula left, IFormula right)
        {
            Disjunction disj = new Disjunction(left, right);
            return disj;
        }

        static IFormula MakeImplication(IFormula left, IFormula right)
        {
            return new Implication(left, right);
        }

        static IFormula MakeIff(IFormula left, IFormula right)
        {
            return new Iff(left, right);
        }
    }
}
