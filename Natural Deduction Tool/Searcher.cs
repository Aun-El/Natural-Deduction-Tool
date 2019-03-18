using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public class Searcher
    {
        public static Queue<Frame> Fringe { get; private set; }
        public static Queue<Frame> NextFringe { get; private set; }
        public static Queue<Goal> GoalFringe;
        public static Queue<Goal> NextGoalFringe;
        public static Frame Init;

        public static HashSet<IFormula> SubForms { get; private set; }
        public static HashSet<IFormula> PremiseSubForms { get; private set; }
        public static HashSet<HashSet<IFormula>> ClosedList { get; private set; }

        public static List<Derivation> DeriveWithElim(List<IFormula> premises, HashSet<IFormula> facts)
        {
            Queue<Derivation> derivQueue = new Queue<Derivation>();
            List<Derivation> output = new List<Derivation>();
            List<Implication> impls = new List<Implication>();
            List<Disjunction> disjs = new List<Disjunction>();
            List<IFormula> hypoOrig = new List<IFormula>();
            foreach (IFormula premise in premises)
            {
                facts.Add(premise);
                Derivation newDeriv = new Derivation(premise, new Origin(Rules.HYPO));
                output.Add(newDeriv);
                derivQueue.Enqueue(newDeriv);
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
                            if (!facts.Contains(negForm.Formula))
                            {
                                List<IFormula> orig = new List<IFormula>() { neg };
                                facts.Add(negForm.Formula);
                                if (negForm.Formula is Implication)
                                {
                                    impls.Add(negForm.Formula as Implication);
                                }
                                else if (negForm.Formula is Disjunction)
                                {
                                    disjs.Add(negForm.Formula as Disjunction);
                                }
                                List<Derivation> derivPar = new List<Derivation>() { current };
                                Derivation newDeriv = new Derivation(negForm.Formula, new Origin(orig, Rules.NEG, derivPar));
                                output.Add(newDeriv);
                                derivQueue.Enqueue(newDeriv);
                            }
                        }
                    }
                    else if (current.Form is Conjunction)
                    {
                        Conjunction conj = current.Form as Conjunction;
                        List<IFormula> orig = new List<IFormula>() { conj };
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
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(conj.Left, new Origin(orig, Rules.AND, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
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
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(conj.Right, new Origin(orig, Rules.AND, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                    }
                    else if (current.Form is Iff)
                    {
                        Iff iff = current.Form as Iff;
                        List<IFormula> orig = new List<IFormula>() { iff };
                        Implication left = new Implication(iff.Left, iff.Right);
                        Implication right = new Implication(iff.Right, iff.Left);
                        if (!facts.Contains(left))
                        {
                            facts.Add(left);
                            impls.Add(left);
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(left, new Origin(orig, Rules.BI, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                        if (!facts.Contains(right))
                        {
                            facts.Add(right);
                            impls.Add(right);
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(right, new Origin(orig, Rules.BI, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                    }
                    else if (current.Form is Implication)
                    {
                        Implication impl = current.Form as Implication;
                        if (facts.Contains(impl.Antecedent) && !facts.Contains(impl.Consequent))
                        {
                            List<IFormula> orig = new List<IFormula>() { impl, impl.Antecedent };
                            facts.Add(impl.Consequent);
                            if (impl.Consequent is Implication)
                            {
                                impls.Add(impl.Consequent as Implication);
                            }
                            else if (impl.Consequent is Disjunction)
                            {
                                disjs.Add(impl.Consequent as Disjunction);
                            }
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            foreach (Derivation deriv in output)
                            {
                                if (deriv.Form.Equals(impl.Antecedent))
                                {
                                    derivPar.Add(deriv);
                                    break;
                                }
                            }
                            Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                    }

                    List<Implication> newImpls = new List<Implication>();
                    foreach (Implication impl in impls)
                    {
                        if (!facts.Contains(impl.Consequent))
                        {
                            if (current.Form.Equals(impl.Antecedent))
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
                                List<IFormula> orig = new List<IFormula>() { impl, current.Form };
                                List<Derivation> derivPar = new List<Derivation>() { current };
                                foreach (Derivation deriv in output)
                                {
                                    if (deriv.Form.Equals(impl.Antecedent))
                                    {
                                        derivPar.Add(deriv);
                                        break;
                                    }
                                }
                                Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                                output.Add(newDeriv);
                                derivQueue.Enqueue(newDeriv);
                            }
                            else
                            {
                                newImpls.Add(impl);
                            }
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
                    disjDerivs = disjDerivs.Concat(DeriveWithDisjElim(disj, facts, impls, disjs)).ToList();
                }
                foreach (Derivation deriv in disjDerivs)
                {
                    disjDeriv = true;
                    facts.Add(deriv.Form);
                    output.Add(deriv);
                    derivQueue.Enqueue(deriv);
                }
            }

            return output;
        }

        public static List<Derivation> DeriveWithElim(List<IFormula> premises, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs)
        {
            List<Derivation> output = new List<Derivation>();
            List<IFormula> hypoOrig = new List<IFormula>();
            Queue<Derivation> derivQueue = new Queue<Derivation>();
            foreach (IFormula premise in premises)
            {
                facts.Add(premise);
                Derivation newDeriv = new Derivation(premise, new Origin(Rules.ASS));
                derivQueue.Enqueue(newDeriv);
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
                            if (!facts.Contains(negForm.Formula))
                            {
                                List<IFormula> orig = new List<IFormula>() { neg };
                                facts.Add(negForm.Formula);
                                if (negForm.Formula is Implication)
                                {
                                    impls.Add(negForm.Formula as Implication);
                                }
                                else if (negForm.Formula is Disjunction)
                                {
                                    disjs.Add(negForm.Formula as Disjunction);
                                }
                                List<Derivation> derivPar = new List<Derivation>() { current };
                                Derivation newDeriv = new Derivation(negForm.Formula, new Origin(orig, Rules.NEG, derivPar));
                                output.Add(newDeriv);
                                derivQueue.Enqueue(newDeriv);
                            }
                        }
                    }
                    else if (current.Form is Conjunction)
                    {
                        Conjunction conj = current.Form as Conjunction;
                        List<IFormula> orig = new List<IFormula>() { conj };
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
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(conj.Left, new Origin(orig, Rules.AND, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
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
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(conj.Right, new Origin(orig, Rules.AND, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                    }
                    else if (current.Form is Iff)
                    {
                        Iff iff = current.Form as Iff;
                        List<IFormula> orig = new List<IFormula>() { iff };
                        Implication left = new Implication(iff.Left, iff.Right);
                        Implication right = new Implication(iff.Right, iff.Left);
                        if (!facts.Contains(left))
                        {
                            facts.Add(left);
                            impls.Add(left);
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(left, new Origin(orig, Rules.BI, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                        if (!facts.Contains(right))
                        {
                            facts.Add(right);
                            impls.Add(right);
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            Derivation newDeriv = new Derivation(right, new Origin(orig, Rules.BI, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                    }
                    else if (current.Form is Implication)
                    {
                        Implication impl = current.Form as Implication;
                        if (facts.Contains(impl.Antecedent) && !facts.Contains(impl.Consequent))
                        {
                            List<IFormula> orig = new List<IFormula>() { impl, impl.Antecedent };
                            facts.Add(impl.Consequent);
                            if (impl.Consequent is Implication)
                            {
                                impls.Add(impl.Consequent as Implication);
                            }
                            else if (impl.Consequent is Disjunction)
                            {
                                disjs.Add(impl.Consequent as Disjunction);
                            }
                            List<Derivation> derivPar = new List<Derivation>() { current };
                            foreach (Derivation deriv in output)
                            {
                                if (deriv.Form.Equals(impl.Antecedent))
                                {
                                    derivPar.Add(deriv);
                                    break;
                                }
                            }
                            Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                            output.Add(newDeriv);
                            derivQueue.Enqueue(newDeriv);
                        }
                    }

                    List<Implication> newImpls = new List<Implication>();
                    foreach (Implication impl in impls)
                    {
                        if (!facts.Contains(impl.Consequent))
                        {
                            if (current.Form.Equals(impl.Antecedent))
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
                                List<IFormula> orig = new List<IFormula>() { impl, current.Form };
                                List<Derivation> derivPar = new List<Derivation>() { current };
                                foreach (Derivation deriv in output)
                                {
                                    if (deriv.Form.Equals(impl.Antecedent))
                                    {
                                        derivPar.Add(deriv);
                                        break;
                                    }
                                }
                                Derivation newDeriv = new Derivation(impl.Consequent, new Origin(orig, Rules.IMP, derivPar));
                                output.Add(newDeriv);
                                derivQueue.Enqueue(newDeriv);
                            }
                            else
                            {
                                newImpls.Add(impl);
                            }
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
                    disjDerivs = disjDerivs.Concat(DeriveWithDisjElim(disj, facts, impls, disjs)).ToList();
                }
                foreach (Derivation deriv in disjDerivs)
                {
                    disjDeriv = true;
                    facts.Add(deriv.Form);
                    output.Add(deriv);
                    derivQueue.Enqueue(deriv);
                }
            }

            return output;
        }

        private static List<Derivation> DeriveWithDisjElim(Disjunction disj, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs)
        {
            List<Derivation> output = new List<Derivation>();

            HashSet<IFormula> leftFacts = new HashSet<IFormula> { disj.Left };
            List<IFormula> leftList = new List<IFormula>() { disj.Left };
            List<Implication> leftImpls = new List<Implication>();
            foreach (Implication impl in impls)
            {
                leftImpls.Add(impl);
            }
            List<Disjunction> leftDisjs = new List<Disjunction>();
            foreach (Disjunction newDisj in disjs)
            {
                leftDisjs.Add(newDisj);
            }
            List<Derivation> left = DeriveWithElim(leftList, leftFacts, leftImpls, leftDisjs);

            HashSet<IFormula> rightFacts = new HashSet<IFormula> { disj.Right };
            List<IFormula> rightList = new List<IFormula>() { disj.Right };
            List<Implication> rightImpls = new List<Implication>();
            foreach (Implication impl in impls)
            {
                rightImpls.Add(impl);
            }
            List<Disjunction> rightDisjs = new List<Disjunction>();
            foreach (Disjunction newDisj in disjs)
            {
                rightDisjs.Add(newDisj);
            }
            List<Derivation> right = DeriveWithElim(rightList, rightFacts, rightImpls, rightDisjs);

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

            return output;
        }

        public static string Prove(List<IFormula> premises, IFormula conclusion)
        {
            //Step 1: (Happens in Form1) Check if the premises are UNSAT. If so, go to 2. Else, go to 3.

            //Step 2: Derive conclusion from UNSAT premises

            //Step 3: Derive full subgoal tree, derive all possible derivations from premises

            HashSet<IFormula> facts = new HashSet<IFormula>();
            List<Derivation> derivations = DeriveWithElim(premises, facts);

            Goal goal = new Goal(conclusion);

            goal.DeriveSubgoalTree();

            //Step 4: Check if derivations satisfy goal

            Queue<Goal> goalQueue = new Queue<Goal>();
            goalQueue.Enqueue(goal);
            while (goalQueue.Any())
            {
                Goal current = goalQueue.Dequeue();
                foreach (Derivation deriv in derivations)
                {
                    if (MatchGoal(deriv.Form, current))
                    {
                        Frame output = new Frame(premises);
                        output = MakeFrame(output, derivations, goal);
                        return output.ToString();
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

        public static string BidirectionalSearch(List<IFormula> premises, IFormula conclusion)
        {
            SubForms = conclusion.GetSubForms(new HashSet<IFormula>());
            if (!premises.Any())
            {
                premises.Add(new PropVar("\u22a4"));
            }
            else
            {
                foreach (IFormula premise in premises)
                {
                    SubForms = premise.GetSubForms(SubForms);
                    //PremiseSubForms = premise.GetSubForms(PremiseSubForms);
                }
            }

            Init = new Frame(premises);
            Goal goal = new Goal(conclusion);

            Fringe = new Queue<Frame>();
            NextFringe = new Queue<Frame>();
            GoalFringe = new Queue<Goal>();
            NextGoalFringe = new Queue<Goal>();
            ClosedList = new HashSet<HashSet<IFormula>>();

            Fringe.Enqueue(Init);
            GoalFringe.Enqueue(goal);

            while (true)
            {
                //Check if any frame matches a new subgoal
                Queue<Frame> frames = new Queue<Frame>();
                frames.Enqueue(Init);

                while (frames.Any())
                {
                    Frame currentFrame = frames.Dequeue();
                    /*foreach (Goal subgoal in GoalFringe)
                    {
                        if (MatchGoal(currentFrame, subgoal))
                        {
                            return MakeFrame(currentFrame, subgoal).ToString();
                        }
                    }*/
                    foreach (Frame child in currentFrame.DerivedFrames)
                    {
                        frames.Enqueue(child);
                    }
                }

                Queue<Goal> goals = new Queue<Goal>();
                goals.Enqueue(goal);

                //Check if any goal matches a new frame
                while (goals.Any())
                {
                    Goal currentGoal = goals.Dequeue();
                    /*foreach (Frame frame in Fringe)
                    {
                        if (MatchGoal(frame, currentGoal))
                        {
                            return MakeFrame(frame, currentGoal).ToString();
                        }
                    }*/
                    foreach (Goal subgoal in currentGoal.subGoals)
                    {
                        goals.Enqueue(subgoal);
                    }
                }

                //If no frame can be matched, generate new frames and subgoals

                DeriveSubframes();
                DeriveSubgoals();

                Fringe = NextFringe;
                GoalFringe = NextGoalFringe;
                NextFringe = new Queue<Frame>();
                NextGoalFringe = new Queue<Goal>();

                if (!Fringe.Any() && !GoalFringe.Any())
                {
                    //There is nothing more to be done; it cannot be proven
                    return "I cannot prove this (yet).";
                }
            }
        }

        /// <summary>
        /// Tries to prove a conclusion from a set of premises.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="conclusion"></param>
        /// <returns></returns>
        public static void DeriveSubframes()
        {
            while (Fringe.Any())
            {
                Frame currentFrame = Fringe.Dequeue();

                AddToNextFringe(ConjIntro(currentFrame), currentFrame);
                AddToNextFringe(ConjElim(currentFrame), currentFrame);
                AddToNextFringe(DisjIntro(currentFrame), currentFrame);
                //AddToNextFringe(DisjElim(currentFrame), currentFrame);
                //AddToNextFringe(NegIntro(currentFrame), currentFrame);
                AddToNextFringe(NegElim(currentFrame), currentFrame);
                //AddToNextFringe(ImplIntro(currentFrame), currentFrame);
                AddToNextFringe(ImplElim(currentFrame), currentFrame);
                AddToNextFringe(IffIntro(currentFrame), currentFrame);
                AddToNextFringe(IffElim(currentFrame), currentFrame);
            }
        }

        public static void DeriveSubgoals()
        {
            while (GoalFringe.Any())
            {
                Goal currentGoal = GoalFringe.Dequeue();
                /*List<Goal> derivedSubgoals = currentGoal.DeriveSubgoals();

                if (derivedSubgoals != null)
                {
                    /*foreach (Goal subgoal in currentGoal.DeriveSubgoals())
                    {
                        NextGoalFringe.Enqueue(subgoal);
                        currentGoal.subGoals.Add(subgoal);
                    }
                }*/
            }
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

        public static Frame ApplyNegIntro(Frame frame, IFormula negation, IFormula contra1, IFormula contra2)
        {
            Tuple<Interval, Interval, Interval> negInt = FindInt(frame, negation, contra1, contra2);
            if (negInt.Item1 == null || negInt.Item2 == null || negInt.Item3 == null)
            {
                throw new Exception("Tried applying NegIntro on non-existing assumption or non-existing contradictory formulas.");
            }
            List<IFormula> forms = new List<IFormula> { negation, contra1, contra2 };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            return newFrame.Item1.AddForm(new Negation(negation), newFrame.Item1.Last.Item2.parent, new Annotation(newFrame.Item2, Rules.NEG, true));
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
            return newFrame.Item1.AddForm(neg2.Formula, new Annotation(newFrame.Item2, Rules.NEG, false));
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
                return newFrame.Item1.AddForm(disj, new Annotation(newFrame.Item2, Rules.OR, true));
            }
            else
            {
                List<IFormula> forms = new List<IFormula> { disj.Right };
                Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                return newFrame.Item1.AddForm(disj, new Annotation(newFrame.Item2, Rules.OR, true));
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
                    lines.Add(i);
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
                if (frame.frame[i].Item2 != left)
                {
                    throw new Exception("Desired goal cannot be found in left disjunct interval.");
                }
                if (frame.frame[i].Item1.Equals(goal))
                {
                    lines.Add(i);
                    break;
                }
            }
            for (int i = rightInt; i < frame.frame.Count; i++)
            {
                if (frame.frame[i].Item2 != right)
                {
                    throw new Exception("Desired goal cannot be found in right disjunct interval.");
                }
                if (frame.frame[i].Item1.Equals(goal))
                {
                    lines.Add(i);
                    break;
                }
            }

            frame = frame.AddForm(goal, frame.Last.Item2.parent, new Annotation(lines, Rules.OR, false));

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
            return newFrame.Item1.AddForm(new Implication(ante, cons), newFrame.Item1.Last.Item2.parent, new Annotation(newFrame.Item2, Rules.IMP, true));
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
                    return newFrame.Item1.AddForm(impl.Consequent, new Annotation(newFrame.Item2, Rules.IMP, false));
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
            return newFrame.Item1.AddForm(new Iff(left, right), new Annotation(newFrame.Item2, Rules.BI, true));
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
                return newFrame.Item1.AddForm(new Implication(iff.Left, iff.Right), new Annotation(newFrame.Item2, Rules.BI, false));
            }
            else
            {
                return newFrame.Item1.AddForm(new Implication(iff.Right, iff.Left), new Annotation(newFrame.Item2, Rules.BI, false));
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
            return newFrame.Item1.AddForm(new Conjunction(left, right), new Annotation(newFrame.Item2, Rules.AND, true));
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
                return newFrame.Item1.AddForm(conj.Left, new Annotation(newFrame.Item2, Rules.AND, false));
            }
            else
            {
                return newFrame.Item1.AddForm(conj.Right, new Annotation(newFrame.Item2, Rules.AND, false));
            }
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
                        List<int> lines = new List<int>
                        {
                            frame.frame.IndexOf(line) + 1
                        };
                        output.Add(frame.AddForm(disj, new Annotation(lines, Rules.OR, true)));
                    }
                    Disjunction disjMirror = new Disjunction(form2, line.Item1);
                    if (!facts.Contains(disjMirror) && SubForms.Contains(disjMirror))
                    {
                        List<int> lines = new List<int>
                        {
                            frame.frame.IndexOf(line) + 1
                        };
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
                            if (line.Item2.facts.Count == 1 && forms.Count == 1 && line.Item2.parent != null)
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

        private static void AddToNextFringe(List<Frame> frameList, Frame parentFrame)
        {
            foreach (Frame inference in frameList)
            {
                /*if (!MatchFacts(inference))
                {
                    NextFringe.Enqueue(inference);
                    parentFrame.DerivedFrames.Add(inference);
                    ClosedList.Add(inference.ReturnFacts());
                }*/
            }
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
            if (goal.goal.Equals(form))
            {
                return goal.Complete();
            }
            return false;
        }

        public static Frame MakeFrame(Frame output, List<Derivation> derivs, Goal goal)
        {
            Stack<Goal> goalStack = new Stack<Goal>();
            Stack<Derivation> derivStack = new Stack<Derivation>();
            Queue<Goal> buildOrder = new Queue<Goal>();
            Stack<Derivation> buildOrderDeriv = new Stack<Derivation>();
            goalStack.Push(goal);
            bool noCompleteChildren = true;

            while (goalStack.Any())
            {
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
                    noCompleteChildren = false;
                    foreach (Derivation deriv in derivs)
                    {
                        if (deriv.Form.Equals(current.goal))
                        {
                            if (!buildOrderDeriv.Contains(deriv))
                            {
                                derivStack.Push(deriv);
                            }
                            break;
                        }
                    }
                }
                buildOrder.Enqueue(current);
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
                    if (line.Item1.Equals(goal.goal))
                    {
                        lines.Add(output.frame.IndexOf(line) + 1);
                        break;
                    }
                }
                return output.AddForm(goal.goal, new Annotation(lines, Rules.REI, false));
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
                        //TODO: wordt leuk dit
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
                        output = output.AddAss(orig.Left);
                        output = MakeFrame(output, leftOR, new Goal(current.Form));

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
                        output = output.AddAss(orig.Right, output.Last.Item2.parent);
                        output = MakeFrame(output, rightOR, new Goal(current.Form));
                        output = ApplyDisjElim(output, orig, current.Form);
                        break;
                }
            }

            while (buildOrder.Any())
            {
                Goal current = buildOrder.Dequeue();
                if (current.goal is Disjunction)
                {
                    output = ApplyDisjIntro(output, goal.parent.goal as Disjunction);
                }
                else if (current.goal is Conjunction)
                {
                    Conjunction conj = goal.parent.goal as Conjunction;
                    output = ApplyConjIntro(output, conj.Left, conj.Right);
                }
                else if (current.goal is Implication)
                {
                    Implication impl = goal.parent.goal as Implication;
                    output = output.AddAss(impl.Antecedent);
                    output = ApplyImplIntro(output, impl.Antecedent, impl.Consequent);
                }
                else if (current.goal is Iff)
                {
                    Iff iff = goal.parent.goal as Iff;
                    output = ApplyIffIntro(output, new Implication(iff.Left, iff.Right), new Implication(iff.Right, iff.Left));
                }

                goal = goal.parent;
            }

            return output;
        }
    }
}
