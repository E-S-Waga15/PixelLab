namespace PixelLab
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            btnOpen = new Button();
            lblColorInfo = new Label();
            cmbColorMode = new ComboBox();
            lblC1 = new Label();
            lblC2 = new Label();
            lblC3 = new Label();
            lblC4 = new Label();
            trackC1 = new TrackBar();
            trackC2 = new TrackBar();
            trackC3 = new TrackBar();
            trackC4 = new TrackBar();
            lblV1 = new Label();
            lblV2 = new Label();
            lblV3 = new Label();
            lblV4 = new Label();
            chkC1 = new CheckBox();
            chkC2 = new CheckBox();
            chkC3 = new CheckBox();
            chkC4 = new CheckBox();
            pictureBoxSpace = new PictureBox();
            trackZoom = new TrackBar();
            trackRotate = new TrackBar();
            lblSpaceInfo = new Label();
            panelSelectedColor = new Panel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackC1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackC2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackC3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackC4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSpace).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackZoom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackRotate).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ControlLightLight;
            pictureBox1.Location = new Point(12, 80);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(262, 206);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(12, 292);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(139, 29);
            btnOpen.TabIndex = 1;
            btnOpen.Text = "Open Image";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click;
            // 
            // lblColorInfo
            // 
            lblColorInfo.AutoSize = true;
            lblColorInfo.Location = new Point(316, 80);
            lblColorInfo.Name = "lblColorInfo";
            lblColorInfo.Size = new Size(54, 20);
            lblColorInfo.TabIndex = 5;
            lblColorInfo.Text = "RGB →";
            // 
            // cmbColorMode
            // 
            cmbColorMode.FormattingEnabled = true;
            cmbColorMode.Location = new Point(685, 41);
            cmbColorMode.Name = "cmbColorMode";
            cmbColorMode.Size = new Size(201, 28);
            cmbColorMode.TabIndex = 6;
            // 
            // lblC1
            // 
            lblC1.AutoSize = true;
            lblC1.Location = new Point(591, 91);
            lblC1.Name = "lblC1";
            lblC1.Size = new Size(50, 20);
            lblC1.TabIndex = 7;
            lblC1.Text = "label1";
            // 
            // lblC2
            // 
            lblC2.AutoSize = true;
            lblC2.Location = new Point(591, 150);
            lblC2.Name = "lblC2";
            lblC2.Size = new Size(50, 20);
            lblC2.TabIndex = 8;
            lblC2.Text = "label2";
            // 
            // lblC3
            // 
            lblC3.AutoSize = true;
            lblC3.Location = new Point(591, 212);
            lblC3.Name = "lblC3";
            lblC3.Size = new Size(50, 20);
            lblC3.TabIndex = 9;
            lblC3.Text = "label3";
            // 
            // lblC4
            // 
            lblC4.AutoSize = true;
            lblC4.Location = new Point(591, 274);
            lblC4.Name = "lblC4";
            lblC4.Size = new Size(50, 20);
            lblC4.TabIndex = 10;
            lblC4.Text = "label4";
            // 
            // trackC1
            // 
            trackC1.Location = new Point(691, 91);
            trackC1.Name = "trackC1";
            trackC1.Size = new Size(195, 56);
            trackC1.TabIndex = 11;
            // 
            // trackC2
            // 
            trackC2.Location = new Point(691, 150);
            trackC2.Name = "trackC2";
            trackC2.Size = new Size(195, 56);
            trackC2.TabIndex = 12;
            // 
            // trackC3
            // 
            trackC3.Location = new Point(691, 212);
            trackC3.Name = "trackC3";
            trackC3.Size = new Size(195, 56);
            trackC3.TabIndex = 13;
            // 
            // trackC4
            // 
            trackC4.Location = new Point(691, 274);
            trackC4.Name = "trackC4";
            trackC4.Size = new Size(195, 56);
            trackC4.TabIndex = 14;
            // 
            // lblV1
            // 
            lblV1.AutoSize = true;
            lblV1.Location = new Point(950, 91);
            lblV1.Name = "lblV1";
            lblV1.Size = new Size(50, 20);
            lblV1.TabIndex = 15;
            lblV1.Text = "label5";
            // 
            // lblV2
            // 
            lblV2.AutoSize = true;
            lblV2.Location = new Point(950, 150);
            lblV2.Name = "lblV2";
            lblV2.Size = new Size(50, 20);
            lblV2.TabIndex = 16;
            lblV2.Text = "label6";
            // 
            // lblV3
            // 
            lblV3.AutoSize = true;
            lblV3.Location = new Point(950, 212);
            lblV3.Name = "lblV3";
            lblV3.Size = new Size(50, 20);
            lblV3.TabIndex = 17;
            lblV3.Text = "label7";
            // 
            // lblV4
            // 
            lblV4.AutoSize = true;
            lblV4.Location = new Point(950, 274);
            lblV4.Name = "lblV4";
            lblV4.Size = new Size(50, 20);
            lblV4.TabIndex = 18;
            lblV4.Text = "label8";
            // 
            // chkC1
            // 
            chkC1.AutoSize = true;
            chkC1.Location = new Point(1040, 91);
            chkC1.Name = "chkC1";
            chkC1.Size = new Size(50, 24);
            chkC1.TabIndex = 19;
            chkC1.Text = "On";
            chkC1.UseVisualStyleBackColor = true;
            // 
            // chkC2
            // 
            chkC2.AutoSize = true;
            chkC2.Location = new Point(1040, 150);
            chkC2.Name = "chkC2";
            chkC2.Size = new Size(50, 24);
            chkC2.TabIndex = 20;
            chkC2.Text = "On";
            chkC2.UseVisualStyleBackColor = true;
            // 
            // chkC3
            // 
            chkC3.AutoSize = true;
            chkC3.Location = new Point(1040, 212);
            chkC3.Name = "chkC3";
            chkC3.Size = new Size(50, 24);
            chkC3.TabIndex = 21;
            chkC3.Text = "On";
            chkC3.UseVisualStyleBackColor = true;
            // 
            // chkC4
            // 
            chkC4.AutoSize = true;
            chkC4.Location = new Point(1040, 274);
            chkC4.Name = "chkC4";
            chkC4.Size = new Size(50, 24);
            chkC4.TabIndex = 22;
            chkC4.Text = "On";
            chkC4.UseVisualStyleBackColor = true;
            // 
            // pictureBoxSpace
            // 
            pictureBoxSpace.BackColor = Color.White;
            pictureBoxSpace.Location = new Point(33, 418);
            pictureBoxSpace.Name = "pictureBoxSpace";
            pictureBoxSpace.Size = new Size(206, 166);
            pictureBoxSpace.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSpace.TabIndex = 23;
            pictureBoxSpace.TabStop = false;
            // 
            // trackZoom
            // 
            trackZoom.Location = new Point(791, 449);
            trackZoom.Maximum = 300;
            trackZoom.Minimum = 50;
            trackZoom.Name = "trackZoom";
            trackZoom.Size = new Size(200, 56);
            trackZoom.TabIndex = 24;
            trackZoom.Value = 100;
            // 
            // trackRotate
            // 
            trackRotate.Location = new Point(800, 533);
            trackRotate.Maximum = 360;
            trackRotate.Name = "trackRotate";
            trackRotate.Size = new Size(200, 56);
            trackRotate.TabIndex = 25;
            // 
            // lblSpaceInfo
            // 
            lblSpaceInfo.AutoSize = true;
            lblSpaceInfo.Location = new Point(451, 418);
            lblSpaceInfo.Name = "lblSpaceInfo";
            lblSpaceInfo.Size = new Size(50, 20);
            lblSpaceInfo.TabIndex = 26;
            lblSpaceInfo.Text = "label1";
            // 
            // panelSelectedColor
            // 
            panelSelectedColor.Location = new Point(278, 533);
            panelSelectedColor.Name = "panelSelectedColor";
            panelSelectedColor.Size = new Size(73, 51);
            panelSelectedColor.TabIndex = 27;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.GhostWhite;
            ClientSize = new Size(1168, 630);
            Controls.Add(panelSelectedColor);
            Controls.Add(lblSpaceInfo);
            Controls.Add(trackRotate);
            Controls.Add(trackZoom);
            Controls.Add(pictureBoxSpace);
            Controls.Add(chkC4);
            Controls.Add(chkC3);
            Controls.Add(chkC2);
            Controls.Add(chkC1);
            Controls.Add(lblV4);
            Controls.Add(lblV3);
            Controls.Add(lblV2);
            Controls.Add(lblV1);
            Controls.Add(trackC4);
            Controls.Add(trackC3);
            Controls.Add(trackC2);
            Controls.Add(trackC1);
            Controls.Add(lblC4);
            Controls.Add(lblC3);
            Controls.Add(lblC2);
            Controls.Add(lblC1);
            Controls.Add(cmbColorMode);
            Controls.Add(lblColorInfo);
            Controls.Add(btnOpen);
            Controls.Add(pictureBox1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackC1).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackC2).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackC3).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackC4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSpace).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackZoom).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackRotate).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnOpen;
        private Label lblColorInfo;
        private ComboBox cmbColorMode;
        private Label lblC1;
        private Label lblC2;
        private Label lblC3;
        private Label lblC4;
        private TrackBar trackC1;
        private TrackBar trackC2;
        private TrackBar trackC3;
        private TrackBar trackC4;
        private Label lblV1;
        private Label lblV2;
        private Label lblV3;
        private Label lblV4;
        private CheckBox chkC1;
        private CheckBox chkC2;
        private CheckBox chkC3;
        private CheckBox chkC4;
        private PictureBox pictureBoxSpace;
        private TrackBar trackZoom;
        private TrackBar trackRotate;
        private Label lblSpaceInfo;
        private Panel panelSelectedColor;
    }
}
