namespace XivlyTester
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
            this.txtTransport = new System.Windows.Forms.TextBox();
            this.txtFeedId = new System.Windows.Forms.TextBox();
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.txtBody = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.txtGetBody = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtTransport
            // 
            this.txtTransport.Location = new System.Drawing.Point(90, 12);
            this.txtTransport.Name = "txtTransport";
            this.txtTransport.Size = new System.Drawing.Size(330, 20);
            this.txtTransport.TabIndex = 0;
            this.txtTransport.Text = "https://api.xively.com/v2/feeds/";
            // 
            // txtFeedId
            // 
            this.txtFeedId.Location = new System.Drawing.Point(90, 47);
            this.txtFeedId.Name = "txtFeedId";
            this.txtFeedId.Size = new System.Drawing.Size(169, 20);
            this.txtFeedId.TabIndex = 1;
            this.txtFeedId.Text = "1934589243";
            // 
            // txtApiKey
            // 
            this.txtApiKey.Location = new System.Drawing.Point(90, 88);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.Size = new System.Drawing.Size(357, 20);
            this.txtApiKey.TabIndex = 2;
            this.txtApiKey.Text = "Qq06ah4O9W9WkTfaySwlEqlhrsmSAKwrMHJZMC84TmNBdz0g";
            // 
            // txtBody
            // 
            this.txtBody.Location = new System.Drawing.Point(90, 140);
            this.txtBody.Name = "txtBody";
            this.txtBody.Size = new System.Drawing.Size(422, 20);
            this.txtBody.TabIndex = 3;
            this.txtBody.Text = "data, 77";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(90, 194);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(422, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "do IT PUT";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(90, 247);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(422, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "do IT GET";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(555, 220);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(100, 20);
            this.txtStatus.TabIndex = 6;
            this.txtStatus.Text = "Status";
            // 
            // txtGetBody
            // 
            this.txtGetBody.Location = new System.Drawing.Point(90, 292);
            this.txtGetBody.Multiline = true;
            this.txtGetBody.Name = "txtGetBody";
            this.txtGetBody.Size = new System.Drawing.Size(422, 53);
            this.txtGetBody.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(866, 391);
            this.Controls.Add(this.txtGetBody);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtBody);
            this.Controls.Add(this.txtApiKey);
            this.Controls.Add(this.txtFeedId);
            this.Controls.Add(this.txtTransport);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTransport;
        private System.Windows.Forms.TextBox txtFeedId;
        private System.Windows.Forms.TextBox txtApiKey;
        private System.Windows.Forms.TextBox txtBody;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.TextBox txtGetBody;
    }
}

