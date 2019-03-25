using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public partial class Searcher
    {
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
                            intermediate = output;
                            intermediate.AddForm(form, new Annotation(list, Rules.REI, true));
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
                            if (line.Item2.facts.Count == 1 && forms.Count == 1 && line.Item2.parent != null)
                            {
                                list.Add(output.frame.IndexOf(line) + 1);
                                intermediate = output;
                                intermediate.AddForm(form, new Annotation(list, Rules.REI, true));
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
                            intermediate = output;
                            intermediate.AddForm(tuple.Item1, tuple.Item2, new Annotation(list, Rules.REI, true));
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
        /// Returns the intervals on which the three parameter formulas can be found.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="form1"></param>
        /// <param name="form2"></param>
        /// <returns></returns>
        private static Tuple<Interval, Interval, Interval> FindInt(Frame frame, IFormula form1, IFormula form2, IFormula form3)
        {
            Interval last = frame.Last.Item2;
            Interval int1 = null;
            Interval int2 = null;
            Interval int3 = null;
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
                    if (form3.Equals(frame.frame[i].Item1))
                    {
                        int3 = frame.frame[i].Item2;
                    }
                }
            }
            return new Tuple<Interval, Interval, Interval>(int1, int2, int3);
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

        [DebuggerStepThrough]
        private static List<Implication> CloneStuff(List<Implication> input)
        {
            List<Implication> output = new List<Implication>();
            foreach(Implication e in input)
            {
                output.Add(e);
            }
            return output;
        }

        [DebuggerStepThrough]
        private static List<Disjunction> CloneStuff(List<Disjunction> input)
        {
            List<Disjunction> output = new List<Disjunction>();
            foreach (Disjunction e in input)
            {
                output.Add(e);
            }
            return output;
        }

        [DebuggerStepThrough]
        private static List<Derivation> CloneStuff(List<Derivation> input)
        {
            List<Derivation> output = new List<Derivation>();
            foreach (Derivation e in input)
            {
                output.Add(e);
            }
            return output;
        }

        [DebuggerStepThrough]
        private static HashSet<IFormula> CloneStuff(HashSet<IFormula> input)
        {
            HashSet<IFormula> output = new HashSet<IFormula>();
            foreach (IFormula e in input)
            {
                output.Add(e);
            }
            return output;
        }

        private static Tuple<IFormula, IFormula> FindContra(IFormula negGoal, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth)
        {
            foreach (IFormula fact in facts)
            {
                if (!negGoal.Equals(fact) && fact is Negation)
                {
                    //Try to prove fact.Formula
                    //There might be multiple negations that will lead to a contradiction if their formulas are proved
                    //Check all and return only the shortest path
                    HashSet<IFormula> newFacts = new HashSet<IFormula>();
                    foreach (IFormula fact2 in facts)
                    {
                        newFacts.Add(fact2);
                    }
                    List<Implication> newImpls = new List<Implication>();
                    foreach (Implication impl in Implications)
                    {
                        newImpls.Add(impl);
                    }
                    List<Disjunction> newDisjs = new List<Disjunction>();
                    foreach (Disjunction newDisj in Disjunctions)
                    {
                        newDisjs.Add(newDisj);
                    }
                    List<Derivation> newDerivs = new List<Derivation>();

                    Negation negFact = fact as Negation;
                    Goal newGoal = new Goal(negFact.Formula);
                    newGoal.DeriveSubgoalTree(true);

                    if (facts.Contains(negFact.Formula))
                    {
                        //Contradiction has been shown
                        return null;
                    }
                    else
                    {
                        ElimGoalSearchForContra(newGoal, negGoal, newFacts, newImpls, newDisjs, newDerivs, depth);

                        if (newGoal.Completed)
                        {
                            //The proof worked out somehow
                            //Attach it to the contragoal and complete it
                            //Find some way to write the necessary steps into the proof
                        }
                    }
                }
            }
            return null;
        }
    }
}
