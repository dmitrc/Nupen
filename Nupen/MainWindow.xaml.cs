using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Nupen
{
    public enum DrawingMode { NONE, PEN, HIGHLIGHT, ARROW, RECT, CIRCLE, ERASE };

    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = (-20);

        private IntPtr _hwnd;
        private int _extendedStyle;

        private DrawingMode _mode = DrawingMode.NONE;
        private int _brushSize = 3;
        private Color _brushColor = Color.FromRgb(255, 0, 0);

        [DllImport("user32.dll")]
        public static extern int GetWindowLongPtr(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLongPtr(IntPtr hwnd, int index, int newStyle);

        private bool _canClickThrough = false;
        
        public bool CanClickThrough
        {
            get => _canClickThrough;
            set
            {
                _canClickThrough = value;
                if (value)
                {
                    SetWindowLongPtr(_hwnd, GWL_EXSTYLE, _extendedStyle | WS_EX_TRANSPARENT);
                    Background = null;
                }
                else
                {
                    SetWindowLongPtr(_hwnd, GWL_EXSTYLE, _extendedStyle);
                    Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hwnd = new WindowInteropHelper(this).Handle;
            _extendedStyle = GetWindowLongPtr(_hwnd, GWL_EXSTYLE);

            SetDrawingMode(DrawingMode.NONE);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Left = 0;
            Top = 0;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            var toolsWindow = new ToolsWindow();
            toolsWindow.Owner = this;
            toolsWindow.Show();
        }

        public void Clear()
        {
            inkCanvas.Strokes.Clear();
        }

        public void SetDrawingMode(DrawingMode mode)
        {
            _mode = mode;

            CanClickThrough = mode == DrawingMode.NONE;

            inkCanvas.Cursor = mode == DrawingMode.ERASE ? Cursors.Cross : Cursors.Pen;
            inkCanvas.EditingMode = mode == DrawingMode.ERASE ? InkCanvasEditingMode.EraseByStroke : InkCanvasEditingMode.Ink;
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = mode == DrawingMode.HIGHLIGHT;
            inkCanvas.DefaultDrawingAttributes.FitToCurve = true;

            inkCanvas.DefaultDrawingAttributes.Width = mode == DrawingMode.ERASE ? _brushSize * 2 : _brushSize;
            inkCanvas.DefaultDrawingAttributes.Height = mode == DrawingMode.ERASE ? _brushSize * 2 : _brushSize;
            inkCanvas.DefaultDrawingAttributes.Color = _brushColor;
        }

        private void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (_mode == DrawingMode.ARROW)
            {
                StrokeArrowConvert(e.Stroke);
            }
            else if (_mode == DrawingMode.RECT)
            {
                StrokeRectConvert(e.Stroke);
            }
            else if (_mode == DrawingMode.CIRCLE)
            {
                StrokeCircleConvert(e.Stroke);
            }
        }

        private void StrokeRectConvert(Stroke stroke)
        {
            StylusPointCollection ptsRect = new StylusPointCollection();
            StylusPointCollection pts = stroke.StylusPoints;

            double minX = double.MaxValue;
            double minY = minX;
            double maxX = double.MinValue;
            double maxY = maxX;

            //find bounding area
            foreach (StylusPoint pt in pts)
            {
                if (pt.X > maxX)
                    maxX = pt.X;
                if (pt.X < minX)
                    minX = pt.X;

                if (pt.Y > maxY)
                    maxY = pt.Y;
                if (pt.Y < minY)
                    minY = pt.Y;
            }

            //stroke four corners of the rect
            ptsRect.Add(new StylusPoint(minX, minY));
            ptsRect.Add(new StylusPoint(minX, maxY));
            ptsRect.Add(new StylusPoint(maxX, maxY));
            ptsRect.Add(new StylusPoint(maxX, minY));
            ptsRect.Add(new StylusPoint(minX, minY));
            stroke.StylusPoints = ptsRect;

            //no smoothing
            stroke.DrawingAttributes.FitToCurve = false;
        }

        private void StrokeArrowConvert(Stroke stroke)
        {
            //Console.WriteLine("Stroke Completed");
            double toRadians = Math.PI / 180.0;
            StylusPointCollection ptsRect = new StylusPointCollection();
            StylusPointCollection pts = stroke.StylusPoints;

            StylusPoint pt1 = pts[pts.Count - 1];
            StylusPoint pt2 = pts[0];

            ptsRect.Add(pt1);
            ptsRect.Add(pt2);
            //compute arrow head
            double arrowAngle = 30.0 * toRadians;
            double deltaX = pt2.X - pt1.X;
            double deltaY = pt2.Y - pt1.Y;
            double theta = Math.Atan2(deltaY, deltaX); //radians
            double x1 = Math.Cos(theta + arrowAngle);
            double x2 = Math.Cos(theta - arrowAngle);
            double y1 = Math.Sin(theta + arrowAngle);
            double y2 = Math.Sin(theta - arrowAngle);
            double mag = 10.0; //arrorhead line length

            ptsRect.Add(new StylusPoint(pt2.X - mag * x1, pt2.Y - mag * y1));
            ptsRect.Add(new StylusPoint(pt2.X, pt2.Y));
            ptsRect.Add(new StylusPoint(pt2.X - mag * x2, pt2.Y - mag * y2));
            stroke.StylusPoints = ptsRect;
            stroke.DrawingAttributes.FitToCurve = false;
        }

        private void StrokeCircleConvert(Stroke stroke)
        {
            double toRadians = Math.PI / 180.0;
            StylusPointCollection ptsRect = new StylusPointCollection();
            StylusPointCollection pts = stroke.StylusPoints;

            //rather that try to redraw the circle, consider a diagonal line as indicator of circle center and diameter
            StylusPoint ptStart = pts[0];
            StylusPoint ptEnd = pts[pts.Count - 1];
            StylusPoint ptCenter = new StylusPoint((ptStart.X + ptEnd.X) / 2.0, (ptStart.Y + ptEnd.Y) / 2.0);

            double deltaX = ptEnd.X - ptCenter.X;
            double deltaY = ptEnd.Y - ptCenter.Y;
            double radius = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            for (double theta = 0; theta <= 360.0; theta += 10.0)
            {
                double angle = theta * toRadians;
                ptsRect.Add(new StylusPoint(ptCenter.X + radius * Math.Cos(angle), ptCenter.Y + radius * Math.Sin(angle)));
            }
            stroke.StylusPoints = ptsRect;
            stroke.DrawingAttributes.FitToCurve = false;
        }
    }
}
