using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public interface IFormula
    {
        string ToString();
        bool Equals(IFormula form);
    }

    class PropVar : IFormula
    {
        string name;

        public PropVar(string n)
        {
            name = n;
        }

        public bool Equals(IFormula form)
        {
            if (form is PropVar)
            {
                PropVar vari = form as PropVar;
                return vari.name == name;
            }
            return false;
        }

        public override string ToString()
        {
            return name;
        }
    }

    class Negation : IFormula
    {
        IFormula formula;

        public Negation(IFormula neg)
        {
            formula = neg;
        }

        public bool Equals(IFormula form)
        {
            if(form is Negation)
            {
                Negation neg = form as Negation;
                return formula.Equals(neg.formula);
            }
            return false;
        }

        public override string ToString()
        {
            return "-" + formula.ToString();
        }
    }

    class Disjunction : IFormula
    {
        IFormula left, right;

        public Disjunction(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        public bool Equals(IFormula form)
        {
            if (form is Disjunction)
            {
                Disjunction klos = form as Disjunction;
                if (left.Equals(klos.left))
                {
                    return right.Equals(klos.right);
                }
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + left.ToString() + "\\/" + right.ToString() + ")";
        }
    }

    class Conjunction : IFormula
    {
        IFormula left, right;

        public Conjunction(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        public bool Equals(IFormula form)
        {
            if (form is Conjunction)
            {
                Conjunction conj = form as Conjunction;
                if (left.Equals(conj.left))
                {
                    return right.Equals(conj.right);
                }
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + left.ToString() + "/\\" + right.ToString() + ")";
        }
    }

    class Implication : IFormula
    {
        IFormula left, right;

        public Implication(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        public bool Equals(IFormula form)
        {
            if (form is Implication)
            {
                Implication imp = form as Implication;
                if (left.Equals(imp.left))
                {
                    return right.Equals(imp.right);
                }
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + left.ToString() + "->" + right.ToString() + ")";
        }
    }

    class Iff : IFormula
    {
        IFormula left, right;

        public Iff(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        public bool Equals(IFormula form)
        {
            if (form is Iff)
            {
                Iff iff = form as Iff;
                if (left.Equals(iff.left))
                {
                    return right.Equals(iff.right);
                }
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + left.ToString() + "<->" + right.ToString() + ")";
        }
    }
}
