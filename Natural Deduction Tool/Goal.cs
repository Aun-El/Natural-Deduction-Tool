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
        public List<Goal> subGoals;
        public Goal parent;
        bool halfComplete;
        bool complete;

        public Goal(IFormula form, Goal par)
        {
            goal = form;
            parent = par;
            subGoals = new List<Goal>();
            halfComplete = false;
            complete = false;
        }

        public Goal(IFormula form)
        {
            goal = form;
            parent = null;
            subGoals = new List<Goal>();
            halfComplete = false;
            complete = false;
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
                        Frame antFrame = new Goal(impl.Antecedent, this).Prove(frame);
                        if (antFrame != null)
                        {
                            if (!facts.Contains(subForm))
                            {
                                return Searcher.ApplyImplElim(antFrame, impl);
                            }
                            else
                            {
                                Frame subGoalFrame = new Goal(subForm, this).Prove(antFrame);
                                if (subGoalFrame != null)
                                {
                                    return Searcher.ApplyImplElim(subGoalFrame, impl);
                                }
                            }
                        }
                    }
                }
                else if (subForm is Iff)
                {
                    Iff iff = subForm as Iff;
                    if (!iff.Left.Equals(iff.Right))
                    {
                        //The iff is only useful if the left or right is known
                        //If the left and right are the same, it does not add any new knowledge, and is not worth checking
                        if (goal.Equals(iff.Left))
                        {
                            if (facts.Contains(subForm))
                            {
                                Frame rightFrame = new Goal(iff.Right, this).Prove(frame);
                                if (rightFrame != null)
                                {
                                    Frame intermediate = Searcher.ApplyIffElim(rightFrame, iff, false);
                                    return Searcher.ApplyImplElim(intermediate, intermediate.Last.Item1 as Implication);
                                }
                            }
                            else
                            {
                                Frame subGoalFrame = new Goal(iff, this).Prove(frame);
                                if (subGoalFrame != null)
                                {
                                    Frame rightFrame = new Goal(iff.Right, this).Prove(subGoalFrame);
                                    if (rightFrame != null)
                                    {
                                        Frame intermediate = Searcher.ApplyIffElim(subGoalFrame, iff, false);
                                        return Searcher.ApplyImplElim(intermediate, intermediate.Last.Item1 as Implication);
                                    }
                                }
                            }
                        }
                        else if (goal.Equals(iff.Right))
                        {
                            if (facts.Contains(subForm))
                            {
                                Frame leftFrame = new Goal(iff.Left, this).Prove(frame);
                                if (leftFrame != null)
                                {
                                    Frame intermediate = Searcher.ApplyIffElim(leftFrame, iff, true);
                                    return Searcher.ApplyImplElim(intermediate, intermediate.Last.Item1 as Implication);
                                }
                            }
                            else
                            {
                                Frame subGoalFrame = new Goal(iff, this).Prove(frame);
                                if (subGoalFrame != null)
                                {
                                    Frame leftFrame = new Goal(iff.Left, this).Prove(subGoalFrame);
                                    if (leftFrame != null)
                                    {
                                        Frame intermediate = Searcher.ApplyIffElim(leftFrame, iff, true);
                                        return Searcher.ApplyImplElim(intermediate, intermediate.Last.Item1 as Implication);
                                    }
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
                            if (new Goal(neg.Formula, this).Prove(frame) != null)
                            {
                                //Use disjElim
                            }
                        }
                        else
                        {
                            if (new Goal(new Negation(disj.Right), this).Prove(frame) != null)
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
                            if (new Goal(neg.Formula, this).Prove(frame) != null)
                            {
                                //Use disjElim
                            }
                        }
                        else
                        {
                            if (new Goal(new Negation(disj.Left), this).Prove(frame) != null)
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
                        if (neg.Formula is Negation && goal.Equals(neg.RemoveRedundantNegs()))
                        {
                            //Double negations can be dropped
                            Negation neg2 = neg.Formula as Negation;
                            Frame negFrame = frame;
                            while (!goal.Equals(neg2.Formula))
                            {
                                neg = neg2.Formula as Negation;
                                neg2 = neg.Formula as Negation;
                                negFrame = Searcher.ApplyNegElim(negFrame, neg);
                            }
                            return Searcher.ApplyNegElim(frame, neg);
                        }
                        else
                        {
                            IFormula subGoal = new Negation(goal);
                            Frame negFrame = frame.AddAss(subGoal);
                            negFrame = new Goal(neg.Formula, this).Prove(negFrame);
                            negFrame = Searcher.ApplyNegIntro(negFrame, subGoal, neg, neg.Formula);
                            return Searcher.ApplyNegElim(negFrame, negFrame.Last.Item1 as Negation);
                        }
                    }
                    else
                    {
                        //Derive the subForm from the superform
                        //Assume the negated goal
                        //Show contradiction/derive neg.formula (the non-negated version of the subForm)
                        //Apply negIntro to get the goal
                    }
                }
            }

            //If all else fails, blind search can be used.
            //return Searcher.Prove(frame,goal);
            return null;
        }

        private List<Goal> DeriveSubgoals()
        {
            //Negations or propVars cannot be further divided.

            List<Goal> output = new List<Goal>();

            if (goal is Conjunction)
            {
                Conjunction conj = goal as Conjunction;
                output.Add(new Goal(conj.Left, this));
                output.Add(new Goal(conj.Right, this));
                return output;
            }
            else if (goal is Disjunction)
            {
                Disjunction disj = goal as Disjunction;
                output.Add(new Goal(disj.Left, this));
                output.Add(new Goal(disj.Right, this));
                return output;
            }
            else if (goal is Implication)
            {
                Implication impl = goal as Implication;
                output.Add(new Goal(impl.Consequent, this));
                return output;
            }
            else if (goal is Iff)
            {
                Iff iff = goal as Iff;
                output.Add(new Goal(new Implication(iff.Left, iff.Right), this));
                output.Add(new Goal(new Implication(iff.Right, iff.Left), this));
                return output;
            }

            return null;
        }

        public void DeriveSubgoalTree()
        {
            Queue<Goal> goals = new Queue<Goal>();
            goals.Enqueue(this);
            while (goals.Any())
            {
                Goal current = goals.Dequeue();
                foreach (Goal subgoal in current.DeriveSubgoals())
                {
                    subGoals.Add(subgoal);
                    goals.Enqueue(subgoal);
                }
            }
        }

        /// <summary>
        /// Call this on a goal that has been met. It will automatically complete all subgoals and non-dual supergoals.
        /// Dual supergoals will be half-completed instead, or full completed if they are already half-completed.
        /// </summary>
        /// <returns></returns>
        public bool Complete()
        {
            if (goal is Conjunction)
            {
                if (halfComplete)
                {
                    complete = true;
                }
                else
                {
                    halfComplete = true;
                }
            }
            else if (goal is Iff)
            {
                if (halfComplete)
                {
                    complete = true;
                }
                else
                {
                    halfComplete = true;
                }
            }
            else
            {
                complete = true;
            }

            if (complete)
            {
                if (parent == null)
                {
                    return true;
                }
                else
                {
                    subGoals.Clear();
                    return parent.Complete();
                }
            }
            return false;
        }
    }
}
