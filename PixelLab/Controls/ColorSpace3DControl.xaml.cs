using System;
using System.Windows.Controls;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using Color = System.Windows.Media.Color;

namespace PixelLab.Controls
{
    public partial class ColorSpace3DControl : System.Windows.Controls.UserControl
    {
        private HelixViewport3D viewport;
        private ModelVisual3D modelContainer;
        private Model3DGroup spaceGroup;
        private GeometryModel3D markerModel;
        private double baseCameraDistance = 3.0;
        private string currentMode = "RGB";

        // حدث يتم إطلاقه عند النقر المزدوج على مجسم الـ 3D لاختيار اللون
        public event Action<byte, byte, byte> ColorSelectedFrom3D;

        public ColorSpace3DControl()
        {
            viewport = new HelixViewport3D();

            // إعدادات بيئة الهيليكس الافتراضية للتفاعل الاحترافي
            viewport.ShowCoordinateSystem = false;
            viewport.ShowViewCube = false;

            viewport.Children.Add(new DefaultLights());
            modelContainer = new ModelVisual3D();
            viewport.Children.Add(modelContainer);

            this.Content = viewport;

            spaceGroup = new Model3DGroup();
            modelContainer.Content = spaceGroup;

            // ربط حدث ضغط الماوس
            viewport.MouseDown += Viewport_MouseDown;

            BuildSpace("RGB");
            CreateMarker();
        }

        public void SetColorMode(string mode)
        {
            BuildSpace(mode);
        }

        public void BuildSpace(string mode)
        {
            currentMode = mode;
            spaceGroup.Children.Clear();

            if (mode.ToUpper() == "HSV")
            {
                BuildSolidHsvCylinder();
            }
            else
            {
                BuildSolidRgbCube();
            }

            if (markerModel != null) spaceGroup.Children.Add(markerModel);
            try { viewport.ZoomExtents(); } catch { }
        }

        private void BuildSolidRgbCube()
        {
            int grid = 12;
            double min = -0.5, max = 0.5;
            double step = (max - min) / grid;

            // حل مشكلة تداخل المتغير b عبر تسمية البارامترات بأسماء واضحة لونيًا
            void AddColoredQuad(Point3D ptA, Point3D ptB, Point3D ptC, Point3D ptD, Color clrA, Color clrB, Color clrC, Color clrD)
            {
                var mb = new MeshBuilder(false, false);
                mb.AddQuad(ptA, ptB, ptC, ptD);
                var meshObj = mb.ToMesh(true);

                byte rComp = (byte)((clrA.R + clrB.R + clrC.R + clrD.R) / 4);
                byte gComp = (byte)((clrA.G + clrB.G + clrC.G + clrD.G) / 4);
                byte bComp = (byte)((clrA.B + clrB.B + clrC.B + clrD.B) / 4);

                var mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(rComp, gComp, bComp)));
                var geom = new GeometryModel3D(meshObj, mat) { BackMaterial = mat };
                spaceGroup.Children.Add(geom);
            }

            // Z = -0.5 (back)
            for (int i = 0; i < grid; i++) for (int j = 0; j < grid; j++)
            {
                double x0 = min + i * step; double x1 = min + (i + 1) * step;
                double y0 = min + j * step; double y1 = min + (j + 1) * step; double z = -0.5;
                Point3D pA = new Point3D(x0, y0, z); Point3D pB = new Point3D(x1, y0, z);
                Point3D pC = new Point3D(x1, y1, z); Point3D pD = new Point3D(x0, y1, z);
                Color cA = Color.FromRgb((byte)((x0 + 0.5) * 255), (byte)((y0 + 0.5) * 255), 0);
                Color cB = Color.FromRgb((byte)((x1 + 0.5) * 255), (byte)((y0 + 0.5) * 255), 0);
                Color cC = Color.FromRgb((byte)((x1 + 0.5) * 255), (byte)((y1 + 0.5) * 255), 0);
                Color cD = Color.FromRgb((byte)((x0 + 0.5) * 255), (byte)((y1 + 0.5) * 255), 0);
                AddColoredQuad(pA, pB, pC, pD, cA, cB, cC, cD);
            }

