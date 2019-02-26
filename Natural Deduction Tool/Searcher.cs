using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Searcher
    {
        public static HashSet<Frame> ClosedList { get; private set; }
        public static Queue<Frame> Fringe { get; private set; }


        public static string Proof(List<IFormula> premises, IFormula conclusion)
        {
            Frame init = new Frame(premises);
            Fringe = new Queue<Frame>();

            //TODO: Make the magic happen
            //TODO: Loop through all rules and see which ones can be applied

            foreach (IFormula premise in premises)
            {
                if (premise.Equals(conclusion))
                {
                    return init.ToString();
                }
            }

            Fringe.Enqueue(init);
            while (Fringe.Any())
            {
                Frame currentFrame = Fringe.Dequeue();
                foreach (Frame inference in ConjElim(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in NegElim(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
            }
            return "I can not prove this (yet).";
        }

        private static List<Frame> NegIntro(Frame frame)
        {

            return new List<Frame>();
        }

        private static List<Frame> NegElim(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Tuple<IFormula, Node, Annotation> line in frame.frame)
            {
                if (line.Item1 is Negation)
                {
                    Negation neg = line.Item1 as Negation;
                    if (neg.Formula is Negation)
                    {
                        Negation neg2 = neg.Formula as Negation;
                        if (!facts.Contains(neg2.Formula))
                        {
                            List<int> lines = new List<int>();
                            lines.Add(frame.frame.IndexOf(line) + 1);
                            output.Add(frame.AddForm(neg2.Formula, new Annotation(lines, Rules.NEG, false)));
                        }
                    }
                }
            }
            return output;
        }

        private static List<Frame> ConjIntro(Frame frame)
        {
            return new List<Frame>();
        }

        private static List<Frame> ConjElim(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Tuple<IFormula, Node, Annotation> line in frame.frame)
            {
                if (line.Item1 is Conjunction)
                {
                    Conjunction conj = line.Item1 as Conjunction;
                    if (!facts.Contains(conj.Left))
                    {
                        List<int> lines = new List<int>();
                        lines.Add(frame.frame.IndexOf(line) + 1);
                        output.Add(frame.AddForm(conj.Left, new Annotation(lines, Rules.AND, false)));
                    }
                    if (!facts.Contains(conj.Right))
                    {
                        List<int> lines = new List<int>();
                        lines.Add(frame.frame.IndexOf(line) + 1);
                        output.Add(frame.AddForm(conj.Right, new Annotation(lines, Rules.AND, false)));
                    }
                }
            }
            return output;
        }

        private static List<Frame> DisjIntro(Frame frame)
        {

            return new List<Frame>();
        }

        private static List<Frame> DisjElim(Frame frame)
        {
            return new List<Frame>();
        }

        private static List<Frame> ImplIntro(Frame frame)
        {

            return new List<Frame>();
        }

        private static List<Frame> ImplElim(Frame frame)
        {
            return new List<Frame>();
        }

        private static List<Frame> IffIntro(Frame frame)
        {

            return new List<Frame>();
        }

        private static List<Frame> IffElim(Frame frame)
        {
            return new List<Frame>();
        }
    }
}
