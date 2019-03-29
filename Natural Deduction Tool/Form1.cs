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
            List<IFormula> premises = new List<IFormula>();
            if (premiseTxt.Text != "")
            {
                foreach (string s in premiseTxt.Text.Split(','))
                {
                    try { premises.Add(FormParser.ParseFormula(s.Trim())); }
                    catch { proofTxt.Text = "Please enter valid premises."; }
                }
            }
            if (conclTxt.Text.Trim() != "")
            {
                bool validConcl = false;
                IFormula conclusion = null;
                try
                {
                    conclusion = FormParser.ParseFormula(conclTxt.Text.Trim());
                    validConcl = true;
                }
                catch { proofTxt.Text = "Please enter a valid conclusion."; }

                if (validConcl)
                {
                    if(premises.Count == 1)
                    {
                        Valuation val = SATSolver.Satisfiable(premises[0]);
                        if (val == null)
                        {
                            //Contradiction in premises
                            proofTxt.Text = "Contradiction in the premises.";
                        }
                        else
                        {

                            Conjunction conjoined = new Conjunction(premises[0], new Negation(conclusion));
                            val = SATSolver.Satisfiable(conjoined);
                            if (val != null)
                            {
                                proofTxt.Text = "This conclusion does not follow from these premises.";
                            }
                            else
                            {
                                proofTxt.Text = Searcher.Prove(premises, conclusion);
                            }
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

                        if (val == null)
                        {
                            //Contradiction in premises
                            proofTxt.Text = "Contradiction in the premises.";
                        }
                        else
                        {

                            conjoined = new Conjunction(conjoined, new Negation(conclusion));
                            val = SATSolver.Satisfiable(conjoined);
                            if (val != null)
                            {
                                proofTxt.Text = "This conclusion does not follow from these premises.";
                            }
                            else
                            {
                                proofTxt.Text = Searcher.Prove(premises, conclusion);
                            }
                        }
                    }
                    else
                    {
                        Negation negConcl = new Negation(conclusion);
                        Valuation val = SATSolver.Satisfiable(negConcl);
                        if (val != null)
                        {
                            proofTxt.Text = "This conclusion is not a tautology.";
                        }
                        else
                        {
                            proofTxt.Text = Searcher.Prove(premises, conclusion);
                        }
                    }
                }
            }
            else
            {
                proofTxt.Text = "Please enter a conclusion.";
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

        private void ProofTxt_GotFocus(object sender, EventArgs e)
        {

        }
    }
}
