namespace com.cadmunity.xbuttons
{
    partial class SetupDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialog));
            this.previousButton = new System.Windows.Forms.Button();
            this.nextOrFinishButton = new System.Windows.Forms.Button();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.clearButton = new System.Windows.Forms.Button();
            this.currentChordLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // previousButton
            // 
            this.previousButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.previousButton.Location = new System.Drawing.Point(191, 207);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(75, 23);
            this.previousButton.TabIndex = 0;
            this.previousButton.Text = "Previous";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.previousButtonClicked);
            // 
            // nextOrFinishButton
            // 
            this.nextOrFinishButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.nextOrFinishButton.Location = new System.Drawing.Point(272, 207);
            this.nextOrFinishButton.Name = "nextOrFinishButton";
            this.nextOrFinishButton.Size = new System.Drawing.Size(86, 23);
            this.nextOrFinishButton.TabIndex = 1;
            this.nextOrFinishButton.Text = "Next";
            this.nextOrFinishButton.UseVisualStyleBackColor = true;
            this.nextOrFinishButton.Click += new System.EventHandler(this.nextOrFinishedClicked);
            // 
            // instructionLabel
            // 
            this.instructionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.instructionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.instructionLabel.Location = new System.Drawing.Point(0, 0);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(451, 36);
            this.instructionLabel.TabIndex = 1;
            this.instructionLabel.Text = "Press the keys to send for the X1 (Back) button";
            this.instructionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearButton.Location = new System.Drawing.Point(12, 207);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(75, 23);
            this.clearButton.TabIndex = 3;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButtonPressed);
            // 
            // currentChordLabel
            // 
            this.currentChordLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.currentChordLabel.AutoSize = true;
            this.currentChordLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentChordLabel.Location = new System.Drawing.Point(134, 94);
            this.currentChordLabel.Name = "currentChordLabel";
            this.currentChordLabel.Size = new System.Drawing.Size(190, 24);
            this.currentChordLabel.TabIndex = 5;
            this.currentChordLabel.Text = "<No keys pressed>";
            this.currentChordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(364, 207);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelPressed);
            // 
            // SetupDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 242);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.currentChordLabel);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.instructionLabel);
            this.Controls.Add(this.nextOrFinishButton);
            this.Controls.Add(this.previousButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "XButtons Setup";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.processKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.processKeyUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button nextOrFinishButton;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Label currentChordLabel;
        private System.Windows.Forms.Button cancelButton;
    }
}