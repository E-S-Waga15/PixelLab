using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelLab
{
    public partial class Form1 : Form
    {


        Bitmap originalImage;
        Bitmap editedImage;
        bool isUpdatingControls = false;
        Bitmap colorSpaceImage;

        public Form1()
        {
            InitializeComponent();

            pictureBox1.AllowDrop = true;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            cmbColorMode.Items.Add("RGB");
            cmbColorMode.Items.Add("HSV");
            cmbColorMode.Items.Add("CMYK");
            cmbColorMode.Items.Add("YUV");
            cmbColorMode.Items.Add("LAB");
            cmbColorMode.Items.Add("YCbCr");

            cmbColorMode.SelectedIndexChanged += cmbColorMode_SelectedIndexChanged;

            trackC1.ValueChanged += ApplySelectedColorMode;
            trackC2.ValueChanged += ApplySelectedColorMode;
            trackC3.ValueChanged += ApplySelectedColorMode;
            trackC4.ValueChanged += ApplySelectedColorMode;

            pictureBox1.MouseClick += PictureBox1_MouseClick;
            pictureBox1.DragEnter += pictureBox1_DragEnter;
            pictureBox1.DragDrop += pictureBox1_DragDrop;

            chkC1.CheckedChanged += ApplySelectedColorMode;
            chkC2.CheckedChanged += ApplySelectedColorMode;
            chkC3.CheckedChanged += ApplySelectedColorMode;
            chkC4.CheckedChanged += ApplySelectedColorMode;

            trackZoom.ValueChanged += UpdateColorSpaceView;
            trackRotate.ValueChanged += UpdateColorSpaceView;
            pictureBoxSpace.MouseClick += PictureBoxSpace_MouseClick;

            chkC1.Checked = true;
            chkC2.Checked = true;
            chkC3.Checked = true;
            chkC4.Checked = true;

            cmbColorMode.SelectedIndex = 0;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadImage(dialog.FileName);
            }
        }

        private void LoadImage(string path)
        {
            originalImage = new Bitmap(path);
            editedImage = new Bitmap(originalImage);

            pictureBox1.Image = editedImage;
            ResetTracks();
        }

        private void cmbColorMode_SelectedIndexChanged(object sender, EventArgs e)
        {

            //if (editedImage != null)
            //{
            //    originalImage = new Bitmap(editedImage);
            //}

            ResetTracks();

            string mode = cmbColorMode.SelectedItem.ToString();

            if (mode == "RGB")
                SetupComponents("R", "G", "B", "", -255, 255, -255, 255, -255, 255, 0, 0, false);

            else if (mode == "HSV")
                SetupComponents("H", "S", "V", "", -180, 180, -100, 100, -100, 100, 0, 0, false);

            else if (mode == "CMYK")
                SetupComponents("C", "M", "Y", "K", -100, 100, -100, 100, -100, 100, -100, 100, true);

            else if (mode == "YUV")
                SetupComponents("Y", "U", "V", "", -100, 100, -100, 100, -100, 100, 0, 0, false);

            else if (mode == "LAB")
                SetupComponents("L", "A", "B", "", -100, 100, -100, 100, -100, 100, 0, 0, false);

            else if (mode == "YCbCr")
                SetupComponents("Y", "Cb", "Cr", "", -100, 100, -100, 100, -100, 100, 0, 0, false);

            ApplySelectedColorMode(null, null);

            DrawColorSpace(mode);
        }

        private void SetupComponents(
            string name1, string name2, string name3, string name4,
            int min1, int max1,
            int min2, int max2,
            int min3, int max3,
            int min4, int max4,
            bool showFourth)
        {
            lblC1.Text = name1;
            lblC2.Text = name2;
            lblC3.Text = name3;
            lblC4.Text = name4;

            trackC1.Minimum = min1;
            trackC1.Maximum = max1;

            trackC2.Minimum = min2;
            trackC2.Maximum = max2;

            trackC3.Minimum = min3;
            trackC3.Maximum = max3;

            trackC4.Minimum = min4;
            trackC4.Maximum = max4;

            lblC4.Visible = showFourth;
            trackC4.Visible = showFourth;
            lblV4.Visible = showFourth;

            chkC4.Visible = showFourth;

            chkC1.Checked = true;
            chkC2.Checked = true;
            chkC3.Checked = true;
            chkC4.Checked = true;

            UpdateTrackLabels();
        }

        private void ResetTracks()
        {
            isUpdatingControls = true;

            trackC1.Value = 0;
            trackC2.Value = 0;
            trackC3.Value = 0;
            trackC4.Value = 0;

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

            if (isUpdatingControls)
                return;

            UpdateTrackLabels();

            if (originalImage == null || cmbColorMode.SelectedItem == null)
                return;

            string mode = cmbColorMode.SelectedItem.ToString();

            int v1 = trackC1.Value;
            int v2 = trackC2.Value;
            int v3 = trackC3.Value;
            int v4 = trackC4.Value;



            editedImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int y = 0; y < originalImage.Height; y++)
            {
                for (int x = 0; x < originalImage.Width; x++)
                {
                    Color oldColor = originalImage.GetPixel(x, y);
                    Color newColor = oldColor;
                    if (mode == "RGB")
                    {
                        int r = Clamp(oldColor.R + v1);
                        int g = Clamp(oldColor.G + v2);
                        int b = Clamp(oldColor.B + v3);

                        if (!chkC1.Checked) r = 0;
                        if (!chkC2.Checked) g = 0;
                        if (!chkC3.Checked) b = 0;

                        newColor = Color.FromArgb(r, g, b);
                    }
                    else if (mode == "HSV")
                    {
                        newColor = ModifyHSVWithChannels(oldColor, v1, v2, v3);
                    }
                    else if (mode == "CMYK")
                    {
                        newColor = ModifyCMYKWithChannels(oldColor, v1, v2, v3, v4);
                    }
                    else if (mode == "YUV")
                    {
                        newColor = ModifyYUVWithChannels(oldColor, v1, v2, v3);
                    }
                    else if (mode == "YCbCr")
                    {
                        newColor = ModifyYCbCrWithChannels(oldColor, v1, v2, v3);
                    }
                    else if (mode == "LAB")
                    {
                        newColor = ModifyLABWithChannels(oldColor, v1, v2, v3);
                    }

                    editedImage.SetPixel(x, y, newColor);
                }
            }

            pictureBox1.Image = editedImage;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (editedImage == null)
                return;

            int imageX = e.X * editedImage.Width / pictureBox1.Width;
            int imageY = e.Y * editedImage.Height / pictureBox1.Height;

            if (imageX < 0 || imageX >= editedImage.Width || imageY < 0 || imageY >= editedImage.Height)
                return;

            Color color = editedImage.GetPixel(imageX, imageY);

            lblColorInfo.Text =
                $"RGB   → ({color.R}, {color.G}, {color.B})\n" +
                $"HSV   → {RGBtoHSV(color)}\n" +
                $"CMYK  → {RGBtoCMYK(color)}\n" +
                $"YUV   → {RGBtoYUV(color)}\n" +
                $"YCbCr → {RGBtoYCbCr(color)}\n" +
                $"LAB   → {RGBtoLAB(color)}";
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
                LoadImage(files[0]);
        }

        private int Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

        private Color ModifyHSV(Color color, int hChange, int sChange, int vChange)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double h = 0;

            if (delta == 0)
                h = 0;
            else if (max == r)
                h = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                h = 60 * (((b - r) / delta) + 2);
            else
                h = 60 * (((r - g) / delta) + 4);

            if (h < 0)
                h += 360;

            double s = max == 0 ? 0 : delta / max;
            double v = max;

            h = (h + hChange) % 360;
            if (h < 0) h += 360;

            s = Math.Max(0, Math.Min(1, s + sChange / 100.0));
            v = Math.Max(0, Math.Min(1, v + vChange / 100.0));

            return HSVtoRGB(h, s, v);
        }

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

        private Color ModifyCMYK(Color color, int cChange, int mChange, int yChange, int kChange)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double k = 1 - Math.Max(r, Math.Max(g, b));

            double c = 0, m = 0, y = 0;

            if (k < 1)
            {
                c = (1 - r - k) / (1 - k);
                m = (1 - g - k) / (1 - k);
                y = (1 - b - k) / (1 - k);
            }

            c = Math.Max(0, Math.Min(1, c + cChange / 100.0));
            m = Math.Max(0, Math.Min(1, m + mChange / 100.0));
            y = Math.Max(0, Math.Min(1, y + yChange / 100.0));
            k = Math.Max(0, Math.Min(1, k + kChange / 100.0));

            int newR = Clamp((int)(255 * (1 - c) * (1 - k)));
            int newG = Clamp((int)(255 * (1 - m) * (1 - k)));
            int newB = Clamp((int)(255 * (1 - y) * (1 - k)));

            return Color.FromArgb(newR, newG, newB);
        }

        private Color ModifyYUV(Color color, int yChange, int uChange, int vChange)
        {
            double r = color.R;
            double g = color.G;
            double b = color.B;

            double y = 0.299 * r + 0.587 * g + 0.114 * b;
            double u = -0.14713 * r - 0.28886 * g + 0.436 * b;
            double v = 0.615 * r - 0.51499 * g - 0.10001 * b;

            y += yChange;
            u += uChange;
            v += vChange;

            int newR = Clamp((int)(y + 1.13983 * v));
            int newG = Clamp((int)(y - 0.39465 * u - 0.58060 * v));
            int newB = Clamp((int)(y + 2.03211 * u));

            return Color.FromArgb(newR, newG, newB);
        }

        private Color ModifyYCbCr(Color color, int yChange, int cbChange, int crChange)
        {
            double r = color.R;
            double g = color.G;
            double b = color.B;

            double y = 0.299 * r + 0.587 * g + 0.114 * b;
            double cb = 128 - 0.168736 * r - 0.331264 * g + 0.5 * b;
            double cr = 128 + 0.5 * r - 0.418688 * g - 0.081312 * b;

            y += yChange;
            cb += cbChange;
            cr += crChange;

            int newR = Clamp((int)(y + 1.402 * (cr - 128)));
            int newG = Clamp((int)(y - 0.344136 * (cb - 128) - 0.714136 * (cr - 128)));
            int newB = Clamp((int)(y + 1.772 * (cb - 128)));

            return Color.FromArgb(newR, newG, newB);
        }

        private Color ModifyLABSimple(Color color, int lChange, int aChange, int bChange)
        {
            return Color.FromArgb(
                Clamp(color.R + lChange),
                Clamp(color.G + aChange),
                Clamp(color.B + bChange)
            );
        }

        private string RGBtoHSV(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double h = 0;

            if (delta == 0) h = 0;
            else if (max == r) h = 60 * (((g - b) / delta) % 6);
            else if (max == g) h = 60 * (((b - r) / delta) + 2);
            else h = 60 * (((r - g) / delta) + 4);

            if (h < 0) h += 360;

            double s = max == 0 ? 0 : delta / max;
            double v = max;

            return $"({Math.Round(h)}°, {Math.Round(s * 100)}%, {Math.Round(v * 100)}%)";
        }

        private string RGBtoCMYK(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double k = 1 - Math.Max(r, Math.Max(g, b));

            if (k == 1)
                return "(0%, 0%, 0%, 100%)";

            double c = (1 - r - k) / (1 - k);
            double m = (1 - g - k) / (1 - k);
            double y = (1 - b - k) / (1 - k);

            return $"({Math.Round(c * 100)}%, {Math.Round(m * 100)}%, {Math.Round(y * 100)}%, {Math.Round(k * 100)}%)";
        }

        private string RGBtoYUV(Color color)
        {
            double r = color.R;
            double g = color.G;
            double b = color.B;

            double y = 0.299 * r + 0.587 * g + 0.114 * b;
            double u = -0.14713 * r - 0.28886 * g + 0.436 * b;
            double v = 0.615 * r - 0.51499 * g - 0.10001 * b;

            return $"({Math.Round(y, 2)}, {Math.Round(u, 2)}, {Math.Round(v, 2)})";
        }

        private string RGBtoYCbCr(Color color)
        {
            double r = color.R;
            double g = color.G;
            double b = color.B;

            double y = 0.299 * r + 0.587 * g + 0.114 * b;
            double cb = 128 - 0.168736 * r - 0.331264 * g + 0.5 * b;
            double cr = 128 + 0.5 * r - 0.418688 * g - 0.081312 * b;

            return $"({Math.Round(y, 2)}, {Math.Round(cb, 2)}, {Math.Round(cr, 2)})";
        }

        private string RGBtoLAB(Color color)
        {
            double r = PivotRGB(color.R / 255.0);
            double g = PivotRGB(color.G / 255.0);
            double b = PivotRGB(color.B / 255.0);

            double x = r * 0.4124 + g * 0.3576 + b * 0.1805;
            double y = r * 0.2126 + g * 0.7152 + b * 0.0722;
            double z = r * 0.0193 + g * 0.1192 + b * 0.9505;

            x = x / 0.95047;
            y = y / 1.00000;
            z = z / 1.08883;

            x = PivotXYZ(x);
            y = PivotXYZ(y);
            z = PivotXYZ(z);

            double l = 116 * y - 16;
            double a = 500 * (x - y);
            double bb = 200 * (y - z);

            return $"({Math.Round(l, 2)}, {Math.Round(a, 2)}, {Math.Round(bb, 2)})";
        }

        private double PivotRGB(double value)
        {
            if (value > 0.04045)
                return Math.Pow((value + 0.055) / 1.055, 2.4);

            return value / 12.92;
        }

        private double PivotXYZ(double value)
        {
            if (value > 0.008856)
                return Math.Pow(value, 1.0 / 3.0);

            return (7.787 * value) + (16.0 / 116.0);
        }
        private Color ModifyHSVWithChannels(Color color, int hChange, int sChange, int vChange)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double h = 0;

            if (delta == 0)
                h = 0;
            else if (max == r)
                h = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                h = 60 * (((b - r) / delta) + 2);
            else
                h = 60 * (((r - g) / delta) + 4);

            if (h < 0)
                h += 360;

            double s = max == 0 ? 0 : delta / max;
            double v = max;

            h = (h + hChange) % 360;
            if (h < 0) h += 360;

            s = Math.Max(0, Math.Min(1, s + sChange / 100.0));
            v = Math.Max(0, Math.Min(1, v + vChange / 100.0));

            if (!chkC1.Checked) h = 0;
            if (!chkC2.Checked) s = 0;
            if (!chkC3.Checked) v = 0;

            return HSVtoRGB(h, s, v);
        }

        private Color ModifyCMYKWithChannels(Color color, int cChange, int mChange, int yChange, int kChange)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double k = 1 - Math.Max(r, Math.Max(g, b));

            double c = 0, m = 0, y = 0;

            if (k < 1)
            {
                c = (1 - r - k) / (1 - k);
                m = (1 - g - k) / (1 - k);
                y = (1 - b - k) / (1 - k);
            }

            c = Math.Max(0, Math.Min(1, c + cChange / 100.0));
            m = Math.Max(0, Math.Min(1, m + mChange / 100.0));
            y = Math.Max(0, Math.Min(1, y + yChange / 100.0));
            k = Math.Max(0, Math.Min(1, k + kChange / 100.0));

            if (!chkC1.Checked) c = 0;
            if (!chkC2.Checked) m = 0;
            if (!chkC3.Checked) y = 0;
            if (!chkC4.Checked) k = 0;

            int newR = Clamp((int)(255 * (1 - c) * (1 - k)));
            int newG = Clamp((int)(255 * (1 - m) * (1 - k)));
            int newB = Clamp((int)(255 * (1 - y) * (1 - k)));

            return Color.FromArgb(newR, newG, newB);
        }

        private Color ModifyYUVWithChannels(Color color, int yChange, int uChange, int vChange)
        {
            double r = color.R;
            double g = color.G;
            double b = color.B;

            double y = 0.299 * r + 0.587 * g + 0.114 * b;
            double u = -0.14713 * r - 0.28886 * g + 0.436 * b;
            double v = 0.615 * r - 0.51499 * g - 0.10001 * b;

            y += yChange;
            u += uChange;
            v += vChange;

            if (!chkC1.Checked) y = 0;
            if (!chkC2.Checked) u = 0;
            if (!chkC3.Checked) v = 0;

            int newR = Clamp((int)(y + 1.13983 * v));
            int newG = Clamp((int)(y - 0.39465 * u - 0.58060 * v));
            int newB = Clamp((int)(y + 2.03211 * u));

            return Color.FromArgb(newR, newG, newB);
        }

        private Color ModifyYCbCrWithChannels(Color color, int yChange, int cbChange, int crChange)
        {
            double r = color.R;
            double g = color.G;
            double b = color.B;

            double y = 0.299 * r + 0.587 * g + 0.114 * b;
            double cb = 128 - 0.168736 * r - 0.331264 * g + 0.5 * b;
            double cr = 128 + 0.5 * r - 0.418688 * g - 0.081312 * b;

            y += yChange;
            cb += cbChange;
            cr += crChange;

            if (!chkC1.Checked) y = 0;
            if (!chkC2.Checked) cb = 128;
            if (!chkC3.Checked) cr = 128;

            int newR = Clamp((int)(y + 1.402 * (cr - 128)));
            int newG = Clamp((int)(y - 0.344136 * (cb - 128) - 0.714136 * (cr - 128)));
            int newB = Clamp((int)(y + 1.772 * (cb - 128)));

            return Color.FromArgb(newR, newG, newB);
        }

        private Color ModifyLABWithChannels(Color color, int lChange, int aChange, int bChange)
        {
            int r = Clamp(color.R + lChange);
            int g = Clamp(color.G + aChange);
            int b = Clamp(color.B + bChange);

            if (!chkC1.Checked) r = 0;
            if (!chkC2.Checked) g = 0;
            if (!chkC3.Checked) b = 0;

            return Color.FromArgb(r, g, b);
        }

        private void DrawColorSpace(string mode)
        {
            int width = 300;
            int height = 300;

            colorSpaceImage = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color;

                    if (mode == "RGB")
                    {
                        int r = Clamp(x * 255 / width);
                        int g = Clamp(y * 255 / height);
                        int b = 128;
                        color = Color.FromArgb(r, g, b);
                    }
                    else if (mode == "HSV")
                    {
                        double h = x * 360.0 / width;
                        double s = y / (double)height;
                        double v = 1;
                        color = HSVtoRGB(h, s, v);
                    }
                    else if (mode == "CMYK")
                    {
                        double c = x / (double)width;
                        double m = y / (double)height;
                        double yy = 0.3;
                        double k = 0.1;

                        int r = Clamp((int)(255 * (1 - c) * (1 - k)));
                        int g = Clamp((int)(255 * (1 - m) * (1 - k)));
                        int b = Clamp((int)(255 * (1 - yy) * (1 - k)));

                        color = Color.FromArgb(r, g, b);
                    }
                    else if (mode == "YUV")
                    {
                        double yy = 128;
                        double u = x - width / 2;
                        double v = y - height / 2;

                        int r = Clamp((int)(yy + 1.13983 * v));
                        int g = Clamp((int)(yy - 0.39465 * u - 0.58060 * v));
                        int b = Clamp((int)(yy + 2.03211 * u));

                        color = Color.FromArgb(r, g, b);
                    }
                    else if (mode == "YCbCr")
                    {
                        double yy = 128;
                        double cb = x * 255.0 / width;
                        double cr = y * 255.0 / height;

                        int r = Clamp((int)(yy + 1.402 * (cr - 128)));
                        int g = Clamp((int)(yy - 0.344136 * (cb - 128) - 0.714136 * (cr - 128)));
                        int b = Clamp((int)(yy + 1.772 * (cb - 128)));

                        color = Color.FromArgb(r, g, b);
                    }
                    else
                    {
                        int l = x * 255 / width;
                        int a = y * 255 / height;
                        int b = 128;

                        color = Color.FromArgb(Clamp(l), Clamp(a), Clamp(b));
                    }

                    colorSpaceImage.SetPixel(x, y, color);
                }
            }

            UpdateColorSpaceView(null, null);
        }

        private void UpdateColorSpaceView(object sender, EventArgs e)
        {


            if (colorSpaceImage == null)
                return;

            int zoom = trackZoom.Value;
            int angle = trackRotate.Value;

            int newWidth = colorSpaceImage.Width * zoom / 100;
            int newHeight = colorSpaceImage.Height * zoom / 100;

            Bitmap zoomed = new Bitmap(colorSpaceImage, newWidth, newHeight);
            Bitmap rotated = new Bitmap(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(rotated))
            {
                g.Clear(Color.White);
                g.TranslateTransform(newWidth / 2, newHeight / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-newWidth / 2, -newHeight / 2);
                g.DrawImage(zoomed, 0, 0);
            }

            pictureBoxSpace.Image = rotated;

            lblSpaceInfo.Text =
                "Zoom: " + zoom + "%   Rotate: " + angle + "°";
        }

        private void PictureBoxSpace_MouseClick(object sender, MouseEventArgs e)
        {
            if (pictureBoxSpace.Image == null)
                return;

            Bitmap bmp = new Bitmap(pictureBoxSpace.Image);

            int x = e.X * bmp.Width / pictureBoxSpace.Width;
            int y = e.Y * bmp.Height / pictureBoxSpace.Height;

            if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height)
                return;

            Color color = bmp.GetPixel(x, y);

            panelSelectedColor.BackColor = color;

            lblSpaceInfo.Text =
                $"Selected Color:\n" +
                $"RGB   → ({color.R}, {color.G}, {color.B})\n" +
                $"HSV   → {RGBtoHSV(color)}\n" +
                $"CMYK  → {RGBtoCMYK(color)}\n" +
                $"YUV   → {RGBtoYUV(color)}\n" +
                $"YCbCr → {RGBtoYCbCr(color)}\n" +
                $"LAB   → {RGBtoLAB(color)}";
        }

    }
}