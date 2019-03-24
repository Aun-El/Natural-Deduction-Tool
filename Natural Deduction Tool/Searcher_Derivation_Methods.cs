using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natural_Deduction_Tool
{
    public partial class Searcher
    {
        private static List<Derivation> DeriveWithDisjElim(Disjunction disj, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs)
        {
            derivLength++;
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
            derivLength--;
            return output;
        }

        private static void DeriveWithNegElim(Derivation current, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, PriorityQueue derivQueue)
        {
            Negation neg = current.Form as Negation;
            if (neg.Formula is Negation)
            {
                Negation negForm = neg.Formula as Negation;
                List<IFormula> orig = new List<IFormula>() { neg };
                List<Derivation> derivPar = new List<Derivation>() { current };
                Derivation newDeriv = new Derivation(negForm.Formula, new Origin(orig, Rules.NEG, derivPar));
                derivQueue.Enqueue(newDeriv, derivs);
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
        private static void DeriveWithConjElim(Derivation current, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, PriorityQueue derivQueue)
        {
            Conjunction conj = current.Form as Conjunction;
            List<IFormula> orig = new List<IFormula>() { conj };
            List<Derivation> derivPar = new List<Derivation>() { current };
            Derivation newLeftDeriv = new Derivation(conj.Left, new Origin(orig, Rules.AND, derivPar));
            derivQueue.Enqueue(newLeftDeriv, derivs);
            Derivation newRightDeriv = new Derivation(conj.Right, new Origin(orig, Rules.AND, derivPar));
            derivQueue.Enqueue(newRightDeriv, derivs);

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
        private static void DeriveWithIffElim(Derivation current, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, PriorityQueue derivQueue)
        {
            Iff iff = current.Form as Iff;
            List<IFormula> orig = new List<IFormula>() { iff };
            Implication left = new Implication(iff.Left, iff.Right);
            Implication right = new Implication(iff.Right, iff.Left);
            List<Derivation> derivPar = new List<Derivation>() { current };
            Derivation newLeftDeriv = new Derivation(left, new Origin(orig, Rules.BI, derivPar));
            Derivation newRightDeriv = new Derivation(right, new Origin(orig, Rules.BI, derivPar));
            derivQueue.Enqueue(newLeftDeriv, derivs);
            derivQueue.Enqueue(newRightDeriv, derivs);
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
        private static void DeriveWithImplElim(Derivation current, HashSet<IFormula> facts, List<Implication> impls, List<Disjunction> disjs, List<Derivation> derivs, PriorityQueue derivQueue)
        {
            Implication impl = current.Form as Implication;
            if (facts.Contains(impl.Antecedent))
            {
                List<IFormula> orig = new List<IFormula>() { impl, impl.Antecedent };
                List<Derivation> derivPar = new List<Derivation>() { current };
                bool anteFound = false;
                foreach (Derivation deriv in derivs)
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
                derivQueue.Enqueue(newDeriv, derivs);
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
    }
}
