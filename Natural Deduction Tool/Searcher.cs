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
        public static HashSet<IFormula> SubForms { get; private set; }

        public static string Proof(List<IFormula> premises, IFormula conclusion)
        {
            if (!premises.Any())
            {
                premises.Add(new PropVar("\u22a4"));
            }
            Frame init = new Frame(premises);
            Fringe = new Queue<Frame>();
            SubForms = conclusion.GetSubForms(new HashSet<IFormula>());

            //TODO: Make the magic happen
            //TODO: Loop through all rules and see which ones can be applied

            foreach (IFormula premise in premises)
            {
                if (premise.Equals(conclusion))
                {
                    return init.ToString();
                }
                SubForms = premise.GetSubForms(SubForms);
            }

            if (premises.Count == 1 && premises[0].Equals(new PropVar("\u22a4")))
            {
                init = init.AddAss(new Negation(conclusion));
            }
            else
            {
                HashSet<IFormula> facts = init.ReturnFacts();
                if (conclusion is Implication && !facts.Contains(conclusion))
                {
                    Implication impl = conclusion as Implication;
                    init = init.AddAss(impl.Antecedent);
                }

                else if (conclusion is Negation && !facts.Contains(conclusion))
                {
                    Negation neg = conclusion as Negation;
                    init = init.AddAss(neg.Formula);
                }
            }

            Fringe.Enqueue(init);

            Fringe.Enqueue(init.AddAss(new Negation(conclusion)));

            while (Fringe.Any())
            {
                Frame currentFrame = Fringe.Dequeue();

                //Only perform conjunction introduction if the conclusion is a conjunction (to prevent memory overload)
                foreach (Frame inference in ConjIntro(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ConjElim(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in DisjIntro(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in NegIntro(currentFrame))
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
                foreach (Frame inference in ImplIntro(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ImplElim(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in IffIntro(currentFrame))
                {
                    if (inference.frame.Last().Item1.Equals(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in IffElim(currentFrame))
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
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                //Assumptions always have a parent hypothesis interval
                if (line.Item2 == frame.frame.Last().Item2 && line.Item3.rule == Rules.ASS)
                {

                    //Remove all redundant negation signs for comparison
                    List<IFormula> noRedundant = new List<IFormula>();
                    foreach (IFormula form in facts)
                    {
                        if (form is Negation)
                        {
                            Negation neg = form as Negation;
                            noRedundant.Add(neg.RemoveRedundantNegs());
                        }
                        else
                        {
                            noRedundant.Add(form);
                        }
                    }
                    foreach (IFormula form in noRedundant)
                    {
                        if (noRedundant.Contains(new Negation(form)))
                        {
                            Negation neg = new Negation(line.Item1);
                            List<IFormula> forms = new List<IFormula> { form, new Negation(form) };
                            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                            output.Add(newFrame.Item1.AddForm(neg, newFrame.Item1.Last.Item2.parent, new Annotation(newFrame.Item2, Rules.NEG, true)));
                        }
                    }
                }
            }
            return output;
        }

        private static List<Frame> NegElim(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                if (line.Item1 is Negation)
                {
                    Negation neg = line.Item1 as Negation;
                    if (neg.Formula is Negation)
                    {
                        Negation neg2 = neg.Formula as Negation;
                        if (!facts.Contains(neg2.Formula))
                        {
                            List<IFormula> forms = new List<IFormula> { line.Item1 };
                            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                            output.Add(newFrame.Item1.AddForm(neg2.Formula, new Annotation(newFrame.Item2, Rules.NEG, false)));
                        }
                    }
                }
            }
            return output;
        }

        private static List<Frame> ConjIntro(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                foreach (Line line2 in frame.frame)
                {
                    if (line2.Item2.ThisOrParent(line.Item2))
                    {
                        Conjunction conj = new Conjunction(line.Item1, line2.Item1);
                        if (!facts.Contains(conj) && SubForms.Contains(conj))
                        {
                            List<IFormula> forms = new List<IFormula> { line.Item1, line2.Item1 };
                            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                            output.Add(newFrame.Item1.AddForm(conj, new Annotation(newFrame.Item2, Rules.AND, true)));
                        }
                        Conjunction conjMirror = new Conjunction(line2.Item1, line.Item1);
                        if (!facts.Contains(conjMirror) && SubForms.Contains(conjMirror))
                        {
                            List<IFormula> forms = new List<IFormula> { line2.Item1, line.Item1 };
                            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                            output.Add(newFrame.Item1.AddForm(conjMirror, new Annotation(newFrame.Item2, Rules.AND, true)));
                        }
                    }
                }
            }
            return output;
        }

        private static List<Frame> ConjElim(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                if (line.Item1 is Conjunction)
                {
                    Conjunction conj = line.Item1 as Conjunction;
                    if (!facts.Contains(conj.Left))
                    {
                        List<IFormula> forms = new List<IFormula> { conj };
                        Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                        output.Add(newFrame.Item1.AddForm(conj.Left, new Annotation(newFrame.Item2, Rules.AND, false)));
                    }
                    if (!facts.Contains(conj.Right))
                    {
                        List<IFormula> forms = new List<IFormula> { conj };
                        Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                        output.Add(newFrame.Item1.AddForm(conj.Right, new Annotation(newFrame.Item2, Rules.AND, false)));
                    }
                }
            }
            return output;
        }

        private static List<Frame> DisjIntro(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                foreach (Line line2 in frame.frame)
                {

                    Disjunction disj = new Disjunction(line.Item1, line2.Item1);
                    if (!facts.Contains(disj) && SubForms.Contains(disj))
                    {
                        List<IFormula> forms = new List<IFormula> { line.Item1, line2.Item1 };
                        Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                        output.Add(newFrame.Item1.AddForm(disj, new Annotation(newFrame.Item2, Rules.OR, true)));
                    }
                    Disjunction disjMirror = new Disjunction(line2.Item1, line.Item1);
                    if (!facts.Contains(disjMirror) && SubForms.Contains(disjMirror))
                    {
                        List<IFormula> forms = new List<IFormula> { line2.Item1, line.Item1 };
                        Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                        output.Add(newFrame.Item1.AddForm(disjMirror, new Annotation(newFrame.Item2, Rules.OR, true)));
                    }
                }

                foreach (IFormula form2 in SubForms)
                {
                    Disjunction disj = new Disjunction(line.Item1, form2);
                    if (!facts.Contains(disj) && SubForms.Contains(disj))
                    {
                        List<int> lines = new List<int>();
                        lines.Add(frame.frame.IndexOf(line) + 1);
                        output.Add(frame.AddForm(disj, new Annotation(lines, Rules.OR, true)));
                    }
                    Disjunction disjMirror = new Disjunction(form2, line.Item1);
                    if (!facts.Contains(disjMirror) && SubForms.Contains(disjMirror))
                    {
                        List<int> lines = new List<int>();
                        lines.Add(frame.frame.IndexOf(line) + 1);
                        output.Add(frame.AddForm(disjMirror, new Annotation(lines, Rules.OR, true)));
                    }
                }
            }
            return output;
        }

        private static List<Frame> DisjElim(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                if (line.Item1 is Disjunction)
                {
                    Disjunction disj = line.Item1 as Disjunction;
                    bool leftFound = false;
                    bool rightFound = false;
                    int leftIndex = 0;
                    int rightIndex = 0;
                    HashSet<IFormula> leftFacts = null;
                    HashSet<IFormula> rightFacts = null;
                    Node leftNode = null;
                    Node rightNode = null;

                    foreach (Line line2 in frame.frame)
                    {
                        if (line2.Item1.Equals(disj.Left) && line2.Item3.rule == Rules.ASS)
                        {
                            leftIndex = frame.frame.IndexOf(line2);
                            leftFound = true;
                            leftFacts = line2.Item2.ReturnFacts();
                            leftNode = line2.Item2;
                        }
                        if (line2.Item1.Equals(disj.Right) && line2.Item3.rule == Rules.ASS)
                        {
                            rightIndex = frame.frame.IndexOf(line2);
                            rightFound = true;
                            rightFacts = line2.Item2.ReturnFacts();
                            rightNode = line2.Item2;
                        }
                        if (leftFound && rightFound)
                        {
                            leftFound = false;
                            rightFound = false;

                            //Intersect the facts of both hypothesis intervals
                            rightFacts.IntersectWith(leftFacts);
                            foreach (IFormula sharedFact in rightFacts)
                            {
                                bool leftREI = false;
                                bool rightREI = false;
                                foreach (Line line3 in frame.frame)
                                {
                                    if (line3.Item1.Equals(sharedFact) && line3.Item2.ThisOrParent(leftNode))
                                    {
                                        leftREI = line3.Item2 != leftNode;
                                    }
                                    if (line3.Item1.Equals(sharedFact) && line3.Item2.ThisOrParent(rightNode))
                                    {
                                        rightREI = line3.Item2 != rightNode;
                                    }
                                    break;
                                }
                                bool cont1 = true;
                                Node currentNode = frame.frame[leftIndex].Item2;
                                Line leftLine = new Line(null, null, null);
                                Line rightLine = new Line(null, null, null);
                                leftIndex++;
                                while (cont1)
                                {
                                    if (frame.frame[leftIndex].Item1 == sharedFact)
                                    {
                                        break;
                                    }
                                    if (!frame.frame[leftIndex].Item2.ThisOrParent(currentNode))
                                    {
                                        cont1 = false;
                                    }
                                    leftIndex++;
                                }

                                bool cont2 = true;
                                currentNode = frame.frame[rightIndex].Item2;
                                rightIndex++;
                                while (cont2)
                                {
                                    if (frame.frame[rightIndex].Item1 == sharedFact)
                                    {
                                        break;
                                    }
                                    if (!frame.frame[rightIndex].Item2.ThisOrParent(currentNode))
                                    {
                                        cont2 = false;
                                    }
                                    rightIndex++;
                                }
                                List<int> lines = new List<int>();
                                if (cont1)
                                {
                                    lines.Add(leftIndex + 1);
                                }
                                else
                                {
                                    output.Add(frame.AddForm(sharedFact, line2.Item2.parent, new Annotation(lines, Rules.REI, false)));
                                }
                                lines.Add(frame.frame.IndexOf(line2) + 1);
                                output.Add(frame.AddForm(sharedFact, line2.Item2.parent, new Annotation(lines, Rules.OR, false)));

                            }
                        }
                    }
                }
                //Assumptions always have a parent hypothesis interval
                if (line.Item3.rule == Rules.ASS)
                {
                    foreach (Line line2 in frame.frame)
                    {
                        if (line2.Item2.ThisOrParent(line.Item2))
                        {
                            Implication impl = new Implication(line.Item1, line2.Item1);
                            if (!line.Item2.parent.ReturnFacts().Contains(impl))
                            {
                                List<int> lines = new List<int>();
                                lines.Add(frame.frame.IndexOf(line) + 1);
                                lines.Add(frame.frame.IndexOf(line2) + 1);
                                output.Add(frame.AddForm(impl, line.Item2.parent, new Annotation(lines, Rules.IMP, true)));
                            }
                        }
                    }
                }
            }
            return output;
        }

        private static List<Frame> ImplIntro(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                //Assumptions always have a parent hypothesis interval
                if (line.Item3.rule == Rules.ASS)
                {
                    int AssLine = frame.frame.IndexOf(line);
                    foreach (Line line2 in frame.frame)
                    {
                        if (line2.Item2.ThisOrParent(line.Item2))
                        {
                            Implication impl = new Implication(line.Item1, line2.Item1);
                            if (!line.Item2.parent.ReturnFacts().Contains(impl) && SubForms.Contains(impl))
                            {
                                List<IFormula> forms = new List<IFormula> { line.Item1, line2.Item1 };
                                Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                                output.Add(newFrame.Item1.AddForm(impl, newFrame.Item1.frame[AssLine].Item2.parent, new Annotation(newFrame.Item2, Rules.IMP, true)));
                            }
                        }
                    }
                }
            }
            return output;
        }

        private static List<Frame> ImplElim(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            Node currentNode = frame.Last.Item2;
            foreach (Line line in frame.frame)
            {
                if (line.Item2.ThisOrParent(currentNode) && line.Item1 is Implication)
                {
                    Implication impl = line.Item1 as Implication;
                    foreach (Line line2 in frame.frame)
                    {
                        if (line2.Item1.Equals(impl.Antecedent) && !frame.Last.Item2.facts.Contains(impl.Consequent) && line2.Item2.ThisOrParent(currentNode))
                        {
                            List<IFormula> forms = new List<IFormula> { impl, impl.Antecedent };
                            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                            output.Add(newFrame.Item1.AddForm(impl.Consequent, new Annotation(newFrame.Item2, Rules.IMP, false)));
                        }
                    }
                }
            }

            return output;
        }

        private static List<Frame> IffIntro(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                if (line.Item1 is Implication)
                {
                    foreach (Line line2 in frame.frame)
                    {
                        if (line2.Item2.ThisOrParent(line.Item2) && line2.Item1 is Implication)
                        {
                            Implication impl = line.Item1 as Implication;
                            Implication impl2 = line2.Item1 as Implication;
                            if (impl.Antecedent.Equals(impl2.Consequent) && impl.Consequent.Equals(impl2.Antecedent))
                            {
                                Iff iff = new Iff(impl.Antecedent, impl.Consequent);
                                if (!facts.Contains(iff))
                                {
                                    List<IFormula> forms = new List<IFormula> { impl, impl2 };
                                    Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                                    output.Add(newFrame.Item1.AddForm(iff, new Annotation(newFrame.Item2, Rules.BI, true)));
                                }
                                Iff iffMirror = new Iff(impl.Consequent, impl.Antecedent);
                                if (!facts.Contains(iffMirror))
                                {
                                    List<IFormula> forms = new List<IFormula> { impl2, impl };
                                    Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                                    output.Add(newFrame.Item1.AddForm(iffMirror, new Annotation(newFrame.Item2, Rules.BI, true)));
                                }
                            }
                        }
                    }

                }
            }
            return output;
        }

        private static List<Frame> IffElim(Frame frame)
        {
            List<Frame> output = new List<Frame>();
            HashSet<IFormula> facts = frame.ReturnFacts();

            foreach (Line line in frame.frame)
            {
                if (line.Item1 is Iff)
                {
                    Iff iff = line.Item1 as Iff;

                    Implication left = new Implication(iff.Left, iff.Right);
                    Implication right = new Implication(iff.Right, iff.Left);
                    if (!facts.Contains(left))
                    {
                        List<IFormula> forms = new List<IFormula> { iff };
                        Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                        output.Add(newFrame.Item1.AddForm(left, new Annotation(newFrame.Item2, Rules.BI, false)));
                    }
                    if (!facts.Contains(right))
                    {
                        List<IFormula> forms = new List<IFormula> { iff };
                        Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                        output.Add(newFrame.Item1.AddForm(right, new Annotation(newFrame.Item2, Rules.BI, false)));
                    }

                }
            }
            return output;
        }

        private static Tuple<Frame, List<int>> REI(Frame frame, List<IFormula> forms)
        {
            Frame output = frame;
            Node lastNode = output.Last.Item2;
            List<int> list = new List<int>();
            List<int> outputList = new List<int>();
            foreach (IFormula form in forms)
            {
                if (!lastNode.facts.Contains(form))
                {
                    Frame intermediate = output;
                    foreach (Line line in output.frame)
                    {
                        if (line.Item1.Equals(form) && line.Item2.ThisOrParent(lastNode))
                        {
                            list.Add(output.frame.IndexOf(line) + 1);
                            intermediate = output.AddForm(form, new Annotation(list, Rules.REI, true));
                            outputList.Add(intermediate.frame.Count);
                            break;
                        }
                    }
                    output = intermediate;
                    lastNode = output.Last.Item2;
                    list.Clear();
                }
                else
                {
                    foreach (Line line in output.frame)
                    {
                        if (line.Item1.Equals(form) && lastNode == line.Item2)
                        {
                            outputList.Add(output.frame.IndexOf(line) + 1);
                            break;
                        }
                    }
                }
            }
            return new Tuple<Frame, List<int>>(output, outputList);
        }
    }
}
