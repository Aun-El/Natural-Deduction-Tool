using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Natural_Deduction_Tool
{
    public partial class Form1 : Form
    {
        bool premiseTxtActive;
        public Form1()
        {
            premiseTxtActive = true;
            InitializeComponent();
            ActiveControl = premiseTxt;
            NegButton.Text = "\u00AC";
            DisjButton.Text = "\u2228";
            ConjButton.Text = "\u2227";
            ImplButton.Text = "\u2192";
            IffButton.Text = "\u2194";
        }

        private void proveBut_Click(object sender, EventArgs e)
        {
            List<IFormula> premises = SetupPremises();
            IFormula conclusion = SetupConclusion();
            if (conclusion != null)
            {
                if (SATCheck(premises, conclusion))
                {
                    proofTxt.Text = Searcher.Prove(premises, conclusion);
                }
            }
        }

        private void selfProveBut_Click(object sender, EventArgs e)
        {
            List<IFormula> premises = SetupPremises();
            IFormula conclusion = SetupConclusion();
            if (conclusion != null)
            {
                if (SATCheck(premises, conclusion))
                {
                    premiseTxt.Enabled = false;
                    premiseTxt.TabStop = false;
                    conclTxt.Enabled = false;
                    conclTxt.TabStop = false;
                    Writer.Initialize(premises, this);
                    Writer.Write();
                }
            }
        }

        private List<IFormula> SetupPremises()
        {
            List<IFormula> premises = new List<IFormula>();
            if (premiseTxt.Text != "")
            {
                foreach (string s in premiseTxt.Text.Split(','))
                {
                    try { premises.Add(FormParser.ParseFormula(s.Trim())); }
                    catch { proofTxt.Text = "Please enter well-formed premises."; }
                }
            }
            return premises;
        }

        private IFormula SetupConclusion()
        {
            IFormula conclusion = null;
            if (conclTxt.Text.Trim() != "")
            {
                try
                {
                    conclusion = FormParser.ParseFormula(conclTxt.Text.Trim());
                }
                catch { proofTxt.Text = "Please enter a well-formed conclusion."; }
            }
            else
            {
                proofTxt.Text = "Please enter a conclusion.";
            }
            return conclusion;
        }

        private bool SATCheck(List<IFormula> premises, IFormula conclusion)
        {
            if (premises.Count == 1)
            {
                Valuation val = SATSolver.Satisfiable(premises[0]);
                Conjunction conjoined = new Conjunction(premises[0], new Negation(conclusion));
                val = SATSolver.Satisfiable(conjoined);
                if (val != null)
                {
                    proofTxt.Text = "This conclusion does not follow from these premises.";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if (premises.Count > 1)
            {
                List<IFormula> conjoining = new List<IFormula>();
                foreach (IFormula form in premises)
                {
                    conjoining.Add(form);
                }

                //Check if the premises are UNSAT. If so, derive the conclusion from the falsum.
                Conjunction conjoined = Conjunction.Conjoin(conjoining);
                Valuation val = SATSolver.Satisfiable(conjoined);

                conjoined = new Conjunction(conjoined, new Negation(conclusion));
                val = SATSolver.Satisfiable(conjoined);
                if (val != null)
                {
                    proofTxt.Text = "This conclusion does not follow from these premises.";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                Negation negConcl = new Negation(conclusion);
                Valuation val = SATSolver.Satisfiable(negConcl);
                if (val != null)
                {
                    proofTxt.Text = "This conclusion is not a tautology.";
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private void PremiseTxt_GotFocus(object sender, EventArgs e)
        {
            premiseTxtActive = true;
        }

        private void ConclTxt_GotFocus(object sender, EventArgs e)
        {
            premiseTxtActive = false;
        }

        private void NegButton_Click(object sender, EventArgs e)
        {
            if (premiseTxtActive)
            {
                premiseTxt.Text += "-";
                premiseTxt.Focus();
                premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
            }
            else
            {
                conclTxt.Text += "-";
                conclTxt.Focus();
                conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
            }
        }

        private void DisjButton_Click(object sender, EventArgs e)
        {
            if (premiseTxtActive)
            {
                premiseTxt.Text += @"\/";
                premiseTxt.Focus();
                premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
            }
            else
            {
                conclTxt.Text += @"\/";
                conclTxt.Focus();
                conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
            }
        }

        private void ConjButton_Click(object sender, EventArgs e)
        {
            if (premiseTxtActive)
            {
                premiseTxt.Text += @"/\";
                premiseTxt.Focus();
                premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
            }
            else
            {
                conclTxt.Text += @"/\";
                conclTxt.Focus();
                conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
            }
        }

        private void ImplButton_Click(object sender, EventArgs e)
        {
            if (premiseTxtActive)
            {
                premiseTxt.Text += "->";
                premiseTxt.Focus();
                premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
            }
            else
            {
                conclTxt.Text += "->";
                conclTxt.Focus();
                conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
            }
        }

        private void IffButton_Click(object sender, EventArgs e)
        {
            if (premiseTxtActive)
            {
                premiseTxt.Text += "<->";
                premiseTxt.Focus();
                premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
            }
            else
            {
                conclTxt.Text += "<->";
                conclTxt.Focus();
                conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
            }
        }
    }
}
