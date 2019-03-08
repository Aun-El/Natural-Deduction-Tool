using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Goal
    {
        public IFormula goal;

        public Goal(IFormula form)
        {
            goal = form;
        }

        /// <summary>
        /// Attempts to prove the goal within the given frame. Returns a frame where the goal is proven if successful, 
        /// returns null otherwise.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Frame Prove(Frame frame)
        {
            HashSet<IFormula> subForms = Searcher.PremiseSubForms;
            HashSet<IFormula> facts = frame.ReturnFacts();

            //Is the goal is already known?
            if (facts.Contains(goal))
            {
                return Searcher.ApplyREI(frame, goal);
            }

            //Can it be proven from what is already known, or by deriving a subgoal from what is already known?
            foreach (IFormula subForm in subForms)
            {
                if (subForm is Implication)
                {
                    Implication impl = subForm as Implication;
                    if (goal.Equals(impl.Consequent))
                    {
                        Frame antFrame = new Goal(impl.Antecedent).Prove(frame);
                        if (antFrame != null)
                        {
                            if (facts.Contains(subForm))
                            {
                                return Searcher.ApplyImplElim(antFrame, impl);
                            }
                            else
                            {
                                Frame subGoalFrame = new Goal(subForm).Prove(antFrame);
                                return Searcher.ApplyImplElim(subGoalFrame, impl);
                            }
                        }
                    }
                }
                else if (subForm is Iff)
                {
                    Iff iff = subForm as Iff;
                    if (!iff.Left.Equals(iff.Right))
                    {
                        if (goal.Equals(iff.Left))
                        {
                            if (facts.Contains(subForm))
                            {
                                if (new Goal(iff.Right).Prove(frame) != null)
                                {
                                    //Derive Right -> Left and apply ImplElim
                                }
                            }
                            else
                            {
                                //Derive the iff from what is known
                                if (new Goal(iff.Right).Prove(frame) != null)
                                {
                                    //Derive Right -> Left and apply ImplElim
                                }
                            }
                        }
                        else if (goal.Equals(iff.Right))
                        {
                            if (facts.Contains(subForm))
                            {
                                if (new Goal(iff.Left).Prove(frame) != null)
                                {
                                    //Derive Left -> Right and apply ImplElim
                                }
                            }
                            else
                            {
                                //Derive the iff from what is known
                                if (new Goal(iff.Left).Prove(frame) != null)
                                {
                                    //Derive Left -> Right and apply ImplElim
                                }
                            }
                        }
                    }
                }
                else if (subForm is Disjunction)
                {
                    Disjunction disj = subForm as Disjunction;
                    if (disj.Left.Equals(goal))
                    {
                        if (disj.Right is Negation)
                        {
                            Negation neg = disj.Right as Negation;
                            if (new Goal(neg.Formula).Prove(frame) != null)
                            {
                                //Use disjElim
                            }
                        }
                        else
                        {
                            if (new Goal(new Negation(disj.Right)).Prove(frame) != null)
                            {
                                //Use disjElim
                            }
                        }
                    }
                    else if (disj.Right.Equals(goal))
                    {
                        if (disj.Left is Negation)
                        {
                            Negation neg = disj.Left as Negation;
                            if (new Goal(neg.Formula).Prove(frame) != null)
                            {
                                //Use disjElim
                            }
                        }
                        else
                        {
                            if (new Goal(new Negation(disj.Left)).Prove(frame) != null)
                            {
                                //Use disjElim
                            }
                        }
                    }

                    //Show that the goal follows when assuming disj.Left and when assuming disj.Right
                    //Apply disjElim to derive the goal
                }
                else if (subForm is Conjunction)
                {
                    Conjunction conj = subForm as Conjunction;
                    if (conj.Right.Equals(goal))
                    {
                        if (facts.Contains(subForm))
                        {
                            //Apply conjElim
                        }
                        else
                        {
                            //Derive the conjunction
                            //Apply conjElim
                        }
                    }
                    else if (conj.Left.Equals(goal))
                    {
                        if (facts.Contains(subForm))
                        {
                            //Apply conjElim
                        }
                        else
                        {
                            //Derive the conjunction
                            //Apply conjElim
                        }

                    }
                }
                else if (subForm is Negation)
                {
                    Negation neg = subForm as Negation;
                    if (facts.Contains(subForm))
                    {
                        //Assume the negated goal
                        //Derive neg.formula (the non-negated version of the subForm)
                        //Apply negIntro to get the goal
                    }
                    else
                    {
                        //Derive the subForm from the superform
                        //Assume the negated goal
                        //Derive neg.formula (the non-negated version of the subForm)
                        //Apply negIntro to get the goal
                    }
                }
            }

            //Can it be proven by (further) division into subgoals?
            if (goal is Conjunction)
            {
                Conjunction conj = goal as Conjunction;
                Goal left = new Goal(conj.Left);
                Goal right = new Goal(conj.Right);
                if (left.Prove(frame) != null)
                {
                    return right.Prove(frame);
                }
            }
            else if (goal is Disjunction)
            {
                Disjunction disj = goal as Disjunction;
                Goal left = new Goal(disj.Left);
                Goal right = new Goal(disj.Right);
                if (left.Prove(frame) != null)
                {
                    return left.Prove(frame);
                }
                if (right.Prove(frame) != null)
                {
                    return right.Prove(frame);
                }
            }
            else if (goal is Negation)
            {
                //Dividing negations into subgoals would not help.
            }
            else if (goal is Implication)
            {
                Implication impl = goal as Implication;
                Goal right = new Goal(impl.Consequent);
                Frame newFrame = frame.AddAss(impl.Antecedent);
                newFrame = right.Prove(newFrame);
                if(newFrame != null)
                {
                    return Searcher.ApplyImplIntro(newFrame,impl.Antecedent,impl.Consequent);
                }
            }
            else if (goal is Iff)
            {
                Iff iff = goal as Iff;
                Goal left = new Goal(new Implication(iff.Left, iff.Right));
                Goal right = new Goal(new Implication(iff.Right, iff.Left));
                if (left.Prove(frame) != null)
                {
                    return right.Prove(frame);
                }
            }

            return null;
        }
    }
}
