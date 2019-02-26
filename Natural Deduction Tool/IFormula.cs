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
        bool Equals(object e);
    }

    class PropVar : IFormula
    {
        string name;

        public PropVar(string n)
        {
            name = n;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return ToString() == ((PropVar)obj).ToString();
            }
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }
    }

    class Negation : IFormula
    {
        IFormula formula;
        public IFormula Formula { get { return formula; } }

        public Negation(IFormula neg)
        {
            formula = neg;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return ToString() == ((Negation)obj).ToString();
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
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

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return ToString() == ((Disjunction)obj).ToString();
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return "(" + left.ToString() + "\\/" + right.ToString() + ")";
        }
    }

    class Conjunction : IFormula
    {
        IFormula left, right;
        public IFormula Left { get { return left; } }
        public IFormula Right { get { return right; } }

        public Conjunction(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return ToString() == ((Conjunction)obj).ToString();
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
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

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return ToString() == ((Implication)obj).ToString();
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
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

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return ToString() == ((Iff)obj).ToString();
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return "(" + left.ToString() + "<->" + right.ToString() + ")";
        }
    }
}
