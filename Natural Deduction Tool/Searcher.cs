using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Natural_Deduction_Tool
{
    public class Searcher
    {
        public static List<Derivation> Derivations { get; set; }
        public static Goal goal;
        public static HashSet<IFormula> facts;
        private static int depth;

        public static List<Derivation> DeriveWithElim(List<IFormula> premises, HashSet<IFormula> facts)
        {

            PriorityQueue derivQueue = new PriorityQueue();
            List<Derivation> output = new List<Derivation>();
            List<Implication> impls = new List<Implication>();
            List<Disjunction> disjs = new List<Disjunction>();
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
                        Negation neg = current.Form as Negation;
                        if (neg.Formula is Negation)
                        {
                            Negation negForm = neg.Formula as Negation;
                            List<IFormula> orig = new List<IFormula>() { neg };
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(negForm.Formula, new Origin(orig, Rules.NEG, derivPar));
                            derivQueue.Enqueue(newDeriv, output);
                            if (!facts.Contains(negForm.Formula))
                            {
                                facts.Add(negForm.Formula);
                                if (negForm.Formula is Implication)
                                {
                                    impls.Add(negForm.Formula as Implication);
                                }
                                else if (negForm.Formula is Disjunction)
                                {
                                    disjs.Add(negForm.Formula as Disjunction);
                                }
                            }
                        }
                    }
                    else if (current.Form is Conjunction)
                    {
                        Conjunction conj = current.Form as Conjunction;
                        List<IFormula> orig = new List<IFormula>() { conj };
                        List<Derivation> derivPar = new List<Derivation>() { current };
                        Derivation newLeftDeriv = new Derivation(conj.Left, new Origin(orig, Rules.AND, derivPar));
                        derivQueue.Enqueue(newLeftDeriv, output);
                        Derivation newRightDeriv = new Derivation(conj.Right, new Origin(orig, Rules.AND, derivPar));
                        derivQueue.Enqueue(newRightDeriv, output);

                        if (!facts.Contains(conj.Left))
                        {
                            facts.Add(conj.Left);
                            if (conj.Left is Implication)
                            {
                                impls.Add(conj.Left as Implication);
                            }
                            else if (conj.Left is Disjunction)
                            {
                                disjs.Add(conj.Left as Disjunction);
                            }
                        }
                        if (!facts.Contains(conj.Right))
                        {
                            facts.Add(conj.Right);
                            if (conj.Right is Implication)
                            {
                                impls.Add(conj.Right as Implication);
                            }
                            else if (conj.Right is Disjunction)
                            {
                                disjs.Add(conj.Right as Disjunction);
                            }
                        }
                    }
                    else if (current.Form is Iff)
                    {
                        Iff iff = current.Form as Iff;
                        List<IFormula> orig = new List<IFormula>() { iff };
                        Implication left = new Implication(iff.Left, iff.Right);
                        Implication right = new Implication(iff.Right, iff.Left);
                        List<Derivation> derivPar = new List<Derivation>() { current };
                        Derivation newLeftDeriv = new Derivation(left, new Origin(orig, Rules.BI, derivPar));
                        Derivation newRightDeriv = new Derivation(right, new Origin(orig, Rules.BI, derivPar));
                        derivQueue.Enqueue(newLeftDeriv, output);
                        derivQueue.Enqueue(newRightDeriv, output);
                        if (!facts.Contains(left))
                        {
                            facts.Add(left);
                            impls.Add(left);
                        }
                        if (!facts.Contains(right))
                        {
                            facts.Add(right);
                            impls.Add(right);
                        }
                    }
                    else if (current.Form is Implication)
                    {
                        Implication impl = current.Form as Implication;
                        if (facts.Contains(impl.Antecedent))
                        {
                            List<IFormula> orig = new List<IFormula>() { impl, impl.Antecedent };
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
                    }

                    if (current.Origin.rule != Rules.HYPO && current.Origin.rule != Rules.ASS)
                    {
                        if(current.Form is Implication)
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

        public static List<Derivation> DeriveWithElim(List<IFormula> premises, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs)
        {
            List<Derivation> output = new List<Derivation>();
            PriorityQueue derivQueue = new PriorityQueue();
            foreach (IFormula premise in premises)
            {
                facts.Add(premise);
                Derivation newDeriv = new Derivation(premise, new Origin(Rules.ASS, depth));
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
                        Negation neg = current.Form as Negation;
                        if (neg.Formula is Negation)
                        {
                            Negation negForm = neg.Formula as Negation;
                            List<IFormula> orig = new List<IFormula>() { neg };
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(negForm.Formula, new Origin(orig, Rules.NEG, derivPar));
                            derivQueue.Enqueue(newDeriv, output);
                            if (!facts.Contains(negForm.Formula))
                            {
                                facts.Add(negForm.Formula);
                                if (negForm.Formula is Implication)
                                {
                                    impls.Add(negForm.Formula as Implication);
                                }
                                else if (negForm.Formula is Disjunction)
                                {
                                    disjs.Add(negForm.Formula as Disjunction);
                                }
                            }
                        }
                    }
                    else if (current.Form is Conjunction)
                    {
                        Conjunction conj = current.Form as Conjunction;
                        List<IFormula> orig = new List<IFormula>() { conj };
                        List<Derivation> derivPar = new List<Derivation>() { current };
                        Derivation newLeftDeriv = new Derivation(conj.Left, new Origin(orig, Rules.AND, derivPar));
                        derivQueue.Enqueue(newLeftDeriv, output);
                        Derivation newRightDeriv = new Derivation(conj.Right, new Origin(orig, Rules.AND, derivPar));
                        derivQueue.Enqueue(newRightDeriv, output);

                        if (!facts.Contains(conj.Left))
                        {
                            facts.Add(conj.Left);
                            if (conj.Left is Implication)
                            {
                                impls.Add(conj.Left as Implication);
                            }
                            else if (conj.Left is Disjunction)
                            {
                                disjs.Add(conj.Left as Disjunction);
                            }
                        }
                        if (!facts.Contains(conj.Right))
                        {
                            facts.Add(conj.Right);
                            if (conj.Right is Implication)
                            {
                                impls.Add(conj.Right as Implication);
                            }
                            else if (conj.Right is Disjunction)
                            {
                                disjs.Add(conj.Right as Disjunction);
                            }
                        }
                    }
                    else if (current.Form is Iff)
                    {
                        Iff iff = current.Form as Iff;
                        List<IFormula> orig = new List<IFormula>() { iff };
                        Implication left = new Implication(iff.Left, iff.Right);
                        Implication right = new Implication(iff.Right, iff.Left);
                        List<Derivation> derivPar = new List<Derivation>() { current };
                        Derivation newLeftDeriv = new Derivation(left, new Origin(orig, Rules.BI, derivPar));
                        Derivation newRightDeriv = new Derivation(right, new Origin(orig, Rules.BI, derivPar));
                        derivQueue.Enqueue(newLeftDeriv, output);
                        derivQueue.Enqueue(newRightDeriv, output);
                        if (!facts.Contains(left))
                        {
                            facts.Add(left);
                            impls.Add(left);
                        }
                        if (!facts.Contains(right))
                        {
                            facts.Add(right);
                            impls.Add(right);
                        }
                    }
                    else if (current.Form is Implication)
                    {
                        Implication impl = current.Form as Implication;
                        if (facts.Contains(impl.Antecedent))
                        {
                            List<IFormula> orig = new List<IFormula>() { impl, impl.Antecedent };
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

        private static List<Derivation> DeriveWithDisjElim(Disjunction disj, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs)
        {
            depth++;
            List<Derivation> output = new List<Derivation>();

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

            List<IFormula> leftList = new List<IFormula>() { disj.Left };
            List<Derivation> left = DeriveWithElim(leftList, leftFacts, leftImpls, leftDisjs, leftDerivs);

            List<Derivation> right = null;
            if (disj.Left.Equals(disj.Right))
            {
                right = left;
                List<IFormula> orig = new List<IFormula>() { disj };
                foreach (Derivation deriv in left)
                {
                    List<Derivation> parDeriv = new List<Derivation>() { deriv };
                    output.Add(new Derivation(deriv.Form, new Origin(orig, Rules.OR, parDeriv)));
                }

            }
            else
            {
                List<IFormula> rightList = new List<IFormula>() { disj.Right };
                right = DeriveWithElim(rightList, rightFacts, rightImpls, rightDisjs, rightDerivs);
                List<IFormula> orig = new List<IFormula>() { disj };
                for (int i = 0; i < left.Count; i++)
                {
                    for (int j = 0; j < right.Count; j++)
                    {
                        if (left[i].Form.Equals(right[j].Form))
                        {
                            List<Derivation> parDeriv = new List<Derivation>() { left[i], right[j] };
                            output.Add(new Derivation(left[i].Form, new Origin(orig, Rules.OR, parDeriv)));
                        }
                    }
                }
            }



            depth--;
            return output;
        }

        public static string Prove(List<IFormula> premises, IFormula conclusion)
        {
            depth = 0;
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

            //Step 5: Go down the goal tree
            goalQueue.Clear();
            goalQueue.Enqueue(goal);

            while (goalQueue.Any())
            {
                Goal current = goalQueue.Dequeue();
                //Step 5.1: Add the necessary assumption in case the goal is an implication and try to derive the consequent
                //Step 5.2: Try an indirect proof to reach the goal
                //If the goal can be completed in either 5.1 or 5.2, check for completeness and close this branch.
                //Else, continue with 5.3.
                //Step 5.3: This goal cannot be proved. Continue down the tree if possible, otherwise this branch is closed.
                foreach (Goal subgoal in current.subGoals)
                {
                    goalQueue.Enqueue(subgoal);
                }
            }

            //Step 6: The proof has not been reached from any possible branch. The proof cannot be made. 

            return "I cannot prove this (yet).";
        }

        public static Frame ApplyNegIntro(Frame frame, IFormula negation, IFormula contra1, IFormula contra2)
        {
            Tuple<Interval, Interval, Interval> negInt = FindInt(frame, negation, contra1, contra2);
            if (negInt.Item1 == null || negInt.Item2 == null || negInt.Item3 == null)
            {
                throw new Exception("Tried applying NegIntro on non-existing assumption or non-existing contradictory formulas.");
            }
            List<IFormula> forms = new List<IFormula> { negation, contra1, contra2 };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            newFrame.Item1.AddForm(new Negation(negation), newFrame.Item1.Last.Item2.parent, new Annotation(newFrame.Item2, Rules.NEG, true));
            return newFrame.Item1;
        }

        public static Frame ApplyNegElim(Frame frame, Negation neg)
        {
            if (!(neg.Formula is Negation))
            {
                throw new Exception("Tried applying NegElim on non-double negation.");
            }
            Interval negInt = FindInt(frame, neg);
            if (negInt == null)
            {
                throw new Exception("Tried applying NegElim on non-existing negation.");
            }
            Negation neg2 = neg.Formula as Negation;
            List<IFormula> forms = new List<IFormula> { neg };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            newFrame.Item1.AddForm(neg2.Formula, new Annotation(newFrame.Item2, Rules.NEG, false));
            return newFrame.Item1;
        }

        public static Frame ApplyDisjIntro(Frame frame, Disjunction disj)
        {
            Interval leftInt = FindInt(frame, disj.Left);
            Interval rightInt = FindInt(frame, disj.Right);
            if (leftInt == null && rightInt == null)
            {
                throw new Exception("Tried applying disjIntro on non-existing disjuncts.");
            }
            if (leftInt != null)
            {
                List<IFormula> forms = new List<IFormula> { disj.Left };
                Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                newFrame.Item1.AddForm(disj, new Annotation(newFrame.Item2, Rules.OR, true));
                return newFrame.Item1;
            }
            else
            {
                List<IFormula> forms = new List<IFormula> { disj.Right };
                Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                newFrame.Item1.AddForm(disj, new Annotation(newFrame.Item2, Rules.OR, true));
                return newFrame.Item1;
            }

        }

        public static Frame ApplyDisjElim(Frame frame, Disjunction disj, IFormula goal)
        {
            List<int> lines = new List<int>();
            Interval left = null;
            Interval right = null;
            Interval disjInt = null;
            int leftInt = 0;
            int rightInt = 0;
            for (int i = frame.frame.Count - 1; i >= 0; i--)
            {
                if (frame.frame[i].Item1.Equals(disj.Left) && frame.frame[i].Item3.rule == Rules.ASS)
                {
                    leftInt = i;
                    left = frame.frame[i].Item2;
                }
                if (frame.frame[i].Item1.Equals(disj.Right) && frame.frame[i].Item3.rule == Rules.ASS)
                {
                    rightInt = i;
                    right = frame.frame[i].Item2;
                }
            }
            for (int i = 0; i < frame.frame.Count; i++)
            {
                if (frame.frame[i].Item1.Equals(disj))
                {
                    disjInt = frame.frame[i].Item2;
                    lines.Add(i + 1);
                    break;
                }
            }
            if (disjInt == null)
            {
                throw new Exception("Tried to apply disjunction elimination on non-existing disjunction.");
            }
            if (left == null || right == null)
            {
                throw new Exception("Tried to apply disjunction elimination on non-existing disjunct intervals.");
            }

            for (int i = leftInt; i < frame.frame.Count; i++)
            {
                if (frame.frame[i].Item2 == left.parent)
                {
                    throw new Exception("Desired goal cannot be found in left disjunct interval.");
                }
                if (frame.frame[i].Item2 == left && frame.frame[i].Item1.Equals(goal) && frame.frame[i].Item3.rule != Rules.ASS)
                {
                    lines.Add(i + 1);
                    break;
                }
            }
            for (int i = rightInt; i < frame.frame.Count; i++)
            {
                if (frame.frame[i].Item2 == right.parent)
                {
                    throw new Exception("Desired goal cannot be found in right disjunct interval.");
                }
                if (frame.frame[i].Item2 == right && frame.frame[i].Item1.Equals(goal) && frame.frame[i].Item3.rule != Rules.ASS)
                {
                    lines.Add(i + 1);
                    break;
                }
            }

            frame.AddForm(goal, frame.Last.Item2.parent, new Annotation(lines, Rules.OR, false));

            return frame;
        }

        public static Frame ApplyImplIntro(Frame frame, IFormula ante, IFormula cons)
        {
            Tuple<Interval, Interval> intervals = FindInt(frame, ante, cons);
            if (intervals.Item1 == null || intervals.Item2 == null)
            {
                throw new Exception("Tried applying ImplIntro on non-existing conjuncts.");
            }
            List<IFormula> forms = new List<IFormula> { ante, cons };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            newFrame.Item1.AddForm(new Implication(ante, cons), newFrame.Item1.Last.Item2.parent, new Annotation(newFrame.Item2, Rules.IMP, true));
            return newFrame.Item1;
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
                if (line.Item1.Equals(impl.Antecedent) && implInt.ThisOrParent(line.Item2))
                {
                    List<IFormula> forms = new List<IFormula> { impl, impl.Antecedent };
                    Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                    newFrame.Item1.AddForm(impl.Consequent, new Annotation(newFrame.Item2, Rules.IMP, false));
                    return newFrame.Item1;
                }
            }
            throw new Exception("Tried applying ImplElim while antecedent was not known.");
        }

        public static Frame ApplyIffIntro(Frame frame, Implication left, Implication right)
        {
            Tuple<Interval, Interval> intervals = FindInt(frame, left, right);
            if (intervals.Item1 == null || intervals.Item2 == null)
            {
                throw new Exception("Tried applying IffIntro on non-existing impls.");
            }
            List<IFormula> forms = new List<IFormula> { left, right };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            newFrame.Item1.AddForm(new Iff(left, right), new Annotation(newFrame.Item2, Rules.BI, true));
            return newFrame.Item1;
        }

        public static Frame ApplyIffElim(Frame frame, Iff iff, bool leftToRight)
        {
            Interval iffInt = FindInt(frame, iff);
            if (iffInt == null)
            {
                throw new Exception("Tried applying IffElim on non-existing bi-implication.");
            }
            List<IFormula> forms = new List<IFormula> { iff };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            if (leftToRight)
            {
                newFrame.Item1.AddForm(new Implication(iff.Left, iff.Right), new Annotation(newFrame.Item2, Rules.BI, false));
                return newFrame.Item1;
            }
            else
            {
                newFrame.Item1.AddForm(new Implication(iff.Right, iff.Left), new Annotation(newFrame.Item2, Rules.BI, false));
                return newFrame.Item1;
            }
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
            newFrame.Item1.AddForm(new Conjunction(left, right), new Annotation(newFrame.Item2, Rules.AND, true));
            return newFrame.Item1;
        }

        public static Frame ApplyConjElim(Frame frame, Conjunction conj, bool left)
        {
            Interval conjInt = FindInt(frame, conj);
            if (conjInt == null)
            {
                throw new Exception("Tried applying IffElim on non-existing conjunction.");
            }
            List<IFormula> forms = new List<IFormula> { conj };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            if (left)
            {
                newFrame.Item1.AddForm(conj.Left, new Annotation(newFrame.Item2, Rules.AND, false));
                return newFrame.Item1;
            }
            else
            {
                newFrame.Item1.AddForm(conj.Right, new Annotation(newFrame.Item2, Rules.AND, false));
                return newFrame.Item1;
            }
        }

        public static Frame ApplyREI(Frame frame, IFormula goal)
        {
            List<IFormula> forms = new List<IFormula>() { goal };
            return REI(frame, forms).Item1;
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

        public static Frame MakeFrame(Frame output, List<Derivation> derivs, Goal goal)
        {
            Stack<Goal> goalStack = new Stack<Goal>();
            Stack<Derivation> derivStack = new Stack<Derivation>();
            Stack<Goal> buildOrder = new Stack<Goal>();
            Stack<Derivation> buildOrderDeriv = new Stack<Derivation>();
            goalStack.Push(goal);
            bool noCompleteChildren = true;

            while (goalStack.Any())
            {
                noCompleteChildren = true;
                Goal current = goalStack.Pop();
                foreach (Goal subgoal in current.subGoals)
                {
                    if (subgoal.Completed)
                    {
                        noCompleteChildren = false;
                        goalStack.Push(subgoal);
                    }
                }
                if (noCompleteChildren)
                {
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
                if (!(current is MPGoal))
                {
                    buildOrder.Push(current);
                }
            }

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
                bool advanceGoal = false;
                Derivation current = buildOrderDeriv.Pop();
                foreach (IFormula form in current.Origin.origins)
                {
                    if (!output.ReturnFacts().Contains(form))
                    {
                        //We must first advance up the goal tree
                        advanceGoal = true;
                        break;
                    }
                }
                if (advanceGoal)
                {
                    break;
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
                if (current is ImplGoal)
                {
                    Goal cons = buildOrder.Pop();
                    if (!output.ReturnFacts().Contains(cons.goal))
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
                    if (!output.ReturnFacts().Contains(impl))
                    {
                        output.AddAss(impl.Antecedent);
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
                if(current is MPGoal)
                {
                    MPGoal mpg = current as MPGoal;
                    if (mpg.consequent.Equals(impl.Consequent))
                    {
                        ImplGoal newGoal = new ImplGoal(impl,mpg);
                        newGoal.DeriveMPSubgoalTree();
                    }
                }
                foreach(Goal subgoal in current.subGoals)
                {
                    queue.Enqueue(subgoal);
                }
            }
            
        }
    }

    public class ReadOnlyTextBox : TextBox
    {
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        public ReadOnlyTextBox()
        {
            ReadOnly = true;
            GotFocus += TextBoxGotFocus;
            Cursor = Cursors.Arrow; // mouse cursor like in other controls
        }

        private void TextBoxGotFocus(object sender, EventArgs args)
        {
            HideCaret(this.Handle);
        }
    }
}
