using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Natural_Deduction_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
    }
}
