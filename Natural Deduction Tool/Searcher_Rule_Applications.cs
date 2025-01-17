﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public partial class Searcher
    {
        public static Frame ApplyNegIntro(Frame frame, IFormula negation, IFormula contra1, Negation contra2)
        {
            if (!contra2.Formula.Equals(contra1))
            {
                throw new Exception("Tried applying NegIntro on non-conflicting proposition.");
            }
            if (!frame.frame[frame.Last.Item2.startLine].Item1.Equals(negation))
            {
                throw new Exception("Tried applying NegIntro on non-existing assumption.");
            }
            Tuple<Interval, Interval> negInt = FindInt(frame, contra1, contra2);
            if (negInt.Item1 == null || negInt.Item2 == null)
            {
                throw new Exception("Tried applying NegIntro on non-existing contradictory formulas.");
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
                throw new Exception("Tried applying ImplIntro on non-existing antecedent/consequent.");
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
                if (line.Item1.Equals(impl.Antecedent) && (line.Item2.ThisOrParent(implInt) || implInt.ThisOrParent(line.Item2)))
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
            newFrame.Item1.AddForm(new Iff(left.Antecedent, right.Antecedent), new Annotation(newFrame.Item2, Rules.BI, true));
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

        public static Frame ApplyMorg(Frame frame, Negation neg)
        {
            IFormula output = null;
            if(neg.Formula is Disjunction)
            {
                Disjunction disj = neg.Formula as Disjunction;
                output = new Conjunction(new Negation(disj.Left), new Negation(disj.Right));
            }
            else if (neg.Formula is Conjunction)
            {
                Conjunction conj = neg.Formula as Conjunction;
                output = new Disjunction(new Negation(conj.Left), new Negation(conj.Right));
            }
            else
            {
                throw new Exception("Tried applying De Morgan on non-negation.");
            }
            Interval negInt = FindInt(frame, neg);
            if (negInt == null)
            {
                throw new Exception("Tried applying De Morgan on non-existing negation.");
            }
            List<IFormula> forms = new List<IFormula> { neg };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            newFrame.Item1.AddForm(output, new Annotation(newFrame.Item2, Rules.MORG, true));
            return newFrame.Item1;
        }

        public static Frame ApplyImplToDisj(Frame frame, Implication impl)
        {
            Disjunction disj = new Disjunction(new Negation(impl.Antecedent), impl.Consequent);
            Interval implInt = FindInt(frame, impl);
            if (implInt == null)
            {
                throw new Exception("Tried applying ImplToDisj on non-existing implication.");
            }
            List<IFormula> forms = new List<IFormula> { impl };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            newFrame.Item1.AddForm(disj, new Annotation(newFrame.Item2, Rules.ImplToDisj, true));
            return newFrame.Item1;
        }

        public static Frame ApplyDisjToImpl(Frame frame, Disjunction disj)
        {
            Implication impl = null;
            if(disj.Left is Negation)
            {
                Negation neg = disj.Left as Negation;
                impl = new Implication(neg.Formula, disj.Right);
            }
            else
            {
                impl = new Implication(new Negation(disj.Left), disj.Right);
            }
            Interval disjInt = FindInt(frame, disj);
            if (disjInt == null)
            {
                throw new Exception("Tried applying DisjToImpl on non-existing disjunction.");
            }
            List<IFormula> forms = new List<IFormula> { disj };
            Tuple<Frame, List<int>> newFrame = REI(frame, forms);
            newFrame.Item1.AddForm(impl, new Annotation(newFrame.Item2, Rules.DisjToImpl, true));
            return newFrame.Item1;
        }

        public static Frame ApplyNegImplToConj(Frame frame, Negation neg)
        {
            if (neg.Formula is Implication)
            {
                Implication impl = neg.Formula as Implication;
                Conjunction conj = null;
                if (impl.Consequent is Negation)
                {
                    Negation negCons = impl.Consequent as Negation;
                    conj = new Conjunction(impl.Antecedent, negCons.Formula);
                }
                else
                {
                    conj = new Conjunction(impl.Antecedent, new Negation(impl.Consequent));
                }
                Interval negInt = FindInt(frame, neg);
                if (negInt == null)
                {
                    throw new Exception("Tried applying NegImplToConj on non-existing negation.");
                }
                List<IFormula> forms = new List<IFormula> { neg };
                Tuple<Frame, List<int>> newFrame = REI(frame, forms);
                newFrame.Item1.AddForm(conj, new Annotation(newFrame.Item2, Rules.NegImplToConj, true));
                return newFrame.Item1;
            }
            else
            {
                throw new Exception("Tried applying NegImplToConj on non-neg-implication.");
            }
        }
    }
}
