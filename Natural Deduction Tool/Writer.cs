using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Natural_Deduction_Tool
{
    public static class Writer
    {
        static Frame Frame;
        static Form1 Form;
        static int counter;
        static ReadOnlyTextBox numberLine;
        static TextBox formTxt;
        static ComboBox derivRuleBox;
        static List<String> derivRules;
        static TextBox numberTxt;

        public static void Write()
        {
            
            NewLine(counter);

            //Also, allow the user to remove the last line of the frame (but not the premises)

            //If everything is filled out, check if the derivation is correct
            //If the derivation is correct, write it to the frame and check if it is the conclusion
            //If it is the conclusion, done. Else, go to the next line
        }

        public static void Initialize(List<IFormula> premises, Form1 form)
        {
            Frame = new Frame(premises);
            Form = form;
            Form.proofTxt.Text = Frame.ToString();
            counter = premises.Count;
            derivRules = new List<string> { "Assume", "- intro", "- elim", @"/\ intro", @"/\ elim", @"\/ intro", @"\/ elim", "-> intro", "-> elim", "<-> intro", "<-> elim", "Reiterate" };
        }

        private static void NewLine(int line)
        {
            numberLine = new ReadOnlyTextBox
            {
                Text = (line + 1) + "\t",
                Font = Form.proofTxt.Font,
                Width = 77,
                Location = new Point(Form.proofTxt.Location.X, Form.proofTxt.Location.Y + line * 25),
                TabStop = false
            };
            Form.Controls.Add(numberLine);
            numberLine.BringToFront();

            //Open a new textbox to let the user write a formula
            formTxt = new TextBox
            {
                Font = Form.proofTxt.Font,
                Width = 150,
                Location = new Point(Form.proofTxt.Location.X + 78, Form.proofTxt.Location.Y + line * 25)
            };
            Form.Controls.Add(formTxt);
            formTxt.BringToFront();

            //Open a new scrollbox to let the user choose a derivation rule
            derivRuleBox = new ComboBox
            {
                Font = new Font(Form.proofTxt.Font.FontFamily, 11),
                Location = new Point(Form.proofTxt.Location.X + 78 + 151, Form.proofTxt.Location.Y + line * 25)
            };
            derivRuleBox.DataSource = derivRules;
            derivRuleBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            derivRuleBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            Form.Controls.Add(derivRuleBox);
            derivRuleBox.BringToFront();
            
            //Open a new textbox to let the user enter the annotation (the numbers of the lines the pertinent formulae are on)
            numberTxt = new TextBox()
            {
                Font = Form.proofTxt.Font,
                Width = 50,
                Location = new Point(Form.proofTxt.Location.X + 78+ 151 + derivRuleBox.Size.Width + 1, Form.proofTxt.Location.Y + line * 25)
            };
            Form.Controls.Add(numberTxt);
            numberTxt.BringToFront();

            Button addButton = new Button()
            {
                Font = Form.proofTxt.Font,
                Text = "Add",
                Width = 50,
                Height = 25,
                Location = new Point(Form.proofTxt.Location.X + 78 + 151 + derivRuleBox.Size.Width + 51, Form.proofTxt.Location.Y + line * 25)
            };
            addButton.Click += new EventHandler(AddNewLine);
            Form.Controls.Add(addButton);
            addButton.BringToFront();

            formTxt.Focus();
        }

        private static void AddNewLine(object sender, EventArgs e)
        {
            //Check if the derivation on the new line is valid
            IFormula formToAdd = null;
            if (formTxt.Text.Trim() != "")
            {
                try
                {
                    formToAdd = FormParser.ParseFormula(formTxt.Text.Trim());
                }
                catch { formTxt.Text = "Please enter a well-formed formula."; }
            }
            else
            {
                formTxt.Text = "Please enter a formula.";
            }
            //If so, add it to the frame
            //Check if it is the conclusion
            //If so, done. Else, add another new line
        }
    }
}
