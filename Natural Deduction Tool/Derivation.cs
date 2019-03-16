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
        public List<Derivation> orLeftOrig;
        public List<Derivation> orRightOrig;
        public Rules rule;

        public Origin(List<IFormula> orig, Rules rul)
        {
            origins = orig;
            rule = rul;
        }

        public Origin(IFormula orig, Rules rul, List<Derivation> left, List<Derivation> right)
        {
            if(rul != Rules.OR)
            {
                throw new Exception("Tried making a disjunction elimination origin on a non-disjunction.");
            }
            origins = new List<IFormula>() { orig };
            rule = rul;
            orLeftOrig = left;
            orRightOrig = right;
        }
    }
}
