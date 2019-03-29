using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public partial class Searcher
    {
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

            if (current is ContraGoal)
            {
                newAssumption = NegForm(current.goal);
                assumptions.Add(newAssumption);
            }
            else if (current.goal is Implication && !(current is ImplGoal))
            {
                Implication impl = current.goal as Implication;
                newAssumption = impl.Antecedent;
                assumptions.Add(newAssumption);
            }
            else if (current is DisjGoal)
            {
                DisjGoal disjG = current as DisjGoal;

                //Right goes first so it appears last on the tree
                newAssumption = disjG.provenIn.Right;
                assumptions.Add(newAssumption);
                StackGoals(current.subGoals[1], output, derivStack, derivs, assumptions);
                assumptions.Remove(newAssumption);
                newAssumption = disjG.provenIn.Left;
                assumptions.Add(newAssumption);
                StackGoals(current.subGoals[0], output, derivStack, derivs, assumptions);
                assumptions.Remove(newAssumption);
                return;
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
                    int min_length = int.MaxValue;
                    ImplGoal completeImpl = null;
                    foreach (ImplGoal ig in completeMP.subGoals)
                    {
                        if (ig.Completed)
                        {
                            if (completeImpl == null)
                            {
                                min_length = BranchLength(ig, 0);
                                completeImpl = ig;
                            }
                            else
                            {
                                int temp = BranchLength(ig, 0);
                                if (temp < min_length)
                                {
                                    min_length = temp;
                                    completeImpl = ig;
                                }
                            }
                        }
                    }
                    StackGoals(completeImpl, output, derivStack, derivs, assumptions);
                }
                else
                {
                    StackGoals(completeGoal, output, derivStack, derivs, assumptions);
                }
            }
            else if (completeMP != null)
            {
                int min_length = int.MaxValue;
                ImplGoal completeImpl = null;
                foreach (ImplGoal ig in completeMP.subGoals)
                {
                    if (ig.Completed)
                    {
                        if (completeImpl == null)
                        {
                            min_length = BranchLength(ig, 0);
                            completeImpl = ig;
                        }
                        else
                        {
                            int temp = BranchLength(ig, 0);
                            if (temp < min_length)
                            {
                                min_length = temp;
                                completeImpl = ig;
                            }
                        }
                    }
                }
                StackGoals(completeImpl, output, derivStack, derivs, assumptions);
            }
            else if (completeGoal != null)
            {
                if (halfCompleteGoal != null)
                {
                    StackGoals(halfCompleteGoal, output, derivStack, derivs, assumptions);
                }
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

            if (output.ReturnFacts().Contains(goal.goal) && !buildOrderDeriv.Any() && buildOrder.Count == 1)
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
                    case Rules.MORG:
                        output = ApplyMorg(output, current.Origin.origins[0] as Negation);
                        break;
                }
            }

            while (buildOrder.Any())
            {
                Goal current = buildOrder.Pop();
                if (current.deriv != null && !output.ReturnFacts().Contains(current.goal))
                {
                    List<IFormula> assumptions = new List<IFormula>();
                    Interval startingInt = output.Last.Item2;

                    foreach (Line line in output.frame)
                    {
                        if (line.Item3.rule == Rules.ASS)
                        {
                            assumptions.Add(line.Item1);
                        }
                    }

                    for (int i = assumptions.Count - 1; i >= 0; i--)
                    {
                        if (current.Assumptions.Contains(assumptions[i]))
                        {
                            break;
                        }
                        else
                        {
                            startingInt = startingInt.parent;
                        }
                    }

                    foreach (IFormula form in current.Assumptions)
                    {
                        if (!output.ReturnFacts().Contains(form))
                        {
                            if (startingInt != null)
                            {
                                output.AddAss(form, startingInt);
                                startingInt = null;
                            }
                            else
                            {
                                output.AddAss(form);
                            }
                        }
                    }
                    if (current.deriv.Origin.rule != Rules.ASS)
                    {
                        List<Derivation> temp = new List<Derivation> { current.deriv };
                        current.deriv = null;
                        output = MakeFrame(output, temp, current);
                    }
                    continue;
                }
                if (current is ContraGoal)
                {
                    ContraGoal contra = current as ContraGoal;
                    if (!output.ReturnFacts().Contains(current.goal))
                    {
                        IFormula ass = null;
                        if (current.goal is Negation)
                        {
                            Negation neg = current.goal as Negation;
                            ass = neg.Formula;
                        }
                        else
                        {
                            ass = new Negation(current.goal);
                        }

                        if (contra.contraDeriv != null)
                        {
                            Goal newGoal = new Goal(contra.contraDeriv.Form);
                            List<Derivation> temp = new List<Derivation> { contra.contraDeriv };
                            output = MakeFrame(output, temp, newGoal);
                        }

                        /*if (contra.NeedsAdditionalDerivs)
                        {
                            //TODO: Bouw de derivatie uit
                        }*/

                        output = ApplyNegIntro(output, ass, current.subGoals[0].goal, new Negation(current.subGoals[0].goal));
                    }
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
                if (current is DisjGoal)
                {
                    DisjGoal disjG = current as DisjGoal;
                    Disjunction origDisj = disjG.provenIn;
                    IFormula disj = disjG.goal;
                    output = ApplyDisjElim(output, origDisj, disj);
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
                        if (!output.ReturnFacts().Contains(impl.Antecedent))
                        {
                            output.AddAss(impl.Antecedent);
                        }
                        else
                        {
                            bool assFound = false;
                            foreach (Line line in output.frame)
                            {
                                if (impl.Antecedent.Equals(line.Item1) && line.Item3.rule != Rules.HYPO && output.Last.Item2.ThisOrParent(line.Item2))
                                {
                                    assFound = true;
                                    break;
                                }
                            }
                            if (!assFound)
                            {
                                output.AddAss(impl.Antecedent);
                            }
                        }
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
    }
}
