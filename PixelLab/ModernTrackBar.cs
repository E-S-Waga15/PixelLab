using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PixelLab
{
    /// <summary>
    /// Minimal horizontal slider with neon track and square thumb.
    /// </summary>
    internal class ModernTrackBar : UserControl
    {
        private int _minimum;
        private int _maximum = 100;
        private int _value;
        private bool _dragging;

        [Browsable(false)]
        public int LargeChange { get; set; } = 5;

        public int Minimum
        {
            get => _minimum;
            set { _minimum = value; ClampValue(); Invalidate(); }
        }

        public int Maximum
        {
            get => _maximum;
            set { _maximum = value; ClampValue(); Invalidate(); }
        }

        public int Value
        {
            get => _value;
            set
            {
                int v = Math.Clamp(value, _minimum, _maximum);
                if (_value == v) return;
                _value = v;
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? ValueChanged;

        public ModernTrackBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            Height = 28;
            MinimumSize = new Size(80, 22);
            BackColor = UiTheme.Background;
            Cursor = Cursors.Hand;
        }

        private void ClampValue() => _value = Math.Clamp(_value, _minimum, _maximum);

        private Rectangle GetTrackRect()
        {
            const int pad = 10;
            int left = pad + Padding.Left;
            int right = pad + Padding.Right;
            int cy = Height / 2;
            return new Rectangle(left, cy - 1, Math.Max(20, Width - left - right), 2);
        }

        private int ValueToX(int trackLeft, int trackWidth)
        {
            if (_maximum <= _minimum) return trackLeft;
            double t = (_value - _minimum) / (double)(_maximum - _minimum);
            return trackLeft + (int)Math.Round(t * trackWidth);
        }

        private void SetValueFromX(int x)
        {
            var tr = GetTrackRect();
            if (tr.Width <= 0) return;
            double t = Math.Clamp((x - tr.Left) / (double)tr.Width, 0, 1);
            Value = _minimum + (int)Math.Round(t * (_maximum - _minimum));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var tr = GetTrackRect();
            int thumbX = ValueToX(tr.Left, tr.Width);
            int cy = Height / 2;

            using (var trackBrush = new Pen(Color.FromArgb(70, 90, 115), 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(trackBrush, tr.Left, cy, tr.Right, cy);

            if (thumbX > tr.Left)
            {
                using var fillPen = new Pen(UiTheme.NeonCyan, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                g.DrawLine(fillPen, tr.Left, cy, thumbX, cy);
            }

            var glow = new Rectangle(thumbX - 11, cy - 11, 22, 22);
            using (var outer = new SolidBrush(Color.FromArgb(45, UiTheme.NeonCyan)))
                g.FillEllipse(outer, glow);
            using (var thumb = new SolidBrush(UiTheme.NeonCyan))
            {
                var sq = new Rectangle(thumbX - 6, cy - 6, 12, 12);
                using var path = UiTheme.RoundedRect(sq, 3);
                g.FillPath(thumb, path);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            Capture = true;
            SetValueFromX(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
                SetValueFromX(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
            Capture = false;
        }
    }
}
