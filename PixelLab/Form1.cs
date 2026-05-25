using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace PixelLab
{
    // بنية لتخزين نقطة في الفضاء اللوني ثلاثي الأبعاد
    public struct PixelPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Color DrawColor { get; set; }

        public PixelPoint(float x, float y, float z, Color color)
        {
            X = x;
            Y = y;
            Z = z;
            DrawColor = color;
        }
    }

    public partial class Form1 : Form
    {
        private Bitmap originalImage;
        private Bitmap editedImage;
        private Bitmap colorSpaceImage;
        private string currentImageName = string.Empty;
        private string currentImagePath = string.Empty;
        private bool isUpdatingControls = false;
        private bool isColorSelectionLocked = false;
        private PixelLab.Controls.ColorSpace3DControl colorSpace3D;

        // متغيرات الحالة الرياضية للعرض ثلاثي الأبعاد
        private float customYaw = 30f;
        private float customPitch = -25f;
        private float customZoom = 180f;
        private List<PixelPoint> customVisualPoints = new List<PixelPoint>();
        private Point lastMousePos = Point.Empty;
        private bool isDraggingRotation = false;

        // متغيرات الحالة للعرض الثاني (Visualization 2)
        private PictureBox? pictureBoxSpace2;
        private System.Windows.Forms.Integration.ElementHost? elementHost2;
        private float customYaw2 = 30f;
        private float customPitch2 = -25f;
        private float customZoom2 = 180f;
        private List<PixelPoint> customVisualPoints2 = new List<PixelPoint>();
        private Point lastMousePos2 = Point.Empty;
        private bool isDraggingRotation2 = false;

        public Form1()
        {
            InitializeComponent();
            ApplyGlassTheme();

            pictureBox1.AllowDrop = true;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;


            // تعبئة أوضاع الألوان
            cmbColorMode.Items.AddRange(new object[] { "RGB", "HSV", "CMYK", "YUV", "LAB", "YCbCr" });
            cmbColorMode.SelectedIndexChanged += cmbColorMode_SelectedIndexChanged;

            // تعبئة أوضاع العرض (مخفي — يُحدَّث عبر أزرار 2D/3D)
            cmbViewMode.Items.AddRange(new object[] { "2D", "3D" });
            cmbViewMode.SelectedIndexChanged += cmbViewMode_SelectedIndexChanged;
            cmbViewMode.SelectedIndex = 0;
            btnView2D.Click += (s, e) => SetViewMode(false);
            btnView3D.Click += (s, e) => SetViewMode(true);
            UpdateViewModeButtons(false);

            // Initialize quantization controls
            if (cmbQuantColors != null)
            {
                // values already added in Designer; ensure event wiring
                cmbQuantColors.SelectedIndexChanged += (s, e) => ApplySelectedColorMode(s, e);
            }
            if (chkQuantizeEnable != null)
            {
                chkQuantizeEnable.CheckedChanged += (s, e) => ApplySelectedColorMode(s, e);
                chkQuantizeEnable.Checked = true; // default ON
            }

            // تهيئة عنصر WPF والـ Host (تأكد أن الاسم مطابق لما هو موجود في الـ Designer)
            colorSpace3D = new PixelLab.Controls.ColorSpace3DControl();
            if (elementHost != null)
            {
                elementHost.Child = colorSpace3D;
                elementHost.Visible = false;
                // الاشتراك في أحداث اختيار اللون من المشهد ثلاثي الأبعاد
                colorSpace3D.ColorClickedFrom3D += ColorSpaceControl_ColorClickedFrom3D;
                colorSpace3D.ColorSelectedFrom3D += ColorSpaceControl_ColorSelectedFrom3D;
                colorSpace3D.ColorHoveredFrom3D += ColorSpaceControl_ColorHoveredFrom3D;
            }

            SetupPixelPickerCursor(pictureBox1);
            SetupPixelPickerCursor(pictureBoxSpace);
            SetupPixelPickerCursor(elementHost);

            // ربط الأحداث
            trackC1.ValueChanged += ApplySelectedColorMode;
            trackC2.ValueChanged += ApplySelectedColorMode;
            trackC3.ValueChanged += ApplySelectedColorMode;
            trackC4.ValueChanged += ApplySelectedColorMode;

            pictureBox1.MouseClick += PictureBox1_MouseClick;
            pictureBox1.MouseDoubleClick += PictureBox1_MouseDoubleClick;
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;

            // Click on pictureBox should pick the pixel color and synchronize UI and 3D marker
            void pictureBox1_MouseDown(object sender, MouseEventArgs e)
            {
                if (editedImage == null) return;
                int imageX = e.X * editedImage.Width / pictureBox1.Width;
                int imageY = e.Y * editedImage.Height / pictureBox1.Height;
                if (imageX < 0 || imageX >= editedImage.Width || imageY < 0 || imageY >= editedImage.Height) return;
                var c = editedImage.GetPixel(imageX, imageY);
                SynchronizeAndDisplaySystemInfo(c.R, c.G, c.B);
                if (colorSpace3D != null) colorSpace3D.MoveMarkerToRgb(c.R, c.G, c.B);
            }
            pictureBox1.DragEnter += pictureBox1_DragEnter;
            pictureBox1.DragDrop += pictureBox1_DragDrop;

            chkC1.CheckedChanged += ApplySelectedColorMode;
            chkC2.CheckedChanged += ApplySelectedColorMode;
            chkC3.CheckedChanged += ApplySelectedColorMode;
            chkC4.CheckedChanged += ApplySelectedColorMode;

            // make zoom range wider and rotation full circle for more control
            trackZoom.Minimum = 50; trackZoom.Maximum = 300; trackZoom.Value = 180;
            trackRotate.Minimum = -180; trackRotate.Maximum = 180; trackRotate.Value = 0;
            trackZoom.ValueChanged += UpdateColorSpaceView;
            trackRotate.ValueChanged += UpdateColorSpaceView;
            trackRotate.ValueChanged += (s, e) => { if (colorSpace3D != null) colorSpace3D.SetCamera(trackZoom.Value, trackRotate.Value); UpdateColorSpaceView(this, EventArgs.Empty); };
            trackZoom.ValueChanged += (s, e) => { if (colorSpace3D != null) colorSpace3D.SetCamera(trackZoom.Value, trackRotate.Value); UpdateColorSpaceView(this, EventArgs.Empty); };
            pictureBoxSpace.MouseClick += PictureBoxSpace_MouseClick;
            pictureBoxSpace.MouseDoubleClick += PictureBoxSpace_MouseDoubleClick;
            pictureBoxSpace.MouseMove += PictureBoxSpace_MouseMove;

            // Brightness control wiring (added control in Designer)
            if (trackBrightness != null)
            {
                trackBrightness.Minimum = -100;
                trackBrightness.Maximum = 100;
                trackBrightness.Value = 0;
                trackBrightness.ValueChanged += ApplySelectedColorMode;
                trackBrightness.ValueChanged += (s, e) => { try { lblBrightness.Text = $"Brightness: {trackBrightness.Value}"; } catch { } };
            }

            // 3D host double-click commits changes
            if (elementHost != null)
            {
                elementHost.MouseDoubleClick += ElementHost_MouseDoubleClick;
            }

            // إنشاء العناصر الثانية (Visualization 2)
            InitializeSecondVisualization();

            // تفعيل القنوات افتراضياً
            chkC1.Checked = true;
            chkC2.Checked = true;
            chkC3.Checked = true;
            chkC4.Checked = true;

            cmbColorMode.SelectedIndex = 0;
            Resize += Form1_Resize;
            Form1_Resize(this, EventArgs.Empty);
        }

        private void InitializeSecondVisualization()
        {
            // إنشاء PictureBox الثاني
            pictureBoxSpace2 = new PictureBox();
            pictureBoxSpace2.Name = "pictureBoxSpace2";
            pictureBoxSpace2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxSpace2.BackColor = Color.Black;
            pictureBoxSpace2.MouseDown += PictureBoxSpace2_MouseDown;
            pictureBoxSpace2.MouseUp += PictureBoxSpace2_MouseUp;
            pictureBoxSpace2.MouseMove += PictureBoxSpace2_MouseMove;
            pictureBoxSpace2.MouseWheel += PictureBoxSpace2_MouseWheel;
            pictureBoxSpace2.Paint += PictureBoxSpace2_Paint;

            // إنشاء ElementHost الثاني
            elementHost2 = new System.Windows.Forms.Integration.ElementHost();
            elementHost2.Name = "elementHost2";
            elementHost2.BackColor = UiTheme.WorkspaceBg;

            // إضافتهما إلى panelRight أسفل panelSpaceView
            if (panelRight != null)
            {
                panelRight.Controls.Add(pictureBoxSpace2);
                panelRight.Controls.Add(elementHost2);
                LayoutSecondVisualization();
            }
        }

        private void LayoutSecondVisualization()
        {
            if (panelRight == null || panelSpaceView == null) return;

            int w = panelRight.ClientSize.Width;
            int h = panelRight.ClientSize.Height;
            const int pad = 10;

            // تقسيم المساحة بين العرضين
            int spaceH = (int)(h * 0.25);
            int spaceV2Y = panelSpaceView.Bottom + 10;

            // تعيين الحجم والموضع للعرض الثاني
            pictureBoxSpace2.SetBounds(pad, spaceV2Y, w - pad * 2, spaceH);
            elementHost2.SetBounds(pad, spaceV2Y, w - pad * 2, spaceH);
        }

        private void ApplyGlassTheme()
        {
            UiTheme.StyleFlatButton(btnOpen, accent: true);
            UiTheme.StyleFlatButton(btnSave);
            UiTheme.StyleFlatButton(btnReset);
            UiTheme.StyleFlatButton(btnView2D);
            UiTheme.StyleFlatButton(btnView3D);
            UiTheme.StyleComboBox(cmbColorMode);
            UiTheme.StyleComboBox(cmbQuantColors);
            UiTheme.StyleCheckBox(chkQuantizeEnable);

            foreach (var lbl in new[] { lblAppTitle, lblOpenHint, lblExportCaption, lblPropsTitle,
                lblColorModeTitle, lblQuantTitle, lblViewTitle, lblChannelsTitle, lblZoom, lblRotate,
                lblC1, lblC2, lblC3, lblC4, lblV1, lblV2, lblV3, lblV4, lblImageProperties,
                lblColorInfoTitle, lblColorInfo })
                UiTheme.StyleLabel(lbl);
            lblColorInfoTitle.Font = UiTheme.SmallFont;
            lblColorInfo.Font = UiTheme.SmallFont;

            panelSelectedColor.Paint += PanelSelectedColor_Paint;
            panelSelectedColor.BackColor = UiTheme.WorkspaceBg;
        }

        private void PanelSelectedColor_Paint(object? sender, PaintEventArgs e)
        {
            var panel = (Panel)sender!;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(2, 2, panel.Width - 5, panel.Height - 5);
            using var path = UiTheme.RoundedRect(rect, 8);
            using var glow = new Pen(Color.FromArgb(120, UiTheme.NeonCyan), 2f);
            e.Graphics.DrawPath(glow, path);
            if (panel.BackColor != UiTheme.WorkspaceBg)
            {
                using var fill = new SolidBrush(panel.BackColor);
                e.Graphics.FillPath(fill, path);
            }
        }

        private void SetViewMode(bool threeD)
        {
            if (cmbViewMode.SelectedIndex == (threeD ? 1 : 0)) return;
            cmbViewMode.SelectedIndex = threeD ? 1 : 0;
        }

        private void UpdateViewModeButtons(bool is3D)
        {
            UiTheme.StyleToggleButton(btnView2D, !is3D);
            UiTheme.StyleToggleButton(btnView3D, is3D);
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (panelCenter == null) return;
            int h = panelCenter.ClientSize.Height;
            int toolbarH = panelToolbar?.Height ?? 48;
            const int overlayH = 560;
            const int gap = 6;
            if (panelWorkspace != null)
                panelWorkspace.SetBounds(0, toolbarH + gap, panelCenter.ClientSize.Width,
                    Math.Max(180, h - toolbarH - overlayH - gap * 2));
            if (panelColorOverlay != null)
                panelColorOverlay.SetBounds(0, panelWorkspace!.Bottom + gap, panelCenter.ClientSize.Width, overlayH);

            LayoutToolbar();
            LayoutColorOverlay();
            LayoutRightChannels();
        }

        private void LayoutToolbar()
        {
            if (panelToolbar == null) return;
            int w = panelToolbar.ClientSize.Width;
            const int pad = 12;
            const int labelW = 82;
            const int sliderH = 28;
            // Three equal columns: Zoom | Brightness | Rotate
            int colW = Math.Max(120, (w - pad * 2) / 3);
            int col1X = pad;
            int col2X = pad + colW;
            int col3X = pad + colW * 2;

            int labelY = (panelToolbar.Height - sliderH) / 2 - 2;
            // Zoom
            lblZoom.SetBounds(col1X, labelY, labelW, 22);
            trackZoom.SetBounds(col1X + labelW + 6, labelY, colW - labelW - 16, sliderH);
            // Brightness
            lblBrightness.SetBounds(col2X, labelY, labelW, 22);
            trackBrightness.SetBounds(col2X + labelW + 6, labelY, colW - labelW - 16, sliderH);
            // Rotate
            lblRotate.SetBounds(col3X, labelY, labelW, 22);
            trackRotate.SetBounds(col3X + labelW + 6, labelY, colW - labelW - 16, sliderH);
        }

        private void LayoutColorOverlay()
        {
            if (panelColorOverlay == null) return;
            int w = panelColorOverlay.ClientSize.Width;
            const int pad = 14;
            const int swatch = 56;
            const int labelH = 52;
            const int labelGap = 10;
            const int comboH = 28;
            const int btnH = 46;
            const int colGap = 12;

            panelSelectedColor.SetBounds(pad, pad, swatch, swatch);

            int viewW = 210;
            int viewLeft = w - viewW - pad;
            int modeLeft = pad + swatch + colGap;
            int modeWidth = Math.Max(120, viewLeft - modeLeft - colGap);
            int controlsTop = pad + labelH + labelGap;

            lblColorModeTitle.SetBounds(modeLeft, pad, modeWidth, labelH);
            cmbColorMode.SetBounds(modeLeft, controlsTop, modeWidth, comboH);

            lblViewTitle.SetBounds(viewLeft, pad, viewW, labelH);
            int btnW = (viewW - colGap) / 2;
            btnView2D.SetBounds(viewLeft, controlsTop, btnW, btnH);
            btnView3D.SetBounds(viewLeft + btnW + colGap, controlsTop, btnW, btnH);

            int quantLabelY = controlsTop + btnH + 14;
            lblQuantTitle.SetBounds(pad, quantLabelY, w - pad * 2, labelH);
            int quantControlsTop = quantLabelY + labelH + labelGap;
            cmbQuantColors.SetBounds(pad, quantControlsTop, 120, comboH);
            chkQuantizeEnable.SetBounds(pad + 132, quantControlsTop + 2, 110, 24);

            int colorInfoTitleY = quantControlsTop + comboH + 14;
            lblColorInfoTitle.SetBounds(pad, colorInfoTitleY, w - pad * 2, 35);
            int colorInfoY = colorInfoTitleY + 58;
            int colorInfoH = Math.Max(72, panelColorOverlay.ClientSize.Height - colorInfoY - pad);
            lblColorInfo.SetBounds(pad, colorInfoY, w - pad * 2, colorInfoH);

            lblColorModeTitle.BringToFront();
            lblViewTitle.BringToFront();
            lblQuantTitle.BringToFront();
            lblColorInfoTitle.BringToFront();
            lblColorInfo.BringToFront();
        }

        private static void SetupPixelPickerCursor(Control control)
        {
            control.MouseEnter += (_, _) =>
            {
                if (control.Enabled && control.Visible)
                    control.Cursor = Cursors.Cross;
            };
            control.MouseLeave += (_, _) => control.Cursor = Cursors.Default;
        }

        private bool TryMapPictureBoxPixel(PictureBox box, Bitmap? bmp, MouseEventArgs e, out int imageX, out int imageY)
        {
            imageX = imageY = 0;
            if (bmp == null || box.Width <= 0 || box.Height <= 0) return false;
            imageX = e.X * bmp.Width / box.Width;
            imageY = e.Y * bmp.Height / box.Height;
            return imageX >= 0 && imageX < bmp.Width && imageY >= 0 && imageY < bmp.Height;
        }

        private void PictureBox1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isColorSelectionLocked) return;

            if (!TryMapPictureBoxPixel(pictureBox1, editedImage, e, out int imageX, out int imageY)) return;
            Color color = editedImage!.GetPixel(imageX, imageY);
            UpdateColorInfoLabel(color, "الصورة", imageX, imageY);

            if (cmbColorMode.SelectedItem?.ToString() == "RGB")
            {
                isUpdatingControls = true;
                trackC1.Value = color.R;
                trackC2.Value = color.G;
                trackC3.Value = color.B;
                isUpdatingControls = false;
                UpdateTrackLabels();
                colorSpace3D?.MoveMarkerToRgb(color.R, color.G, color.B);
            }
            if (!isColorSelectionLocked)
                panelSelectedColor.BackColor = color;
        }

        private void PictureBoxSpace_MouseMove(object? sender, MouseEventArgs e)
        {
            // إذا كان يتم السحب للتدوير
            if (isDraggingRotation)
            {
                int deltaX = e.X - lastMousePos.X;
                int deltaY = e.Y - lastMousePos.Y;

                customYaw += deltaX * 0.5f;
                customPitch += deltaY * 0.5f;

                // تقيد الزوايا
                customPitch = Math.Max(-90, Math.Min(90, customPitch));

                lastMousePos = e.Location;
                pictureBoxSpace.Invalidate();
                return;
            }

            if (isColorSelectionLocked) return;

            if (pictureBoxSpace.Image == null) return;
            using var bmp = new Bitmap(pictureBoxSpace.Image);
            if (!TryMapPictureBoxPixel(pictureBoxSpace, bmp, e, out int x, out int y)) return;
            Color color = bmp.GetPixel(x, y);
            UpdateColorInfoLabel(color, "Color Space_2D", x, y);
            if (!isColorSelectionLocked)
                panelSelectedColor.BackColor = color;
        }

        private void LayoutRightChannels()
        {
            if (panelRight == null || panelSpaceView == null) return;
            int w = panelRight.ClientSize.Width;
            int h = panelRight.ClientSize.Height;
            const int pad = 10;
            int spaceH = (int)(h * 0.40);
            int spaceH2 = (int)(h * 0.25);

            // لا تغيّر حجم panelSpaceView - استخدم القيم من Designer
            // panelSpaceView.SetBounds(pad, pad, w - pad * 2, spaceH);

            // تعيين الحجم والموضع للعرض الثاني
            if (pictureBoxSpace2 != null && elementHost2 != null)
            {
                int spaceV2Y = panelSpaceView.Bottom + 10;
                pictureBoxSpace2.SetBounds(pad, spaceV2Y, w - pad * 2, spaceH2);
                elementHost2.SetBounds(pad, spaceV2Y, w - pad * 2, spaceH2);
            }

            int titleY = panelSpaceView.Bottom + (pictureBoxSpace2 != null ? spaceH2 + 20 : 10);
            lblChannelsTitle.SetBounds(pad, titleY, w - pad * 2, 22);

            int rowY = titleY + 28;
            const int rowH = 46;
            const int lblW = 30;
            const int lblH = 36;
            const int valW = 20;
            const int chkW = 70;
            int trackW = w - pad * 2 - lblW - valW - chkW - 126;

            LayoutChannelRow(lblC1, trackC1, lblV1, chkC1, pad, rowY, lblW, lblH, trackW, valW, chkW);
            LayoutChannelRow(lblC2, trackC2, lblV2, chkC2, pad, rowY + rowH, lblW, lblH, trackW, valW, chkW);
            LayoutChannelRow(lblC3, trackC3, lblV3, chkC3, pad, rowY + rowH * 2, lblW, lblH, trackW, valW, chkW);
            LayoutChannelRow(lblC4, trackC4, lblV4, chkC4, pad, rowY + rowH * 3, lblW, lblH, trackW, valW, chkW);
        }

        private static void LayoutChannelRow(Label ch, ModernTrackBar track, Label val, CheckBox on,
            int pad, int y, int lblW, int lblH, int trackW, int valW, int chkW)
        {
            ch.SetBounds(pad, y + 8, lblW, lblH);
            track.SetBounds(pad + lblW + 6, y + 8, trackW, 28);
            val.SetBounds(pad + lblW + trackW + 12, y + 8, valW, lblH);
            on.SetBounds(pad + lblW + trackW + valW + 18, y + 10, chkW, 24);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadImage(dialog.FileName);
                }
            }
        }

        private void LoadImage(string path)
        {
            originalImage = new Bitmap(path);
            editedImage = new Bitmap(originalImage);
            currentImageName = System.IO.Path.GetFileNameWithoutExtension(path);
            currentImagePath = path;
            pictureBox1.Image = editedImage;
            ResetTracks();
            GenerateCustomColorSpacePoints();
            GenerateCustomColorSpacePoints2();
            pictureBoxSpace.Invalidate();
            pictureBoxSpace2?.Invalidate();
            UpdateImageInfo(); // update the properties panel after loading
        }

        // ----- New method: UpdateImageInfo (Request #8) -----
        private void UpdateImageInfo()
        {
            if (pictureBox1?.Image != null)
            {
                string formatName = GetImageFormatName(pictureBox1.Image, currentImagePath);
                string fileSize = GetFileSizeDisplay(currentImagePath);
                lblImageProperties.Text =
                    "\r\n" +
                    $"اسم الصورة: {(string.IsNullOrWhiteSpace(currentImageName) ? "Unknown" : currentImageName)}\r\n" +
                    $"الصيغة: {formatName}\r\n" +
                    $"الحجم: {fileSize}\r\n" +
                    $"الأبعاد: {pictureBox1.Image.Width}×{pictureBox1.Image.Height} بكسل\r\n" +
                    $"Ppi: 96 - " +
                    $"Depth: 8-bit\r\n" +
                     $"Colorspace: {formatName} (Original)" 
                    ;
                  
            }
            else
            {
                lblImageProperties.Text = "No image loaded";
            }
        }

        private string GetImageFormatName(Image img, string path = "")
        {
            try
            {
                string extension = System.IO.Path.GetExtension(path ?? string.Empty).ToLowerInvariant();
                return extension switch
                {
                    ".jpg" or ".jpeg" => "JPEG",
                    ".png" => "PNG",
                    ".bmp" => "BMP",
                    ".gif" => "GIF",
                    ".tif" or ".tiff" => "TIFF",
                    _ => img.RawFormat.Equals(ImageFormat.Jpeg) ? "JPEG"
                        : img.RawFormat.Equals(ImageFormat.Png) ? "PNG"
                        : img.RawFormat.Equals(ImageFormat.Bmp) ? "BMP"
                        : img.RawFormat.Equals(ImageFormat.Gif) ? "GIF"
                        : img.RawFormat.Equals(ImageFormat.Tiff) ? "TIFF"
                        : "Unknown"
                };
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetFileSizeDisplay(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return "Unknown";
                long bytes = new System.IO.FileInfo(path).Length;
                double mb = bytes / 1024.0 / 1024.0;
                if (mb >= 1)
                    return $"{mb:F2} MB";
                return $"{bytes / 1024.0:F2} KB";
            }
            catch
            {
                return "Unknown";
            }
        }

        // ----- New method: btnReset_Click (Request #9) -----
        private void btnReset_Click(object sender, EventArgs e)
        {
            // 1. Restore original image
            if (originalImage != null)
            {
                try { editedImage?.Dispose(); } catch { }
                editedImage = new Bitmap(originalImage);
                pictureBox1.Image = editedImage;
            }

            // 2. Reset UI controls (Zoom/Rotate and color tracks)
            if (trackZoom != null) trackZoom.Value = 100;
            if (trackRotate != null) trackRotate.Value = 0;
            ResetTracks();

            // 3. Update image info display
            UpdateImageInfo();

            // 4. Regenerate both visualizations so Visualization 2 (second space) updates in real-time
            GenerateCustomColorSpacePoints();
            GenerateCustomColorSpacePoints2();
            pictureBoxSpace.Invalidate();
            pictureBoxSpace2?.Invalidate();
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("لا توجد صورة لحفظها!");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            saveFileDialog.Title = "حفظ الصورة";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
                    string extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();

                    switch (extension)
                    {
                        case ".jpg": format = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                        case ".bmp": format = System.Drawing.Imaging.ImageFormat.Bmp; break;
                    }

                    pictureBox1.Image.Save(saveFileDialog.FileName, format);
                    MessageBox.Show("تم حفظ الصورة بنجاح!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء الحفظ: " + ex.Message);
                }
            }
        }





        private void cmbColorMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetTracks();

            if (cmbColorMode.SelectedItem == null) return;
            string mode = cmbColorMode.SelectedItem.ToString();

            switch (mode)
            {
                case "RGB":
                    SetupComponents("R", "G", "B", "", -255, 255, -255, 255, -255, 255, 0, 0, false);
                    break;
                case "HSV":
                    SetupComponents("H", "S", "V", "", -180, 180, -100, 100, -100, 100, 0, 0, false);
                    break;
                case "CMYK":
                    SetupComponents("C", "M", "Y", "K", -100, 100, -100, 100, -100, 100, -100, 100, true);
                    break;
                case "YUV":
                    SetupComponents("Y", "U", "V", "", -100, 100, -100, 100, -100, 100, 0, 0, false);
                    break;
                case "LAB":
                    SetupComponents("L", "A", "B", "", -100, 100, -100, 100, -100, 100, 0, 0, false);
                    break;
                case "YCbCr":
                    SetupComponents("Y", "Cb", "Cr", "", -100, 100, -100, 100, -100, 100, 0, 0, false);
                    break;
            }

            ApplySelectedColorMode(this, EventArgs.Empty);
            DrawColorSpace(mode);
            GenerateCustomColorSpacePoints();
            GenerateCustomColorSpacePoints2();
            pictureBoxSpace.Invalidate();
            pictureBoxSpace2?.Invalidate();

            // تحديث العرض ثلاثي الأبعاد مباشرة عند تغيير النظام اللوني إن كان فعالاً
            if (elementHost != null && elementHost.Visible && colorSpace3D != null)
            {
                colorSpace3D.SetColorMode(mode);
            }
        }

        private void SetupComponents(string name1, string name2, string name3, string name4,
                                     int min1, int max1, int min2, int max2, int min3, int max3, int min4, int max4, bool showFourth)
        {
            lblC1.Text = name1; lblC2.Text = name2; lblC3.Text = name3; lblC4.Text = name4;

            trackC1.Minimum = min1; trackC1.Maximum = max1;
            trackC2.Minimum = min2; trackC2.Maximum = max2;
            trackC3.Minimum = min3; trackC3.Maximum = max3;
            trackC4.Minimum = min4; trackC4.Maximum = max4;

            lblC4.Visible = trackC4.Visible = lblV4.Visible = chkC4.Visible = showFourth;

            chkC1.Checked = chkC2.Checked = chkC3.Checked = chkC4.Checked = true;
            UpdateTrackLabels();
        }

        private void ResetTracks()
        {
            isUpdatingControls = true;
            trackC1.Value = trackC2.Value = trackC3.Value = trackC4.Value = 0;
            isUpdatingControls = false;
            UpdateTrackLabels();
        }

        private void UpdateTrackLabels()
        {
            lblV1.Text = trackC1.Value.ToString();
            lblV2.Text = trackC2.Value.ToString();
            lblV3.Text = trackC3.Value.ToString();
            lblV4.Text = trackC4.Value.ToString();
        }

        private void ApplySelectedColorMode(object sender, EventArgs e)
        {
            if (isUpdatingControls || originalImage == null || cmbColorMode.SelectedItem == null)
                return;

            UpdateTrackLabels();
            string mode = cmbColorMode.SelectedItem.ToString();

            int v1 = trackC1.Value; int v2 = trackC2.Value; int v3 = trackC3.Value; int v4 = trackC4.Value;

            editedImage = new Bitmap(originalImage.Width, originalImage.Height);

            // يمكنك لاحقاً تسريع هذه العملية باستخدام LockBits بدلاً من GetPixel/SetPixel المجهدة للمعالج
            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color oldColor = originalImage.GetPixel(x, y);
                    Color newColor = oldColor;

                    switch (mode)
                    {
                        case "RGB":
                            int r = Clamp(oldColor.R + v1);
                            int g = Clamp(oldColor.G + v2);
                            int b = Clamp(oldColor.B + v3);
                            if (!chkC1.Checked) r = 0;
                            if (!chkC2.Checked) g = 0;
                            if (!chkC3.Checked) b = 0;
                            newColor = Color.FromArgb(r, g, b);
                            break;

                        case "HSV":
                            newColor = ModifyHSVWithChannels(oldColor, v1, v2, v3);
                            break;

                        case "CMYK":
                            newColor = ModifyCMYKWithChannels(oldColor, v1, v2, v3, v4);
                            break;

                        case "YUV":
                            newColor = ModifyYUVWithChannels(oldColor, v1, v2, v3);
                            break;

                        case "YCbCr":
                            newColor = ModifyYCbCrWithChannels(oldColor, v1, v2, v3);
                            break;

                        case "LAB":
                            newColor = ModifyLABWithChannels(oldColor, v1, v2, v3);
                            break;
                    }

                    editedImage.SetPixel(x, y, newColor);
                }
            }

            // Apply brightness if the control exists
            if (trackBrightness != null && trackBrightness.Value != 0)
            {
                try
                {
                    Bitmap bright = ApplyBrightnessToBitmap(editedImage, trackBrightness.Value);
                    try { editedImage.Dispose(); } catch { }
                    editedImage = bright;
                }
                catch { }
            }

            // Apply quantization if enabled
            if (chkQuantizeEnable != null && chkQuantizeEnable.Checked && cmbQuantColors != null && cmbQuantColors.SelectedItem != null)
            {
                if (int.TryParse(cmbQuantColors.SelectedItem.ToString(), out int colorCount) && colorCount >= 2)
                {
                    Bitmap quant = ApplyColorQuantization(editedImage, colorCount);
                    // dispose previous editedImage to avoid leaks
                    try { editedImage.Dispose(); } catch { }
                    editedImage = quant;
                    pictureBox1.Image = editedImage;
                }
                else
                {
                    pictureBox1.Image = editedImage;
                }
            }
            else
            {
                pictureBox1.Image = editedImage;
            }

            // Update second visualization in real-time when editedImage changes
            GenerateCustomColorSpacePoints2();
            pictureBoxSpace2?.Invalidate();
        }

        /// <summary>
        /// Fast uniform color quantization using LockBits.
        /// Reduces total number of colors to <= colorCount using equal partitioning of RGB cube.
        /// </summary>
        /// <param name="sourceImage">source bitmap</param>
        /// <param name="colorCount">desired approximate total color count (e.g. 2..256)</param>
        /// <returns>new quantized Bitmap (Format32bppArgb)</returns>
        private Bitmap ApplyColorQuantization(Bitmap sourceImage, int colorCount)
        {
            if (sourceImage == null) return null;
            colorCount = Math.Max(2, Math.Min(256, colorCount));

            // Work in 32bpp ARGB for simplicity and speed
            Bitmap bmp = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(sourceImage, 0, 0, sourceImage.Width, sourceImage.Height);
            }

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                         ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int bytes = Math.Abs(bd.Stride) * bmp.Height;
            byte[] pixels = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, pixels, 0, bytes);

            // Determine number of levels per channel (uniform) such that levels^3 <= colorCount
            int levels = Math.Max(2, (int)Math.Round(Math.Pow(colorCount, 1.0 / 3.0)));
            while (Math.Pow(levels, 3) > colorCount && levels > 2) levels--;

            // Precompute centers for each level
            int[] centers = new int[levels];
            for (int i = 0; i < levels; i++)
            {
                // center placed in the middle of each bin
                centers[i] = (int)(((i + 0.5) * 256.0) / levels);
                if (centers[i] < 0) centers[i] = 0;
                if (centers[i] > 255) centers[i] = 255;
            }

            // For performance, create a small lookup table mapping original value (0..255) -> quantized value
            byte[] lut = new byte[256];
            for (int v = 0; v < 256; v++)
            {
                // find nearest center
                int idx = (int)((v * levels) / 256.0);
                if (idx < 0) idx = 0;
                if (idx >= levels) idx = levels - 1;
                lut[v] = (byte)centers[idx];
            }

            // pixels in Format32bppArgb: B, G, R, A
            int stride = bd.Stride;
            for (int y = 0; y < bmp.Height; y++)
            {
                int row = y * stride;
                for (int x = 0; x < bmp.Width; x++)
                {
                    int i = row + x * 4;
                    byte b = pixels[i + 0];
                    byte g = pixels[i + 1];
                    byte r = pixels[i + 2];
                    // alpha left unchanged
                    pixels[i + 0] = lut[b];
                    pixels[i + 1] = lut[g];
                    pixels[i + 2] = lut[r];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bd.Scan0, bytes);
            bmp.UnlockBits(bd);
            return bmp;
        }

        // Apply a brightness offset (-100..100) to a bitmap using LockBits for performance
        private Bitmap ApplyBrightnessToBitmap(Bitmap src, int brightness)
        {
            if (src == null) return null;
            Bitmap result = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);

            BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dstData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(srcData.Stride) * src.Height;
            byte[] buffer = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(srcData.Scan0, buffer, 0, bytes);

            for (int i = 0; i < bytes; i += 4)
            {
                int b = buffer[i] + brightness;
                int g = buffer[i + 1] + brightness;
                int r = buffer[i + 2] + brightness;
                buffer[i] = (byte)Math.Max(0, Math.Min(255, b));
                buffer[i + 1] = (byte)Math.Max(0, Math.Min(255, g));
                buffer[i + 2] = (byte)Math.Max(0, Math.Min(255, r));
                // alpha (buffer[i+3]) unchanged
            }

            System.Runtime.InteropServices.Marshal.Copy(buffer, 0, dstData.Scan0, bytes);
            src.UnlockBits(srcData);
            result.UnlockBits(dstData);
            return result;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (editedImage == null) return;
            isColorSelectionLocked = false;

            int imageX = e.X * editedImage.Width / pictureBox1.Width;
            int imageY = e.Y * editedImage.Height / pictureBox1.Height;

            if (imageX < 0 || imageX >= editedImage.Width || imageY < 0 || imageY >= editedImage.Height)
                return;

            Color color = editedImage.GetPixel(imageX, imageY);
            UpdateColorInfoLabel(color);
        }

        private void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (editedImage == null) return;

            int imageX = e.X * editedImage.Width / pictureBox1.Width;
            int imageY = e.Y * editedImage.Height / pictureBox1.Height;

            if (imageX < 0 || imageX >= editedImage.Width || imageY < 0 || imageY >= editedImage.Height)
                return;

            Color color = editedImage.GetPixel(imageX, imageY);
            isColorSelectionLocked = true;
            UpdateColorInfoLabel(color, "الصورة", imageX, imageY);
            colorSpace3D?.MoveMarkerToRgb(color.R, color.G, color.B);
        }

        private void UpdateColorInfoLabel(Color color, string source = "", int? pixelX = null, int? pixelY = null)
        {
            string header = string.IsNullOrEmpty(source) ? "" : $"{source}\r\n";
            if (pixelX.HasValue && pixelY.HasValue)
                header += $"Pixel ({pixelX}, {pixelY})";
            if (!string.IsNullOrEmpty(header))
                header += "\r\n";

            lblColorInfo.Text =
                header +
                $"RGB   → ({color.R}, {color.G}, {color.B})\r\n" +
                $"HSV   → {RGBtoHSV(color)}\r\n" +
                $"CMYK  → {RGBtoCMYK(color)}\r\n" +
                $"YUV   → {RGBtoYUV(color)}\r\n" +
                $"YCbCr → {RGBtoYCbCr(color)}\r\n" +
                $"LAB   → {RGBtoLAB(color)}";
            if (!isColorSelectionLocked)
                panelSelectedColor.BackColor = color;
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
                LoadImage(files[0]);
        }

        private int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        // --- معادلات التحويل وتعديل الأنظمة اللونية ---

        private Color HSVtoRGB(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;
            double r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromArgb(
                Clamp((int)((r + m) * 255)),
                Clamp((int)((g + m) * 255)),
                Clamp((int)((b + m) * 255))
            );
        }

        private string RGBtoHSV(Color color)
        {
            double r = color.R / 255.0; double g = color.G / 255.0; double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)); double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;
            double h = 0;

            if (delta != 0)
            {
                if (max == r) h = 60 * (((g - b) / delta) % 6);
                else if (max == g) h = 60 * (((b - r) / delta) + 2);
                else h = 60 * (((r - g) / delta) + 4);
            }
            if (h < 0) h += 360;
            double s = max == 0 ? 0 : delta / max;
            return $"({Math.Round(h)}°, {Math.Round(s * 100)}%, {Math.Round(max * 100)}%)";
        }

        private string RGBtoCMYK(Color color)
        {
            double r = color.R / 255.0; double g = color.G / 255.0; double b = color.B / 255.0;
            double k = 1 - Math.Max(r, Math.Max(g, b));
            if (k == 1) return "(0%, 0%, 0%, 100%)";
            double c = (1 - r - k) / (1 - k);
            double m = (1 - g - k) / (1 - k);
            double y = (1 - b - k) / (1 - k);
            return $"({Math.Round(c * 100)}%, {Math.Round(m * 100)}%, {Math.Round(y * 100)}%, {Math.Round(k * 100)}%)";
        }

        private string RGBtoYUV(Color color)
        {
            double y = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
            double u = -0.14713 * color.R - 0.28886 * color.G + 0.436 * color.B;
            double v = 0.615 * color.R - 0.51499 * color.G - 0.10001 * color.B;
            return $"({Math.Round(y, 2)}, {Math.Round(u, 2)}, {Math.Round(v, 2)})";
        }

        private string RGBtoYCbCr(Color color)
        {
            double y = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B;
            double cb = 128 - 0.168736 * color.R - 0.331264 * color.G + 0.5 * color.B;
            double cr = 128 + 0.5 * color.R - 0.418688 * color.G - 0.081312 * color.B;
            return $"({Math.Round(y, 2)}, {Math.Round(cb, 2)}, {Math.Round(cr, 2)})";
        }

        private string RGBtoLAB(Color color)
        {
            double r = PivotRGB(color.R / 255.0); double g = PivotRGB(color.G / 255.0); double b = PivotRGB(color.B / 255.0);
            double x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            double y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            double z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = PivotXYZ(x); y = PivotXYZ(y); z = PivotXYZ(z);
            double l = 116 * y - 16; double a = 500 * (x - y); double bb = 200 * (y - z);
            return $"({Math.Round(l, 2)}, {Math.Round(a, 2)}, {Math.Round(bb, 2)})";
        }

        private double PivotRGB(double value) => value > 0.04045 ? Math.Pow((value + 0.055) / 1.055, 2.4) : value / 12.92;
        private double PivotXYZ(double value) => value > 0.008856 ? Math.Pow(value, 1.0 / 3.0) : (7.787 * value) + (16.0 / 116.0);


        private double[] GetLABValues(Color color)
        {
            double r = PivotRGB(color.R / 255.0);
            double g = PivotRGB(color.G / 255.0);
            double b = PivotRGB(color.B / 255.0);
            double x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            double y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            double z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;
            x = PivotXYZ(x); y = PivotXYZ(y); z = PivotXYZ(z);
            double l = 116 * y - 16;
            double a = 500 * (x - y);
            double bb = 200 * (y - z);
            return new double[] { l, a, bb };
        }

        // دالة التحويل العكسي (توضع هنا)
        private Color LABtoRGB(double l, double a, double b)
        {
            double y = (l + 16.0) / 116.0;
            double x = a / 500.0 + y;
            double z = y - b / 200.0;
            x = 0.95047 * ((Math.Pow(x, 3) > 0.008856) ? Math.Pow(x, 3) : (x - 16.0 / 116.0) / 7.787);
            y = 1.00000 * ((Math.Pow(y, 3) > 0.008856) ? Math.Pow(y, 3) : (y - 16.0 / 116.0) / 7.787);
            z = 1.08883 * ((Math.Pow(z, 3) > 0.008856) ? Math.Pow(z, 3) : (z - 16.0 / 116.0) / 7.787);
            double r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            double g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            double bl = x * 0.0557 + y * -0.2040 + z * 1.0570;
            r = (r > 0.0031308 ? 1.055 * Math.Pow(r, 1 / 2.4) - 0.055 : 12.92 * r) * 255;
            g = (g > 0.0031308 ? 1.055 * Math.Pow(g, 1 / 2.4) - 0.055 : 12.92 * g) * 255;
            bl = (bl > 0.0031308 ? 1.055 * Math.Pow(bl, 1 / 2.4) - 0.055 : 12.92 * bl) * 255;
            return Color.FromArgb(Clamp((int)r), Clamp((int)g), Clamp((int)bl));
        }















        private Color ModifyHSVWithChannels(Color color, int hChange, int sChange, int vChange)
        {
            double r = color.R / 255.0; double g = color.G / 255.0; double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)); double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;
            double h = 0;

            if (delta != 0)
            {
                if (max == r) h = 60 * (((g - b) / delta) % 6);
                else if (max == g) h = 60 * (((b - r) / delta) + 2);
                else h = 60 * (((r - g) / delta) + 4);
            }
            if (h < 0) h += 360;
            double s = max == 0 ? 0 : delta / max; double v = max;

            h = (h + hChange) % 360; if (h < 0) h += 360;
            s = Math.Max(0, Math.Min(1, s + sChange / 100.0));
            v = Math.Max(0, Math.Min(1, v + vChange / 100.0));

            if (!chkC1.Checked) h = 0;
            if (!chkC2.Checked) s = 0;
            if (!chkC3.Checked) v = 0;

            return HSVtoRGB(h, s, v);
        }

        private Color ModifyCMYKWithChannels(Color color, int cChange, int mChange, int yChange, int kChange)
        {
            double r = color.R / 255.0; double g = color.G / 255.0; double b = color.B / 255.0;
            double k = 1 - Math.Max(r, Math.Max(g, b));
            double c = 0, m = 0, y = 0;

            if (k < 1)
            {
                c = (1 - r - k) / (1 - k); m = (1 - g - k) / (1 - k); y = (1 - b - k) / (1 - k);
            }

            c = Math.Max(0, Math.Min(1, c + cChange / 100.0));
            m = Math.Max(0, Math.Min(1, m + mChange / 100.0));
            y = Math.Max(0, Math.Min(1, y + yChange / 100.0));
            k = Math.Max(0, Math.Min(1, k + kChange / 100.0));

            if (!chkC1.Checked) c = 0;
            if (!chkC2.Checked) m = 0;
            if (!chkC3.Checked) y = 0;
            if (!chkC4.Checked) k = 0;

            return Color.FromArgb(
                Clamp((int)(255 * (1 - c) * (1 - k))),
                Clamp((int)(255 * (1 - m) * (1 - k))),
                Clamp((int)(255 * (1 - y) * (1 - k)))
            );
        }

        private Color ModifyYUVWithChannels(Color color, int yChange, int uChange, int vChange)
        {
            double y = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B + yChange;
            double u = -0.14713 * color.R - 0.28886 * color.G + 0.436 * color.B + uChange;
            double v = 0.615 * color.R - 0.51499 * color.G - 0.10001 * color.B + vChange;

            if (!chkC1.Checked) y = 0;
            if (!chkC2.Checked) u = 0;
            if (!chkC3.Checked) v = 0;

            return Color.FromArgb(
                Clamp((int)(y + 1.13983 * v)),
                Clamp((int)(y - 0.39465 * u - 0.58060 * v)),
                Clamp((int)(y + 2.03211 * u))
            );
        }

        private Color ModifyYCbCrWithChannels(Color color, int yChange, int cbChange, int crChange)
        {
            double y = 0.299 * color.R + 0.587 * color.G + 0.114 * color.B + yChange;
            double cb = 128 - 0.168736 * color.R - 0.331264 * color.G + 0.5 * color.B + cbChange;
            double cr = 128 + 0.5 * color.R - 0.418688 * color.G - 0.081312 * color.B + crChange;

            if (!chkC1.Checked) y = 0;
            if (!chkC2.Checked) cb = 128;
            if (!chkC3.Checked) cr = 128;

            return Color.FromArgb(
                Clamp((int)(y + 1.402 * (cr - 128))),
                Clamp((int)(y - 0.344136 * (cb - 128) - 0.714136 * (cr - 128))),
                Clamp((int)(y + 1.772 * (cb - 128)))
            );
        }

        private Color ModifyLABWithChannels(Color color, int lChange, int aChange, int bChange)
        {
            double[] lab = GetLABValues(color);
            double l = chkC1.Checked ? lab[0] + lChange : 0;
            double a = chkC2.Checked ? lab[1] + aChange : 0;
            double bb = chkC3.Checked ? lab[2] + bChange : 0;
            return LABtoRGB(l, a, bb);
        }

        // --- رسم الفضاء اللوني ثنائي الأبعاد وتدويره ---

        private void DrawColorSpace(string mode)
        {
            int width = 300; int height = 300;
            colorSpaceImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = Color.Black;
                    switch (mode)
                    {
                        case "RGB":
                            color = Color.FromArgb(Clamp(x * 255 / width), Clamp(y * 255 / height), 128);
                            break;
                        case "HSV":
                            color = HSVtoRGB(x * 360.0 / width, y / (double)height, 1);
                            break;
                        case "CMYK":
                            color = Color.FromArgb(
                                Clamp((int)(255 * (1 - (x / (double)width)) * 0.9)),
                                Clamp((int)(255 * (1 - (y / (double)height)) * 0.9)),
                                Clamp((int)(255 * (1 - 0.3) * 0.9))
                            );
                            break;
                        case "YUV":
                            double u = x - width / 2.0; double v = y - height / 2.0;
                            color = Color.FromArgb(
                                Clamp((int)(128 + 1.13983 * v)),
                                Clamp((int)(128 - 0.39465 * u - 0.58060 * v)),
                                Clamp((int)(128 + 2.03211 * u))
                            );
                            break;
                        case "YCbCr":
                            double cb = x * 255.0 / width; double cr = y * 255.0 / height;
                            color = Color.FromArgb(
                                Clamp((int)(128 + 1.402 * (cr - 128))),
                                Clamp((int)(128 - 0.344136 * (cb - 128) - 0.714136 * (cr - 128))),
                                Clamp((int)(128 + 1.772 * (cb - 128)))
                            );
                            break;
                        default:
                            color = Color.FromArgb(Clamp(x * 255 / width), Clamp(y * 255 / height), 128);
                            break;
                    }
                    colorSpaceImage.SetPixel(x, y, color);
                }
            }
            UpdateColorSpaceView(this, EventArgs.Empty);
        }

        private void UpdateColorSpaceView(object sender, EventArgs e)
        {
            if (colorSpaceImage == null) return;

            int zoom = trackZoom.Value; int angle = trackRotate.Value;
            int newWidth = Math.Max(1, colorSpaceImage.Width * zoom / 100);
            int newHeight = Math.Max(1, colorSpaceImage.Height * zoom / 100);

            Bitmap zoomed = new Bitmap(colorSpaceImage, newWidth, newHeight);
            Bitmap rotated = new Bitmap(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.Clear(UiTheme.WorkspaceBg);
                g.TranslateTransform(newWidth / 2f, newHeight / 2f);
                g.RotateTransform(angle);
                g.TranslateTransform(-newWidth / 2f, -newHeight / 2f);
                g.DrawImage(zoomed, 0, 0);
            }

            pictureBoxSpace.Image = rotated;
            lblZoom.Text = $"Zoom: {zoom}%";
            lblRotate.Text = $"Rotate: {angle}°";
            zoomed.Dispose();

            // Update 3D camera as well when 2D preview changes
            if (colorSpace3D != null)
            {
                colorSpace3D.SetCamera(trackZoom.Value, trackRotate.Value);
            }
        }

        // Central synchronization function: updates UI controls and labels from an RGB color
        private void SynchronizeAndDisplaySystemInfo(byte r, byte g, byte b)
        {
            string source = elementHost?.Visible == true ? "Color Space_3D" : "Color Space_2D";
            UpdateColorInfoLabel(Color.FromArgb(r, g, b), source);

            // sync trackbars and labels without re-applying the color to the image
            try
            {
                isUpdatingControls = true;
                if (trackC1.Minimum <= r && trackC1.Maximum >= r) trackC1.Value = r;
                if (trackC2.Minimum <= g && trackC2.Maximum >= g) trackC2.Value = g;
                if (trackC3.Minimum <= b && trackC3.Maximum >= b) trackC3.Value = b;
                lblV1.Text = r.ToString(); lblV2.Text = g.ToString(); lblV3.Text = b.ToString();
                panelSelectedColor.BackColor = System.Drawing.Color.FromArgb(r, g, b);
            }
            catch { }
            finally
            {
                isUpdatingControls = false;
            }
        }

        private void ColorSpaceControl_ColorClickedFrom3D(byte r, byte g, byte b)
        {
            isColorSelectionLocked = false;
            SynchronizeAndDisplaySystemInfo(r, g, b);
        }

        private void ColorSpaceControl_ColorSelectedFrom3D(byte r, byte g, byte b)
        {
            SynchronizeAndDisplaySystemInfo(r, g, b);
            ApplySelectedColorMode(this, EventArgs.Empty);
            // If HSV mode, show cone representing local HSV distribution
            if (cmbColorMode.SelectedItem?.ToString() == "HSV")
            {
                colorSpace3D?.ShowHsvCone(r, g, b);
            }
            else
            {
                colorSpace3D?.ClearCone();
            }
        }

        private void ColorSpaceControl_ColorHoveredFrom3D(byte r, byte g, byte b)
        {
            if (isColorSelectionLocked)
                return;

            UpdateColorInfoLabel(Color.FromArgb(r, g, b), "Color Space_3D");
            if (cmbColorMode.SelectedItem?.ToString() == "RGB")
            {
                isUpdatingControls = true;
                try
                {
                    if (trackC1.Minimum <= r && trackC1.Maximum >= r) trackC1.Value = r;
                    if (trackC2.Minimum <= g && trackC2.Maximum >= g) trackC2.Value = g;
                    if (trackC3.Minimum <= b && trackC3.Maximum >= b) trackC3.Value = b;
                    UpdateTrackLabels();
                }
                finally { isUpdatingControls = false; }
            }
            colorSpace3D?.MoveMarkerToRgb(r, g, b);
        }

        private void cmbViewMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbViewMode == null || elementHost == null) return;

            bool is3D = cmbViewMode.SelectedItem?.ToString() == "3D";
            UpdateViewModeButtons(is3D);

            // Toggle visibility so the WPF host doesn't get obscured by the 2D PictureBox
            elementHost.Visible = is3D;
            pictureBoxSpace.Visible = !is3D;
            if (is3D) elementHost.BringToFront();
            else pictureBoxSpace.BringToFront();

            if (is3D)
            {
                if (colorSpace3D == null)
                {
                    colorSpace3D = new PixelLab.Controls.ColorSpace3DControl();
                    elementHost.Child = colorSpace3D;
                    colorSpace3D.ColorClickedFrom3D += ColorSpaceControl_ColorClickedFrom3D;
                    colorSpace3D.ColorSelectedFrom3D += ColorSpaceControl_ColorSelectedFrom3D;
                    colorSpace3D.ColorHoveredFrom3D += ColorSpaceControl_ColorHoveredFrom3D;
                }

                // make sure the host is in front and has focus so WPF renders correctly
                elementHost.BringToFront();
                try { elementHost.Focus(); } catch { }

                colorSpace3D.SetColorMode(cmbColorMode.SelectedItem?.ToString() ?? "RGB");
            }
            else
            {
                // keep the child so switching back is fast; optionally clear to free resources
                // elementHost.Child = null;
            }
        }

        private void PictureBoxSpace_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBoxSpace.Image == null) return;
            isColorSelectionLocked = false;

            using (Bitmap bmp = new Bitmap(pictureBoxSpace.Image))
            {
                int x = e.X * bmp.Width / pictureBoxSpace.Width;
                int y = e.Y * bmp.Height / pictureBoxSpace.Height;

                if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height) return;

                Color color = bmp.GetPixel(x, y);
                UpdateColorInfoLabel(color, "Color Space_2D", x, y);
            }
        }

        private void PictureBoxSpace_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (pictureBoxSpace.Image == null) return;

            using (Bitmap bmp = new Bitmap(pictureBoxSpace.Image))
            {
                int x = e.X * bmp.Width / pictureBoxSpace.Width;
                int y = e.Y * bmp.Height / pictureBoxSpace.Height;

                if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height) return;

                Color color = bmp.GetPixel(x, y);
                SynchronizeAndDisplaySystemInfo(color.R, color.G, color.B);
                ApplySelectedColorMode(this, EventArgs.Empty);
            }
        }

        private void ElementHost_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            // When 3D host is double-clicked, commit adjustments similarly
            try
            {
                ApplySelectedColorMode(this, EventArgs.Empty);
                if (editedImage != null)
                {
                    try { originalImage.Dispose(); } catch { }
                    originalImage = new Bitmap(editedImage);
                    UpdateImageInfo();
                    GenerateCustomColorSpacePoints();
                    GenerateCustomColorSpacePoints2();
                    pictureBoxSpace.Invalidate();
                    pictureBoxSpace2?.Invalidate();
                }
            }
            catch { }
        }

        // ===== معالجات الماوس والرسم للفضاء اللوني ثلاثي الأبعاد GDI+ =====

        private void PictureBoxSpace_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingRotation = true;
            lastMousePos = e.Location;
        }

        private void PictureBoxSpace_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingRotation = false;
        }

        private void PictureBoxSpace_MouseWheel(object sender, MouseEventArgs e)
        {
            customZoom += e.Delta > 0 ? 10 : -10;
            customZoom = Math.Max(50, Math.Min(300, customZoom));
            pictureBoxSpace.Invalidate();
        }

        private void PictureBoxSpace_Paint(object sender, PaintEventArgs e)
        {
            if (customVisualPoints.Count == 0)
            {
                e.Graphics.Clear(Color.Black);
                e.Graphics.DrawString("عرّض صورة أولاً", Font, Brushes.White, 10, 10);
                return;
            }

            e.Graphics.Clear(Color.Black);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int width = pictureBoxSpace.Width;
            int height = pictureBoxSpace.Height;
            int centerX = width / 2;
            int centerY = height / 2;

            // رسم المحاور
            DrawAxes3D(e.Graphics, centerX, centerY);

            // رسم النقاط
            DrawColorPoints(e.Graphics, centerX, centerY);
        }

        private void DrawAxes3D(Graphics g, int centerX, int centerY)
        {
            float scale = customZoom / 100f;

            // تحويل درجات إلى راديان
            float yawRad = customYaw * (float)Math.PI / 180f;
            float pitchRad = customPitch * (float)Math.PI / 180f;

            // نقاط المحاور
            var origin = new Point3D(0, 0, 0);
            var xAxis = new Point3D(0.5f, 0, 0);
            var yAxis = new Point3D(0, 0.5f, 0);
            var zAxis = new Point3D(0, 0, 0.5f);

            var p0 = Project3DTo2D(origin, centerX, centerY, yawRad, pitchRad, scale);
            var px = Project3DTo2D(xAxis, centerX, centerY, yawRad, pitchRad, scale);
            var py = Project3DTo2D(yAxis, centerX, centerY, yawRad, pitchRad, scale);
            var pz = Project3DTo2D(zAxis, centerX, centerY, yawRad, pitchRad, scale);

            using (var penX = new Pen(Color.Red, 2))
            using (var penY = new Pen(Color.Green, 2))
            using (var penZ = new Pen(Color.Blue, 2))
            {
                g.DrawLine(penX, p0, px);
                g.DrawLine(penY, p0, py);
                g.DrawLine(penZ, p0, pz);
            }

            // تسميات المحاور
            var labelMode = cmbColorMode?.SelectedItem?.ToString() ?? "RGB";
            string[] labels = labelMode switch
            {
                "HSV" => new[] { "H", "S", "V" },
                "CMYK" => new[] { "C", "M", "Y" },
                "YUV" => new[] { "Y", "U", "V" },
                "LAB" => new[] { "L", "a", "b" },
                "YCbCr" => new[] { "Y", "Cb", "Cr" },
                _ => new[] { "R", "G", "B" }
            };

            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(labels[0], Font, brush, px.X + 5, px.Y);
                g.DrawString(labels[1], Font, brush, py.X + 5, py.Y);
                g.DrawString(labels[2], Font, brush, pz.X + 5, pz.Y);
            }
        }

        private void DrawColorPoints(Graphics g, int centerX, int centerY)
        {
            float scale = customZoom / 100f;
            float yawRad = customYaw * (float)Math.PI / 180f;
            float pitchRad = customPitch * (float)Math.PI / 180f;

            foreach (var point in customVisualPoints)
            {
                var p3d = new Point3D(point.X, point.Y, point.Z);
                var p2d = Project3DTo2D(p3d, centerX, centerY, yawRad, pitchRad, scale);

                if (p2d.X >= 0 && p2d.X < pictureBoxSpace.Width && p2d.Y >= 0 && p2d.Y < pictureBoxSpace.Height)
                {
                    using (var brush = new SolidBrush(point.DrawColor))
                    {
                        g.FillEllipse(brush, p2d.X - 2, p2d.Y - 2, 4, 4);
                    }
                }
            }
        }

        private Point Project3DTo2D(Point3D p, int centerX, int centerY, float yaw, float pitch, float scale)
        {
            // تطبيق التدوير
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);

            float x = p.X;
            float y = p.Y;
            float z = p.Z;

            // تدوير حول محور Y (Yaw)
            float x1 = x * cosYaw - z * sinYaw;
            float z1 = x * sinYaw + z * cosYaw;

            // تدوير حول محور X (Pitch)
            float y2 = y * cosPitch - z1 * sinPitch;
            float z2 = y * sinPitch + z1 * cosPitch;

            // الإسقاط المنظوري (Perspective projection)
            float distance = 2.5f;
            float perspective = distance / (distance + z2 * 0.5f);
            float screenX = x1 * perspective * scale * 100;
            float screenY = y2 * perspective * scale * 100;

            return new Point((int)(centerX + screenX), (int)(centerY - screenY));
        }

        private void GenerateCustomColorSpacePoints()
        {
            if (editedImage == null)
            {
                customVisualPoints.Clear();
                return;
            }

            customVisualPoints.Clear();
            string colorMode = cmbColorMode?.SelectedItem?.ToString() ?? "RGB";

            // أخذ عينة من البكسلات (كل 15 بكسل للأداء)
            int stride = Math.Max(1, editedImage.Width / 20);
            for (int y = 0; y < editedImage.Height; y += stride)
            {
                for (int x = 0; x < editedImage.Width; x += stride)
                {
                    Color pixelColor = editedImage.GetPixel(x, y);
                    float px = 0, py = 0, pz = 0;

                    switch (colorMode)
                    {
                        case "HSV":
                            RgbToHsvNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            px = (float)(px % 360) / 360f; // تطبيع Hue
                            break;
                        case "CMYK":
                            RgbToCmykNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        case "YUV":
                            RgbToYuvNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        case "LAB":
                            RgbToLabNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        case "YCbCr":
                            RgbToYCbCrNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        default: // RGB
                            px = pixelColor.R / 255f;
                            py = pixelColor.G / 255f;
                            pz = pixelColor.B / 255f;
                            break;
                    }

                    customVisualPoints.Add(new PixelPoint(px, py, pz, pixelColor));
                }
            }
        }

        private void RgbToHsvNormalized(int r, int g, int b, out float h, out float s, out float v)
        {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            h = 0;
            if (delta != 0)
            {
                if (max == rf) h = 60 * (((gf - bf) / delta) % 6);
                else if (max == gf) h = 60 * (((bf - rf) / delta) + 2);
                else h = 60 * (((rf - gf) / delta) + 4);
            }
            if (h < 0) h += 360;

            s = max == 0 ? 0 : delta / max;
            v = max;
        }

        private void RgbToCmykNormalized(int r, int g, int b, out float c, out float m, out float y)
        {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float k = 1 - Math.Max(rf, Math.Max(gf, bf));

            c = (1 - rf - k) / (1 - k);
            m = (1 - gf - k) / (1 - k);
            y = (1 - bf - k) / (1 - k);

            c = Math.Max(0, Math.Min(1, c));
            m = Math.Max(0, Math.Min(1, m));
            y = Math.Max(0, Math.Min(1, y));
        }

        private void RgbToYuvNormalized(int r, int g, int b, out float y, out float u, out float v)
        {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            y = 0.299f * rf + 0.587f * gf + 0.114f * bf;
            u = (bf - y) / 1.772f + 0.5f;
            v = (rf - y) / 1.402f + 0.5f;

            y = Math.Max(0, Math.Min(1, y));
            u = Math.Max(0, Math.Min(1, u));
            v = Math.Max(0, Math.Min(1, v));
        }

        private void RgbToLabNormalized(int r, int g, int b, out float l, out float a, out float lab_b)
        {
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;

            // تحويل إلى XYZ أولاً
            rf = rf > 0.04045f ? (float)Math.Pow((rf + 0.055f) / 1.055f, 2.4f) : rf / 12.92f;
            gf = gf > 0.04045f ? (float)Math.Pow((gf + 0.055f) / 1.055f, 2.4f) : gf / 12.92f;
            bf = bf > 0.04045f ? (float)Math.Pow((bf + 0.055f) / 1.055f, 2.4f) : bf / 12.92f;

            float x = rf * 0.4124f + gf * 0.3576f + bf * 0.1805f;
            float y_temp = rf * 0.2126f + gf * 0.7152f + bf * 0.0722f;
            float z = rf * 0.0193f + gf * 0.1192f + bf * 0.9505f;

            // تطبيع بواسطة D65
            x = x / 0.95047f;
            y_temp = y_temp / 1.00000f;
            z = z / 1.08883f;

            float fx = x > 0.008856f ? (float)Math.Pow(x, 1 / 3f) : (7.787f * x) + (16 / 116f);
            float fy = y_temp > 0.008856f ? (float)Math.Pow(y_temp, 1 / 3f) : (7.787f * y_temp) + (16 / 116f);
            float fz = z > 0.008856f ? (float)Math.Pow(z, 1 / 3f) : (7.787f * z) + (16 / 116f);

            l = (116 * fy) - 16;
            a = 500 * (fx - fy);
            lab_b = 200 * (fy - fz);

            l = Math.Max(0, Math.Min(100, l)) / 100f;
            a = (a + 128) / 256f;
            lab_b = (lab_b + 128) / 256f;
        }

        private void RgbToYCbCrNormalized(int r, int g, int b, out float y, out float cb, out float cr)
        {
            y = 0.299f * r + 0.587f * g + 0.114f * b;
            cb = 128 - 0.168736f * r - 0.331264f * g + 0.5f * b;
            cr = 128 + 0.5f * r - 0.418688f * g - 0.081312f * b;

            y = Math.Max(0, Math.Min(255, y)) / 255f;
            cb = Math.Max(0, Math.Min(255, cb)) / 255f;
            cr = Math.Max(0, Math.Min(255, cr)) / 255f;
        }

        private class Point3D
        {
            public float X, Y, Z;

            public Point3D(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        // ===== معالجات العرض الثاني (Visualization 2) =====

        private void PictureBoxSpace2_MouseDown(object? sender, MouseEventArgs e)
        {
            isDraggingRotation2 = true;
            lastMousePos2 = e.Location;
        }

        private void PictureBoxSpace2_MouseUp(object? sender, MouseEventArgs e)
        {
            isDraggingRotation2 = false;
        }

        private void PictureBoxSpace2_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDraggingRotation2)
            {
                int deltaX = e.X - lastMousePos2.X;
                int deltaY = e.Y - lastMousePos2.Y;

                customYaw2 += deltaX * 0.5f;
                customPitch2 += deltaY * 0.5f;

                customPitch2 = Math.Max(-90, Math.Min(90, customPitch2));

                lastMousePos2 = e.Location;
                pictureBoxSpace2?.Invalidate();
            }
        }

        private void PictureBoxSpace2_MouseWheel(object? sender, MouseEventArgs e)
        {
            customZoom2 += e.Delta > 0 ? 10 : -10;
            customZoom2 = Math.Max(50, Math.Min(300, customZoom2));
            pictureBoxSpace2?.Invalidate();
        }

        private void PictureBoxSpace2_Paint(object? sender, PaintEventArgs e)
        {
            if (customVisualPoints2.Count == 0)
            {
                e.Graphics.Clear(Color.Black);
                e.Graphics.DrawString("عرّض صورة أولاً", Font, Brushes.White, 10, 10);
                return;
            }

            e.Graphics.Clear(Color.Black);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int width = pictureBoxSpace2?.Width ?? 100;
            int height = pictureBoxSpace2?.Height ?? 100;
            int centerX = width / 2;
            int centerY = height / 2;

            // رسم المحاور
            DrawAxes3D2(e.Graphics, centerX, centerY);

            // رسم النقاط
            DrawColorPoints2(e.Graphics, centerX, centerY);
        }

        private void DrawAxes3D2(Graphics g, int centerX, int centerY)
        {
            float scale = customZoom2 / 100f;

            float yawRad = customYaw2 * (float)Math.PI / 180f;
            float pitchRad = customPitch2 * (float)Math.PI / 180f;

            var origin = new Point3D(0, 0, 0);
            var xAxis = new Point3D(0.5f, 0, 0);
            var yAxis = new Point3D(0, 0.5f, 0);
            var zAxis = new Point3D(0, 0, 0.5f);

            var p0 = Project3DTo2D2(origin, centerX, centerY, yawRad, pitchRad, scale);
            var px = Project3DTo2D2(xAxis, centerX, centerY, yawRad, pitchRad, scale);
            var py = Project3DTo2D2(yAxis, centerX, centerY, yawRad, pitchRad, scale);
            var pz = Project3DTo2D2(zAxis, centerX, centerY, yawRad, pitchRad, scale);

            using (var penX = new Pen(Color.Red, 2))
            using (var penY = new Pen(Color.Green, 2))
            using (var penZ = new Pen(Color.Blue, 2))
            {
                g.DrawLine(penX, p0, px);
                g.DrawLine(penY, p0, py);
                g.DrawLine(penZ, p0, pz);
            }

            var labelMode = cmbColorMode?.SelectedItem?.ToString() ?? "RGB";
            string[] labels = labelMode switch
            {
                "HSV" => new[] { "H", "S", "V" },
                "CMYK" => new[] { "C", "M", "Y" },
                "YUV" => new[] { "Y", "U", "V" },
                "LAB" => new[] { "L", "a", "b" },
                "YCbCr" => new[] { "Y", "Cb", "Cr" },
                _ => new[] { "R", "G", "B" }
            };

            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(labels[0], Font, brush, px.X + 5, px.Y);
                g.DrawString(labels[1], Font, brush, py.X + 5, py.Y);
                g.DrawString(labels[2], Font, brush, pz.X + 5, pz.Y);
            }
        }

        private void DrawColorPoints2(Graphics g, int centerX, int centerY)
        {
            float scale = customZoom2 / 100f;
            float yawRad = customYaw2 * (float)Math.PI / 180f;
            float pitchRad = customPitch2 * (float)Math.PI / 180f;

            int width = pictureBoxSpace2?.Width ?? 100;
            int height = pictureBoxSpace2?.Height ?? 100;

            foreach (var point in customVisualPoints2)
            {
                var p3d = new Point3D(point.X, point.Y, point.Z);
                var p2d = Project3DTo2D2(p3d, centerX, centerY, yawRad, pitchRad, scale);

                if (p2d.X >= 0 && p2d.X < width && p2d.Y >= 0 && p2d.Y < height)
                {
                    using (var brush = new SolidBrush(point.DrawColor))
                    {
                        g.FillEllipse(brush, p2d.X - 2, p2d.Y - 2, 4, 4);
                    }
                }
            }
        }

        private Point Project3DTo2D2(Point3D p, int centerX, int centerY, float yaw, float pitch, float scale)
        {
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);
            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);

            float x = p.X;
            float y = p.Y;
            float z = p.Z;

            float x1 = x * cosYaw - z * sinYaw;
            float z1 = x * sinYaw + z * cosYaw;

            float y2 = y * cosPitch - z1 * sinPitch;
            float z2 = y * sinPitch + z1 * cosPitch;

            float distance = 2.5f;
            float perspective = distance / (distance + z2 * 0.5f);
            float screenX = x1 * perspective * scale * 100;
            float screenY = y2 * perspective * scale * 100;

            return new Point((int)(centerX + screenX), (int)(centerY - screenY));
        }

        private void GenerateCustomColorSpacePoints2()
        {
            if (editedImage == null)
            {
                customVisualPoints2.Clear();
                return;
            }

            customVisualPoints2.Clear();
            string colorMode = cmbColorMode?.SelectedItem?.ToString() ?? "RGB";

            // نفس العملية كالـ customVisualPoints
            int stride = Math.Max(1, editedImage.Width / 20);
            for (int y = 0; y < editedImage.Height; y += stride)
            {
                for (int x = 0; x < editedImage.Width; x += stride)
                {
                    Color pixelColor = editedImage.GetPixel(x, y);
                    float px = 0, py = 0, pz = 0;

                    switch (colorMode)
                    {
                        case "HSV":
                            RgbToHsvNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            px = (float)(px % 360) / 360f;
                            break;
                        case "CMYK":
                            RgbToCmykNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        case "YUV":
                            RgbToYuvNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        case "LAB":
                            RgbToLabNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        case "YCbCr":
                            RgbToYCbCrNormalized(pixelColor.R, pixelColor.G, pixelColor.B, out px, out py, out pz);
                            break;
                        default:
                            px = pixelColor.R / 255f;
                            py = pixelColor.G / 255f;
                            pz = pixelColor.B / 255f;
                            break;
                    }

                    customVisualPoints2.Add(new PixelPoint(px, py, pz, pixelColor));
                }
            }
        }
    }
}