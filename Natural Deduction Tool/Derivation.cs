using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Derivation
    {
        public IFormula Form { get; }
        public Origin Origin { get; set; }
        public int Length { get; private set; }
        public List<Derivation> Children { get; }

        public Derivation(IFormula form, Origin orig)
        {
            Form = form;
            Origin = orig;
            Children = new List<Derivation>();
            if (Origin.rule != Rules.HYPO && Origin.rule != Rules.ASS)
            {
                int temp = 0;
                foreach (Derivation deriv in Origin.parents)
                {
                    temp += deriv.Length;
                }
                Length = temp + 1;
                foreach (Derivation parent in Origin.parents)
                {
                    parent.Children.Add(this);
                }
            }
            else
            {
                Length = Origin.depth;
            }
        }

        public void ReplaceParent(Derivation newParent)
        {
            for (int i = 0; i < Origin.parents.Count; i++)
            {
                if(Origin.parents[i].Form.Equals(newParent.Form) && Origin.parents[i].Length > newParent.Length)
                {
                    Origin.parents.RemoveAt(i);
                    Origin.parents.Insert(i, newParent);
                    RecalcLength();
                    break;
                }
            }
        }

        private void RecalcLength()
        {
            int temp = 0;
            foreach (Derivation deriv in Origin.parents)
            {
                temp += deriv.Length;
            }
            Length = temp + 1;
            foreach(Derivation child in Children)
            {
                child.RecalcLength();
            }
        }
    }

    public class Origin
    {
        public List<IFormula> origins;
        public List<Derivation> parents;
        public Rules rule;
        public int depth;

        public Origin(List<IFormula> orig, Rules rul, List<Derivation> par)
        {
            if (rul == Rules.HYPO || rul == Rules.ASS)
            {
                throw new Exception("Tried making a non-hypothesis/non-assumption origin on a hypothesis/assumption.");
            }
            origins = orig;
            rule = rul;
            parents = par;
        }

        public Origin(Rules rul, int dpt = 0)
        {
            if(rul != Rules.HYPO && rul != Rules.ASS)
            {
                throw new Exception("Tried making a hypothesis/assumption origin on non-hypothesis/non-assumption.");
            }
            origins = null;
            parents = null;
            depth = dpt;
            rule = rul;
        }
    }
}
