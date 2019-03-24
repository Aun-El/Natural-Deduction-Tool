using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public interface IFormula
    {
        string ToString();
        bool Equals(object e);
        bool PossiblyTrue(Valuation valuation);
        bool PossiblyUntrue(Valuation valuation);
        HashSet<IFormula> GetSubForms(HashSet<IFormula> set);
        void Collect(ISet<string> set);
    }

    class PropVar : IFormula
    {
        string name;

        [DebuggerStepThrough]
        public PropVar(string n)
        {
            name = n;
        }

        public HashSet<IFormula> GetSubForms(HashSet<IFormula> set)
        {
            set.Add(this);
            return set;
        }

        public bool PossiblyTrue(Valuation val)
        {
            if (val.ContainsVar(name))
            {
                return val.GiveVal(name);
            }
            return true;
        }

        public bool PossiblyUntrue(Valuation val)
        {
            if (val.ContainsVar(name))
            {
                return !val.GiveVal(name);
            }
            return true;
        }

        public void Collect(ISet<string> set)
        {
            set.Add(name);
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return name;
        }
    }

    public class Negation : IFormula
    {
        IFormula formula;
        public IFormula Formula { get { return formula; } }

        public Negation(IFormula neg)
        {
            formula = neg;
        }

        [DebuggerStepThrough]
        public HashSet<IFormula> GetSubForms(HashSet<IFormula> set)
        {
            set.Add(this);
            formula.GetSubForms(set);
            return set;
        }

        public bool PossiblyTrue(Valuation val)
        {
            return formula.PossiblyUntrue(val);
        }

        public bool PossiblyUntrue(Valuation val)
        {
            return formula.PossiblyTrue(val);
        }

        public void Collect(ISet<string> set)
        {
            formula.Collect(set);
        }

        public IFormula RemoveRedundantNegs()
        {
            if (formula is Negation)
            {
                Negation neg = formula as Negation;
                if(neg.formula is Negation)
                {
                    Negation neg2 = neg.formula as Negation;
                    return neg2.RemoveRedundantNegs();
                }
                return neg.formula;
            }
            else
            {
                return this;
            }
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "-" + formula.ToString();
        }
    }

    public class Disjunction : IFormula
    {
        IFormula left, right;

        public IFormula Left { get { return left; } }
        public IFormula Right { get { return right; } }

        [DebuggerStepThrough]
        public Disjunction(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        public HashSet<IFormula> GetSubForms(HashSet<IFormula> set)
        {
            set.Add(this);
            left.GetSubForms(set);
            right.GetSubForms(set);
            return set;
        }

        public bool PossiblyTrue(Valuation val)
        {
            return left.PossiblyTrue(val) || right.PossiblyTrue(val);
        }

        public bool PossiblyUntrue(Valuation val)
        {
            return left.PossiblyUntrue(val) && right.PossiblyUntrue(val);
        }

        public void Collect(ISet<string> set)
        {
            left.Collect(set);
            right.Collect(set);
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "(" + left.ToString() + "\\/" + right.ToString() + ")";
        }
    }

    public class Conjunction : IFormula
    {
        IFormula left, right;
        public IFormula Left { get { return left; } }
        public IFormula Right { get { return right; } }

        [DebuggerStepThrough]
        public Conjunction(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }
        
        public static Conjunction Conjoin(List<IFormula> forms)
        {
            Conjunction output = new Conjunction(forms[0], forms[1]);
            for (int i = 2; i < forms.Count; i++)
            {
                output = new Conjunction(output, forms[i]);
            }
            return output;
        }

        public HashSet<IFormula> GetSubForms(HashSet<IFormula> set)
        {
            set.Add(this);
            left.GetSubForms(set);
            right.GetSubForms(set);
            return set;
        }

        public bool PossiblyTrue(Valuation val)
        {
            return left.PossiblyTrue(val) && right.PossiblyTrue(val);
        }

        public bool PossiblyUntrue(Valuation val)
        {
            return left.PossiblyUntrue(val) || right.PossiblyUntrue(val);
        }

        public void Collect(ISet<string> set)
        {
            left.Collect(set);
            right.Collect(set);
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "(" + left.ToString() + "/\\" + right.ToString() + ")";
        }
    }

    public class Implication : IFormula
    {
        IFormula left, right;
        public IFormula Antecedent { get { return left; } }
        public IFormula Consequent { get { return right; } }

        [DebuggerStepThrough]
        public Implication(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        /// <summary>
        /// Returns an implication with only a consequent. Used for modus ponens checking.
        /// </summary>
        /// <param name="r"></param>
        public Implication(IFormula r, bool reminder)
        {
            right = r;
            left = null;
        }

        public HashSet<IFormula> GetSubForms(HashSet<IFormula> set)
        {
            set.Add(this);
            left.GetSubForms(set);
            right.GetSubForms(set);
            return set;
        }

        public bool PossiblyTrue(Valuation val)
        {
            return left.PossiblyUntrue(val) || right.PossiblyTrue(val);
        }

        public bool PossiblyUntrue(Valuation val)
        {
            return left.PossiblyTrue(val) && right.PossiblyUntrue(val);
        }

        public void Collect(ISet<string> set)
        {
            left.Collect(set);
            right.Collect(set);
        }

        [DebuggerStepThrough]
        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()) || left == null)
            {
                return false;
            }
            else
            {
                return ToString() == ((Implication)obj).ToString();
            }
        }

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "(" + left.ToString() + "->" + right.ToString() + ")";
        }
    }

    public class Iff : IFormula
    {
        IFormula left, right;
        public IFormula Left { get { return left; } }
        public IFormula Right { get { return right; } }

        [DebuggerStepThrough]
        public Iff(IFormula l, IFormula r)
        {
            left = l;
            right = r;
        }

        public HashSet<IFormula> GetSubForms(HashSet<IFormula> set)
        {
            set.Add(this);
            left.GetSubForms(set);
            right.GetSubForms(set);
            return set;
        }

        public bool PossiblyTrue(Valuation val)
        {
            return (left.PossiblyTrue(val) && right.PossiblyTrue(val)) || (left.PossiblyUntrue(val) && right.PossiblyUntrue(val));
        }

        public bool PossiblyUntrue(Valuation val)
        {
            return (left.PossiblyUntrue(val) && right.PossiblyTrue(val)) || (left.PossiblyTrue(val) && right.PossiblyUntrue(val));
        }

        public void Collect(ISet<string> set)
        {
            left.Collect(set);
            right.Collect(set);
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return "(" + left.ToString() + "<->" + right.ToString() + ")";
        }
    }
}
