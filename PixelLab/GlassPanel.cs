using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PixelLab
{
    internal class GlassPanel : Panel
    {
        public int CornerRadius { get; set; } = 12;
        public bool StrongGlow { get; set; }

        // خصائص جديدة للتحكم بحجم الحدود وقوة الظل
        public float BorderThickness { get; set; } = 2f; // حجم الحدود الجديد (يمكنك زيادته من هنا)
        public int ShadowSize { get; set; } = 8;        // مدى انتشار الظل الخارجي

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

            // ترك مساحة (Margin) كافية داخل اللوحة لرسم الظل الخارجي والحدود السميكة دون أن تترقرق أو تُقطع
            int offset = ShadowSize + (int)char.ToUpper((char)BorderThickness);
            var rect = new Rectangle(offset, offset, Width - (offset * 2), Height - (offset * 2));

            // ---- 1. رسم الظل الخارجي (Outer Shadow/Glow) ----
            // نقوم برسم عدة طبقات شفافة متداخلة لمحاكاة تأثير الـ Blur الناعم
            int shadowSteps = ShadowSize;
            int maxShadowAlpha = StrongGlow ? 45 : 20; // قوة تعتيم الظل بناءً على خاصية StrongGlow

            for (int i = shadowSteps; i > 0; i--)
            {
                var shadowRect = Rectangle.Inflate(rect, i, i);
                using var shadowPath = UiTheme.RoundedRect(shadowRect, CornerRadius + i);

                // حساب الشفافية تدريجياً (تكون أغمق بالقرب من الحدود وتتلاشى للخارج)
                int alpha = (maxShadowAlpha * (shadowSteps - i + 1)) / shadowSteps;

                using var shadowPen = new Pen(Color.FromArgb(alpha, Color.Black), 2f);
                e.Graphics.DrawPath(shadowPen, shadowPath);
            }

            // ---- 2. رسم خلفية الزجاج الداخلي ----
            using var path = UiTheme.RoundedRect(rect, CornerRadius);
            using var fill = new SolidBrush(UiTheme.GlassFill);
            e.Graphics.FillPath(fill, path);

            using var inner = new LinearGradientBrush(
                rect,
                Color.FromArgb(30, 255, 255, 255),
                Color.Transparent,
                LinearGradientMode.Vertical);
            e.Graphics.FillPath(inner, path);

            // ---- 3. رسم الحدود الأساسية (مع زيادة الحجم) ----
            int glowAlpha = StrongGlow ? 200 : 120; // زيادة الشفافية لتتناسب مع الحدود السميكة
            using var borderPen = new Pen(Color.FromArgb(glowAlpha, UiTheme.NeonCyan), BorderThickness);

            // لضمان رسم الحدود بدقة في منتصف المسار
            borderPen.Alignment = PenAlignment.Center;
            e.Graphics.DrawPath(borderPen, path);

            // ---- 4. توهج إضافي اختياري في حال تفعيل StrongGlow ----
            if (StrongGlow)
            {
                var neonGlowRect = Rectangle.Inflate(rect, (int)(BorderThickness / 2), (int)(BorderThickness / 2));
                using var neonGlowPath = UiTheme.RoundedRect(neonGlowRect, CornerRadius);
                using var neonGlowPen = new Pen(Color.FromArgb(40, UiTheme.NeonCyan), BorderThickness + 4f);
                e.Graphics.DrawPath(neonGlowPen, neonGlowPath);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent != null)
            {
                // مسح الخلفية بلون الأب لضمان شفافية ناعمة خالية من العيوب (Anti-aliasing artifacts)
                e.Graphics.Clear(Parent.BackColor);
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }
    }
}