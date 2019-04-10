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
        TxtActive ActiveTxt;
        public bool selfProving;

        public Form1()
        {
            selfProving = false;
            ActiveTxt = TxtActive.premiseTxt;
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
            if (selfProving)
            {
                Size = new Size(651, 586);
                Writer.RemoveControls();
                premiseTxt.Enabled = true;
                premiseTxt.TabStop = true;
                conclTxt.Enabled = true;
                conclTxt.TabStop = true;
                proveBut.TabStop = true;
                selfProveBut.TabStop = true;
            }
            selfProving = false;
            if (Writer.errorTxt != null && Writer.errorTxt.Visible)
            {
                Writer.errorTxt.Visible = false;
            }
            List<IFormula> premises = SetupPremises();
            IFormula conclusion = SetupConclusion();
            if (conclusion != null)
            {
                if (SATCheck(premises, conclusion))
                {
                    proofTxt.Text = Searcher.Prove(premises, conclusion, false);
                }
            }
        }

        private void selfProveBut_Click(object sender, EventArgs e)
        {
            if (selfProving)
            {
                Size = new Size(651, 586);
                Writer.RemoveControls();
                selfProving = false;
                premiseTxt.Enabled = true;
                premiseTxt.TabStop = true;
                conclTxt.Enabled = true;
                conclTxt.TabStop = true;
                proveBut.TabStop = true;
                selfProveBut.TabStop = true;
                selfProveBut.Text = "&Let me prove";
                proofTxt.Clear();
            }
            else
            {
                selfProveBut.Text = "&Stop proving";
                selfProving = true;
                if (Writer.errorTxt != null && Writer.errorTxt.Visible)
                {
                    Writer.errorTxt.Visible = false;
                }
                List<IFormula> premises = SetupPremises();
                IFormula conclusion = SetupConclusion();
                if (conclusion != null)
                {
                    if (SATCheck(premises, conclusion))
                    {
                        string output = Searcher.Prove(premises, conclusion, true);
                        Goal graph = null;
                        if (output != "Graph complete")
                        {
                            MessageBox.Show("I could not make a proof graph for this, so the hint function is disabled. You can still make the proof yourself.");
                        }
                        else
                        {
                            graph = Searcher.goal;
                        }
                        Size = new Size(800, 640);
                        premiseTxt.Enabled = false;
                        premiseTxt.TabStop = false;
                        conclTxt.Enabled = false;
                        conclTxt.TabStop = false;
                        proveBut.TabStop = false;
                        selfProveBut.TabStop = false;
                        Writer.Initialize(premises, conclusion, this, graph != null);
                        Writer.Write();
                    }
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
            else
            {
                premises.Add(new PropVar("\u22a4"));
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
            ActiveTxt = TxtActive.premiseTxt;
        }

        private void ConclTxt_GotFocus(object sender, EventArgs e)
        {
            ActiveTxt = TxtActive.conclTxt;
        }

        public void FormTxt_GotFocus(object sender, EventArgs e)
        {
            ActiveTxt = TxtActive.formTxt;
        }

        private void NegButton_Click(object sender, EventArgs e)
        {
            switch (ActiveTxt)
            {
                case TxtActive.premiseTxt:
                    premiseTxt.Text += "-";
                    premiseTxt.Focus();
                    premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
                    break;
                case TxtActive.conclTxt:
                    conclTxt.Text += "-";
                    conclTxt.Focus();
                    conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
                    break;
                case TxtActive.formTxt:
                    if (Writer.formTxt != null)
                    {
                        Writer.formTxt.Text += "-";
                        Writer.formTxt.Focus();
                        Writer.formTxt.Select(Writer.formTxt.Text.Length, Writer.formTxt.Text.Length);
                    }
                    break;
            }
        }

        private void DisjButton_Click(object sender, EventArgs e)
        {
            switch (ActiveTxt)
            {
                case TxtActive.premiseTxt:
                    premiseTxt.Text += @"\/";
                    premiseTxt.Focus();
                    premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
                    break;
                case TxtActive.conclTxt:
                    conclTxt.Text += @"\/";
                    conclTxt.Focus();
                    conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
                    break;
                case TxtActive.formTxt:
                    if (Writer.formTxt != null)
                    {
                        Writer.formTxt.Text += @"\/";
                        Writer.formTxt.Focus();
                        Writer.formTxt.Select(Writer.formTxt.Text.Length, Writer.formTxt.Text.Length);
                    }
                    break;
            }
        }

        private void ConjButton_Click(object sender, EventArgs e)
        {
            switch (ActiveTxt)
            {
                case TxtActive.premiseTxt:
                    premiseTxt.Text += @"/\";
                    premiseTxt.Focus();
                    premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
                    break;
                case TxtActive.conclTxt:
                    conclTxt.Text += @"/\";
                    conclTxt.Focus();
                    conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
                    break;
                case TxtActive.formTxt:
                    if (Writer.formTxt != null)
                    {
                        Writer.formTxt.Text += @"/\";
                        Writer.formTxt.Focus();
                        Writer.formTxt.Select(Writer.formTxt.Text.Length, Writer.formTxt.Text.Length);
                    }
                    break;
            }
        }

        private void ImplButton_Click(object sender, EventArgs e)
        {
            switch (ActiveTxt)
            {
                case TxtActive.premiseTxt:
                    premiseTxt.Text += "->";
                    premiseTxt.Focus();
                    premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
                    break;
                case TxtActive.conclTxt:
                    conclTxt.Text += "->";
                    conclTxt.Focus();
                    conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
                    break;
                case TxtActive.formTxt:
                    if (Writer.formTxt != null)
                    {
                        Writer.formTxt.Text += "->";
                        Writer.formTxt.Focus();
                        Writer.formTxt.Select(Writer.formTxt.Text.Length, Writer.formTxt.Text.Length);
                    }
                    break;
            }
        }

        private void IffButton_Click(object sender, EventArgs e)
        {
            switch (ActiveTxt)
            {
                case TxtActive.premiseTxt:
                    premiseTxt.Text += "<->";
                    premiseTxt.Focus();
                    premiseTxt.Select(premiseTxt.Text.Length, premiseTxt.Text.Length);
                    break;
                case TxtActive.conclTxt:
                    conclTxt.Text += "<->";
                    conclTxt.Focus();
                    conclTxt.Select(conclTxt.Text.Length, conclTxt.Text.Length);
                    break;
                case TxtActive.formTxt:
                    if (Writer.formTxt != null)
                    {
                        Writer.formTxt.Text += "<->";
                        Writer.formTxt.Focus();
                        Writer.formTxt.Select(Writer.formTxt.Text.Length, Writer.formTxt.Text.Length);
                    }
                    break;
            }
        }
    }

    enum TxtActive
    {
        conclTxt,
        premiseTxt,
        formTxt
    }
}
