using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Natural_Deduction_Tool
{
    public partial class Searcher
    {
        public static List<Derivation> Derivations { get; set; }
        public static List<Implication> Implications { get; set; }
        public static List<Disjunction> Disjunctions { get; set; }
        public static Goal goal;
        public static HashSet<IFormula> facts;
        public static bool findWholeTree;
        private static int derivLength;
        private static int max_depth;

        public static string Prove(List<IFormula> premises, IFormula conclusion)
        {
            findWholeTree = false;
            max_depth = 0;
            derivLength = 0;
            facts = new HashSet<IFormula>();

            //Step 1: (Happens in Form1) Check if the premises are UNSAT. If so, go to 2. Else, go to 3.

            //Step 2: Derive conclusion from UNSAT premises

            //Step 3: Derive full subgoal tree, derive all possible derivations from premises

            goal = new Goal(conclusion);

            goal.DeriveSubgoalTree();

            Derivations = DeriveWithElim(premises, facts);

            //Step 4: Check if derivations satisfy goal

            Queue<Goal> goalQueue = new Queue<Goal>();
            goalQueue.Enqueue(goal);
            while (goalQueue.Any())
            {
                Goal current = goalQueue.Dequeue();
                foreach (Derivation deriv in Derivations)
                {
                    if (MatchGoal(deriv.Form, current))
                    {
                        Frame output = new Frame(premises);
                        output = MakeFrame(output, Derivations, goal);
                        return output.ToString();
                    }
                }
                if (!current.Completed)
                {
                    foreach (Goal subgoal in current.subGoals)
                    {
                        goalQueue.Enqueue(subgoal);
                    }
                }
            }

            //Step 5: Go down the goal tree with Iterative deepening search

            while (goal.subGoals.Any())
            {
                DLS(goal, 0);
                if (goal.Completed)
                {
                    Frame output = new Frame(premises);
                    output = MakeFrame(output, Derivations, goal);
                    return output.ToString();
                }
                max_depth++;
            }

            //Step 6: The proof has not been reached from any possible branch. The proof cannot be made. 

            return "I cannot prove this (yet).";
        }

        /// <summary>
        /// Depth limited search
        /// </summary>
        /// <param name="goal"></param>
        /// <param name="depth"></param>
        private static void DLS(Goal goal, int depth)
        {
            bool checkedWithElim = false;
            //If the goal is not a disjunction or implication, further derivation will not prove it at this point.
            if ((goal.parent != null && goal.parent.goal is Implication) || goal.goal is Disjunction)
            {
                checkedWithElim = true;
                HashSet<IFormula> newFacts = new HashSet<IFormula>();
                foreach (IFormula fact in facts)
                {
                    newFacts.Add(fact);
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
                ElimGoalSearch(goal, newFacts, newImpls, newDisjs, newDerivs, depth);
            }

            if (Searcher.goal.Completed && !findWholeTree)
            {
                return;
            }

            //TODO: Check goal by indirect proof

            if (Searcher.goal.Completed && !findWholeTree)
            {
                return;
            }

            //Call DLS on all uncompleted subgoals if there are any, otherwise close this branch.
            if (!checkedWithElim)
            {
                List<Goal> setForRemoval = new List<Goal>();
                if (goal.subGoals.Any())
                {
                    if (depth < max_depth)
                    {
                        foreach (Goal subgoal in goal.subGoals)
                        {
                            if (subgoal.Completed)
                            {
                                continue;
                            }
                            if (subgoal is MPGoal && (!subgoal.Completed || findWholeTree))
                            {
                                if (subgoal.subGoals.Any())
                                {
                                    foreach (Goal subsubgoal in subgoal.subGoals)
                                    {
                                        //ImplGoals always have exactly one subgoal
                                        DLS(subsubgoal.subGoals[0], ++depth);
                                        if (Searcher.goal.Completed && !findWholeTree)
                                        {
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    setForRemoval.Add(subgoal);
                                }
                            }
                            else
                            {
                                DLS(subgoal, ++depth);
                                if (Searcher.goal.Completed && !findWholeTree)
                                {
                                    return;
                                }
                            }
                        }
                        foreach (Goal subgoal in setForRemoval)
                        {
                            subgoal.CloseBranch();
                        }
                    }
                    return;
                }
                else
                {
                    //This branch cannot be proved
                    goal.CloseBranch();
                    return;
                }
            }
        }

        /// <summary>
        /// The initial derivation method. It derives everything that can be directly derived with elimation rules.
        /// If there are multiple ways to derive the same formula, the shortest derivation is saved.
        /// </summary>
        /// <param name="premises"></param>
        /// <param name="facts"></param>
        /// <returns></returns>
        public static List<Derivation> DeriveWithElim(List<IFormula> premises, HashSet<IFormula> facts)
        {

            PriorityQueue derivQueue = new PriorityQueue();
            List<Derivation> output = new List<Derivation>();
            List<Implication> impls = new List<Implication>();
            List<Disjunction> disjs = new List<Disjunction>();
            Implications = impls;
            Disjunctions = disjs;
            foreach (IFormula premise in premises)
            {
                facts.Add(premise);
                Derivation newDeriv = new Derivation(premise, new Origin(Rules.HYPO));
                derivQueue.Enqueue(newDeriv, output);
                output.Add(newDeriv);
                if (premise is Implication)
                {
                    impls.Add(premise as Implication);
                    MatchWithMPGoals(premise as Implication);
                }
                else if (premise is Disjunction)
                {
                    disjs.Add(premise as Disjunction);
                }
            }

            bool disjDeriv = true;

            while (disjDeriv)
            {
                disjDeriv = false;

                while (derivQueue.Any())
                {
                    Derivation current = derivQueue.Dequeue();

                    if (current.Form is Negation)
                    {
                        DeriveWithNegElim(current, facts, impls, disjs, output, derivQueue);
                    }
                    else if (current.Form is Conjunction)
                    {
                        DeriveWithConjElim(current, facts, impls, disjs, output, derivQueue);
                    }
                    else if (current.Form is Iff)
                    {
                        DeriveWithIffElim(current, facts, impls, disjs, output, derivQueue);
                    }
                    else if (current.Form is Implication)
                    {
                        DeriveWithImplElim(current, facts, impls, disjs, output, derivQueue);
                    }

                    if (current.Origin.rule != Rules.HYPO && current.Origin.rule != Rules.ASS)
                    {
                        if (current.Form is Implication)
                        {
                            MatchWithMPGoals(current.Form as Implication);
                        }
                        output.Add(current);
                    }

                    List<Implication> newImpls = new List<Implication>();
                    foreach (Implication impl in impls)
                    {
                        if (current.Form.Equals(impl.Antecedent))
                        {
                            if (!facts.Contains(impl.Consequent))
                            {
                                facts.Add(impl.Consequent);
                                if (impl.Consequent is Implication)
                                {
                                    impls.Add(impl.Consequent as Implication);
                                }
                                else if (impl.Consequent is Disjunction)
                                {
                                    disjs.Add(impl.Consequent as Disjunction);
                                }
                            }
                            List<IFormula> orig = new List<IFormula>() { impl, current.Form };
                            List<Derivation> derivPar = new List<Derivation>();
                            bool anteFound = false;
                            foreach (Derivation deriv in output)
                            {
                                if (deriv.Form.Equals(impl))
                                {
                                    anteFound = true;
                                    derivPar.Add(deriv);
                                    break;
                                }
                            }
                            if (!anteFound)
                            {
                                foreach (Derivation deriv in derivQueue.Queue)
                                {
                                    if (deriv.Form.Equals(impl.Antecedent))
                                    {
                                        derivPar.Add(deriv);
                                        break;
                                    }
                                }
                            }
                            derivPar.Add(current);
                            Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                            derivQueue.Enqueue(newDeriv, output);
                        }
                        else
                        {
                            newImpls.Add(impl);
                        }
                    }
                    impls = newImpls;

                    List<Disjunction> newDisjs = new List<Disjunction>();
                    foreach (Disjunction disj in disjs)
                    {
                        if (!(facts.Contains(disj.Left) || facts.Contains(disj.Right)))
                        {
                            newDisjs.Add(disj);
                        }
                    }
                    disjs = newDisjs;
                }

                //Check if anything can be done with disj-elim
                //If so, put the newly derived formulae in premises and run the above while-loop again
                //Otherwise, return the output

                List<Derivation> disjDerivs = new List<Derivation>();
                foreach (Disjunction disj in disjs)
                {
                    disjDerivs = disjDerivs.Concat(DeriveWithDisjElim(disj, facts, impls, disjs, output)).ToList();
                    if (disjDerivs.Any())
                    {
                        disjs.Remove(disj);
                        break;
                    }
                }
                foreach (Derivation deriv in disjDerivs)
                {
                    if (!facts.Contains(deriv.Form))
                    {
                        disjDeriv = true;
                        facts.Add(deriv.Form);
                        derivQueue.Enqueue(deriv, output);
                    }
                    else
                    {
                        for (int i = 0; i < output.Count; i++)
                        {
                            if (output[i].Form.Equals(deriv.Form) && output[i].Length > deriv.Length)
                            {
                                Derivation oldParent = output[i];
                                output.RemoveAt(i);
                                output.Insert(i, deriv);
                                foreach (Derivation childDeriv in output)
                                {
                                    if (childDeriv.Origin.parents != null && childDeriv.Origin.parents.Contains(oldParent))
                                    {
                                        childDeriv.ReplaceParent(deriv);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// This method is called during disjunction elimation. Do not call this in another context.
        /// </summary>
        /// <param name="premises"></param>
        /// <param name="facts"></param>
        /// <param name="impls"></param>
        /// <param name="disjs"></param>
        /// <param name="derivs"></param>
        /// <returns></returns>
        public static List<Derivation> DeriveWithElim(List<IFormula> premises, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs)
        {
            List<Derivation> output = new List<Derivation>();
            PriorityQueue derivQueue = new PriorityQueue();
            foreach (IFormula premise in premises)
            {
                facts.Add(premise);
                Derivation newDeriv = new Derivation(premise, new Origin(Rules.ASS, derivLength));
                derivQueue.Enqueue(newDeriv, output);
                derivs.Add(newDeriv);
                output.Add(newDeriv);
                if (premise is Implication)
                {
                    impls.Add(premise as Implication);
                }
                else if (premise is Disjunction)
                {
                    disjs.Add(premise as Disjunction);
                }
            }

            bool disjDeriv = true;

            while (disjDeriv)
            {
                disjDeriv = false;

                while (derivQueue.Any())
                {
                    Derivation current = derivQueue.Dequeue();

                    if (current.Form is Negation)
                    {
                        DeriveWithNegElim(current, facts, impls, disjs, output, derivQueue);
                    }
                    else if (current.Form is Conjunction)
                    {
                        DeriveWithConjElim(current, facts, impls, disjs, output, derivQueue);
                    }
                    else if (current.Form is Iff)
                    {
                        DeriveWithIffElim(current, facts, impls, disjs, output, derivQueue);
                    }
                    else if (current.Form is Implication)
                    {
                        DeriveWithImplElim(current, facts, impls, disjs, output, derivQueue);
                    }

                    if (current.Origin.rule != Rules.HYPO && current.Origin.rule != Rules.ASS)
                    {
                        derivs.Add(current);
                        output.Add(current);
                    }

                    List<Implication> newImpls = new List<Implication>();
                    foreach (Implication impl in impls)
                    {
                        if (current.Form.Equals(impl.Antecedent))
                        {
                            List<IFormula> orig = new List<IFormula>() { impl, current.Form };
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            bool anteFound = false;
                            foreach (Derivation deriv in output)
                            {
                                if (deriv.Form.Equals(impl.Antecedent))
                                {
                                    anteFound = true;
                                    derivPar.Add(deriv);
                                    break;
                                }
                            }
                            if (!anteFound)
                            {
                                foreach (Derivation deriv in derivQueue.Queue)
                                {
                                    if (deriv.Form.Equals(impl.Antecedent))
                                    {
                                        derivPar.Add(deriv);
                                        break;
                                    }
                                }
                            }
                            Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                            derivs.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv, output);
                            if (!facts.Contains(impl.Consequent))
                            {
                                facts.Add(impl.Consequent);
                                if (impl.Consequent is Implication)
                                {
                                    impls.Add(impl.Consequent as Implication);
                                }
                                else if (impl.Consequent is Disjunction)
                                {
                                    disjs.Add(impl.Consequent as Disjunction);
                                }
                            }
                        }
                        else
                        {
                            newImpls.Add(impl);
                        }
                    }
                    impls = newImpls;

                    List<Disjunction> newDisjs = new List<Disjunction>();
                    foreach (Disjunction disj in disjs)
                    {
                        if (!(facts.Contains(disj.Left) || facts.Contains(disj.Right)))
                        {
                            newDisjs.Add(disj);
                        }
                    }
                    disjs = newDisjs;
                }

                //Check if anything can be done with disj-elim
                //If so, put the newly derived formulae in premises and run the above while-loop again
                //Otherwise, return the output

                List<Derivation> disjDerivs = new List<Derivation>();
                foreach (Disjunction disj in disjs)
                {
                    disjDerivs = disjDerivs.Concat(DeriveWithDisjElim(disj, facts, impls, disjs, output)).ToList();
                    if (disjDerivs.Any())
                    {
                        disjs.Remove(disj);
                        break;
                    }
                }
                foreach (Derivation deriv in disjDerivs)
                {
                    if (!facts.Contains(deriv.Form))
                    {
                        disjDeriv = true;
                        facts.Add(deriv.Form);
                        derivQueue.Enqueue(deriv, output);
                    }
                    else
                    {
                        for (int i = 0; i < output.Count; i++)
                        {
                            if (output[i].Form.Equals(deriv.Form) && output[i].Length > deriv.Length)
                            {
                                Derivation oldParent = output[i];
                                output.RemoveAt(i);
                                output.Insert(i, deriv);
                                foreach (Derivation childDeriv in output)
                                {
                                    if (childDeriv.Origin.parents.Contains(oldParent))
                                    {
                                        childDeriv.ReplaceParent(deriv);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return output;
        }

        private static void ElimGoalSearch(Goal goal, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth)
        {
            bool newAss = false;
            PriorityQueue derivQueue = new PriorityQueue();

            if (goal.parent != null && goal.parent.goal is Implication)
            {
                newAss = true;
                derivLength++;
                Implication impl = goal.parent.goal as Implication;
                Derivation newDeriv = new Derivation(impl.Antecedent, new Origin(Rules.ASS, derivLength));
                derivQueue.Enqueue(newDeriv, derivs);
                derivs.Add(newDeriv);
                facts.Add(impl.Antecedent);
            }
            else if (goal.goal is Disjunction)
            {
                foreach (Disjunction disj in disjs)
                {
                    DisjElimGoalSearch(goal, disj, facts, impls, disjs, derivs, depth);
                }
                if (goal.Completed)
                {
                    return;
                }
            }

            bool disjDeriv = newAss;

            while (disjDeriv)
            {
                disjDeriv = false;

                while (derivQueue.Any())
                {
                    Derivation current = derivQueue.Dequeue();
                    if (current.Form.Equals(goal.goal))
                    {
                        goal.deriv = current;
                        goal.Complete();
                        if (newAss)
                        {
                            derivLength--;
                        }
                        return;
                    }

                    if (current.Form is Negation)
                    {
                        DeriveWithNegElim(current, facts, impls, disjs, derivs, derivQueue);
                    }
                    else if (current.Form is Conjunction)
                    {
                        DeriveWithConjElim(current, facts, impls, disjs, derivs, derivQueue);
                    }
                    else if (current.Form is Iff)
                    {
                        DeriveWithIffElim(current, facts, impls, disjs, derivs, derivQueue);
                    }
                    else if (current.Form is Implication)
                    {
                        DeriveWithImplElim(current, facts, impls, disjs, derivs, derivQueue);
                    }

                    if (current.Origin.rule != Rules.HYPO && current.Origin.rule != Rules.ASS)
                    {
                        derivs.Add(current);
                    }

                    List<Implication> newImpls = new List<Implication>();
                    foreach (Implication impl in impls)
                    {
                        if (current.Form.Equals(impl.Antecedent))
                        {
                            List<IFormula> orig = new List<IFormula>() { impl, current.Form };
                            bool anteFound = false;
                            List<Derivation> derivPar = new List<Derivation>();
                            foreach (Derivation deriv in derivs)
                            {
                                if (deriv.Form.Equals(impl))
                                {
                                    anteFound = true;
                                    derivPar.Add(deriv);
                                    break;
                                }
                            }
                            if (!anteFound)
                            {
                                foreach (Derivation deriv in derivQueue.Queue)
                                {
                                    if (deriv.Form.Equals(impl.Antecedent))
                                    {
                                        derivPar.Add(deriv);
                                        break;
                                    }
                                }
                            }
                            derivPar.Add(current);
                            Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                            derivQueue.Enqueue(newDeriv, derivs);
                            derivs.Add(newDeriv);
                            if (!facts.Contains(impl.Consequent))
                            {
                                facts.Add(impl.Consequent);
                                if (impl.Consequent is Implication)
                                {
                                    impls.Add(impl.Consequent as Implication);
                                }
                                else if (impl.Consequent is Disjunction)
                                {
                                    disjs.Add(impl.Consequent as Disjunction);
                                }
                            }
                        }
                        else
                        {
                            newImpls.Add(impl);
                        }
                    }
                    impls = newImpls;

                    List<Disjunction> newDisjs = new List<Disjunction>();
                    foreach (Disjunction disj in disjs)
                    {
                        if (!(facts.Contains(disj.Left) || facts.Contains(disj.Right)))
                        {
                            newDisjs.Add(disj);
                        }
                    }
                    disjs = newDisjs;
                }

                //Check if anything can be done with disj-elim

                foreach (Disjunction disj in disjs)
                {
                    DisjElimGoalSearch(goal, disj, facts, impls, disjs, derivs, depth);
                }
                if (goal.Completed)
                {
                    return;
                }
            }

            List<Goal> setForRemoval = new List<Goal>();
            if (goal.subGoals.Any())
            {
                if (depth < max_depth)
                {
                    //Move down the goal tree
                    foreach (Goal subgoal in goal.subGoals)
                    {
                        HashSet<IFormula> newFacts = new HashSet<IFormula>();
                        foreach (IFormula fact in facts)
                        {
                            newFacts.Add(fact);
                        }
                        List<Implication> newImpls = new List<Implication>();
                        foreach (Implication impl in impls)
                        {
                            newImpls.Add(impl);
                        }
                        List<Disjunction> newDisjs = new List<Disjunction>();
                        foreach (Disjunction newDisj in disjs)
                        {
                            newDisjs.Add(newDisj);
                        }
                        List<Derivation> newDerivs = new List<Derivation>();
                        foreach (Derivation deriv in derivs)
                        {
                            newDerivs.Add(deriv);
                        }
                        if (!(subgoal is MPGoal))
                        {
                            ElimGoalSearch(subgoal, newFacts, newImpls, newDisjs, newDerivs, ++depth);
                        }
                        else
                        {
                            if (subgoal.subGoals.Any())
                            {
                                foreach (Goal subsubgoal in subgoal.subGoals)
                                {
                                    //ImplGoal always has one subgoal
                                    ElimGoalSearch(subsubgoal, newFacts, newImpls, newDisjs, newDerivs, ++depth);
                                }
                            }
                            else
                            {
                                setForRemoval.Add(subgoal);
                            }
                        }
                    }
                    foreach (Goal subgoal in setForRemoval)
                    {
                        subgoal.CloseBranch();
                    }
                }
                if (newAss)
                {
                    derivLength--;
                }
                return;
            }
            else
            {
                goal.CloseBranch();
                if (newAss)
                {
                    derivLength--;
                }
                return;
            }
        }

        private static void ElimGoalSearchForDisj(Goal goal, IFormula disjunct, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth)
        {
            derivLength++;
            PriorityQueue derivQueue = new PriorityQueue();

            Derivation newDisjDeriv = new Derivation(disjunct, new Origin(Rules.ASS, derivLength));
            derivQueue.Enqueue(newDisjDeriv, derivs);

            bool disjDeriv = true;

            while (disjDeriv)
            {
                disjDeriv = false;

                while (derivQueue.Any())
                {
                    Derivation current = derivQueue.Dequeue();
                    if (current.Form.Equals(goal.goal))
                    {
                        goal.deriv = current;
                        goal.Complete();
                        derivLength--;
                        return;
                    }

                    if (current.Form is Negation)
                    {
                        DeriveWithNegElim(current, facts, impls, disjs, derivs, derivQueue);
                    }
                    else if (current.Form is Conjunction)
                    {
                        DeriveWithConjElim(current, facts, impls, disjs, derivs, derivQueue);
                    }
                    else if (current.Form is Iff)
                    {
                        DeriveWithIffElim(current, facts, impls, disjs, derivs, derivQueue);
                    }
                    else if (current.Form is Implication)
                    {
                        DeriveWithImplElim(current, facts, impls, disjs, derivs, derivQueue);
                    }

                    if (current.Origin.rule != Rules.HYPO && current.Origin.rule != Rules.ASS)
                    {
                        derivs.Add(current);
                    }

                    List<Implication> newImpls = new List<Implication>();
                    foreach (Implication impl in impls)
                    {
                        if (current.Form.Equals(impl.Antecedent))
                        {
                            List<IFormula> orig = new List<IFormula>() { impl, current.Form };
                            bool anteFound = false;
                            List<Derivation> derivPar = new List<Derivation>();
                            foreach (Derivation deriv in derivs)
                            {
                                if (deriv.Form.Equals(impl))
                                {
                                    anteFound = true;
                                    derivPar.Add(deriv);
                                    break;
                                }
                            }
                            if (!anteFound)
                            {
                                foreach (Derivation deriv in derivQueue.Queue)
                                {
                                    if (deriv.Form.Equals(impl.Antecedent))
                                    {
                                        derivPar.Add(deriv);
                                        break;
                                    }
                                }
                            }
                            derivPar.Add(current);
                            Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                            derivQueue.Enqueue(newDeriv, derivs);
                            derivs.Add(newDeriv);
                            if (!facts.Contains(impl.Consequent))
                            {
                                facts.Add(impl.Consequent);
                                if (impl.Consequent is Implication)
                                {
                                    impls.Add(impl.Consequent as Implication);
                                }
                                else if (impl.Consequent is Disjunction)
                                {
                                    disjs.Add(impl.Consequent as Disjunction);
                                }
                            }
                        }
                        else
                        {
                            newImpls.Add(impl);
                        }
                    }
                    impls = newImpls;

                    List<Disjunction> newDisjs = new List<Disjunction>();
                    foreach (Disjunction disj in disjs)
                    {
                        if (!(facts.Contains(disj.Left) || facts.Contains(disj.Right)))
                        {
                            newDisjs.Add(disj);
                        }
                    }
                    disjs = newDisjs;
                }

                //Check if anything can be done with disj-elim

                foreach (Disjunction disj in disjs)
                {
                    DisjElimGoalSearch(goal, disj, facts, impls, disjs, derivs, depth);
                }
                if (goal.Completed)
                {
                    return;
                }
            }

            List<Goal> setForRemoval = new List<Goal>();
            if (goal.subGoals.Any())
            {
                if (depth < max_depth)
                {
                    //Move down the goal tree
                    foreach (Goal subgoal in goal.subGoals)
                    {
                        HashSet<IFormula> newFacts = new HashSet<IFormula>();
                        foreach (IFormula fact in facts)
                        {
                            newFacts.Add(fact);
                        }
                        List<Implication> newImpls = new List<Implication>();
                        foreach (Implication impl in impls)
                        {
                            newImpls.Add(impl);
                        }
                        List<Disjunction> newDisjs = new List<Disjunction>();
                        foreach (Disjunction newDisj in disjs)
                        {
                            newDisjs.Add(newDisj);
                        }
                        List<Derivation> newDerivs = new List<Derivation>();
                        foreach (Derivation deriv in derivs)
                        {
                            newDerivs.Add(deriv);
                        }
                        if (!(subgoal is MPGoal))
                        {
                            ElimGoalSearchForDisj(subgoal, disjunct, newFacts, newImpls, newDisjs, newDerivs, ++depth);
                        }
                        else
                        {
                            if (subgoal.subGoals.Any())
                            {
                                foreach (Goal subsubgoal in subgoal.subGoals)
                                {
                                    //ImplGoal always has one subgoal
                                    ElimGoalSearchForDisj(subsubgoal, disjunct, newFacts, newImpls, newDisjs, newDerivs, ++depth);
                                }
                            }
                            else
                            {
                                setForRemoval.Add(subgoal);
                            }
                        }
                    }
                    foreach (Goal subgoal in setForRemoval)
                    {
                        subgoal.CloseBranch();
                    }
                }
                derivLength--;
                return;
            }
            else
            {
                goal.CloseBranch();
                derivLength--;
                return;
            }
        }

        private static void DisjElimGoalSearch(Goal goal, Disjunction disj, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth)
        {
            derivLength++;

            HashSet<IFormula> leftFacts = new HashSet<IFormula> { disj.Left };
            HashSet<IFormula> rightFacts = new HashSet<IFormula> { disj.Right };
            foreach (IFormula fact in facts)
            {
                leftFacts.Add(fact);
                rightFacts.Add(fact);
            }
            List<Implication> leftImpls = new List<Implication>();
            List<Implication> rightImpls = new List<Implication>();
            foreach (Implication impl in impls)
            {
                leftImpls.Add(impl);
                rightImpls.Add(impl);
            }
            List<Disjunction> leftDisjs = new List<Disjunction>();
            List<Disjunction> rightDisjs = new List<Disjunction>();
            foreach (Disjunction newDisj in disjs)
            {
                leftDisjs.Add(newDisj);
                rightDisjs.Add(newDisj);
            }
            List<Derivation> leftDerivs = new List<Derivation>();
            List<Derivation> rightDerivs = new List<Derivation>();
            foreach (Derivation deriv in derivs)
            {
                leftDerivs.Add(deriv);
                rightDerivs.Add(deriv);
            }
            Goal leftGoal = new Goal(goal.goal);
            leftGoal.DeriveSubgoalTree();
            Goal rightGoal = new Goal(goal.goal);
            rightGoal.DeriveSubgoalTree();
            ElimGoalSearchForDisj(leftGoal, disj.Left, leftFacts, leftImpls, leftDisjs, leftDerivs, depth);

            if (disj.Left.Equals(disj.Right))
            {
                rightGoal = leftGoal;
            }
            else
            {
                ElimGoalSearchForDisj(rightGoal, disj.Right, rightFacts, rightImpls, rightDisjs, rightDerivs, depth);
            }

            if (leftGoal.Completed && rightGoal.Completed)
            {
                //TODO: Search down the goal trees for the one with an attached deriv
                //Combine those two derivs in a new deriv which can be applied to the goal
                List<Derivation> par = new List<Derivation> { leftGoal.deriv, rightGoal.deriv };
                List<IFormula> orig = new List<IFormula> { disj };
                Derivation newDeriv = new Derivation(goal.goal, new Origin(orig, Rules.OR, par));
                if (goal.deriv != null)
                {
                    goal.deriv = newDeriv;
                    goal.Complete();
                }
                if (goal.deriv.Length > newDeriv.Length)
                {
                    goal.deriv = newDeriv;
                }
            }

            derivLength--;
        }

        /// <summary>
        /// If the formula matches the goal, the goal is checked for completeness.
        /// Conjunctions and bi-impls need both sides to be completed before they are considered complete;
        /// all other formula types are automatically completed.
        /// Once a formula is completed, its subgoals are removed and its parent is checked for completeness.
        /// If there is no parent, the conclusion has been proven.
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        private static bool MatchGoal(IFormula form, Goal goal)
        {
            if (!(goal is ImplGoal || goal is MPGoal))
            {
                if (goal.goal.Equals(form))
                {
                    return goal.Complete();
                }
            }
            return false;
        }

        private static void StackGoals(Goal current, Stack<Goal> output, Stack<Derivation> derivStack, List<Derivation> derivs, List<IFormula> assumptions)
        {
            output.Push(current);

            bool noCompleteChildren = true;
            Goal halfCompleteGoal = null;
            Goal completeMP = null;
            Goal completeGoal = null;
            IFormula newAssumption = null;

            bool halfComplete = !(current.goal is Iff || current.goal is Conjunction);

            foreach (IFormula assumption in assumptions)
            {
                current.Assumptions.Add(assumption);
            }

            if (current.goal is Implication)
            {
                Implication impl = current.goal as Implication;
                newAssumption = impl.Antecedent;
                assumptions.Add(newAssumption);
            }

            foreach (Goal subgoal in current.subGoals)
            {
                if (subgoal.Completed)
                {
                    if (subgoal is MPGoal)
                    {
                        noCompleteChildren = false;
                        completeMP = subgoal;
                    }
                    else if (!halfComplete)
                    {
                        halfCompleteGoal = subgoal;
                        halfComplete = true;
                    }
                    else
                    {
                        noCompleteChildren = false;
                        completeGoal = subgoal;
                    }
                }
            }
            if (noCompleteChildren)
            {
                //A leaf is reached

                foreach (Derivation deriv in derivs)
                {
                    if (deriv.Form.Equals(current.goal))
                    {
                        if (!derivStack.Contains(deriv))
                        {
                            derivStack.Push(deriv);
                        }
                        break;
                    }
                }
            }
            if (completeMP != null && completeGoal != null)
            {
                //Two ways of proving this; pick the shortest one and add the necessary assumptions to Antecedents
                if (BranchLength(completeMP, 0) < BranchLength(completeGoal, 0))
                {
                    StackGoals(completeMP, output, derivStack, derivs, assumptions);
                }
                else
                {
                    StackGoals(completeGoal, output, derivStack, derivs, assumptions);
                }
            }
            else if (completeMP != null)
            {
                StackGoals(completeMP, output, derivStack, derivs, assumptions);
            }
            else if (completeGoal != null)
            {
                StackGoals(completeGoal, output, derivStack, derivs, assumptions);
            }

            if (newAssumption != null)
            {
                assumptions.Remove(newAssumption);
            }
        }

        private static int BranchLength(Goal current, int depthSoFar)
        {
            bool noCompleteChildren = true;
            Goal halfCompleteGoal = null;
            Goal completeMP = null;
            Goal completeGoal = null;
            bool halfComplete = !(current.goal is Iff || current.goal is Conjunction);

            foreach (Goal subgoal in current.subGoals)
            {
                if (subgoal.Completed)
                {
                    if (subgoal is MPGoal)
                    {
                        noCompleteChildren = false;
                        completeMP = subgoal;
                    }
                    else if (!halfComplete)
                    {
                        halfCompleteGoal = subgoal;
                        halfComplete = true;
                    }
                    else
                    {
                        noCompleteChildren = false;
                        completeGoal = subgoal;
                    }
                }
            }

            if (noCompleteChildren)
            {
                foreach (Derivation deriv in Derivations)
                {
                    if (deriv.Form.Equals(current.goal))
                    {
                        depthSoFar += deriv.Length;
                    }
                }
            }
            else
            {
                if (completeMP != null && completeGoal != null)
                {
                    int temp1 = BranchLength(completeMP, depthSoFar);
                    int temp2 = BranchLength(completeGoal, depthSoFar);
                    if (temp1 < temp2)
                    {
                        depthSoFar += temp1;
                    }
                    depthSoFar += temp2;
                }
                else if (completeMP != null)
                {
                    depthSoFar += BranchLength(completeMP, depthSoFar);
                }
                else
                {
                    if (halfCompleteGoal != null)
                    {
                        depthSoFar += BranchLength(halfCompleteGoal, depthSoFar);
                    }
                    depthSoFar += BranchLength(completeGoal, depthSoFar);
                }
            }
            return depthSoFar;

        }

        public static Frame MakeFrame(Frame output, List<Derivation> derivs, Goal goal)
        {
            Stack<Derivation> derivStack = new Stack<Derivation>();

            //Goes from the subgoals down to the conclusion
            Stack<Goal> buildOrder = new Stack<Goal>();

            //Goes from the premises down to the derivations
            Stack<Derivation> buildOrderDeriv = new Stack<Derivation>();

            StackGoals(goal, buildOrder, derivStack, derivs, new List<IFormula>());

            //Make the necessary derivations
            while (derivStack.Any())
            {
                Derivation current = derivStack.Pop();
                if (current.Origin.parents != null)
                {
                    if (current.Origin.rule != Rules.OR)
                    {
                        foreach (Derivation parDeriv in current.Origin.parents)
                        {
                            derivStack.Push(parDeriv);
                        }
                    }
                    buildOrderDeriv.Push(current);
                }
            }

            if (!buildOrderDeriv.Any() && buildOrder.Count == 1)
            {
                //The conclusion is one of the premises
                //Only reiterate
                List<int> lines = new List<int>();
                foreach (Line line in output.frame)
                {
                    if (line.Item1.Equals(goal.goal) && line.Item2 == output.Last.Item2)
                    {
                        lines.Add(output.frame.IndexOf(line) + 1);
                        break;
                    }
                }
                output.AddForm(goal.goal, new Annotation(lines, Rules.REI, false));
                return output;
            }

            while (buildOrderDeriv.Any())
            {
                Derivation current = buildOrderDeriv.Pop();
                foreach (IFormula form in current.Origin.origins)
                {
                    if (!output.ReturnFacts().Contains(form))
                    {
                        output.AddAss(form);
                    }
                }
                switch (current.Origin.rule)
                {
                    case Rules.AND:
                        Conjunction conj = current.Origin.origins[0] as Conjunction;
                        output = ApplyConjElim(output, conj, conj.Left.Equals(current.Form));
                        break;
                    case Rules.NEG:
                        output = ApplyNegElim(output, current.Origin.origins[0] as Negation);
                        break;
                    case Rules.IMP:
                        Implication impl = null;
                        foreach (IFormula form in current.Origin.origins)
                        {
                            if (form is Implication)
                            {
                                impl = form as Implication;
                            }
                        }
                        output = ApplyImplElim(output, impl);
                        break;
                    case Rules.BI:
                        Iff iff = current.Origin.origins[0] as Iff;
                        Implication left = new Implication(iff.Left, iff.Right);
                        output = ApplyIffElim(output, iff, left.Equals(current.Form));
                        break;
                    case Rules.OR:
                        Disjunction orig = current.Origin.origins[0] as Disjunction;
                        List<Derivation> leftOR = new List<Derivation>();
                        if (current.Origin.parents[0].Origin.parents != null)
                        {
                            leftOR.Add(current.Origin.parents[0]);
                            Stack<Derivation> parentStack = new Stack<Derivation>();
                            parentStack.Push(current.Origin.parents[0]);
                            while (parentStack.Any())
                            {
                                Derivation currentParent = parentStack.Pop();
                                foreach (Derivation deriv in currentParent.Origin.parents)
                                {
                                    if (deriv.Origin.parents != null && !output.ReturnFacts().Contains(deriv.Form))
                                    {
                                        parentStack.Push(deriv);
                                        leftOR.Add(deriv);
                                    }
                                }
                            }
                        }
                        output.AddAss(orig.Left);
                        output = MakeFrame(output, leftOR, new Goal(current.Form));
                        if (!orig.Left.Equals(orig.Right))
                        {
                            List<Derivation> rightOR = new List<Derivation>();
                            if (current.Origin.parents[1].Origin.parents != null)
                            {
                                rightOR.Add(current.Origin.parents[1]);
                                Stack<Derivation> parentStack = new Stack<Derivation>();
                                parentStack.Push(current.Origin.parents[1]);
                                while (parentStack.Any())
                                {
                                    Derivation currentParent = parentStack.Pop();
                                    foreach (Derivation deriv in currentParent.Origin.parents)
                                    {
                                        if (deriv.Origin.parents != null && !output.ReturnFacts().Contains(deriv.Form))
                                        {
                                            parentStack.Push(deriv);
                                            rightOR.Add(deriv);
                                        }
                                    }
                                }
                            }
                            output.AddAss(orig.Right, output.Last.Item2.parent);
                            output = MakeFrame(output, rightOR, new Goal(current.Form));
                        }
                        output = ApplyDisjElim(output, orig, current.Form);
                        break;
                }
            }

            while (buildOrder.Any())
            {
                Goal current = buildOrder.Pop();
                if (current.deriv != null)
                {
                    List<Derivation> temp = new List<Derivation> { current.deriv };
                    current.deriv = null;
                    output = MakeFrame(output, temp, current);
                    continue;
                }
                if (current is ImplGoal)
                {
                    Implication impl = current.goal as Implication;
                    if (!output.ReturnFacts().Contains(impl.Consequent))
                    {
                        output = ApplyImplElim(output, current.goal as Implication);
                    }
                    continue;
                }
                if (current.goal is Disjunction)
                {
                    Disjunction disj = current.goal as Disjunction;
                    if (!output.ReturnFacts().Contains(disj))
                    {
                        output = ApplyDisjIntro(output, disj);
                    }
                }
                else if (current.goal is Conjunction)
                {
                    Conjunction conj = current.goal as Conjunction;
                    if (!output.ReturnFacts().Contains(conj))
                    {
                        output = ApplyConjIntro(output, conj.Left, conj.Right);
                    }
                }
                else if (current.goal is Implication)
                {
                    Implication impl = current.goal as Implication;
                    if (!output.ReturnFacts().Contains(impl.Antecedent))
                    {
                        output.AddAss(impl.Antecedent);
                    }
                    if (!output.ReturnFacts().Contains(impl))
                    {
                        output = ApplyImplIntro(output, impl.Antecedent, impl.Consequent);
                    }
                }
                else if (current.goal is Iff)
                {
                    Iff iff = current.goal as Iff;
                    if (!output.ReturnFacts().Contains(iff))
                    {
                        output = ApplyIffIntro(output, new Implication(iff.Left, iff.Right), new Implication(iff.Right, iff.Left));
                    }
                }
            }
            return output;
        }

        private static void MatchWithMPGoals(Implication impl)
        {
            Queue<Goal> queue = new Queue<Goal>();
            queue.Enqueue(goal);

            while (queue.Any())
            {
                Goal current = queue.Dequeue();
                if (current is MPGoal)
                {
                    MPGoal mpg = current as MPGoal;
                    if (mpg.consequent.Equals(impl.Consequent))
                    {
                        ImplGoal newGoal = new ImplGoal(impl, mpg);
                        newGoal.DeriveMPSubgoalTree();
                    }
                }
                foreach (Goal subgoal in current.subGoals)
                {
                    queue.Enqueue(subgoal);
                }
            }

        }
    }
}