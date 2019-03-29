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
            if (!premises.Any())
            {
                premises.Add(new PropVar("\u22A4"));
            }
            facts = new HashSet<IFormula>();

            //Step 1: (Happens in Form1) Check if the premises are UNSAT. If so, go to 2. Else, go to 3.

            //Step 2: Derive conclusion from UNSAT premises

            //Step 3: Derive full subgoal tree, derive all possible derivations from premises

            goal = new Goal(conclusion);

            goal.DeriveSubgoalTree(false);

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
                HashSet<IFormula> newFacts = CloneStuff(facts);
                List<Implication> newImpls = CloneStuff(Implications);
                List<Disjunction> newDisjs = CloneStuff(Disjunctions);
                List<Derivation> newDerivs = new List<Derivation>();
                ElimGoalSearch(goal, newFacts, newImpls, newDisjs, newDerivs, depth, false);

                if (Searcher.goal.Completed && !findWholeTree)
                {
                    return;
                }
            }

            //Check if the goal can be proved by an indirect proof
            if (goal is ContraGoal)
            {
                IFormula negGoal = NegForm(goal.goal);

                if (facts.Contains(negGoal))
                {
                    //The (non-)negation was already known
                    goal.Complete();
                }
                else
                {
                    HashSet<IFormula> newFacts = CloneStuff(facts);
                    List<Implication> newImpls = CloneStuff(Implications);
                    List<Disjunction> newDisjs = CloneStuff(Disjunctions);
                    List<Derivation> newDerivs = new List<Derivation>();

                    List<Derivation> contraDerivs = DeriveWithElim(new List<IFormula> { negGoal }, newFacts, newImpls, newDisjs, newDerivs);

                    //TODO: Refine FindContra
                    FindContra(goal as ContraGoal, negGoal, newFacts, newImpls, newDisjs, contraDerivs, depth);

                    goal.ProvedByContra = true;
                }

                if (Searcher.goal.Completed && !findWholeTree)
                {
                    return;
                }
            }

            //Call DLS on all uncompleted subgoals if there are any, otherwise close this branch.
            if (!checkedWithElim && !(goal is ContraGoal))
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
                                    newImpls.Add(impl.Consequent as Implication);
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
                                    newImpls.Add(impl.Consequent as Implication);
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

        private static void ElimGoalSearch(Goal goal, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth, bool calledByContra)
        {
            if (facts.Contains(goal.goal))
            {
                foreach (Derivation deriv in Derivations)
                {
                    if (deriv.Form.Equals(goal.goal))
                    {
                        goal.deriv = deriv;
                        goal.Complete();
                        return;
                    }
                }
                foreach (Derivation deriv in derivs)
                {
                    if (deriv.Form.Equals(goal.goal))
                    {
                        goal.deriv = deriv;
                        goal.Complete();
                        return;
                    }
                }
            }
            bool newAss = false;
            PriorityQueue derivQueue = new PriorityQueue();

            if (goal is ContraGoal)
            {
                newAss = true;
                derivLength++;
                Derivation newDeriv = new Derivation(goal.goal, new Origin(Rules.ASS, derivLength));
                derivQueue.Enqueue(newDeriv, derivs);
                derivs.Add(newDeriv);
                facts.Add(goal.goal);
            }
            else if (goal.parent != null && !(goal.parent is ImplGoal) && goal.parent.goal is Implication)
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
                    DisjElimGoalSearch(goal, disj, facts, impls, disjs, derivs, depth, calledByContra);
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

                    CheckImpls(current, facts, impls, disjs, derivs, derivQueue);

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
                    DisjElimGoalSearch(goal, disj, facts, impls, disjs, derivs, depth, calledByContra);
                }
                if (goal.Completed)
                {
                    return;
                }
            }

            List<Goal> setForRemoval = new List<Goal>();
            if (!(goal is ContraGoal) && goal.subGoals.Any())
            {
                if (depth < max_depth)
                {
                    //Move down the goal tree
                    foreach (Goal subgoal in goal.subGoals)
                    {
                        HashSet<IFormula> newFacts = CloneStuff(facts);
                        List<Implication> newImpls = CloneStuff(impls);
                        List<Disjunction> newDisjs = CloneStuff(disjs);
                        List<Derivation> newDerivs = CloneStuff(derivs);
                        if (!(subgoal is MPGoal || subgoal is ContraGoal))
                        {
                            ElimGoalSearch(subgoal, newFacts, newImpls, newDisjs, newDerivs, ++depth, calledByContra);
                        }
                        else if(subgoal is MPGoal)
                        {
                            if (subgoal.subGoals.Any())
                            {
                                foreach (Goal subsubgoal in subgoal.subGoals)
                                {
                                    //ImplGoal always has one subgoal
                                    ElimGoalSearch(subsubgoal.subGoals[0], newFacts, newImpls, newDisjs, newDerivs, ++depth, calledByContra);
                                }
                            }
                            else
                            {
                                setForRemoval.Add(subgoal);
                            }
                        }
                        else
                        {
                            IFormula negGoal = NegForm(subgoal.goal);

                            if (facts.Contains(negGoal))
                            {
                                //The (non-)negation was already known
                                subgoal.Complete();
                            }
                            else
                            {
                                List<Derivation> contraDerivs = DeriveWithElim(new List<IFormula> { negGoal }, newFacts, newImpls, newDisjs, newDerivs);

                                //TODO: Refine FindContra
                                FindContra(subgoal as ContraGoal, negGoal, newFacts, newImpls, newDisjs, contraDerivs, depth);

                                subgoal.ProvedByContra = true;
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
            else if (!(goal is ContraGoal))
            {
                goal.CloseBranch();
                if (newAss)
                {
                    derivLength--;
                }
                return;
            }
        }

        private static void ElimGoalSearchForContra(Goal goal, IFormula negAss, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth)
        {
            if (facts.Contains(goal.goal))
            {
                foreach (Derivation deriv in Derivations)
                {
                    if (deriv.Form.Equals(goal.goal))
                    {
                        goal.deriv = deriv;
                        goal.Complete();
                        return;
                    }
                }
                foreach (Derivation deriv in derivs)
                {
                    if (deriv.Form.Equals(goal.goal))
                    {
                        goal.deriv = deriv;
                        goal.Complete();
                        return;
                    }
                }
            }
            PriorityQueue derivQueue = new PriorityQueue();

            derivLength++;
            Derivation negAssDeriv = new Derivation(negAss, new Origin(Rules.ASS, derivLength));
            derivQueue.Enqueue(negAssDeriv, derivs);
            derivs.Add(negAssDeriv);
            facts.Add(negAss);

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

                    CheckImpls(current, facts, impls, disjs, derivs, derivQueue);

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
                    DisjElimGoalSearch(goal, disj, facts, impls, disjs, derivs, depth, true);
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
                        HashSet<IFormula> newFacts = CloneStuff(facts);
                        List<Implication> newImpls = CloneStuff(impls);
                        List<Disjunction> newDisjs = CloneStuff(disjs);
                        List<Derivation> newDerivs = CloneStuff(derivs);
                        if (!(subgoal is MPGoal))
                        {
                            ElimGoalSearch(subgoal, newFacts, newImpls, newDisjs, newDerivs, ++depth, true);
                        }
                        else
                        {
                            if (subgoal.subGoals.Any())
                            {
                                foreach (Goal subsubgoal in subgoal.subGoals)
                                {
                                    //ImplGoal always has one subgoal
                                    ElimGoalSearch(subsubgoal.subGoals[0], newFacts, newImpls, newDisjs, newDerivs, ++depth, true);
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
            else if (!(goal is ContraGoal))
            {
                goal.CloseBranch();
                derivLength--;
                return;
            }
        }

        private static void ElimGoalSearchForDisj(Goal goal, IFormula disjunct, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth, bool calledByContra)
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

                    CheckImpls(current, facts, impls, disjs, derivs, derivQueue);

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
                    DisjElimGoalSearch(goal, disj, facts, impls, disjs, derivs, depth, calledByContra);
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
                        HashSet<IFormula> newFacts = CloneStuff(facts);
                        List<Implication> newImpls = CloneStuff(impls);
                        List<Disjunction> newDisjs = CloneStuff(disjs);
                        List<Derivation> newDerivs = CloneStuff(derivs);
                        if (!(subgoal is MPGoal || subgoal is ContraGoal))
                        {
                            ElimGoalSearchForDisj(subgoal, disjunct, newFacts, newImpls, newDisjs, newDerivs, ++depth, calledByContra);
                        }
                        else if (subgoal is MPGoal)
                        {
                            if (subgoal.subGoals.Any())
                            {
                                foreach (Goal subsubgoal in subgoal.subGoals)
                                {
                                    //ImplGoal always has one subgoal
                                    ElimGoalSearchForDisj(subsubgoal, disjunct, newFacts, newImpls, newDisjs, newDerivs, ++depth, calledByContra);
                                }
                            }
                            else
                            {
                                setForRemoval.Add(subgoal);
                            }
                        }
                        else
                        {
                            IFormula negGoal = NegForm(subgoal.goal);

                            if (facts.Contains(negGoal))
                            {
                                //The (non-)negation was already known
                                subgoal.Complete();
                            }
                            else
                            {
                                List<Derivation> contraDerivs = DeriveWithElim(new List<IFormula> { negGoal }, newFacts, newImpls, newDisjs, newDerivs);

                                //TODO: Refine FindContra
                                FindContra(subgoal as ContraGoal, negGoal, newFacts, newImpls, newDisjs, contraDerivs, depth);

                                subgoal.ProvedByContra = true;
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
            else if (!(goal is ContraGoal))
            {
                goal.CloseBranch();
                derivLength--;
                return;
            }
        }

        private static void DisjElimGoalSearch(Goal goal, Disjunction disj, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, int depth, bool calledByContra)
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
            leftGoal.DeriveSubgoalTree(calledByContra);
            Goal rightGoal = new Goal(goal.goal);
            rightGoal.DeriveSubgoalTree(calledByContra);
            ElimGoalSearchForDisj(leftGoal, disj.Left, leftFacts, leftImpls, leftDisjs, leftDerivs, depth, calledByContra);

            if (disj.Left.Equals(disj.Right))
            {
                rightGoal = leftGoal;
            }
            else
            {
                ElimGoalSearchForDisj(rightGoal, disj.Right, rightFacts, rightImpls, rightDisjs, rightDerivs, depth, calledByContra);
            }

            if (leftGoal.Completed && rightGoal.Completed)
            {
                FindShortestSubgoal(leftGoal);
                FindShortestSubgoal(rightGoal);

                Queue<Goal> queue = new Queue<Goal>();
                queue.Enqueue(leftGoal);

                Derivation leftDeriv = null;
                Derivation rightDeriv = null;
                while (queue.Any())
                {
                    Goal current = queue.Dequeue();
                    if (current.deriv != null && current.Completed)
                    {
                        leftDeriv = current.deriv;
                        break;
                    }
                    foreach (Goal subgoal in current.subGoals)
                    {
                        queue.Enqueue(subgoal);
                    }
                }
                queue.Clear();
                queue.Enqueue(rightGoal);
                while (queue.Any())
                {
                    Goal current = queue.Dequeue();
                    if (current.deriv != null && current.Completed)
                    {
                        rightDeriv = current.deriv;
                        break;
                    }
                    foreach (Goal subgoal in current.subGoals)
                    {
                        queue.Enqueue(subgoal);
                    }
                }

                List<Goal> newSubgoals = new List<Goal>();
                foreach(Goal subgoal in goal.subGoals)
                {
                    if(subgoal is MPGoal || subgoal is ContraGoal)
                    {
                        newSubgoals.Add(subgoal);
                    }
                }
                goal.subGoals = newSubgoals;

                DisjGoal newGoal = new DisjGoal(goal.goal, goal)
                {
                    leftDeriv = leftDeriv,
                    rightDeriv = rightDeriv,
                    provenIn = disj
                };
                newGoal.subGoals.Add(leftGoal);
                newGoal.subGoals.Add(rightGoal);
                newGoal.Complete();
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
                        newGoal.DeriveMPSubgoalTree(false);
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