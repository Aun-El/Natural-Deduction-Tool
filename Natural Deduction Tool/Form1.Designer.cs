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
            this.proofTxt = new System.Windows.Forms.TextBox();
            this.premiseTxt = new System.Windows.Forms.TextBox();
            this.conclTxt = new System.Windows.Forms.TextBox();
            this.premLabel = new System.Windows.Forms.Label();
            this.conclLabel = new System.Windows.Forms.Label();
            this.proveBut = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // proofTxt
            // 
            this.proofTxt.Enabled = false;
            this.proofTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.proofTxt.Location = new System.Drawing.Point(196, 99);
            this.proofTxt.Multiline = true;
            this.proofTxt.Name = "proofTxt";
            this.proofTxt.Size = new System.Drawing.Size(639, 564);
            this.proofTxt.TabIndex = 6;
            // 
            // premiseTxt
            // 
            this.premiseTxt.Location = new System.Drawing.Point(103, 12);
            this.premiseTxt.Name = "premiseTxt";
            this.premiseTxt.Size = new System.Drawing.Size(362, 22);
            this.premiseTxt.TabIndex = 7;
            // 
            // conclTxt
            // 
            this.conclTxt.Location = new System.Drawing.Point(103, 52);
            this.conclTxt.Name = "conclTxt";
            this.conclTxt.Size = new System.Drawing.Size(362, 22);
            this.conclTxt.TabIndex = 8;
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
            this.ClientSize = new System.Drawing.Size(847, 675);
            this.Controls.Add(this.proveBut);
            this.Controls.Add(this.conclLabel);
            this.Controls.Add(this.premLabel);
            this.Controls.Add(this.conclTxt);
            this.Controls.Add(this.premiseTxt);
            this.Controls.Add(this.proofTxt);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox proofTxt;
        private System.Windows.Forms.TextBox premiseTxt;
        private System.Windows.Forms.TextBox conclTxt;
        private System.Windows.Forms.Label premLabel;
        private System.Windows.Forms.Label conclLabel;
        private System.Windows.Forms.Button proveBut;
    }
}

