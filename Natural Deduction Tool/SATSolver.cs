using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    class SATSolver
    {
        //Written by Julian Veltman

        public static Valuation Satisfiable(IFormula formula)
        {
            if (formula == null)
                return null;

            SortedSet<string> variabeles = new SortedSet<string>();
            formula.Collect(variabeles);

            Valuation valuation = new Valuation();

            return Satisfiable(formula, variabeles, valuation);
        }

        private static Valuation Satisfiable(IFormula formula, SortedSet<string> variabeles, Valuation val)
        {

            if (variabeles.Count == 0)
                return val;
            string p = GetElement(variabeles);
            variabeles.Remove(p);

            val.Add(p, true);

            if (formula.PossiblyTrue(val))
            {
                Valuation temporary = Satisfiable(formula, variabeles, val);
                if (temporary != null) return temporary;
            }
            val.Remove(p);
            val.Add(p, false);

            if (formula.PossiblyTrue(val))
            {
                Valuation temporary = Satisfiable(formula, variabeles, val);
                if (temporary != null) return temporary;
            }

            variabeles.Add(p);
            val.Remove(p);

            return null;
        }

        private static string GetElement(ISet<string> set)
        {
            foreach (string s in set)   // we prepareren ons om alle elementen te doorlopen...
                return s;               // ...maar we pakken meteen de eerste!
            return null;                // in geval van een lege verzameling
        }
    }
}
