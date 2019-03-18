using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Valuation
    {
        private readonly SortedDictionary<string, bool> dictionary;

        // Constructor.
        public Valuation()
        {
            dictionary = new SortedDictionary<string, bool>();
        }

        // Geeft terug of deze valuatie de gegeven variabele bevat.
        public bool ContainsVar(string var)
        {
            return dictionary.ContainsKey(var);
        }

        // Geeft de waarde van de gegeven variabele terug.
        public bool GiveVal(string var)
        {
            return dictionary[var];
        }

        // Voegt de gegeven variabele met de gegeven waarde toe aan deze valuatie.
        public void Add(string var, bool val)
        {
            dictionary.Add(var, val);
        }

        // Verwijdert de gegeven variabele uit deze valuatie.
        public void Remove(string var)
        {
            dictionary.Remove(var);
        }

        public override string ToString()
        {
            string resultaat = "";
            foreach (KeyValuePair<string, bool> anna in dictionary)
                resultaat += (anna.Key + "=" + anna.Value + " ");
            return resultaat;
        }
    }
}
