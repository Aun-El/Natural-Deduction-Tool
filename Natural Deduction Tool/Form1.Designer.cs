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
            this.formTxt1 = new System.Windows.Forms.TextBox();
            this.formTxt2 = new System.Windows.Forms.MaskedTextBox();
            this.equalsBut = new System.Windows.Forms.Button();
            this.outputTxt = new System.Windows.Forms.TextBox();
            this.formLabel1 = new System.Windows.Forms.Label();
            this.formLabel2 = new System.Windows.Forms.Label();
            this.proofTxt = new System.Windows.Forms.TextBox();
            this.premiseTxt = new System.Windows.Forms.TextBox();
            this.conclTxt = new System.Windows.Forms.TextBox();
            this.premLabel = new System.Windows.Forms.Label();
            this.conclLabel = new System.Windows.Forms.Label();
            this.proveBut = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // formTxt1
            // 
            this.formTxt1.Location = new System.Drawing.Point(106, 54);
            this.formTxt1.Name = "formTxt1";
            this.formTxt1.Size = new System.Drawing.Size(668, 22);
            this.formTxt1.TabIndex = 0;
            // 
            // formTxt2
            // 
            this.formTxt2.Location = new System.Drawing.Point(106, 97);
            this.formTxt2.Name = "formTxt2";
            this.formTxt2.Size = new System.Drawing.Size(668, 22);
            this.formTxt2.TabIndex = 1;
            // 
            // equalsBut
            // 
            this.equalsBut.Location = new System.Drawing.Point(82, 155);
            this.equalsBut.Name = "equalsBut";
            this.equalsBut.Size = new System.Drawing.Size(75, 23);
            this.equalsBut.TabIndex = 2;
            this.equalsBut.Text = "Equals?";
            this.equalsBut.UseVisualStyleBackColor = true;
            this.equalsBut.Click += new System.EventHandler(this.equalsBut_Click);
            // 
            // outputTxt
            // 
            this.outputTxt.Enabled = false;
            this.outputTxt.Location = new System.Drawing.Point(208, 155);
            this.outputTxt.Name = "outputTxt";
            this.outputTxt.Size = new System.Drawing.Size(100, 22);
            this.outputTxt.TabIndex = 3;
            // 
            // formLabel1
            // 
            this.formLabel1.AutoSize = true;
            this.formLabel1.Location = new System.Drawing.Point(12, 54);
            this.formLabel1.Name = "formLabel1";
            this.formLabel1.Size = new System.Drawing.Size(56, 17);
            this.formLabel1.TabIndex = 4;
            this.formLabel1.Text = "Form 1:";
            // 
            // formLabel2
            // 
            this.formLabel2.AutoSize = true;
            this.formLabel2.Location = new System.Drawing.Point(12, 102);
            this.formLabel2.Name = "formLabel2";
            this.formLabel2.Size = new System.Drawing.Size(56, 17);
            this.formLabel2.TabIndex = 5;
            this.formLabel2.Text = "Form 2:";
            // 
            // proofTxt
            // 
            this.proofTxt.Enabled = false;
            this.proofTxt.Location = new System.Drawing.Point(529, 180);
            this.proofTxt.Multiline = true;
            this.proofTxt.Name = "proofTxt";
            this.proofTxt.Size = new System.Drawing.Size(306, 306);
            this.proofTxt.TabIndex = 6;
            // 
            // premiseTxt
            // 
            this.premiseTxt.Location = new System.Drawing.Point(106, 239);
            this.premiseTxt.Name = "premiseTxt";
            this.premiseTxt.Size = new System.Drawing.Size(362, 22);
            this.premiseTxt.TabIndex = 7;
            // 
            // conclTxt
            // 
            this.conclTxt.Location = new System.Drawing.Point(106, 279);
            this.conclTxt.Name = "conclTxt";
            this.conclTxt.Size = new System.Drawing.Size(362, 22);
            this.conclTxt.TabIndex = 8;
            // 
            // premLabel
            // 
            this.premLabel.AutoSize = true;
            this.premLabel.Location = new System.Drawing.Point(12, 239);
            this.premLabel.Name = "premLabel";
            this.premLabel.Size = new System.Drawing.Size(70, 17);
            this.premLabel.TabIndex = 9;
            this.premLabel.Text = "Premises:";
            // 
            // conclLabel
            // 
            this.conclLabel.AutoSize = true;
            this.conclLabel.Location = new System.Drawing.Point(12, 279);
            this.conclLabel.Name = "conclLabel";
            this.conclLabel.Size = new System.Drawing.Size(81, 17);
            this.conclLabel.TabIndex = 10;
            this.conclLabel.Text = "Conclusion:";
            // 
            // proveBut
            // 
            this.proveBut.Location = new System.Drawing.Point(82, 338);
            this.proveBut.Name = "proveBut";
            this.proveBut.Size = new System.Drawing.Size(75, 23);
            this.proveBut.TabIndex = 11;
            this.proveBut.Text = "Prove";
            this.proveBut.UseVisualStyleBackColor = true;
            this.proveBut.Click += new System.EventHandler(this.proveBut_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 498);
            this.Controls.Add(this.proveBut);
            this.Controls.Add(this.conclLabel);
            this.Controls.Add(this.premLabel);
            this.Controls.Add(this.conclTxt);
            this.Controls.Add(this.premiseTxt);
            this.Controls.Add(this.proofTxt);
            this.Controls.Add(this.formLabel2);
            this.Controls.Add(this.formLabel1);
            this.Controls.Add(this.outputTxt);
            this.Controls.Add(this.equalsBut);
            this.Controls.Add(this.formTxt2);
            this.Controls.Add(this.formTxt1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox formTxt1;
        private System.Windows.Forms.MaskedTextBox formTxt2;
        private System.Windows.Forms.Button equalsBut;
        private System.Windows.Forms.TextBox outputTxt;
        private System.Windows.Forms.Label formLabel1;
        private System.Windows.Forms.Label formLabel2;
        private System.Windows.Forms.TextBox proofTxt;
        private System.Windows.Forms.TextBox premiseTxt;
        private System.Windows.Forms.TextBox conclTxt;
        private System.Windows.Forms.Label premLabel;
        private System.Windows.Forms.Label conclLabel;
        private System.Windows.Forms.Button proveBut;
    }
}

