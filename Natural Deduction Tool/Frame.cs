using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Frame
    {
        public List<Line> frame;
        public Line Last { get { return frame.Last(); } }
        public List<Frame> DerivedFrames;

        public Frame()
        {
            frame = new List<Line>();
            DerivedFrames = new List<Frame>();
        }

        public Frame(List<IFormula> premises)
        {
            frame = new List<Line>();
            DerivedFrames = new List<Frame>();
            Interval currentNode = new Interval();
            foreach (IFormula premise in premises)
            {
                /*if (currentNode != null)
                {
                    currentNode = new Node(currentNode);
                }
                else
                {
                    currentNode = new Node();
                }*/
                currentNode.facts.Add(premise);
                frame.Add(new Line(premise, currentNode, new Annotation(new List<int>(), Rules.HYPO, false)));
            }
        }

        public Frame AddAss(IFormula form)
        {
            Frame output = new Frame();
            Interval newNode = new Interval(frame.Last().Item2);
            Annotation anno = new Annotation(Rules.ASS);

            foreach (Line line in frame)
            {
                //New formulas will only be true in their current hypothesis interval
                //It is only necessary to keep a seperate fact list for the interval the new formula will be added to
                output.frame.Add(new Line(line.Item1, line.Item2, line.Item3));
            }
            output.frame.Add(new Line(form, newNode, anno));
            newNode.facts.Add(form);
            return output;
        }

        /// <summary>
        /// Clones the frame, adds the new formula on the specified hypothesis interval and returns it.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="node"></param>
        /// <param name="anno"></param>
        /// <returns></returns>
        public Frame AddForm(IFormula form, Interval node, Annotation anno)
        {
            Frame output = new Frame();
            Interval currentNode = frame.First().Item2;
            Interval newNode = currentNode.Clone();
            Dictionary<Interval, Interval> links = new Dictionary<Interval, Interval>();
            links[currentNode] = newNode;

            if (frame.Last().Item2 != node && frame.Last().Item2.parent != node)
            {
                throw new Exception("Illegal AddForm call");
            }
            foreach (Line line in frame)
            {
                if (currentNode != line.Item2)
                {
                    if (!links.ContainsKey(line.Item2))
                    {
                        //Add new entry to the dictionary if the interval has not yet been seen, i.e. a new interval has been opened
                        newNode = line.Item2.Clone(newNode);
                        currentNode = line.Item2;
                        links[currentNode] = newNode;
                    }
                    else
                    {
                        currentNode = line.Item2;
                        newNode = links[currentNode];
                    }
                }
                output.frame.Add(new Line(line.Item1, newNode, line.Item3));
            }
            links[node].facts.Add(form);
            output.frame.Add(new Line(form, links[node], anno));
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
            Interval currentNode = frame.First().Item2;
            Interval newNode = currentNode.Clone();
            Interval lastNode = frame.Last().Item2;
            Dictionary<Interval, Interval> links = new Dictionary<Interval, Interval>();
            links[currentNode] = newNode;

            foreach (Line line in frame)
            {
                if (!links.ContainsKey(line.Item2))
                {
                    newNode = line.Item2.Clone(newNode);
                    currentNode = line.Item2;
                    links[currentNode] = newNode;
                }
                else
                {
                    currentNode = line.Item2;
                    newNode = links[currentNode];
                }
                output.frame.Add(new Line(line.Item1, newNode, line.Item3));
            }
            links[lastNode].facts.Add(form);
            output.frame.Add(new Line(form, links[lastNode], anno));
            return output;
        }

        /// <summary>
        /// Returns the formulas that are true in the hypothesis interval at the last line of the frame.
        /// </summary>
        /// <returns></returns>
        public HashSet<IFormula> ReturnFacts()
        {
            return Last.Item2.ReturnFacts();
        }

        /// <summary>
        /// Returns the formulas that are true in the specified hypothesis interval.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public HashSet<IFormula> ReturnFacts(Interval node)
        {
            return node.ReturnFacts();
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            int lineCounter = 1;
            int intervalCounter = 1;
            Interval current = frame.First().Item2;
            bool hypotheses = true;
            bool newAss = false;
            foreach (Line line in frame)
            {
                if ((hypotheses && line.Item3.rule != Rules.HYPO) || newAss)
                {
                    hypotheses = false;
                    newAss = false;
                    output.Append("\t");
                    for (int i = 0; i < intervalCounter; i++)
                    {
                        output.Append("| ");
                    }
                    for (int i = 0; i < 9 - intervalCounter; i++)
                    {
                        output.Append("-");
                    }
                    output.Append("\r\n");
                }

                if (line.Item2 != current)
                {
                    //Hypothesis interval closed
                    if (line.Item3.rule != Rules.ASS)
                    {
                        intervalCounter--;
                    }

                    //Hypothesis interval closed and new one opened
                    else if (line.Item2.parent == current.parent)
                    {
                        newAss = true;
                        for (int i = 0; i < intervalCounter - 1; i++)
                        {
                            output.Append("| ");
                        }
                        output.Append("\r\n");
                    }

                    //New hypothesis interval was opened
                    else
                    {
                        intervalCounter++;
                        newAss = true;
                    }
                    current = line.Item2;
                }
                output.Append($"{lineCounter}\t");
                for (int i = 0; i < intervalCounter; i++)
                {
                    output.Append("| ");
                }
                output.Append($"{line.Item1}\t{line.Item3}\r\n");
                lineCounter++;
            }

            return output.ToString();
        }
    }

    public class Line
    {
        public IFormula Item1 { get; set; }
        public Interval Item2 { get; set; }
        public Annotation Item3 { get; set; }
        public HashSet<IFormula> Facts { get { return Item2.ReturnFacts(); } }

        public Line(IFormula form, Interval node, Annotation anno)
        {
            Item1 = form;
            Item2 = node;
            Item3 = anno;
        }
    }

    public class Interval
    {
        public Interval parent;
        public List<IFormula> facts;

        public Interval()
        {
            facts = new List<IFormula>();
        }

        public Interval(Interval par)
        {
            facts = new List<IFormula>();
            parent = par;
        }

        public Interval Clone()
        {
            Interval output = new Interval();
            foreach (IFormula fact in facts)
            {
                output.facts.Add(fact);
            }
            output.parent = parent;
            return output;
        }

        public Interval Clone(Interval par)
        {
            Interval output = new Interval();
            foreach (IFormula fact in facts)
            {
                output.facts.Add(fact);
            }
            output.parent = par;
            return output;
        }

        /// <summary>
        /// Compares two intervals. If the interval is the same as the parameter interval or an ancestor thereof it will return true.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool ThisOrParent(Interval node)
        {
            if (this == node)
            {
                return true;
            }
            else
            {
                if (node.parent != null)
                {
                    return ThisOrParent(node.parent);
                }
                else
                {
                    return false;
                }
            }
        }

        public HashSet<IFormula> ReturnFacts()
        {
            HashSet<IFormula> output = new HashSet<IFormula>();
            Interval currentNode = this;
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
        public Rules rule;
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

        public Annotation(Rules rul)
        {
            lines = new List<int>();
            rule = rul;
            intro = true;
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
                case Rules.ASS:
                    output.Append("Assumption");
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
        HYPO,
        ASS
    }
}
