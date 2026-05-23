using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PixelLab
{
    internal class GlassPanel : Panel
    {
        public int CornerRadius { get; set; } = 12;
        public bool StrongGlow { get; set; }

        public GlassPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var path = UiTheme.RoundedRect(rect, CornerRadius);
            using var fill = new SolidBrush(UiTheme.GlassFill);
            e.Graphics.FillPath(fill, path);

            using var inner = new LinearGradientBrush(
                rect,
                Color.FromArgb(30, 255, 255, 255),
                Color.Transparent,
                LinearGradientMode.Vertical);
            e.Graphics.FillPath(inner, path);

            int glowAlpha = StrongGlow ? 140 : 80;
            using var borderPen = new Pen(Color.FromArgb(glowAlpha, UiTheme.NeonCyan), StrongGlow ? 2f : 1f);
            e.Graphics.DrawPath(borderPen, path);

            if (StrongGlow)
            {
                var glowRect = Rectangle.Inflate(rect, 2, 2);
                using var glowPath = UiTheme.RoundedRect(glowRect, CornerRadius + 2);
                using var glowPen = new Pen(Color.FromArgb(35, UiTheme.NeonCyan), 4f);
                e.Graphics.DrawPath(glowPen, glowPath);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent != null)
                e.Graphics.Clear(Parent.BackColor);
            else
                base.OnPaintBackground(e);
        }
    }
}
