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
        static ReadOnlyTextBox numberLine;
        public static ReadOnlyTextBox errorTxt;
        public static TextBox formTxt;
        static TextBox closeIntTxt;
        static TextBox numberTxt;
        static ComboBox derivRuleBox;
        static List<String> derivRules;
        static Button addButton;
        static Button cancelButton;
        static Button hintButton;
        static Label closeIntLbl;

        static IFormula conclusion;
        static bool noError;
        static bool noHints;

        public static void Write()
        {

            NewLine();

            //Also, allow the user to remove the last line of the frame (but not the premises)

            //If everything is filled out, check if the derivation is correct
            //If the derivation is correct, write it to the frame and check if it is the conclusion
            //If it is the conclusion, done. Else, go to the next line
        }

        public static void Initialize(List<IFormula> premises, IFormula concl, Form1 form, bool hints)
        {
            Frame = new Frame(premises);
            Form = form;
            noHints = !hints;
            Form.proofTxt.Text = Frame.ToString();
            conclusion = concl;
            errorTxt = new ReadOnlyTextBox();
            errorTxt.Font = Form.proofTxt.Font;
            errorTxt.Location = new Point(Form.proofTxt.Location.X, Form.proofTxt.Location.Y + Form.proofTxt.Size.Height + 5);
            errorTxt.Visible = false;
            Form.Controls.Add(errorTxt);
            errorTxt.BringToFront();
            derivRules = new List<string> { "Assumption", "- intro", "- elim", @"/\ intro", @"/\ elim", @"\/ intro", @"\/ elim", "-> intro", "-> elim", "<-> intro", "<-> elim", "Reiterate" };
        }

        private static void NewLine()
        {
            int line = Form.proofTxt.Lines.Count();
            line -= String.IsNullOrWhiteSpace(Form.proofTxt.Lines.Last()) ? 1 : 0;
            int yPos = 0;
            Font font = Form.proofTxt.Font;

            //Keep updating yPos until line reaches 22
            //After that, just put the input controls below proofTxt
            if (line < 23)
            {
                yPos = Form.proofTxt.Location.Y + 6 + line * 20;
            }
            else
            {
                yPos = Form.proofTxt.Location.Y + Form.proofTxt.Size.Height + 5;
                errorTxt.Location = new Point(errorTxt.Location.X, Form.proofTxt.Location.Y + Form.proofTxt.Size.Height + 33);
            }

            numberLine = new ReadOnlyTextBox
            {
                Text = (Frame.frame.Count + 1) + "\t",
                Font = font,
                Width = 77,
                Location = new Point(Form.proofTxt.Location.X, yPos),
                TabStop = false
            };
            Form.Controls.Add(numberLine);
            numberLine.BringToFront();

            //Open a new textbox to let the user write a formula
            formTxt = new TextBox
            {
                Font = font,
                Text = "Enter formula",
                Width = 150,
                Location = new Point(Form.proofTxt.Location.X + 78, yPos)
            };
            Form.Controls.Add(formTxt);
            formTxt.GotFocus += new EventHandler(Form.FormTxt_GotFocus);
            formTxt.BringToFront();

            //Open a new scrollbox to let the user choose a derivation rule
            derivRuleBox = new ComboBox
            {
                Font = new Font(font.FontFamily, 11),
                Location = new Point(Form.proofTxt.Location.X + 78 + 151, yPos)
            };
            derivRuleBox.DataSource = derivRules;
            derivRuleBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            derivRuleBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            Form.Controls.Add(derivRuleBox);
            derivRuleBox.BringToFront();
            derivRuleBox.Text = "Enter rule";

            //Open a new textbox to let the user enter the annotation (the numbers of the lines the pertinent formulae are on)
            numberTxt = new TextBox()
            {
                Font = font,
                Text = "Lines",
                Width = 50,
                Location = new Point(Form.proofTxt.Location.X + 78 + 151 + derivRuleBox.Size.Width + 1, yPos)
            };
            Form.Controls.Add(numberTxt);
            numberTxt.BringToFront();

            addButton = new Button()
            {
                Font = font,
                Text = "&Add",
                Width = 50,
                Height = 25,
                Location = new Point(Form.proofTxt.Location.X + 78 + 151 + derivRuleBox.Size.Width + 51, yPos)
            };
            addButton.Click += new EventHandler(AddNewLine);
            Form.Controls.Add(addButton);
            addButton.BringToFront();

            cancelButton = new Button()
            {
                Font = font,
                Text = "&Cancel last line",
                Width = 100,
                Height = 25,
                Location = new Point(Form.proofTxt.Location.X + Form.proofTxt.Size.Width + 4, yPos)
            };
            cancelButton.Click += new EventHandler(CancelLastLine);
            Form.Controls.Add(cancelButton);
            cancelButton.BringToFront();

            closeIntTxt = new TextBox()
            {
                Font = font,
                Width = 25,
                Height = 25,
                Text = "0",
                Location = new Point(Form.proofTxt.Location.X + Form.proofTxt.Size.Width + 5, yPos + 25)
            };
            Form.Controls.Add(closeIntTxt);
            closeIntTxt.BringToFront();

            closeIntLbl = new Label()
            {
                Font = font,
                Width = 150,
                Height = 25,
                Text = "intervals to close",
                Location = new Point(Form.proofTxt.Location.X + Form.proofTxt.Size.Width + closeIntTxt.Size.Width + 5, yPos + 25)
            };
            Form.Controls.Add(closeIntLbl);
            closeIntLbl.BringToFront();

            hintButton = new Button()
            {
                Font = font,
                Width = 100,
                Height = 25,
                Text = "Hint",
                Location = new Point(Form.proofTxt.Location.X + Form.proofTxt.Size.Width + 4, yPos - 25)
            };
            Form.Controls.Add(hintButton);
            hintButton.Enabled = !noHints;
            hintButton.BringToFront();

            formTxt.Focus();
        }

        private static void AddNewLine(object sender, EventArgs e)
        {
            //Check if the derivation on the new line is valid
            IFormula formToAdd = null;
            noError = true;
            if (formTxt.Text.Trim() != "")
            {
                try
                {
                    formToAdd = FormParser.ParseFormula(formTxt.Text.Trim());
                }
                catch
                {
                    EnterError("Please enter a well-formed formula.");
                }
            }
            else
            {
                EnterError("Please enter a formula.");
            }
            if (formToAdd != null)
            {
                Interval intToAddOn = Frame.Last.Item2;
                if (closeIntTxt.Text != "")
                {
                    int intsToClose = 0;
                    try
                    {
                        intsToClose = int.Parse(closeIntTxt.Text.Trim());
                        if (intsToClose < 0)
                        {
                            EnterError("Only a positive number of hypothesis intervals can be closed.");
                        }
                        else
                        {
                            for (int i = 0; i < intsToClose; i++)
                            {
                                if (intToAddOn.parent != null)
                                {
                                    intToAddOn = intToAddOn.parent;
                                }
                                else
                                {
                                    EnterError("Cannot close more hypothesis intervals than currently exist.");
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        EnterError("Only a positive number of hypothesis intervals can be closed.");
                    }
                }
                if (noError)
                {
                    string rule = derivRuleBox.Text as string;
                    List<int> lines = new List<int>();
                    List<int> checkLines = new List<int>();
                    if (rule.ToLower() != "assumption")
                    {
                        string[] lineStrings = numberTxt.Text.Split(' ', ',');
                        try
                        {
                            for (int i = 0; i < lineStrings.Length; i++)
                            {
                                int newLine = int.Parse(lineStrings[i]);
                                if (newLine <= Frame.frame.Count)
                                {
                                    lines.Add(newLine);
                                    checkLines.Add(newLine - 1);
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                        }
                        catch
                        {
                            EnterError("Please enter only numbers of existing lines.");
                        }
                    }
                    switch (rule.ToLower())
                    {
                        case (@"/\ intro"):
                            {
                                if (lines.Count == 2)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn) && Frame.frame[checkLines[1]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (formToAdd is Conjunction)
                                        {
                                            Conjunction conj = formToAdd as Conjunction;
                                            if ((Frame.frame[checkLines[0]].Item1.Equals(conj.Left) && Frame.frame[checkLines[1]].Item1.Equals(conj.Right)) || (Frame.frame[checkLines[1]].Item1.Equals(conj.Left) && Frame.frame[checkLines[0]].Item1.Equals(conj.Right)))
                                            {
                                                Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.AND, true));
                                            }
                                            else
                                            {
                                                //Given conjuncts do no match with those of the conjunction
                                                //Or given form is not a conjunction
                                                EnterError("Please enter the correct conjuncts and correct conjunction.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("The new formula is not a conjunction.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the interval on these lines.");
                                    }
                                }
                                else
                                {
                                    EnterError("Please specify 2 lines to perform /\\ intro on.");
                                }
                                break;
                            }
                        case (@"/\ elim"):
                            {
                                if (lines.Count == 1)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (Frame.frame[checkLines[0]].Item1 is Conjunction)
                                        {
                                            Conjunction conj = Frame.frame[checkLines[0]].Item1 as Conjunction;
                                            if (formToAdd.Equals(conj.Left) || formToAdd.Equals(conj.Right))
                                            {
                                                Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.AND, false));
                                            }
                                            else
                                            {
                                                //Given conjuncts do no match with those of the conjunction
                                                //Or given form is not a conjunction
                                                EnterError("The formula on this line does not match either conjunct.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("This formula on this line is not a conjunction.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the intervals on this line.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter only 1 line for /\\ elim.");
                                }
                                break;
                            }
                        case (@"\/ intro"):
                            {
                                if (lines.Count == 1)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (formToAdd is Disjunction)
                                        {
                                            Disjunction disj = formToAdd as Disjunction;
                                            if (Frame.frame[checkLines[0]].Item1.Equals(disj.Left) || Frame.frame[checkLines[0]].Item1.Equals(disj.Right))
                                            {
                                                Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.OR, false));
                                            }
                                            else
                                            {
                                                //Given disjuncts do no match with those of the disjunction
                                                EnterError("The formula on this line does not match either disjunct.");
                                            }
                                        }
                                        else
                                        {
                                            //Given form is not a disjunction
                                            EnterError("The new formula is not a disjunction.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the interval on this line.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter only 1 line for \\/ elim.");
                                }
                                break;
                            }
                        case (@"\/ elim"):
                            {
                                if (lines.Count == 3)
                                {
                                    Interval int0 = Frame.frame[checkLines[0]].Item2;
                                    Interval int1 = Frame.frame[checkLines[1]].Item2;
                                    Interval int2 = Frame.frame[checkLines[2]].Item2;
                                    if (Frame.frame[checkLines[0]].Item1 is Disjunction && DisjElimIntervalCheck(int0, int1, int2))
                                    {
                                        if (int1.parent != null && int2.parent != null)
                                        {
                                            if (int1.parent == int2.parent)
                                            {
                                                Disjunction disj = Frame.frame[checkLines[0]].Item1 as Disjunction;
                                                int startline1 = int1.startLine;
                                                int startline2 = int2.startLine;
                                                if ((Frame.frame[startline1].Item1.Equals(disj.Left) && Frame.frame[startline2].Item1.Equals(disj.Right)) || (Frame.frame[startline1].Item1.Equals(disj.Right) && Frame.frame[startline2].Item1.Equals(disj.Left)))
                                                {
                                                    if (Frame.frame[checkLines[1]].Item1.Equals(Frame.frame[checkLines[2]].Item1))
                                                    {
                                                        if (Frame.frame[checkLines[1]].Item1.Equals(formToAdd))
                                                        {
                                                            if (intToAddOn == int1 || intToAddOn == int2)
                                                            {
                                                                Frame.AddForm(formToAdd, intToAddOn.parent, new Annotation(lines, Rules.OR, false));
                                                            }
                                                            else if (int0.ThisOrParent(intToAddOn) && intToAddOn == int1.parent)
                                                            {
                                                                Frame.AddForm(formToAdd, new Annotation(lines, Rules.OR, false));
                                                            }
                                                            else
                                                            {
                                                                EnterError("Cannot perform this \\/ elim on this hypothesis interval.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            EnterError("The formulae on the lines do not equal the new formula.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        EnterError("The formulae on the lines are not the same.");
                                                    }
                                                }
                                                else
                                                {
                                                    EnterError("The assumptions of the hypothesis intervals the lines refer to do not match the disjuncts of the disjunction.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("The hypothesis intervals of the lines should be on the same level.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("\\/ elim cannot be applied on lines that are in the first hypothesis interval.");
                                        }
                                    }
                                    else if (Frame.frame[checkLines[1]].Item1 is Disjunction && DisjElimIntervalCheck(int1, int0, int2))
                                    {
                                        if (int0.parent != null && int2.parent != null)
                                        {
                                            if (int0.parent == int2.parent)
                                            {
                                                Disjunction disj = Frame.frame[checkLines[1]].Item1 as Disjunction;
                                                int startline1 = int0.startLine;
                                                int startline2 = int2.startLine;
                                                if ((Frame.frame[startline1].Item1.Equals(disj.Left) && Frame.frame[startline2].Item1.Equals(disj.Right)) || (Frame.frame[startline1].Item1.Equals(disj.Right) && Frame.frame[startline2].Item1.Equals(disj.Left)))
                                                {
                                                    if (Frame.frame[checkLines[0]].Item1.Equals(Frame.frame[checkLines[2]].Item1))
                                                    {
                                                        if (Frame.frame[checkLines[0]].Item1.Equals(formToAdd))
                                                        {
                                                            if (intToAddOn == int0 || intToAddOn == int2)
                                                            {
                                                                Frame.AddForm(formToAdd, intToAddOn.parent, new Annotation(lines, Rules.OR, false));
                                                            }
                                                            else if (int1.ThisOrParent(intToAddOn) && intToAddOn == int0.parent)
                                                            {
                                                                Frame.AddForm(formToAdd, new Annotation(lines, Rules.OR, false));
                                                            }
                                                            else
                                                            {
                                                                EnterError("Cannot perform this \\/ elim on this hypothesis interval.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            EnterError("The formulae on the lines do not equal the new formula.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        EnterError("The formulae on the lines are not the same.");
                                                    }
                                                }
                                                else
                                                {
                                                    EnterError("The assumptions of the hypothesis intervals the lines refer to do not match the disjuncts of the disjunction.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("The hypothesis intervals of the lines should be on the same level.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("\\/ elim cannot be applied on lines that are in the first hypothesis interval.");
                                        }
                                    }
                                    else if (Frame.frame[checkLines[2]].Item1 is Disjunction && DisjElimIntervalCheck(int2, int0, int1))
                                    {
                                        if (int0.parent != null && int1.parent != null)
                                        {
                                            if (int0.parent == int1.parent)
                                            {
                                                Disjunction disj = Frame.frame[checkLines[2]].Item1 as Disjunction;
                                                int startline1 = int0.startLine;
                                                int startline2 = int1.startLine;
                                                if ((Frame.frame[startline1].Item1.Equals(disj.Left) && Frame.frame[startline2].Item1.Equals(disj.Right)) || (Frame.frame[startline1].Item1.Equals(disj.Right) && Frame.frame[startline2].Item1.Equals(disj.Left)))
                                                {
                                                    if (Frame.frame[checkLines[0]].Item1.Equals(Frame.frame[checkLines[1]].Item1))
                                                    {
                                                        if (Frame.frame[checkLines[0]].Item1.Equals(formToAdd))
                                                        {
                                                            if (intToAddOn == int0 || intToAddOn == int1)
                                                            {
                                                                Frame.AddForm(formToAdd, intToAddOn.parent, new Annotation(lines, Rules.OR, false));
                                                            }
                                                            else if (int2.ThisOrParent(intToAddOn) && intToAddOn == int0.parent)
                                                            {
                                                                Frame.AddForm(formToAdd, new Annotation(lines, Rules.OR, false));
                                                            }
                                                            else
                                                            {
                                                                EnterError("Cannot perform this \\/ elim on this hypothesis interval.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            EnterError("The formulae on the lines do not equal the new formula.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        EnterError("The formulae on the lines are not the same.");
                                                    }
                                                }
                                                else
                                                {
                                                    EnterError("The assumptions of the hypothesis intervals the lines refer to do not match the disjuncts of the disjunction.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("The hypothesis intervals of the lines should be on the same level.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("\\/ elim cannot be applied on lines that are in the first hypothesis interval.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("None of the lines refers to a disjunctions that is in a hypothesis interval encompassing the other lines.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter 3 lines for \\/ elim.");
                                }
                                break;
                            }
                        case ("- intro"):
                            {
                                if (lines.Count == 3)
                                {
                                    //One line should be an assumption
                                    //Another should be a formula
                                    //The third should be the negation of that formula
                                    if (Frame.frame[checkLines[1]].Item2 == Frame.frame[checkLines[0]].Item2 && Frame.frame[checkLines[2]].Item2 == Frame.frame[checkLines[0]].Item2)
                                    {
                                        if (Frame.frame[checkLines[0]].Item2.parent != null)
                                        {
                                            if (Frame.frame[checkLines[0]].Item3.rule == Rules.ASS)
                                            {
                                                if (Frame.Last.Item2 == Frame.frame[checkLines[0]].Item2 || Frame.frame[checkLines[0]].Item2.parent != null && Frame.Last.Item2 == Frame.frame[checkLines[0]].Item2.parent)
                                                {
                                                    bool contraFound = false;
                                                    if (Frame.frame[checkLines[1]].Item1 is Negation)
                                                    {
                                                        Negation neg = Frame.frame[checkLines[1]].Item1 as Negation;
                                                        if (Frame.frame[checkLines[2]].Item1.Equals(neg.Formula))
                                                        {
                                                            contraFound = true;
                                                        }
                                                    }
                                                    if (Frame.frame[checkLines[2]].Item1 is Negation)
                                                    {
                                                        Negation neg = Frame.frame[checkLines[2]].Item1 as Negation;
                                                        if (Frame.frame[checkLines[1]].Item1.Equals(neg.Formula))
                                                        {
                                                            contraFound = true;
                                                        }
                                                    }

                                                    if (!contraFound)
                                                    {
                                                        EnterError("The formulae on these lines do not contradict each other.");
                                                    }
                                                    else
                                                    {
                                                        if (intToAddOn == Frame.frame[checkLines[0]].Item2)
                                                        {
                                                            Frame.AddForm(new Negation(Frame.frame[checkLines[0]].Item1), intToAddOn.parent, new Annotation(lines, Rules.NEG, true));
                                                        }
                                                        else if (intToAddOn == Frame.frame[checkLines[0]].Item2.parent)
                                                        {
                                                            Frame.AddForm(new Negation(Frame.frame[checkLines[0]].Item1), intToAddOn, new Annotation(lines, Rules.NEG, true));
                                                        }
                                                        else
                                                        {
                                                            EnterError("Cannot perform - intro on this assumption on the current hypothesis interval.");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    EnterError("- intro on this assumption cannot be performed on the current hypothesis interval.");
                                                }
                                            }
                                            else if (Frame.frame[checkLines[1]].Item3.rule == Rules.ASS)
                                            {
                                                if (Frame.Last.Item2 == Frame.frame[checkLines[1]].Item2 || Frame.frame[checkLines[1]].Item2.parent != null && Frame.Last.Item2 == Frame.frame[checkLines[1]].Item2.parent)
                                                {
                                                    bool contraFound = false;
                                                    if (Frame.frame[checkLines[0]].Item1 is Negation)
                                                    {
                                                        Negation neg = Frame.frame[checkLines[0]].Item1 as Negation;
                                                        if (Frame.frame[checkLines[2]].Item1.Equals(neg.Formula))
                                                        {
                                                            contraFound = true;
                                                        }
                                                    }
                                                    if (Frame.frame[checkLines[2]].Item1 is Negation)
                                                    {
                                                        Negation neg = Frame.frame[checkLines[2]].Item1 as Negation;
                                                        if (Frame.frame[checkLines[0]].Item1.Equals(neg.Formula))
                                                        {
                                                            contraFound = true;
                                                        }
                                                    }

                                                    if (!contraFound)
                                                    {
                                                        EnterError("The formulae on these lines do not contradict each other.");
                                                    }
                                                    else
                                                    {
                                                        if (intToAddOn == Frame.Last.Item2)
                                                        {
                                                            Frame.AddForm(new Negation(Frame.frame[checkLines[1]].Item1), Frame.Last.Item2.parent, new Annotation(lines, Rules.NEG, true));
                                                        }
                                                        else
                                                        {
                                                            Frame.AddForm(new Negation(Frame.frame[checkLines[1]].Item1), intToAddOn, new Annotation(lines, Rules.NEG, true));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    EnterError("- intro on this assumption cannot be performed on the current hypothesis interval.");
                                                }
                                            }
                                            else if (Frame.frame[checkLines[2]].Item3.rule == Rules.ASS)
                                            {
                                                if (Frame.Last.Item2 == Frame.frame[checkLines[2]].Item2 || Frame.frame[checkLines[2]].Item2.parent != null && Frame.Last.Item2 == Frame.frame[checkLines[2]].Item2.parent)
                                                {
                                                    bool contraFound = false;
                                                    if (Frame.frame[checkLines[1]].Item1 is Negation)
                                                    {
                                                        Negation neg = Frame.frame[checkLines[1]].Item1 as Negation;
                                                        if (Frame.frame[checkLines[0]].Item1.Equals(neg.Formula))
                                                        {
                                                            contraFound = true;
                                                        }
                                                    }
                                                    if (Frame.frame[checkLines[0]].Item1 is Negation)
                                                    {
                                                        Negation neg = Frame.frame[checkLines[0]].Item1 as Negation;
                                                        if (Frame.frame[checkLines[1]].Item1.Equals(neg.Formula))
                                                        {
                                                            contraFound = true;
                                                        }
                                                    }

                                                    if (!contraFound)
                                                    {
                                                        EnterError("The formulae on these lines do not contradict each other.");
                                                    }
                                                    else
                                                    {
                                                        if (intToAddOn == Frame.Last.Item2)
                                                        {
                                                            Frame.AddForm(new Negation(Frame.frame[checkLines[2]].Item1), Frame.Last.Item2.parent, new Annotation(lines, Rules.NEG, true));
                                                        }
                                                        else
                                                        {
                                                            Frame.AddForm(new Negation(Frame.frame[checkLines[2]].Item1), intToAddOn, new Annotation(lines, Rules.NEG, true));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    EnterError("- intro on this assumption cannot be performed on the current hypothesis interval.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("One line should be an assumption.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("Cannot perform - intro on the first hypothesis interval.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("All three formulae should be on the same hypothesis interval.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter 3 lines for - intro.");
                                }
                                break;
                            }
                        case ("- elim"):
                            {
                                if (lines.Count == 1)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (Frame.frame[checkLines[0]].Item1 is Negation)
                                        {
                                            Negation neg = Frame.frame[checkLines[0]].Item1 as Negation;
                                            if (neg.Formula is Negation)
                                            {
                                                Negation neg2 = neg.Formula as Negation;
                                                if (neg2.Formula.Equals(formToAdd))
                                                {
                                                    Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.NEG, false));
                                                }
                                                else
                                                {
                                                    EnterError("This formula does not match the one on this line.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("The formula on this line is not a double negation.");
                                            }
                                        }
                                        else
                                        {
                                            //Given form is not a negation
                                            EnterError("The formula on this line is not a negation.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the interval on this line.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter only 1 line for - elim.");
                                }
                                break;
                            }
                        case ("-> intro"):
                            {
                                if (lines.Count == 2)
                                {
                                    if (Frame.frame[checkLines[0]].Item2 == Frame.frame[checkLines[1]].Item2)
                                    {
                                        if (Frame.frame[checkLines[0]].Item2.parent != null)
                                        {
                                            IFormula antecedent = null;
                                            IFormula consequent = null;
                                            if (Frame.frame[checkLines[0]].Item3.rule == Rules.ASS)
                                            {
                                                antecedent = Frame.frame[checkLines[0]].Item1;
                                                consequent = Frame.frame[checkLines[1]].Item1;
                                                if (intToAddOn == Frame.frame[checkLines[0]].Item2)
                                                {
                                                    Frame.AddForm(new Implication(antecedent, consequent), intToAddOn.parent, new Annotation(lines, Rules.IMP, true));
                                                }
                                                else if (intToAddOn == Frame.frame[checkLines[0]].Item2.parent)
                                                {
                                                    Frame.AddForm(new Implication(antecedent, consequent), intToAddOn, new Annotation(lines, Rules.IMP, true));
                                                }
                                                else
                                                {
                                                    EnterError("Cannot perform -> intro on this assumption on the current hypothesis interval.");
                                                }
                                            }
                                            else if (Frame.frame[checkLines[1]].Item3.rule == Rules.ASS)
                                            {
                                                antecedent = Frame.frame[checkLines[1]].Item1;
                                                consequent = Frame.frame[checkLines[0]].Item1;
                                                if (intToAddOn == Frame.frame[checkLines[1]].Item2)
                                                {
                                                    Frame.AddForm(new Implication(antecedent, consequent), intToAddOn.parent, new Annotation(lines, Rules.IMP, true));
                                                }
                                                else if (intToAddOn == Frame.frame[checkLines[1]].Item2.parent)
                                                {
                                                    Frame.AddForm(new Implication(antecedent, consequent), intToAddOn, new Annotation(lines, Rules.IMP, true));
                                                }
                                                else
                                                {
                                                    EnterError("Cannot perform -> intro on this assumption on the current hypothesis interval.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("One line should be an assumption.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("Cannot perform -> intro on the first hypothesis interval.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("Both lines should be on the same hypothesis interval.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter 2 lines for -> intro.");
                                }
                                break;
                            }
                        case ("-> elim"):
                            {
                                if (lines.Count == 2)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn) && Frame.frame[checkLines[1]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (Frame.frame[checkLines[0]].Item1 is Implication)
                                        {
                                            Implication impl = Frame.frame[checkLines[0]].Item1 as Implication;
                                            if (Frame.frame[checkLines[1]].Item1.Equals(impl.Antecedent))
                                            {
                                                if (formToAdd.Equals(impl.Consequent))
                                                {
                                                    Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.IMP, false));
                                                }
                                                else
                                                {
                                                    EnterError("The new formula does not match the consequent.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("No formula on these lines matches the antecedent.");
                                            }
                                        }
                                        else if (Frame.frame[checkLines[1]].Item1 is Implication)
                                        {
                                            Implication impl = Frame.frame[checkLines[1]].Item1 as Implication;
                                            if (Frame.frame[checkLines[0]].Item1.Equals(impl.Antecedent))
                                            {
                                                if (formToAdd.Equals(impl.Consequent))
                                                {
                                                    Frame.AddForm(formToAdd, new Annotation(lines, Rules.IMP, false));
                                                }
                                                else
                                                {
                                                    EnterError("The new formula does not match the consequent.");
                                                }
                                            }
                                            else
                                            {
                                                EnterError("No formula on these lines matches the antecedent.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("The formulas on these lines are not implications.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the intervals on these lines.");
                                    }
                                }
                                else
                                {
                                    EnterError("Please specify 2 lines to perform -> elim on.");
                                }
                                break;
                            }
                        case ("<-> intro"):
                            {
                                if (lines.Count == 2)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn) && Frame.frame[checkLines[1]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (formToAdd is Iff)
                                        {
                                            Iff iff = formToAdd as Iff;
                                            Implication left = new Implication(iff.Left, iff.Right);
                                            Implication right = new Implication(iff.Right, iff.Left);
                                            if ((Frame.frame[checkLines[0]].Item1.Equals(left) && Frame.frame[checkLines[1]].Item1.Equals(right)) || (Frame.frame[checkLines[1]].Item1.Equals(right) && Frame.frame[checkLines[0]].Item1.Equals(left)))
                                            {
                                                Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.BI, true));
                                            }
                                            else
                                            {
                                                //Given conjuncts do no match with those of the conjunction
                                                //Or given form is not a conjunction
                                                EnterError("Please enter the lines of the correct implications.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("The new formula is not a bi-implication.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the intervals on these lines.");
                                    }
                                }
                                else
                                {
                                    EnterError("Please specify 2 lines to perform <-> intro on.");
                                }
                                break;
                            }
                        case ("<-> elim"):
                            {
                                if (lines.Count == 1)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (Frame.frame[checkLines[0]].Item1 is Iff)
                                        {
                                            Iff iff = Frame.frame[checkLines[0]].Item1 as Iff;
                                            Implication left = new Implication(iff.Left, iff.Right);
                                            Implication right = new Implication(iff.Right, iff.Left);
                                            if (formToAdd.Equals(left) || formToAdd.Equals(right))
                                            {
                                                Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.BI, false));
                                            }
                                            else
                                            {
                                                EnterError("The formula on this line does not match either implication.");
                                            }
                                        }
                                        else
                                        {
                                            EnterError("This formula on this line is not a bi-implication.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the interval on this line.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter only 1 line for <-> elim.");
                                }
                                break;
                            }
                        case ("assumption"):
                            {
                                Frame.AddAss(formToAdd, intToAddOn);
                                break;
                            }
                        case ("reiterate"):
                            {
                                if (lines.Count == 1)
                                {
                                    if (Frame.frame[checkLines[0]].Item2.ThisOrParent(intToAddOn))
                                    {
                                        if (Frame.frame[checkLines[0]].Item1.Equals(formToAdd))
                                        {
                                            Frame.AddForm(formToAdd, intToAddOn, new Annotation(lines, Rules.REI, false));
                                        }
                                        else
                                        {
                                            //Given disjuncts do no match with those of the disjunction
                                            EnterError("The formula on this line does not match the entered formula.");
                                        }
                                    }
                                    else
                                    {
                                        EnterError("This hypothesis interval is not equal to or a subinterval of the interval on this line.");
                                    }
                                }
                                else
                                {
                                    //The right amount of numbers is not in numbersTxt
                                    EnterError("Please enter only 1 line for REI.");
                                }
                                break;
                            }
                        default:
                            {
                                EnterError("Please enter a valid rule to apply");
                                break;
                            }
                    }
                }
            }

            if (noError)
            {
                //If so, add it to the frame
                Form.proofTxt.Text = Frame.ToString();
                errorTxt.Visible = false;
                Form.proofTxt.SelectionStart = Form.proofTxt.Text.Length;
                Form.proofTxt.ScrollToCaret();

                //Check if it is the conclusion
                if (conclusion.Equals(Frame.Last.Item1) && Frame.Last.Item2.parent == null)
                {
                    //Done
                    RemoveControls();
                    Form.selfProving = false;
                    EnterError("Success!");
                    Form.premiseTxt.Enabled = true;
                    Form.premiseTxt.TabStop = true;
                    Form.conclTxt.Enabled = true;
                    Form.conclTxt.TabStop = true;
                    Form.proveBut.TabStop = false;
                    Form.selfProveBut.TabStop = false;
                }

                //If so, done. Else, add another new line
                else
                {
                    RemoveControls();
                    NewLine();
                }
            }
        }

        private static void GenerateHint(object sender, EventArgs e)
        {
            //Need a method to count, for a direct subgoal of the main goal, how far along the user is along them.
            //Based on that, the next step can be suggested
            //
            //If any of the special rules (such as De Morgan) are encountered and all its prequisites are met, look how far along
            //a predetermined path (matching the rule) the user is. Suggest a step based on that. Remember, a goal path ends when
            //it has a derivation.
            //
            //If the user has not progressed along any paths, pick suggest the first step of the shortest one
            //
            //Premises and reiterates need not be suggested. All the rest can be.
        }

        private static void CancelLastLine(object sender, EventArgs e)
        {
            if (Frame.Last.Item3.rule != Rules.HYPO)
            {
                Frame.frame.Remove(Frame.Last);
                Form.proofTxt.Text = Frame.ToString();
                errorTxt.Visible = false;
                Form.proofTxt.SelectionStart = Form.proofTxt.Text.Length;
                Form.proofTxt.ScrollToCaret();
                RemoveControls();
                NewLine();
            }
            else
            {
                EnterError("Cannot remove premises.");
            }
        }

        public static void RemoveControls()
        {
            if (numberLine != null)
                Form.Controls.Remove(numberLine);
            if (formTxt != null)
                Form.Controls.Remove(formTxt);
            if (numberTxt != null)
                Form.Controls.Remove(numberTxt);
            if (derivRuleBox != null)
                Form.Controls.Remove(derivRuleBox);
            if (addButton != null)
                Form.Controls.Remove(addButton);
            if (cancelButton != null)
                Form.Controls.Remove(cancelButton);
            if (closeIntTxt != null)
                Form.Controls.Remove(closeIntTxt);
            if (closeIntLbl != null)
                Form.Controls.Remove(closeIntLbl);
            if (hintButton != null)
                Form.Controls.Remove(hintButton);
        }

        private static void EnterError(string error)
        {
            errorTxt.Text = error;
            errorTxt.Visible = true;
            noError = false;
            Size size = TextRenderer.MeasureText(errorTxt.Text, errorTxt.Font);
            errorTxt.Width = size.Width;
        }

        private static bool DisjElimIntervalCheck(Interval disjInt, Interval conclInt1, Interval conclInt2)
        {
            if (!disjInt.ThisOrParent(conclInt1))
            {
                return false;
            }
            if (!disjInt.ThisOrParent(conclInt2))
            {
                return false;
            }
            if (disjInt == conclInt1)
            {
                return false;
            }
            if (disjInt == conclInt2)
            {
                return false;
            }
            return true;
        }
    }
}