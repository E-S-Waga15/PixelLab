namespace PixelLab
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            btnOpen = new Button();
            lblColorInfo = new Label();
            lblColorInfoTitle = new Label();
            cmbColorMode = new ComboBox();
            lblC1 = new Label();
            lblC2 = new Label();
            lblC3 = new Label();
            lblC4 = new Label();
            trackC1 = new ModernTrackBar();
            trackC2 = new ModernTrackBar();
            trackC3 = new ModernTrackBar();
            trackC4 = new ModernTrackBar();
            lblV1 = new Label();
            lblV2 = new Label();
            lblV3 = new Label();
            lblV4 = new Label();
            chkC1 = new CheckBox();
            chkC2 = new CheckBox();
            chkC3 = new CheckBox();
            chkC4 = new CheckBox();
            pictureBoxSpace = new PictureBox();
            elementHost = new System.Windows.Forms.Integration.ElementHost();
            trackZoom = new ModernTrackBar();
            trackRotate = new ModernTrackBar();
            lblSpaceInfo = new Label();
            panelSelectedColor = new Panel();
            cmbViewMode = new ComboBox();
            cmbQuantColors = new ComboBox();
            chkQuantizeEnable = new CheckBox();
            btnReset = new Button();
            lblImageProperties = new Label();
            btnSave = new Button();
            panelLeft = new GlassPanel();
            panelCenter = new Panel();
            panelRight = new GlassPanel();
            panelToolbar = new GlassPanel();
            panelWorkspace = new GlassPanel();
            panelColorOverlay = new GlassPanel();
            panelSpaceView = new GlassPanel();
            panelImageProps = new GlassPanel();
            lblAppTitle = new Label();
            lblOpenHint = new Label();
            lblExportCaption = new Label();
            lblPropsTitle = new Label();
            lblColorModeTitle = new Label();
            lblQuantTitle = new Label();
            lblViewTitle = new Label();
            lblChannelsTitle = new Label();
            btnView2D = new Button();
            btnView3D = new Button();
            lblZoom = new Label();
            lblRotate = new Label();

            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSpace).BeginInit();
            panelLeft.SuspendLayout();
            panelCenter.SuspendLayout();
            panelRight.SuspendLayout();
            panelToolbar.SuspendLayout();
            panelWorkspace.SuspendLayout();
            panelColorOverlay.SuspendLayout();
            panelSpaceView.SuspendLayout();
            panelImageProps.SuspendLayout();
            SuspendLayout();

            // --- pictureBox1 ---
            pictureBox1.BackColor = UiTheme.WorkspaceBg;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(8, 8);
            pictureBox1.Margin = new Padding(8);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(584, 364);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // --- btnOpen ---
            btnOpen.FlatStyle = FlatStyle.Flat;

            // حجم 20F ممتاز جداً ليجمع بين الأيقونة والكلمة داخل زر حجمه 220x110
            btnOpen.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
            btnOpen.Location = new Point(15, 48);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(220, 110);
            btnOpen.TabIndex = 1;

            // وضع الأيقونة مع النص هنا 👇
            btnOpen.Text = "➕ Add New";

            btnOpen.TextAlign = ContentAlignment.MiddleCenter;
            btnOpen.Padding = new Padding(0);
            btnOpen.UseVisualStyleBackColor = false;
            btnOpen.Click += btnOpen_Click;

            // --- btnSave ---
            btnSave.Location = new Point(15, 200);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(105, 36);
            btnSave.TabIndex = 2;
            btnSave.Text = "💾 Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;

            // --- btnReset (toolbar) ---
            btnReset.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnReset.Location = new Point(500, 10);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(90, 32);
            btnReset.TabIndex = 3;
            btnReset.Text = "RESET";
            btnReset.UseVisualStyleBackColor = false;
            btnReset.Click += btnReset_Click;

            // --- lblImageProperties ---
            lblImageProperties.Dock = DockStyle.Fill;
            lblImageProperties.Font = new Font("Segoe UI", 9F);
            lblImageProperties.Size = new Size(200, 160);
            lblImageProperties.Name = "lblImageProperties";
            lblImageProperties.Padding = new Padding(6, 6, 6, 6);
            lblImageProperties.Text = "\r\n \r\n \r\n No image loaded";

            // --- lblColorInfo (bottom panel — selected color details) ---
            lblColorInfoTitle.AutoSize = false;
            lblColorInfoTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblColorInfoTitle.Location = new Point(14, 182);
            lblColorInfoTitle.Padding = new Padding(0, 0, 0, 0);
            lblColorInfoTitle.Name = "lblColorInfoTitle";
            lblColorInfoTitle.Size = new Size(400, 25);
            lblColorInfoTitle.Text = "معلومات اللون المختار";

            lblColorInfo.AutoSize = false;
            lblColorInfo.Font = new Font("Segoe UI", 8.5F);
           lblColorInfo.Location = new Point(14, 211);
            lblColorInfo.Name = "lblColorInfo";
            lblColorInfo.Size = new Size(580, 100);
            lblColorInfo.TabIndex = 5;
            lblColorInfo.Text = "مرّر المؤشر فوق الصورة أو الفضاء اللوني لاختيار بكسل";

            // --- cmbColorMode ---
            cmbColorMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbColorMode.FormattingEnabled = true;
            cmbColorMode.Location = new Point(200, 42);
            cmbColorMode.Name = "cmbColorMode";
            cmbColorMode.Size = new Size(200, 28);
            cmbColorMode.TabIndex = 6;

            // --- cmbViewMode (hidden, synced by 2D/3D buttons) ---
            cmbViewMode.FormattingEnabled = true;
            cmbViewMode.Location = new Point(-500, -500);
            cmbViewMode.Name = "cmbViewMode";
            cmbViewMode.Size = new Size(80, 28);
            cmbViewMode.TabIndex = 7;
            cmbViewMode.Visible = false;

            // --- channel labels & tracks ---
            lblC1.AutoSize = false;
            lblC1.Location = new Point(12, 290);
            lblC1.Name = "lblC1";
            lblC1.Text = "R";
            lblC2.AutoSize = false;
            lblC2.Location = new Point(12, 350);
            lblC2.Name = "lblC2";
            lblC2.Text = "G";
            lblC3.AutoSize = false;
            lblC3.Location = new Point(12, 410);
            lblC3.Name = "lblC3";
            lblC3.Text = "B";
            lblC4.AutoSize = false;
            lblC4.Location = new Point(12, 470);
            lblC4.Name = "lblC4";
            lblC4.Text = "K";

            trackC1.Location = new Point(42, 282);
            trackC1.Maximum = 255;
            trackC1.Name = "trackC1";
            trackC1.Size = new Size(50, 28);
            trackC1.TabIndex = 11;
            trackC2.Location = new Point(42, 342);
            trackC2.Maximum = 255;
            trackC2.Name = "trackC2";
            trackC2.Size = new Size(50, 28);
            trackC2.TabIndex = 12;
            trackC3.Location = new Point(42, 402);
            trackC3.Maximum = 255;
            trackC3.Name = "trackC3";
            trackC3.Size = new Size(50, 28);
            trackC3.TabIndex = 13;
            trackC4.Location = new Point(42, 462);
            trackC4.Maximum = 255;
            trackC4.Name = "trackC4";
            trackC4.Size = new Size(50, 28);
            trackC4.TabIndex = 14;

            lblV1.AutoSize = true;
            lblV1.Location = new Point(148, 290);
            lblV1.Name = "lblV1";
            lblV1.Text = "0";
            lblV2.AutoSize = true;
            lblV2.Location = new Point(148, 350);
            lblV2.Name = "lblV2";
            lblV2.Text = "0";
            lblV3.AutoSize = true;
            lblV3.Location = new Point(148, 410);
            lblV3.Name = "lblV3";
            lblV3.Text = "0";
            lblV4.AutoSize = true;
            lblV4.Location = new Point(148, 470);
            lblV4.Name = "lblV4";
            lblV4.Text = "0";

            chkC1.AutoSize = true;
            chkC1.Location = new Point(100, 286);
            chkC1.Name = "chkC1";
            chkC1.Text = "On";
            chkC2.AutoSize = true;
            chkC2.Location = new Point(100, 346);
            chkC2.Name = "chkC2";
            chkC2.Text = "On";
            chkC3.AutoSize = true;
            chkC3.Location = new Point(100, 406);
            chkC3.Name = "chkC3";
            chkC3.Text = "On";
            chkC4.AutoSize = true;
            chkC4.Location = new Point(100, 466);
            chkC4.Name = "chkC4";
            chkC4.Text = "On";

            // --- color space views (right panel) ---
            pictureBoxSpace.BackColor = UiTheme.WorkspaceBg;
            pictureBoxSpace.Dock = DockStyle.Fill;
            pictureBoxSpace.Name = "pictureBoxSpace";
            pictureBoxSpace.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSpace.TabIndex = 23;
            pictureBoxSpace.TabStop = false;

            elementHost.Dock = DockStyle.Fill;
            elementHost.Name = "elementHost";
            elementHost.TabIndex = 24;
            elementHost.Text = "elementHost";

            // --- toolbar zoom/rotate ---
            lblZoom.AutoSize = true;
            lblZoom.Location = new Point(14, 16);
            lblZoom.Name = "lblZoom";
            lblZoom.Text = "Zoom: 100%";

            trackZoom.Location = new Point(110, 14);
            trackZoom.Maximum = 300;
            trackZoom.Minimum = 50;
            trackZoom.Name = "trackZoom";
            trackZoom.Padding = new Padding(4, 0, 0, 0);
            trackZoom.Size = new Size(160, 28);
            trackZoom.TabIndex = 24;
            trackZoom.Value = 100;

            // --- lblBrightness ---
            lblBrightness = new Label();
            lblBrightness.AutoSize = true;
            lblBrightness.Location = new Point(220, 26);
            lblBrightness.Name = "lblBrightness";
            lblBrightness.Text = "Brightness: 0";

            // --- trackBrightness ---
            trackBrightness = new ModernTrackBar();
            trackBrightness.Location = new Point(280, 24);
            trackBrightness.Maximum = 100;
            trackBrightness.Minimum = -100;
            trackBrightness.Name = "trackBrightness";
            trackBrightness.Padding = new Padding(4, 0, 0, 0);
            trackBrightness.Size = new Size(140, 28);
            trackBrightness.TabIndex = 26;
            trackBrightness.Value = 0;

            lblRotate.AutoSize = true;
            lblRotate.Location = new Point(290, 16);
            lblRotate.Name = "lblRotate";
            lblRotate.Text = "Rotate: 0°";

            trackRotate.Location = new Point(380, 14);
            trackRotate.Maximum = 360;
            trackRotate.Name = "trackRotate";
            trackRotate.Size = new Size(110, 28);
            trackRotate.TabIndex = 25;

            // ensure brightness controls are added to toolbar
            panelToolbar.Controls.Add(lblBrightness);
            panelToolbar.Controls.Add(trackBrightness);

            lblSpaceInfo.AutoSize = true;
            lblSpaceInfo.Location = new Point(-500, -500);
            lblSpaceInfo.Name = "lblSpaceInfo";
            lblSpaceInfo.Visible = false;

            // --- color overlay (positions set in LayoutColorOverlay) ---
            panelSelectedColor.Location = new Point(16, 14);
            panelSelectedColor.Name = "panelSelectedColor";
            panelSelectedColor.Size = new Size(56, 56);

            lblColorModeTitle.AutoSize = false;
            lblColorModeTitle.Location = new Point(88, 14);
            lblColorModeTitle.Name = "lblColorModeTitle";
            lblColorModeTitle.Size = new Size(200, 20);
            lblColorModeTitle.Text = "فضاء لوني / Color Space";

            cmbColorMode.Location = new Point(88, 46);
            cmbColorMode.Size = new Size(200, 28);

            lblViewTitle.AutoSize = false;
            lblViewTitle.Location = new Point(400, 14);
            lblViewTitle.Name = "lblViewTitle";
            lblViewTitle.Size = new Size(200, 20);
            lblViewTitle.Text = "عرض الفضاء";

            btnView2D.Location = new Point(400, 38);
            btnView2D.Name = "btnView2D";
            btnView2D.Size = new Size(95, 48);
            btnView2D.TabIndex = 40;
            btnView2D.Text = "2D View";
            btnView2D.UseVisualStyleBackColor = false;

            btnView3D.Location = new Point(503, 38);
            btnView3D.Name = "btnView3D";
            btnView3D.Size = new Size(95, 48);
            btnView3D.TabIndex = 41;
            btnView3D.Text = "3D View";
            btnView3D.UseVisualStyleBackColor = false;

            lblQuantTitle.AutoSize = false;
            lblQuantTitle.Location = new Point(16, 100);
            lblQuantTitle.Name = "lblQuantTitle";
            lblQuantTitle.Size = new Size(300, 20);
            lblQuantTitle.Text = "عدد ألوان الصورة";

            cmbQuantColors.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbQuantColors.FormattingEnabled = true;
            cmbQuantColors.Items.AddRange(new object[] { "2", "4", "8", "16", "32", "64", "128", "256" });
            cmbQuantColors.Location = new Point(16, 132);
            cmbQuantColors.Name = "cmbQuantColors";
            cmbQuantColors.Size = new Size(110, 28);
            cmbQuantColors.TabIndex = 28;
            cmbQuantColors.SelectedIndex = 7;

            chkQuantizeEnable.AutoSize = true;
            chkQuantizeEnable.Location = new Point(140, 134);
            chkQuantizeEnable.Name = "chkQuantizeEnable";
            chkQuantizeEnable.Size = new Size(90, 24);
            chkQuantizeEnable.TabIndex = 29;
            chkQuantizeEnable.Text = "تفعيل";

            // --- section panels ---
            panelLeft.CornerRadius = 14;
            panelLeft.Location = new Point(12, 12);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new Size(250, 676);
            panelLeft.Controls.Add(lblAppTitle);
            panelLeft.Controls.Add(btnOpen);
            panelLeft.Controls.Add(lblOpenHint);
            panelLeft.Controls.Add(btnSave);
            panelLeft.Controls.Add(btnReset);
            panelLeft.Controls.Add(lblExportCaption);
            panelLeft.Controls.Add(panelImageProps);

            lblAppTitle.AutoSize = true;
            lblAppTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblAppTitle.Location = new Point(15, 12);
            lblAppTitle.Name = "lblAppTitle";
            lblAppTitle.Text = "PixelLab";

            lblOpenHint.AutoSize = true;
            lblOpenHint.Location = new Point(15, 162);
            lblOpenHint.Name = "lblOpenHint";
            lblOpenHint.MaximumSize = new Size(220, 0);
            lblOpenHint.Text = "Add / Open / Replace Image";

            lblExportCaption.AutoSize = true;
            lblExportCaption.Location = new Point(130, 242);
            lblExportCaption.Name = "lblExportCaption";
            lblExportCaption.Text = "Export / Save";

            btnReset.Location = new Point(130, 200);
            btnReset.Size = new Size(105, 36);
            btnReset.Text = "↺ Reset";

            panelImageProps.CornerRadius = 10;
            panelImageProps.Location = new Point(15, 280);
            panelImageProps.Name = "panelImageProps";
            panelImageProps.Size = new Size(220, 160);
            panelImageProps.Controls.Add(lblPropsTitle);
            panelImageProps.Controls.Add(lblImageProperties);

            lblPropsTitle.Dock = DockStyle.Top;
            lblPropsTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblPropsTitle.Height = 28;
            lblPropsTitle.Name = "lblPropsTitle";
            lblPropsTitle.Padding = new Padding(6, 6, 0, 0);
            lblPropsTitle.Text = "IMAGE PROPERTIES";

            panelCenter.BackColor = UiTheme.Background;
            panelCenter.Location = new Point(272, 12);
            panelCenter.Name = "panelCenter";
            panelCenter.Size = new Size(480, 676);
            panelCenter.Controls.Add(panelToolbar);
            panelCenter.Controls.Add(panelWorkspace);
            panelCenter.Controls.Add(panelColorOverlay);

            panelToolbar.CornerRadius = 10;
            panelToolbar.Dock = DockStyle.Top;
            panelToolbar.Height = 64;
            panelToolbar.Name = "panelToolbar";
            panelToolbar.Controls.Add(lblZoom);
            panelToolbar.Controls.Add(trackZoom);
            panelToolbar.Controls.Add(lblRotate);
            panelToolbar.Controls.Add(trackRotate);

            panelWorkspace.CornerRadius = 12;
            panelWorkspace.StrongGlow = true;
            panelWorkspace.Location = new Point(0, 58);
            panelWorkspace.Name = "panelWorkspace";
            panelWorkspace.Size = new Size(620, 388);
            panelWorkspace.Controls.Add(pictureBox1);

            panelColorOverlay.CornerRadius = 12;
            panelColorOverlay.Location = new Point(0, 454);
            panelColorOverlay.Name = "panelColorOverlay";
            panelColorOverlay.Size = new Size(620, 300);
            panelColorOverlay.Controls.Add(lblColorInfo);
            panelColorOverlay.Controls.Add(lblColorInfoTitle);
            panelColorOverlay.Controls.Add(panelSelectedColor);
            panelColorOverlay.Controls.Add(lblColorModeTitle);
            panelColorOverlay.Controls.Add(cmbColorMode);
            panelColorOverlay.Controls.Add(lblViewTitle);
            panelColorOverlay.Controls.Add(btnView2D);
            panelColorOverlay.Controls.Add(btnView3D);
            panelColorOverlay.Controls.Add(lblQuantTitle);
            panelColorOverlay.Controls.Add(cmbQuantColors);
            panelColorOverlay.Controls.Add(chkQuantizeEnable);

            panelRight.CornerRadius = 14;
            panelRight.Location = new Point(760, 12);
            panelRight.Name = "panelRight";
            panelRight.Size = new Size(430, 676);
            panelRight.Controls.Add(panelSpaceView);
            panelRight.Controls.Add(lblChannelsTitle);
            panelRight.Controls.Add(lblC1);
            panelRight.Controls.Add(trackC1);
            panelRight.Controls.Add(lblV1);
            panelRight.Controls.Add(chkC1);
            panelRight.Controls.Add(lblC2);
            panelRight.Controls.Add(trackC2);
            panelRight.Controls.Add(lblV2);
            panelRight.Controls.Add(chkC2);
            panelRight.Controls.Add(lblC3);
            panelRight.Controls.Add(trackC3);
            panelRight.Controls.Add(lblV3);
            panelRight.Controls.Add(chkC3);
            panelRight.Controls.Add(lblC4);
            panelRight.Controls.Add(trackC4);
            panelRight.Controls.Add(lblV4);
            panelRight.Controls.Add(chkC4);

            panelSpaceView.CornerRadius = 12;
            panelSpaceView.StrongGlow = true;
            panelSpaceView.Location = new Point(10, 12);
            panelSpaceView.Name = "panelSpaceView";
            panelSpaceView.Size = new Size(250, 250);
            panelSpaceView.Controls.Add(pictureBoxSpace);
            panelSpaceView.Controls.Add(elementHost);

            lblChannelsTitle.AutoSize = true;
            lblChannelsTitle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblChannelsTitle.Location = new Point(12, 270);
            lblChannelsTitle.Name = "lblChannelsTitle";
            lblChannelsTitle.Text = "COLOR CHANNELS";

            // --- Form ---
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = UiTheme.Background;
            ClientSize = new Size(1200, 700);
            Font = new Font("Segoe UI", 9.5F);
            ForeColor = UiTheme.TextPrimary;
            MinimumSize = new Size(1100, 650);
            Controls.Add(panelRight);
            Controls.Add(panelCenter);
            Controls.Add(panelLeft);
            Controls.Add(cmbViewMode);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PixelLab — AeroEdit Style";

            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSpace).EndInit();
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelCenter.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            panelToolbar.ResumeLayout(false);
            panelToolbar.PerformLayout();
            panelWorkspace.ResumeLayout(false);
            panelColorOverlay.ResumeLayout(false);
            panelColorOverlay.PerformLayout();
            panelSpaceView.ResumeLayout(false);
            panelImageProps.ResumeLayout(false);
            panelImageProps.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private Button btnOpen;
        private Label lblColorInfo;
        private Label lblColorInfoTitle;
        private ComboBox cmbColorMode;
        private Label lblC1;
        private Label lblC2;
        private Label lblC3;
        private Label lblC4;
        private ModernTrackBar trackC1;
        private ModernTrackBar trackC2;
        private ModernTrackBar trackC3;
        private ModernTrackBar trackC4;
        private Label lblV1;
        private Label lblV2;
        private Label lblV3;
        private Label lblV4;
        private CheckBox chkC1;
        private CheckBox chkC2;
        private CheckBox chkC3;
        private CheckBox chkC4;
        private PictureBox pictureBoxSpace;
        private System.Windows.Forms.Integration.ElementHost elementHost;
        private ModernTrackBar trackZoom;
        private ModernTrackBar trackRotate;
        private Label lblSpaceInfo;
        private Panel panelSelectedColor;
        private ComboBox cmbViewMode;
        private ComboBox cmbQuantColors;
        private CheckBox chkQuantizeEnable;
        private Button btnReset;
        private Label lblImageProperties;
        private Button btnSave;
        private GlassPanel panelLeft;
        private Panel panelCenter;
        private GlassPanel panelRight;
        private GlassPanel panelToolbar;
        private GlassPanel panelWorkspace;
        private GlassPanel panelColorOverlay;
        private GlassPanel panelSpaceView;
        private GlassPanel panelImageProps;
        private Label lblAppTitle;
        private Label lblOpenHint;
        private Label lblExportCaption;
        private Label lblPropsTitle;
        private Label lblColorModeTitle;
        private Label lblQuantTitle;
        private Label lblViewTitle;
        private Label lblChannelsTitle;
        private Button btnView2D;
        private Button btnView3D;
        private Label lblZoom;
        private Label lblRotate;
        private Label lblBrightness;
        private ModernTrackBar trackBrightness;
    }
}
