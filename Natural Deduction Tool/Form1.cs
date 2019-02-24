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

        private void equalsBut_Click(object sender, EventArgs e)
        {
            IFormula form1 = FormParser.ParseFormula(formTxt1.Text.Trim());
            IFormula form2 = FormParser.ParseFormula(formTxt2.Text.Trim());
            if (form1.Equals(form2))
            {
                outputTxt.Text = "true";
            }
            else
            {
                outputTxt.Text = "false";
            }
        }

        private void proveBut_Click(object sender, EventArgs e)
        {
            List<IFormula> premises = new List<IFormula>();
            if (premiseTxt.Text != "")
            {
                foreach (string s in premiseTxt.Text.Split(','))
                {
                    premises.Add(FormParser.ParseFormula(s.Trim()));
                }
            }
            IFormula conclusion = FormParser.ParseFormula(conclTxt.Text.Trim());

            proofTxt.Text = Searcher.Proof(premises, conclusion) + $"\r\n\t{conclusion}\tconclusion";
        }
    }
}
