namespace Natural_Deduction_Tool
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.premiseTxt = new System.Windows.Forms.TextBox();
            this.conclTxt = new System.Windows.Forms.TextBox();
            this.premLabel = new System.Windows.Forms.Label();
            this.conclLabel = new System.Windows.Forms.Label();
            this.proveBut = new System.Windows.Forms.Button();
            this.proofTxt = new Natural_Deduction_Tool.ReadOnlyTextBox();
            this.NegButton = new System.Windows.Forms.Button();
            this.DisjButton = new System.Windows.Forms.Button();
            this.ConjButton = new System.Windows.Forms.Button();
            this.ImplButton = new System.Windows.Forms.Button();
            this.IffButton = new System.Windows.Forms.Button();
            this.varButton1 = new System.Windows.Forms.Button();
            this.varButton2 = new System.Windows.Forms.Button();
            this.varButton3 = new System.Windows.Forms.Button();
            this.varButton4 = new System.Windows.Forms.Button();
            this.varButton5 = new System.Windows.Forms.Button();
            this.varButton0 = new System.Windows.Forms.Button();
            this.varButton9 = new System.Windows.Forms.Button();
            this.varButton8 = new System.Windows.Forms.Button();
            this.varButton7 = new System.Windows.Forms.Button();
            this.varButton6 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // premiseTxt
            // 
            this.premiseTxt.Location = new System.Drawing.Point(103, 12);
            this.premiseTxt.Name = "premiseTxt";
            this.premiseTxt.Size = new System.Drawing.Size(362, 22);
            this.premiseTxt.TabIndex = 7;
            this.premiseTxt.GotFocus += new System.EventHandler(this.PremiseTxt_GotFocus);
            // 
            // conclTxt
            // 
            this.conclTxt.Location = new System.Drawing.Point(103, 52);
            this.conclTxt.Name = "conclTxt";
            this.conclTxt.Size = new System.Drawing.Size(362, 22);
            this.conclTxt.TabIndex = 8;
            this.conclTxt.GotFocus += new System.EventHandler(this.ConclTxt_GotFocus);
            // 
            // premLabel
            // 
            this.premLabel.AutoSize = true;
            this.premLabel.Location = new System.Drawing.Point(9, 12);
            this.premLabel.Name = "premLabel";
            this.premLabel.Size = new System.Drawing.Size(70, 17);
            this.premLabel.TabIndex = 9;
            this.premLabel.Text = "Premises:";
            // 
            // conclLabel
            // 
            this.conclLabel.AutoSize = true;
            this.conclLabel.Location = new System.Drawing.Point(9, 52);
            this.conclLabel.Name = "conclLabel";
            this.conclLabel.Size = new System.Drawing.Size(81, 17);
            this.conclLabel.TabIndex = 10;
            this.conclLabel.Text = "Conclusion:";
            // 
            // proveBut
            // 
            this.proveBut.Location = new System.Drawing.Point(79, 111);
            this.proveBut.Name = "proveBut";
            this.proveBut.Size = new System.Drawing.Size(88, 35);
            this.proveBut.TabIndex = 11;
            this.proveBut.Text = "&Prove";
            this.proveBut.UseVisualStyleBackColor = true;
            this.proveBut.Click += new System.EventHandler(this.proveBut_Click);
            // 
            // proofTxt
            // 
            this.proofTxt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.proofTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.proofTxt.Location = new System.Drawing.Point(196, 99);
            this.proofTxt.Multiline = true;
            this.proofTxt.Name = "proofTxt";
            this.proofTxt.ReadOnly = true;
            this.proofTxt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.proofTxt.Size = new System.Drawing.Size(639, 564);
            this.proofTxt.TabIndex = 6;
            this.proofTxt.TabStop = false;
            // 
            // NegButton
            // 
            this.NegButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NegButton.Location = new System.Drawing.Point(7, 173);
            this.NegButton.Name = "NegButton";
            this.NegButton.Size = new System.Drawing.Size(55, 55);
            this.NegButton.TabIndex = 12;
            this.NegButton.TabStop = false;
            this.NegButton.Text = "N";
            this.NegButton.UseVisualStyleBackColor = true;
            this.NegButton.Click += new System.EventHandler(this.NegButton_Click);
            // 
            // DisjButton
            // 
            this.DisjButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DisjButton.Location = new System.Drawing.Point(68, 173);
            this.DisjButton.Name = "DisjButton";
            this.DisjButton.Size = new System.Drawing.Size(55, 55);
            this.DisjButton.TabIndex = 13;
            this.DisjButton.TabStop = false;
            this.DisjButton.Text = "D";
            this.DisjButton.UseVisualStyleBackColor = true;
            this.DisjButton.Click += new System.EventHandler(this.DisjButton_Click);
            // 
            // ConjButton
            // 
            this.ConjButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConjButton.Location = new System.Drawing.Point(129, 173);
            this.ConjButton.Name = "ConjButton";
            this.ConjButton.Size = new System.Drawing.Size(55, 55);
            this.ConjButton.TabIndex = 14;
            this.ConjButton.TabStop = false;
            this.ConjButton.Text = "C";
            this.ConjButton.UseVisualStyleBackColor = true;
            this.ConjButton.Click += new System.EventHandler(this.ConjButton_Click);
            // 
            // ImplButton
            // 
            this.ImplButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImplButton.Location = new System.Drawing.Point(35, 234);
            this.ImplButton.Name = "ImplButton";
            this.ImplButton.Size = new System.Drawing.Size(55, 55);
            this.ImplButton.TabIndex = 15;
            this.ImplButton.TabStop = false;
            this.ImplButton.Text = "Im";
            this.ImplButton.UseVisualStyleBackColor = true;
            this.ImplButton.Click += new System.EventHandler(this.ImplButton_Click);
            // 
            // IffButton
            // 
            this.IffButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IffButton.Location = new System.Drawing.Point(96, 234);
            this.IffButton.Name = "IffButton";
            this.IffButton.Size = new System.Drawing.Size(55, 55);
            this.IffButton.TabIndex = 16;
            this.IffButton.TabStop = false;
            this.IffButton.Text = "Iff";
            this.IffButton.UseVisualStyleBackColor = true;
            this.IffButton.Click += new System.EventHandler(this.IffButton_Click);
            // 
            // varButton1
            // 
            this.varButton1.Enabled = false;
            this.varButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton1.Location = new System.Drawing.Point(35, 321);
            this.varButton1.Name = "varButton1";
            this.varButton1.Size = new System.Drawing.Size(55, 55);
            this.varButton1.TabIndex = 17;
            this.varButton1.TabStop = false;
            this.varButton1.Text = "N";
            this.varButton1.UseVisualStyleBackColor = true;
            this.varButton1.Visible = false;
            // 
            // varButton2
            // 
            this.varButton2.Enabled = false;
            this.varButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton2.Location = new System.Drawing.Point(35, 382);
            this.varButton2.Name = "varButton2";
            this.varButton2.Size = new System.Drawing.Size(55, 55);
            this.varButton2.TabIndex = 18;
            this.varButton2.TabStop = false;
            this.varButton2.Text = "N";
            this.varButton2.UseVisualStyleBackColor = true;
            this.varButton2.Visible = false;
            // 
            // varButton3
            // 
            this.varButton3.Enabled = false;
            this.varButton3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton3.Location = new System.Drawing.Point(35, 443);
            this.varButton3.Name = "varButton3";
            this.varButton3.Size = new System.Drawing.Size(55, 55);
            this.varButton3.TabIndex = 19;
            this.varButton3.TabStop = false;
            this.varButton3.Text = "N";
            this.varButton3.UseVisualStyleBackColor = true;
            this.varButton3.Visible = false;
            // 
            // varButton4
            // 
            this.varButton4.Enabled = false;
            this.varButton4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton4.Location = new System.Drawing.Point(35, 504);
            this.varButton4.Name = "varButton4";
            this.varButton4.Size = new System.Drawing.Size(55, 55);
            this.varButton4.TabIndex = 20;
            this.varButton4.TabStop = false;
            this.varButton4.Text = "N";
            this.varButton4.UseVisualStyleBackColor = true;
            this.varButton4.Visible = false;
            // 
            // varButton5
            // 
            this.varButton5.Enabled = false;
            this.varButton5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton5.Location = new System.Drawing.Point(35, 565);
            this.varButton5.Name = "varButton5";
            this.varButton5.Size = new System.Drawing.Size(55, 55);
            this.varButton5.TabIndex = 21;
            this.varButton5.TabStop = false;
            this.varButton5.Text = "N";
            this.varButton5.UseVisualStyleBackColor = true;
            this.varButton5.Visible = false;
            // 
            // varButton0
            // 
            this.varButton0.Enabled = false;
            this.varButton0.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton0.Location = new System.Drawing.Point(102, 565);
            this.varButton0.Name = "varButton0";
            this.varButton0.Size = new System.Drawing.Size(55, 55);
            this.varButton0.TabIndex = 26;
            this.varButton0.TabStop = false;
            this.varButton0.Text = "N";
            this.varButton0.UseVisualStyleBackColor = true;
            this.varButton0.Visible = false;
            // 
            // varButton9
            // 
            this.varButton9.Enabled = false;
            this.varButton9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton9.Location = new System.Drawing.Point(102, 504);
            this.varButton9.Name = "varButton9";
            this.varButton9.Size = new System.Drawing.Size(55, 55);
            this.varButton9.TabIndex = 25;
            this.varButton9.TabStop = false;
            this.varButton9.Text = "N";
            this.varButton9.UseVisualStyleBackColor = true;
            this.varButton9.Visible = false;
            // 
            // varButton8
            // 
            this.varButton8.Enabled = false;
            this.varButton8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton8.Location = new System.Drawing.Point(102, 443);
            this.varButton8.Name = "varButton8";
            this.varButton8.Size = new System.Drawing.Size(55, 55);
            this.varButton8.TabIndex = 24;
            this.varButton8.TabStop = false;
            this.varButton8.Text = "N";
            this.varButton8.UseVisualStyleBackColor = true;
            this.varButton8.Visible = false;
            // 
            // varButton7
            // 
            this.varButton7.Enabled = false;
            this.varButton7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton7.Location = new System.Drawing.Point(102, 382);
            this.varButton7.Name = "varButton7";
            this.varButton7.Size = new System.Drawing.Size(55, 55);
            this.varButton7.TabIndex = 23;
            this.varButton7.TabStop = false;
            this.varButton7.Text = "N";
            this.varButton7.UseVisualStyleBackColor = true;
            this.varButton7.Visible = false;
            // 
            // varButton6
            // 
            this.varButton6.Enabled = false;
            this.varButton6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.varButton6.Location = new System.Drawing.Point(102, 321);
            this.varButton6.Name = "varButton6";
            this.varButton6.Size = new System.Drawing.Size(55, 55);
            this.varButton6.TabIndex = 22;
            this.varButton6.TabStop = false;
            this.varButton6.Text = "N";
            this.varButton6.UseVisualStyleBackColor = true;
            this.varButton6.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 675);
            this.Controls.Add(this.varButton0);
            this.Controls.Add(this.varButton9);
            this.Controls.Add(this.varButton8);
            this.Controls.Add(this.varButton7);
            this.Controls.Add(this.varButton6);
            this.Controls.Add(this.varButton5);
            this.Controls.Add(this.varButton4);
            this.Controls.Add(this.varButton3);
            this.Controls.Add(this.varButton2);
            this.Controls.Add(this.varButton1);
            this.Controls.Add(this.IffButton);
            this.Controls.Add(this.ImplButton);
            this.Controls.Add(this.ConjButton);
            this.Controls.Add(this.DisjButton);
            this.Controls.Add(this.NegButton);
            this.Controls.Add(this.proveBut);
            this.Controls.Add(this.conclLabel);
            this.Controls.Add(this.premLabel);
            this.Controls.Add(this.conclTxt);
            this.Controls.Add(this.premiseTxt);
            this.Controls.Add(this.proofTxt);
            this.Name = "Form1";
            this.Text = "Natural Deduction Solver";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ReadOnlyTextBox proofTxt;
        private System.Windows.Forms.TextBox premiseTxt;
        private System.Windows.Forms.TextBox conclTxt;
        private System.Windows.Forms.Label premLabel;
        private System.Windows.Forms.Label conclLabel;
        private System.Windows.Forms.Button proveBut;
        private System.Windows.Forms.Button NegButton;
        private System.Windows.Forms.Button DisjButton;
        private System.Windows.Forms.Button ConjButton;
        private System.Windows.Forms.Button ImplButton;
        private System.Windows.Forms.Button IffButton;
        private System.Windows.Forms.Button varButton1;
        private System.Windows.Forms.Button varButton2;
        private System.Windows.Forms.Button varButton3;
        private System.Windows.Forms.Button varButton4;
        private System.Windows.Forms.Button varButton5;
        private System.Windows.Forms.Button varButton0;
        private System.Windows.Forms.Button varButton9;
        private System.Windows.Forms.Button varButton8;
        private System.Windows.Forms.Button varButton7;
        private System.Windows.Forms.Button varButton6;
    }
}

