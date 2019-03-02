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
                    premises.Add(FormParser.ParseFormula(s.Trim()));
                }
            }
            IFormula conclusion = FormParser.ParseFormula(conclTxt.Text.Trim());

            proofTxt.Text = Searcher.Proof(premises, conclusion) + $"\r\n\t{conclusion}\tconclusion";
        }
    }
}