            // Z = +0.5 (front)
            for (int i = 0; i < grid; i++) for (int j = 0; j < grid; j++)
            {
                double x0 = min + i * step; double x1 = min + (i + 1) * step;
                double y0 = min + j * step; double y1 = min + (j + 1) * step; double z = 0.5;
                Point3D pA = new Point3D(x0, y0, z); Point3D pB = new Point3D(x1, y0, z);
                Point3D pC = new Point3D(x1, y1, z); Point3D pD = new Point3D(x0, y1, z);
                Color cA = Color.FromRgb((byte)((x0 + 0.5) * 255), (byte)((y0 + 0.5) * 255), 255);
                Color cB = Color.FromRgb((byte)((x1 + 0.5) * 255), (byte)((y0 + 0.5) * 255), 255);
                Color cC = Color.FromRgb((byte)((x1 + 0.5) * 255), (byte)((y1 + 0.5) * 255), 255);
                Color cD = Color.FromRgb((byte)((x0 + 0.5) * 255), (byte)((y1 + 0.5) * 255), 255);
                AddColoredQuad(pA, pB, pC, pD, cA, cB, cC, cD);
            }

            // X = -0.5 (left)
            for (int i = 0; i < grid; i++) for (int j = 0; j < grid; j++)
            {
                double z0 = min + i * step; double z1 = min + (i + 1) * step;
                double y0 = min + j * step; double y1 = min + (j + 1) * step; double x = -0.5;
                Point3D pA = new Point3D(x, y0, z0); Point3D pB = new Point3D(x, y0, z1);
                Point3D pC = new Point3D(x, y1, z1); Point3D pD = new Point3D(x, y1, z0);
                Color cA = Color.FromRgb(0, (byte)((y0 + 0.5) * 255), (byte)((z0 + 0.5) * 255));
                Color cB = Color.FromRgb(0, (byte)((y0 + 0.5) * 255), (byte)((z1 + 0.5) * 255));
                Color cC = Color.FromRgb(0, (byte)((y1 + 0.5) * 255), (byte)((z1 + 0.5) * 255));
                Color cD = Color.FromRgb(0, (byte)((y1 + 0.5) * 255), (byte)((z0 + 0.5) * 255));
                AddColoredQuad(pA, pB, pC, pD, cA, cB, cC, cD);
            }

            // X = +0.5 (right)
            for (int i = 0; i < grid; i++) for (int j = 0; j < grid; j++)
            {
                double z0 = min + i * step; double z1 = min + (i + 1) * step;
                double y0 = min + j * step; double y1 = min + (j + 1) * step; double x = 0.5;
                Point3D pA = new Point3D(x, y0, z0); Point3D pB = new Point3D(x, y0, z1);
                Point3D pC = new Point3D(x, y1, z1); Point3D pD = new Point3D(x, y1, z0);
                Color cA = Color.FromRgb(255, (byte)((y0 + 0.5) * 255), (byte)((z0 + 0.5) * 255));
                Color cB = Color.FromRgb(255, (byte)((y0 + 0.5) * 255), (byte)((z1 + 0.5) * 255));
                Color cC = Color.FromRgb(255, (byte)((y1 + 0.5) * 255), (byte)((z1 + 0.5) * 255));
                Color cD = Color.FromRgb(255, (byte)((y1 + 0.5) * 255), (byte)((z0 + 0.5) * 255));
                AddColoredQuad(pA, pB, pC, pD, cA, cB, cC, cD);
            }

            // Y = +0.5 (top)
            for (int i = 0; i < grid; i++) for (int j = 0; j < grid; j++)
            {
                double x0 = min + i * step; double x1 = min + (i + 1) * step;
                double z0 = min + j * step; double z1 = min + (j + 1) * step; double y = 0.5;
                Point3D pA = new Point3D(x0, y, z0); Point3D pB = new Point3D(x1, y, z0);
                Point3D pC = new Point3D(x1, y, z1); Point3D pD = new Point3D(x0, y, z1);
                Color cA = Color.FromRgb((byte)((x0 + 0.5) * 255), 255, (byte)((z0 + 0.5) * 255));
                Color cB = Color.FromRgb((byte)((x1 + 0.5) * 255), 255, (byte)((z0 + 0.5) * 255));
                Color cC = Color.FromRgb((byte)((x1 + 0.5) * 255), 255, (byte)((z1 + 0.5) * 255));
                Color cD = Color.FromRgb((byte)((x0 + 0.5) * 255), 255, (byte)((z1 + 0.5) * 255));
                AddColoredQuad(pA, pB, pC, pD, cA, cB, cC, cD);
            }

