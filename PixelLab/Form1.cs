using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Drawing.Imaging;

namespace PixelLab
{
    public partial class Form1 : Form
    {
        private Bitmap originalImage;
        private Bitmap editedImage;
        private Bitmap colorSpaceImage;
        private string currentImageName = string.Empty;
        private string currentImagePath = string.Empty;
        private bool isUpdatingControls = false;
        private PixelLab.Controls.ColorSpace3DControl colorSpace3D;

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
                // الاشتراك في حدث اختيار اللون من المشهد ثلاثي الأبعاد
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

            trackZoom.ValueChanged += UpdateColorSpaceView;
            trackRotate.ValueChanged += UpdateColorSpaceView;
            trackRotate.ValueChanged += (s, e) => { if (colorSpace3D != null) colorSpace3D.SetCamera(trackZoom.Value, trackRotate.Value); UpdateColorSpaceView(null, null); };
            trackZoom.ValueChanged += (s, e) => { if (colorSpace3D != null) colorSpace3D.SetCamera(trackZoom.Value, trackRotate.Value); UpdateColorSpaceView(null, null); };
            pictureBoxSpace.MouseClick += PictureBoxSpace_MouseClick;
            pictureBoxSpace.MouseDoubleClick += PictureBoxSpace_MouseDoubleClick;
            pictureBoxSpace.MouseMove += PictureBoxSpace_MouseMove;

            // تفعيل القنوات افتراضياً
            chkC1.Checked = true;
            chkC2.Checked = true;
            chkC3.Checked = true;
            chkC4.Checked = true;

            cmbColorMode.SelectedIndex = 0;
            Resize += Form1_Resize;
            Form1_Resize(this, EventArgs.Empty);
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
            const int y = 12;
            const int sliderH = 28;
            lblZoom.SetBounds(12, 14, 82, 22);
            int mid = w / 2;
            trackZoom.SetBounds(96, y, mid - 110, sliderH);
            lblRotate.SetBounds(mid - 8, 14, 88, 22);
            trackRotate.SetBounds(mid + 82, y, w - mid - 98, sliderH);
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
            panelSelectedColor.BackColor = color;
        }

        private void PictureBoxSpace_MouseMove(object? sender, MouseEventArgs e)
        {
            if (pictureBoxSpace.Image == null) return;
            using var bmp = new Bitmap(pictureBoxSpace.Image);
            if (!TryMapPictureBoxPixel(pictureBoxSpace, bmp, e, out int x, out int y)) return;
            Color color = bmp.GetPixel(x, y);
            UpdateColorInfoLabel(color, "Color Space_2D", x, y);
            panelSelectedColor.BackColor = color;
        }

        private void LayoutRightChannels()
        {
            if (panelRight == null || panelSpaceView == null) return;
            int w = panelRight.ClientSize.Width;
            int h = panelRight.ClientSize.Height;
            const int pad = 10;
            int spaceH = (int)(h * 0.52);
            panelSpaceView.SetBounds(pad, pad, w - pad * 2, spaceH);

            int titleY = panelSpaceView.Bottom + 10;
            lblChannelsTitle.SetBounds(pad, titleY, w - pad * 2, 22);

            int rowY = titleY + 28;
            const int rowH = 46;
            const int lblW = 30;
            const int lblH = 36;
            const int valW = 20;
            const int chkW = 44;
            int trackW = w - pad * 2 - lblW - valW - chkW - 16;

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

            ApplySelectedColorMode(null, null);
            DrawColorSpace(mode);

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

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (editedImage == null) return;

            int imageX = e.X * editedImage.Width / pictureBox1.Width;
            int imageY = e.Y * editedImage.Height / pictureBox1.Height;

            if (imageX < 0 || imageX >= editedImage.Width || imageY < 0 || imageY >= editedImage.Height)
                return;

            Color color = editedImage.GetPixel(imageX, imageY);
            UpdateColorInfoLabel(color);
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
            UpdateColorSpaceView(null, null);
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

        private void ColorSpaceControl_ColorSelectedFrom3D(byte r, byte g, byte b)
        {
            SynchronizeAndDisplaySystemInfo(r, g, b);
            ApplySelectedColorMode(null, EventArgs.Empty);
        }

        private void ColorSpaceControl_ColorHoveredFrom3D(byte r, byte g, byte b)
        {
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
                ApplySelectedColorMode(null, EventArgs.Empty);
            }
        }
    }
}