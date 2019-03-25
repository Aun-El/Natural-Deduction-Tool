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
        public List<IFormula> Assumptions { get; set; }
        public Derivation deriv;
        protected bool halfComplete;
        public bool Completed { get; protected set; }
        public bool ProvedByContra { get; set; }
        public bool NeedsChecking { get { return !Completed && (subGoals.Any() || !ProvedByContra); } }

        public Goal(IFormula form, Goal par)
        {
            goal = form;
            parent = par;
            subGoals = new List<Goal>();
            Assumptions = new List<IFormula>();
            par.subGoals.Add(this);
            halfComplete = false;
            Completed = false;
            ProvedByContra = false;
            deriv = null;
        }

        public Goal(IFormula form)
        {
            goal = form;
            parent = null;
            subGoals = new List<Goal>();
            Assumptions = new List<IFormula>();
            halfComplete = false;
            Completed = false;
            ProvedByContra = false;
            deriv = null;
        }

        private List<Goal> DeriveSubgoals(bool noContra)
        {
            List<Goal> output = new List<Goal>();

            //Any goal can be reached by modus ponens
            MPGoal newMPGoal = new MPGoal(new Implication(goal, true), this);
            //Or by an indirect proof
            if (!ContraGoalAncestor() && !noContra)
            {
                ContraGoal newConGoal = new ContraGoal(goal, this);
            }
            //Check if any known implications have the new MP goal as their consequent
            foreach (IFormula fact in Searcher.facts)
            {
                if (fact is Implication)
                {
                    Implication impl = fact as Implication;
                    if (impl.Consequent.Equals(goal))
                    {
                        ImplGoal newImplGoal = new ImplGoal(impl, newMPGoal);
                        newImplGoal.DeriveMPSubgoalTree(noContra);
                    }
                }
            }

            //Negations or propVars cannot be further divided.            

            if (goal is Conjunction)
            {
                Conjunction conj = goal as Conjunction;
                output.Add(new Goal(conj.Left, this));
                if (!conj.Left.Equals(conj.Right))
                {
                    output.Add(new Goal(conj.Right, this));
                }
            }
            else if (goal is Disjunction)
            {
                Disjunction disj = goal as Disjunction;
                output.Add(new Goal(disj.Left, this));
                if (!disj.Left.Equals(disj.Right))
                {
                    output.Add(new Goal(disj.Right, this));
                }
            }
            else if (goal is Implication)
            {
                Implication impl = goal as Implication;
                output.Add(new Goal(impl.Consequent, this));
            }
            else if (goal is Iff)
            {
                Iff iff = goal as Iff;
                output.Add(new Goal(new Implication(iff.Left, iff.Right), this));
                if (!iff.Left.Equals(iff.Right))
                {
                    output.Add(new Goal(new Implication(iff.Right, iff.Left), this));
                }
            }

            if (output.Any())
            {
                return output;
            }
            return null;
        }

        public void DeriveSubgoalTree(bool noContra)
        {
            Queue<Goal> goals = new Queue<Goal>();
            goals.Enqueue(this);
            while (goals.Any())
            {
                Goal current = goals.Dequeue();
                List<Goal> subgoals = current.DeriveSubgoals(noContra);
                if (subgoals != null)
                {
                    foreach (Goal subgoal in subgoals)
                    {
                        goals.Enqueue(subgoal);
                    }
                }
            }
        }

        public void DeriveMPSubgoalTree(bool noContra)
        {
            Queue<Goal> goals = new Queue<Goal>();
            if (!(goal is Implication))
            {
                throw new Exception("Tried making an MP goal with a non-implication.");
            }
            Implication impl = goal as Implication;
            Goal newGoal = new Goal(impl.Antecedent, this);
            goals.Enqueue(newGoal);
            while (goals.Any())
            {
                Goal current = goals.Dequeue();
                List<Goal> subgoals = current.DeriveSubgoals(noContra);
                if (subgoals != null)
                {
                    foreach (Goal subgoal in subgoals)
                    {
                        goals.Enqueue(subgoal);
                    }
                }
            }
        }

        /// <summary>
        /// Call this on a goal that has been met. It will automatically complete all subgoals and non-dual supergoals.
        /// Dual supergoals will be half-completed instead, or full completed if they are already half-completed.
        /// </summary>
        /// <returns></returns>
        public virtual bool Complete()
        {
            if (goal is Conjunction || goal is Iff)
            {
                if (halfComplete)
                {
                    Completed = true;
                }
                else
                {
                    halfComplete = true;
                }
            }
            else
            {
                Completed = true;
            }

            if (Completed)
            {
                if (parent == null)
                {
                    return true;
                }
                else
                {
                    //subGoals.Clear();
                    return parent.Complete();
                }
            }
            return false;
        }

        /// <summary>
        /// Works like Complete, but bypasses all requirements.
        /// </summary>
        /// <returns></returns>
        public bool DirectComplete()
        {
            Completed = true;
            if (parent == null)
            {
                return true;
            }
            else
            {
                //subGoals.Clear();
                return parent.Complete();
            }
        }

        /// <summary>
        /// Closes a branch by removing the goal this method was called from the list of subgoals of its parent, 
        /// and closing the parent if it was not a disjunction goal.
        /// </summary>
        public virtual void CloseBranch()
        {
            if (parent != null && !subGoals.Any() && ProvedByContra)
            {
                parent.subGoals.Remove(this);
                if (!ProvedByContra)
                {
                    return;
                }
                if (parent.subGoals.Any())
                {
                    if (!(parent.goal is Conjunction || parent.goal is Iff) || !parent.ProvedByContra)
                    {
                        return;
                    }
                    else
                    {
                        foreach (Goal subgoal in parent.subGoals)
                        {
                            if (subgoal is MPGoal)
                            {
                                return;
                            }
                        }
                        parent.CloseBranch();
                    }
                }
                else
                {
                    parent.CloseBranch();
                }
            }
        }

        private bool MPGoalIsThisOrAncestor(MPGoal goal)
        {
            if (this is MPGoal)
            {
                Implication goalImpl = goal.goal as Implication;
                Implication thisImpl = this.goal as Implication;
                if (goalImpl.Consequent.Equals(thisImpl.Consequent))
                {
                    return true;
                }
                else
                {
                    if (parent != null)
                    {
                        return MPGoalIsThisOrAncestor(goal);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (parent != null)
                {
                    return MPGoalIsThisOrAncestor(goal);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ContraGoalAncestor()
        {
            if(parent == null)
            {
                return false;
            }
            else
            {
                if(parent is ContraGoal)
                {
                    return true;
                }
                return parent.ContraGoalAncestor();
            }
        }
    }

    public class MPGoal : Goal
    {
        public readonly IFormula consequent;
        public MPGoal(Implication form, Goal par) : base(form, par)
        {
            consequent = form.Consequent;
            if (form.Antecedent != null)
            {
                throw new Exception("Tried making a modus ponens goal out of a fully-formed implication.");
            }
            ProvedByContra = true;
        }

        public override bool Complete()
        {
            Completed = true;
            if (parent == null)
            {
                return true;
            }
            else
            {
                //subGoals.Clear();
                return parent.DirectComplete();
            }
        }

        public override void CloseBranch()
        {
            parent.subGoals.Remove(this);
            if (!parent.subGoals.Any())
            {
                parent.CloseBranch();
            }
        }
    }

    public class ImplGoal : Goal
    {
        public ImplGoal(Implication form, Goal par) : base(form, par)
        {
            ProvedByContra = true;
        }

        public override bool Complete()
        {
            Completed = true;
            if (parent == null)
            {
                return true;
            }
            else
            {
                //subGoals.Clear();
                return parent.DirectComplete();
            }
        }
    }

    public class ContraGoal : Goal
    {
        public Derivation contraDeriv;

        public ContraGoal(IFormula form, Goal par) : base(form, par)
        {
            //A contragoal cannot be proven by an indirect proof (only the parent can be)
            ProvedByContra = true;
            contraDeriv = null;
            Assumptions.Add(new Negation(par.goal));
        }

        public ContraGoal(IFormula form) : base(form)
        {
            //A contragoal cannot be proven by an indirect proof (only the parent can be)
            ProvedByContra = true;
            contraDeriv = null;
        }

        public override bool Complete()
        {
            Completed = true;
            if (parent == null)
            {
                return true;
            }
            else
            {
                //subGoals.Clear();
                return parent.DirectComplete();
            }
        }
    }
}
