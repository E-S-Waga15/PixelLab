using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PixelLab
{
    internal static class UiTheme
    {
        public static readonly Color Background = Color.FromArgb(22, 28, 41);
        public static readonly Color GlassFill = Color.FromArgb(72, 35, 45, 65);
        public static readonly Color GlassFillDark = Color.FromArgb(96, 28, 36, 52);
        public static readonly Color NeonCyan = Color.FromArgb(0, 190, 255);
        public static readonly Color NeonGlow = Color.FromArgb(90, 0, 190, 255);
        public static readonly Color TextPrimary = Color.FromArgb(240, 245, 255);
        public static readonly Color TextMuted = Color.FromArgb(160, 175, 200);
        public static readonly Color ControlFill = Color.FromArgb(45, 58, 82);
        public static readonly Color ControlHover = Color.FromArgb(58, 72, 98);
        public static readonly Color WorkspaceBg = Color.FromArgb(18, 24, 36);

        public static Font TitleFont => new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        public static Font BodyFont => new Font("Segoe UI", 9.5F);
        public static Font SmallFont => new Font("Segoe UI", 8.5F);

        public static void StyleFlatButton(Button btn, bool accent = false)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = accent ? 1 : 0;
            btn.FlatAppearance.BorderColor = NeonGlow;
            btn.BackColor = accent ? Color.FromArgb(50, 0, 140, 200) : ControlFill;
            btn.ForeColor = TextPrimary;
            btn.Font = BodyFont;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = ControlHover;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 0, 120, 170);
        }

        public static void StyleToggleButton(Button btn, bool active)
        {
            StyleFlatButton(btn, active);
            btn.BackColor = active ? Color.FromArgb(80, 0, 160, 220) : ControlFill;
            btn.ForeColor = active ? NeonCyan : TextPrimary;
        }

        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.FlatStyle = FlatStyle.Flat;
            cmb.BackColor = ControlFill;
            cmb.ForeColor = TextPrimary;
            cmb.Font = BodyFont;
        }

        public static void StyleTrackBar(TrackBar tb)
        {
            // TrackBar does not support Color.Transparent
            tb.BackColor = Background;
            tb.TickStyle = TickStyle.None;
        }

        public static void StyleLabel(Label lbl, bool muted = false)
        {
            lbl.ForeColor = muted ? TextMuted : TextPrimary;
            lbl.BackColor = Color.Transparent;
            lbl.Font = muted ? SmallFont : BodyFont;
        }

        public static void StyleCheckBox(CheckBox chk)
        {
            chk.ForeColor = TextPrimary;
            chk.BackColor = Background;
            chk.FlatStyle = FlatStyle.Flat;
            chk.Font = SmallFont;
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
