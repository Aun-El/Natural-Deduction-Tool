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

        public Derivation(IFormula form, Origin orig)
        {
            Form = form;
            Origin = orig;
        }
    }

    public class Origin
    {
        public List<IFormula> origins;
        public List<Derivation> parents;
        public Rules rule;

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

        public Origin(Rules rul)
        {
            if(rul != Rules.HYPO && rul != Rules.ASS)
            {
                throw new Exception("Tried making a hypothesis/assumption origin on non-hypothesis/non-assumption.");
            }
            origins = null;
            parents = null;
            rule = rul;
        }
    }
}
