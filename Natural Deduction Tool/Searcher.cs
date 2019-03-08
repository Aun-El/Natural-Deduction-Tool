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
        public static HashSet<IFormula> PremiseSubForms { get; private set; }

        /// <summary>
        /// Tries to prove a conclusion from a set of premises.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="conclusion"></param>
        /// <returns></returns>
        public static string Prove(List<IFormula> premises, IFormula conclusion)
        {
            if (!premises.Any())
            {
                premises.Add(new PropVar("\u22a4"));
            }
            Frame init = new Frame(premises);
            Fringe = new Queue<Frame>();
            SubForms = conclusion.GetSubForms(new HashSet<IFormula>());
            PremiseSubForms = new HashSet<IFormula>();
            //TODO: Make the magic happen
            //TODO: Loop through all rules and see which ones can be applied

            foreach (IFormula premise in premises)
            {
                if (premise.Equals(conclusion))
                {
                    return init.ToString();
                }
                SubForms = premise.GetSubForms(SubForms);
                PremiseSubForms = premise.GetSubForms(PremiseSubForms);
            }

            Goal goal = new Goal(conclusion);
            Frame finalFrame = goal.Prove(init);
            if(finalFrame != null)
            {
                return finalFrame.ToString();
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
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ConjElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in DisjIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in DisjElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in NegIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in NegElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ImplIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ImplElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in IffIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in IffElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(conclusion))
                    {
                        return inference.ToString();
                    }
                    Fringe.Enqueue(inference);
                }
            }
            return "I can not prove this (yet).";
        }

        /// <summary>
        /// Tries to prove a goal within a frame. Returns the new frame if successful, returns null otherwise.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="goal"></param>
        /// <returns></returns>
        public static Frame Prove(Frame frame, IFormula goal)
        {
            //TODO: Make the magic happen
            //TODO: Loop through all rules and see which ones can be applied

            Fringe.Enqueue(frame);

            while (Fringe.Any())
            {
                Frame currentFrame = Fringe.Dequeue();

                //Only perform conjunction introduction if the conclusion is a conjunction (to prevent memory overload)
                foreach (Frame inference in ConjIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ConjElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in DisjIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in DisjElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in NegIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in NegElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ImplIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in ImplElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in IffIntro(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
                foreach (Frame inference in IffElim(currentFrame))
                {
                    if (inference.frame.First().Facts.Contains(goal))
                    {
                        return inference;
                    }
                    Fringe.Enqueue(inference);
                }
            }
            return null;
        }

        public static Frame ApplyImplIntro(Frame frame, IFormula ante, IFormula cons)
        {
            Tuple<Interval,Interval> intervals = FindInt(frame, ante, cons);
            if (intervals.Item1 == null || intervals.Item2 == null)
            {
                throw new Exception("Tried applying ImplIntro on non-existing conjuncts.");
            }
            List<IFormula> forms = new List<IFormula> { cons };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            return newFrame.Item1.AddForm(new Implication(ante,cons), newFrame.Item1.Last.Item2.parent, new Annotation(newFrame.Item2, Rules.IMP, true));
        }

        public static Frame ApplyImplElim(Frame frame, Implication impl)
        {
            Interval implInt = FindInt(frame, impl);
            if (implInt == null)
            {
                throw new Exception("Tried applying ImplElim on non-existing implication.");
            }
            foreach (Line line in frame.frame)
            {
                if (line.Item1.Equals(impl.Antecedent) && line.Item2.ThisOrParent(implInt))
                {
                    List<IFormula> forms = new List<IFormula> { impl, impl.Antecedent };
                    Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                    return newFrame.Item1.AddForm(impl.Consequent, new Annotation(newFrame.Item2, Rules.IMP, false));
                }
            }
            throw new Exception("Tried applying ImplElim while antecedent was not known.");
        }

        public static Frame ApplyConjIntro(Frame frame, IFormula left, IFormula right)
        {
            Tuple<Interval, Interval> intervals = FindInt(frame, left, right);
            if (intervals.Item1 == null || intervals.Item2 == null)
            {
                throw new Exception("Tried applying ConjIntro on non-existing conjuncts.");
            }
            List<IFormula> forms = new List<IFormula> { left, right };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            return newFrame.Item1.AddForm(new Conjunction(left, right), new Annotation(newFrame.Item2, Rules.AND, true));
        }

        public static Frame ApplyREI(Frame frame, IFormula goal)
        {
            List<IFormula> forms = new List<IFormula>() { goal };
            return REI(frame, forms).Item1;
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

            foreach (Line line in frame.frame)
            {
                if (line.Item1 is Disjunction)
                {
                    HashSet<IFormula> facts = line.Item2.ReturnFacts();
                    Disjunction disj = line.Item1 as Disjunction;
                    int DisjLine = frame.frame.IndexOf(line);
                    List<Tuple<IFormula, Interval>> factsLeft = new List<Tuple<IFormula, Interval>>();
                    List<Tuple<IFormula, Interval>> factsRight = new List<Tuple<IFormula, Interval>>();
                    List<Tuple<IFormula, Interval>> factsBoth = new List<Tuple<IFormula, Interval>>();
                    foreach (Line line2 in frame.frame)
                    {
                        if (line2.Item3.rule == Rules.ASS && line2.Item2.parent == line.Item2 && line2.Item1.Equals(disj.Left))
                        {
                            foreach (IFormula fact in line2.Item2.facts)
                            {
                                factsLeft.Add(new Tuple<IFormula, Interval>(fact, line2.Item2));
                            }
                        }
                        if (line2.Item3.rule == Rules.ASS && line2.Item2.parent == line.Item2 && line2.Item1.Equals(disj.Right))
                        {
                            foreach (IFormula fact in line2.Item2.facts)
                            {
                                factsRight.Add(new Tuple<IFormula, Interval>(fact, line2.Item2));
                            }
                        }
                    }
                    foreach (Tuple<IFormula, Interval> fact in factsLeft)
                    {
                        foreach (Tuple<IFormula, Interval> fact2 in factsRight)
                        {
                            if (fact.Item1.Equals(fact2.Item1) && !facts.Contains(fact.Item1))
                            {
                                List<Tuple<IFormula, Interval>> forms = new List<Tuple<IFormula, Interval>> { fact, fact2 };
                                Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                                output.Add(newFrame.Item1.AddForm(fact.Item1, newFrame.Item1.frame[DisjLine].Item2, new Annotation(newFrame.Item2, Rules.OR, false)));
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
                                List<IFormula> forms = new List<IFormula> { line2.Item1 };
                                Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                                newFrame.Item2.Insert(0, frame.frame.IndexOf(line) + 1);
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
            Interval currentNode = frame.Last.Item2;
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

        /// <summary>
        /// Reiterates the formulas that do not appear in the last interval of the frame.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="forms"></param>
        /// <returns></returns>
        private static Tuple<Frame, List<int>> REI(Frame frame, List<IFormula> forms)
        {
            Frame output = frame;
            Interval lastNode = output.Last.Item2;
            List<int> list = new List<int>();
            List<int> outputList = new List<int>();
            Frame intermediate = output;
            foreach (IFormula form in forms)
            {
                //Reiterate the formula if it is not present in the current hypothesis interval
                //Otherwise just add the number of its location to the list of numbers to cite in the actual rule application

                intermediate = output;

                if (!lastNode.facts.Contains(form))
                {
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
                }
                else
                {
                    foreach (Line line in output.frame)
                    {
                        if (line.Item1.Equals(form) && lastNode == line.Item2)
                        {
                            //Reiterate an assumption in its own interval if there will be no other things in that interval
                            if (line.Item2.facts.Count == 1 && forms.Count == 1)
                            {
                                list.Add(output.frame.IndexOf(line) + 1);
                                intermediate = output.AddForm(form, new Annotation(list, Rules.REI, true));
                                outputList.Add(intermediate.frame.Count);
                            }
                            else
                            {
                                outputList.Add(output.frame.IndexOf(line) + 1);
                            }
                            break;
                        }
                    }
                }
                output = intermediate;
                lastNode = output.Last.Item2;
                list.Clear();
            }
            return new Tuple<Frame, List<int>>(output, outputList);
        }

        /// <summary>
        /// Reiterates the formulas that do not appear in the interval they are linked to in the forms parameter.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="forms"></param>
        /// <returns></returns>
        private static Tuple<Frame, List<int>> REI(Frame frame, List<Tuple<IFormula, Interval>> forms)
        {
            Frame output = frame;
            List<int> list = new List<int>();
            List<int> outputList = new List<int>();
            foreach (Tuple<IFormula, Interval> tuple in forms)
            {
                //Reiterate the formula if it is not present in the current hypothesis interval
                //Otherwise just add the number of its location to the list of numbers to cite in the actual rule application
                if (!tuple.Item2.facts.Contains(tuple.Item1))
                {
                    Frame intermediate = output;
                    foreach (Line line in output.frame)
                    {
                        if (line.Item1.Equals(tuple.Item1) && line.Item2.ThisOrParent(tuple.Item2))
                        {
                            list.Add(output.frame.IndexOf(line) + 1);
                            intermediate = output.AddForm(tuple.Item1, tuple.Item2, new Annotation(list, Rules.REI, true));
                            outputList.Add(intermediate.frame.Count);
                            break;
                        }
                    }
                    output = intermediate;
                    list.Clear();
                }
                else
                {
                    foreach (Line line in output.frame)
                    {
                        if (line.Item1.Equals(tuple.Item1) && tuple.Item2 == line.Item2)
                        {
                            outputList.Add(output.frame.IndexOf(line) + 1);
                            break;
                        }
                    }
                }
            }
            return new Tuple<Frame, List<int>>(output, outputList);
        }


        /// <summary>
        /// Returns the intervals on which the two parameter formulas can be found.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="form1"></param>
        /// <param name="form2"></param>
        /// <returns></returns>
        private static Tuple<Interval, Interval> FindInt(Frame frame, IFormula form1, IFormula form2)
        {
            Interval last = frame.Last.Item2;
            Interval int1 = null;
            Interval int2 = null;
            for (int i = 0; i < frame.frame.Count; i++)
            {
                if (frame.frame[i].Item2.ThisOrParent(last))
                {
                    if (form1.Equals(frame.frame[i].Item1))
                    {
                        int1 = frame.frame[i].Item2;
                    }
                    if (form2.Equals(frame.frame[i].Item1))
                    {
                        int2 = frame.frame[i].Item2;
                    }
                }
            }
            return new Tuple<Interval, Interval>(int1, int2);
        }

        /// <summary>
        /// Returns the interval on which the parameter formula can be found.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        private static Interval FindInt(Frame frame, IFormula form)
        {
            Interval last = frame.Last.Item2;
            Interval interval = null;
            for (int i = 0; i < frame.frame.Count; i++)
            {
                if (frame.frame[i].Item2.ThisOrParent(last))
                {
                    if (form.Equals(frame.frame[i].Item1))
                    {
                        interval = frame.frame[i].Item2;
                        break;
                    }
                }
            }
            return interval;
        }
    }
}