            // Y = -0.5 (bottom)
            for (int i = 0; i < grid; i++) for (int j = 0; j < grid; j++)
            {
                double x0 = min + i * step; double x1 = min + (i + 1) * step;
                double z0 = min + j * step; double z1 = min + (j + 1) * step; double y = -0.5;
                Point3D pA = new Point3D(x0, y, z0); Point3D pB = new Point3D(x1, y, z0);
                Point3D pC = new Point3D(x1, y, z1); Point3D pD = new Point3D(x0, y, z1);
                Color cA = Color.FromRgb((byte)((x0 + 0.5) * 255), 0, (byte)((z0 + 0.5) * 255));
                Color cB = Color.FromRgb((byte)((x1 + 0.5) * 255), 0, (byte)((z0 + 0.5) * 255));
                Color cC = Color.FromRgb((byte)((x1 + 0.5) * 255), 0, (byte)((z1 + 0.5) * 255));
                Color cD = Color.FromRgb((byte)((x0 + 0.5) * 255), 0, (byte)((z1 + 0.5) * 255));
                AddColoredQuad(pA, pB, pC, pD, cA, cB, cC, cD);
            }
        }

        private void BuildSolidHsvCylinder()
        {
            int segments = 45;
            double radius = 0.5;

            for (int i = 0; i < segments; i++)
            {
                // حساب زاوية الشريحة الحالية والشريحة التالية
                double angleDeg1 = (i * 360.0) / segments;
                double angleRad1 = angleDeg1 * (Math.PI / 180.0);

                double angleDeg2 = ((i + 1) * 360.0) / segments;
                double angleRad2 = angleDeg2 * (Math.PI / 180.0);

                // نقاط الشريحة الحالية
                double x1 = radius * Math.Cos(angleRad1);
                double z1 = radius * Math.Sin(angleRad1);

                // نقاط الشريحة التالية
                double x2 = radius * Math.Cos(angleRad2);
                double z2 = radius * Math.Sin(angleRad2);

                // إنشاء مش منفصل لهذه الشريحة الصغيرة
                var sliceMesh = new MeshGeometry3D();
                sliceMesh.Positions.Add(new Point3D(x1, -0.5, z1)); // 0
                sliceMesh.Positions.Add(new Point3D(x1, 0.5, z1));  // 1
                sliceMesh.Positions.Add(new Point3D(x2, -0.5, z2)); // 2
                sliceMesh.Positions.Add(new Point3D(x2, 0.5, z2));  // 3

                // بناء المثلثين لتشكيل المربع الجانبي الصغير
                sliceMesh.TriangleIndices.Add(0); sliceMesh.TriangleIndices.Add(1); sliceMesh.TriangleIndices.Add(2);
                sliceMesh.TriangleIndices.Add(1); sliceMesh.TriangleIndices.Add(3); sliceMesh.TriangleIndices.Add(2);

                // حساب متوسط اللون للشريحة بناءً على الزاوية الحالية في نظام HSV
                HsvToRgb(angleDeg1, 1.0, 0.9, out byte r, out byte g, out byte b);

                // تلوين الشريحة باللون النقي الفعلي
                var mat = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(r, g, b)));
                var geom = new GeometryModel3D(sliceMesh, mat) { BackMaterial = mat };

                spaceGroup.Children.Add(geom);
            }
        }

        private void CreateMarker()
        {
            var mb = new MeshBuilder(true, true);
            mb.AddSphere(new Point3D(0, 0, 0), 0.045, 16, 16);
            var markerMat = new EmissiveMaterial(new SolidColorBrush(Colors.White));
            markerModel = new GeometryModel3D(mb.ToMesh(true), markerMat);
            spaceGroup.Children.Add(markerModel);
        }

        // إضافة دالة الكاميرا المفقودة SetCamera لمنع أخطاء الاستدعاء في الفورم الرئيسي
        public void SetCamera(int zoomPercent, int angleDegrees)
        {
            double rad = angleDegrees * (Math.PI / 180.0);
            double dist = baseCameraDistance * (100.0 / Math.Max(10, zoomPercent));
            dist = Math.Max(0.6, Math.Min(10.0, dist));

            double camX = Math.Sin(rad) * dist;
            double camZ = Math.Cos(rad) * dist;
            double camY = dist * 0.5;

            var cameraPosition = new Point3D(camX, camY, camZ);
            var lookDirection = new Vector3D(-camX, -camY, -camZ);

            if (viewport.Camera is PerspectiveCamera pCam)
            {
                pCam.Position = cameraPosition;
                pCam.LookDirection = lookDirection;
            }
            else
            {
                viewport.Camera = new PerspectiveCamera(cameraPosition, lookDirection, new Vector3D(0, 1, 0), 45);
            }
        }

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                var mousePos = e.GetPosition(viewport);
                var hits = HelixToolkit.Wpf.Viewport3DHelper.FindHits(viewport.Viewport, mousePos);

                foreach (var hit in hits)
                {
                    var hitType = hit.GetType();
                    var modelProp = hitType.GetProperty("Model");
                    var modelObj = modelProp?.GetValue(hit) as Model3D;
                    if (modelObj is GeometryModel3D geom && geom != markerModel)
                    {
                        var pointProp = hitType.GetProperty("PointHit") ?? hitType.GetProperty("Point") ?? hitType.GetProperty("Position");
                        if (pointProp == null) continue;
                        var pObj = pointProp.GetValue(hit);
                        if (!(pObj is Point3D p)) continue;

                        byte r = 0, g = 0, bVal = 0;

                        if (currentMode.ToUpper() == "HSV")
                        {
                            double hue = Math.Atan2(p.Z, p.X) * (180.0 / Math.PI);
                            if (hue < 0) hue += 360;
                            double sat = Math.Min(1.0, Math.Sqrt(p.X * p.X + p.Z * p.Z) / 0.5);
                            double val = Math.Min(1.0, Math.Max(0.0, p.Y + 0.5));

                            HsvToRgb(hue, sat, val, out r, out g, out bVal);
                        }
                        else
                        {
                            r = (byte)Math.Max(0, Math.Min(255, (p.X + 0.5) * 255));
                            g = (byte)Math.Max(0, Math.Min(255, (p.Y + 0.5) * 255));
                            bVal = (byte)Math.Max(0, Math.Min(255, (p.Z + 0.5) * 255));
                        }

                        MoveMarkerToRgb(r, g, bVal);
                        ColorSelectedFrom3D?.Invoke(r, g, bVal);
                        break;
                    }
                }
            }
        }

        public void MoveMarkerToRgb(int r, int g, int b)
        {
            if (markerModel == null) return;

            if (currentMode.ToUpper() == "HSV")
            {
                RgbToHsv(r, g, b, out double h, out double s, out double v);
                double angleRad = h * (Math.PI / 180.0);
                double x = s * Math.Cos(angleRad) * 0.5;
                double z = s * Math.Sin(angleRad) * 0.5;
                double y = v - 0.5;
                markerModel.Transform = new TranslateTransform3D(x, y, z);
            }
            else
            {
                double x = (r / 255.0) - 0.5;
                double y = (g / 255.0) - 0.5;
                double z = (b / 255.0) - 0.5;
                markerModel.Transform = new TranslateTransform3D(x, y, z);
            }
        }

        private void RgbToHsv(int r, int g, int b, out double h, out double s, out double v)
        {
            double rd = r / 255.0; double gd = g / 255.0; double bd = b / 255.0;
            double max = Math.Max(rd, Math.Max(gd, bd)); double min = Math.Min(rd, Math.Min(gd, bd));
            double delta = max - min;
            h = 0;
            if (delta > 0)
            {
                if (max == rd) h = 60 * (((gd - bd) / delta) % 6);
                else if (max == gd) h = 60 * (((bd - rd) / delta) + 2);
                else if (max == bd) h = 60 * (((rd - gd) / delta) + 4);
            }
            if (h < 0) h += 360;
            s = (max == 0) ? 0 : delta / max;
            v = max;
        }

        private void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
        {
            double c = v * s; double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1)); double m = v - c;
            double r1 = 0, g1 = 0, b1 = 0;
            if (h >= 0 && h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h >= 60 && h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h >= 120 && h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h >= 180 && h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h >= 240 && h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }
            r = (byte)((r1 + m) * 255); g = (byte)((g1 + m) * 255); b = (byte)((b1 + m) * 255);
        }
    }
}