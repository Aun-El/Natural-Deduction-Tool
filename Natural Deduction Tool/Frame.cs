using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Frame
    {
        public List<Tuple<IFormula, Node, Annotation>> frame;

        public Frame()
        {
            frame = new List<Tuple<IFormula, Node, Annotation>>();
        }

        public Frame(List<IFormula> premises)
        {
            frame = new List<Tuple<IFormula, Node, Annotation>>();
            Node currentNode = null;
            foreach (IFormula premise in premises)
            {
                if (currentNode != null)
                {
                    currentNode = new Node(currentNode);
                }
                else
                {
                    currentNode = new Node();
                }
                currentNode.facts.Add(premise);
                frame.Add(new Tuple<IFormula, Node, Annotation>(premise, currentNode, new Annotation(new List<int>(), Rules.HYPO, false)));
            }
        }

        /// <summary>
        /// Clones the frame, adds the new formula on the specified hypothesis interval and returns it.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="node"></param>
        /// <param name="anno"></param>
        /// <returns></returns>
        public Frame AddForm(IFormula form, Node node, Annotation anno)
        {
            Frame output = new Frame();
            Node newNode = null;

            foreach (Tuple<IFormula, Node, Annotation> line in frame)
            {
                //New formulas will only be true in their current hypothesis interval
                //It is only necessary to keep a seperate fact list for the interval the new formula will be added to
                if (line.Item2 == node && newNode == null)
                {
                    newNode = line.Item2.Clone();
                    newNode.facts.Add(form);
                    output.frame.Add(new Tuple<IFormula, Node, Annotation>(line.Item1, newNode, line.Item3));
                }
                else if (line.Item2 == node)
                {
                    output.frame.Add(new Tuple<IFormula, Node, Annotation>(line.Item1, newNode, line.Item3));
                }
                else
                {
                    output.frame.Add(new Tuple<IFormula, Node, Annotation>(line.Item1, line.Item2, line.Item3));
                }
            }
            output.frame.Add(new Tuple<IFormula, Node, Annotation>(form, node, anno));
            return output;
        }

        /// <summary>
        /// Clones the frame, adds a new formula on the current hypothesis interval, and returns it.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="anno"></param>
        /// <returns></returns>
        public Frame AddForm(IFormula form, Annotation anno)
        {
            Frame output = new Frame();
            Node newNode = null;
            Node lastNode = frame.Last().Item2;

            foreach (Tuple<IFormula, Node, Annotation> line in frame)
            {
                //New formulas will only be true in their current hypothesis interval
                //It is only necessary to keep a seperate fact list for the interval the new formula will be added to
                if (line.Item2 == lastNode && newNode == null)
                {
                    newNode = line.Item2.Clone();
                    newNode.facts.Add(form);
                    lastNode = newNode;
                    output.frame.Add(new Tuple<IFormula, Node, Annotation>(line.Item1, newNode, line.Item3));
                }
                else if (line.Item2 == lastNode)
                {
                    output.frame.Add(new Tuple<IFormula, Node, Annotation>(line.Item1, newNode, line.Item3));
                }
                else
                {
                    output.frame.Add(new Tuple<IFormula, Node, Annotation>(line.Item1, line.Item2, line.Item3));
                }
            }
            output.frame.Add(new Tuple<IFormula, Node, Annotation>(form, lastNode, anno));
            return output;
        }

        public HashSet<IFormula> ReturnFacts()
        {
            return frame.Last().Item2.ReturnFacts();
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            int lineCounter = 1;

            foreach (Tuple<IFormula, Node, Annotation> line in frame)
            {
                output.Append($"{lineCounter}\t{line.Item1}\t{line.Item3}\r\n");
                lineCounter++;
            }

            return output.ToString();
        }
    }

    public class Node
    {
        public Node parent;
        public List<IFormula> facts;

        public Node()
        {
            facts = new List<IFormula>();
        }

        public Node(Node par)
        {
            facts = new List<IFormula>();
            foreach (IFormula fact in par.facts)
            {
                facts.Add(fact);
            }
            parent = par;
        }

        public Node Clone()
        {
            Node output = new Node();
            foreach (IFormula fact in facts)
            {
                output.facts.Add(fact);
            }
            output.parent = parent;
            return output;
        }

        public HashSet<IFormula> ReturnFacts()
        {
            HashSet<IFormula> output = new HashSet<IFormula>();
            Node currentNode = this;
            while (currentNode.parent != null)
            {
                foreach (IFormula fact in currentNode.facts)
                {
                    output.Add(fact);
                }
                currentNode = currentNode.parent;
            }
            foreach (IFormula fact in currentNode.facts)
            {
                output.Add(fact);
            }

            return output;
        }
    }

    public class Annotation
    {
        List<int> lines;
        Rules rule;
        bool intro;

        public Annotation(List<int> lin, Rules rul, bool intr)
        {
            lines = new List<int>();
            rule = rul;
            intro = intr;
            foreach (int i in lin)
            {
                lines.Add(i);
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            switch (rule)
            {
                case Rules.REI:
                    output.Append($"REI {lines[0]}");
                    break;
                case Rules.HYPO:
                    output.Append("Hypothesis");
                    break;
                case Rules.BI:
                    output.Append($"<-> {HelpAnno()}");
                    break;
                case Rules.IMP:
                    output.Append($"-> {HelpAnno()}");
                    break;
                case Rules.NEG:
                    output.Append($"- {HelpAnno()}");
                    break;
                case Rules.OR:
                    output.Append($@"\/ {HelpAnno()}");
                    break;
                case Rules.AND:
                    output.Append($@"/\ {HelpAnno()}");
                    break;
            }
            return output.ToString();
        }

        /// <summary>
        /// A helper method for building the annotation string
        /// </summary>
        /// <returns></returns>
        private string HelpAnno()
        {
            StringBuilder output = new StringBuilder();
            if (intro)
            {
                output.Append("intro");
            }
            else
            {
                output.Append("elim");
            }
            foreach (int i in lines)
            {
                output.Append($", {i}");
            }
            return output.ToString();
        }
    }

    public enum Rules
    {
        REI,
        OR,
        AND,
        IMP,
        BI,
        NEG,
        HYPO
    }
}
