using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Searcher
    {
        public HashSet<Frame> ClosedList { get; private set; }
        public Queue<Frame> Fringe { get; private set; }


        public static string Proof(List<IFormula> premises, IFormula conclusion)
        {
            Frame init = new Frame(premises);

            //TODO: Make the magic happen
            //TODO: Loop through all rules and see which ones can be applied
            

            foreach(IFormula premise in premises)
            {
                if (premise.Equals(conclusion))
                {
                    return init.ToString();
                }
            }

            return "I can not prove this (yet).";
        }
    }
}
