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
    public enum DrawingMode { NONE, PEN, HIGHLIGHT, ARROW, RECT, ERASE };
    public enum BrushSize { S = 3, M = 5, L = 10, XL = 24 };
    public enum BrushColor { WHITE, BLACK, RED, GREEN, BLUE, YELLOW, CYAN, MAGENTA };

    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = (-20);
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;

        private IntPtr _hwnd;
        private int _extendedStyle;

        private ToolsWindow? _toolsWindow;
        private DrawingMode _mode = DrawingMode.NONE;
        private BrushSize _brushSize = BrushSize.S;
        private BrushColor _brushColor = BrushColor.RED;

        [DllImport("user32.dll")]
        public static extern int GetWindowLongPtr(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLongPtr(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

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

            SetupHotkeys();

            _toolsWindow = new ToolsWindow();
            _toolsWindow.Owner = this;

            SetDrawingMode(_mode);
            SetSize(_brushSize);
            SetColor(_brushColor);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Left = 0;
            Top = 0;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            _toolsWindow?.Show();
        }

        public void Reset()
        {
            if (_mode != DrawingMode.NONE)
            {
                SetDrawingMode(DrawingMode.NONE);
            }
        }

        public void Clear()
        {
            inkCanvas?.Strokes?.Clear();
        }

        public void Undo()
        {
            var count = inkCanvas.Strokes?.Count ?? 0;
            if (count > 0)
            {
                inkCanvas?.Strokes?.RemoveAt(count - 1);
            }
        }

        public void SetDrawingMode(DrawingMode mode)
        {
            _mode = mode;
            CanClickThrough = mode == DrawingMode.NONE;

            inkCanvas.EditingMode = mode == DrawingMode.ERASE ? InkCanvasEditingMode.EraseByStroke : InkCanvasEditingMode.Ink;

            inkCanvas.DefaultDrawingAttributes.FitToCurve = false;
            inkCanvas.DefaultDrawingAttributes.IsHighlighter = mode == DrawingMode.HIGHLIGHT;
            inkCanvas.DefaultDrawingAttributes.Width = mode == DrawingMode.ERASE ? (int)_brushSize * 2 : (int)_brushSize;
            inkCanvas.DefaultDrawingAttributes.Height = mode == DrawingMode.ERASE ? (int)_brushSize * 2 : (int)_brushSize;
            inkCanvas.DefaultDrawingAttributes.Color = GetBrushColor(_brushColor);

            _toolsWindow?.UpdateDrawingModeButtonStates(mode);
        }

        public void SetColor(BrushColor color)
        {
            _brushColor = color;
            inkCanvas.DefaultDrawingAttributes.Color = GetBrushColor(_brushColor);

            _toolsWindow?.UpdateColorButtonStates(color);
        }

        public void SetSize(BrushSize size)
        {
            _brushSize = size;

            inkCanvas.DefaultDrawingAttributes.Width = _mode == DrawingMode.ERASE ? (int)size * 2 : (int)size;
            inkCanvas.DefaultDrawingAttributes.Height = _mode == DrawingMode.ERASE ? (int)size * 2 : (int)size;

            _toolsWindow?.UpdateSizeButtonStates(size);
        }

        private Color GetBrushColor(BrushColor color)
        {
            if (color == BrushColor.WHITE)
            {
                return Color.FromRgb(255, 255, 255);
            }
            else if (color == BrushColor.BLACK)
            {
                return Color.FromRgb(0, 0, 0);
            }
            else if (color == BrushColor.RED)
            {
                return Color.FromRgb(255, 0, 0);
            }
            else if (color == BrushColor.GREEN)
            {
                return Color.FromRgb(0, 255, 0);
            }
            else if (color == BrushColor.BLUE)
            {
                return Color.FromRgb(0, 0, 255);
            }
            else if (color == BrushColor.YELLOW)
            {
                return Color.FromRgb(255, 255, 0);
            }
            else if (color == BrushColor.CYAN)
            {
                return Color.FromRgb(0, 255, 255);
            }
            else if (color == BrushColor.MAGENTA)
            {
                return Color.FromRgb(255, 0, 255);
            }
            return Color.FromRgb(0, 0, 0);
        }

        private void SetupHotkeys()
        {
            var id = GetType().GetHashCode();

            RegisterHotKey(_hwnd, id, 0, 0x60);
            RegisterHotKey(_hwnd, id, 0, 0x61);
            RegisterHotKey(_hwnd, id, 0, 0x62);
            RegisterHotKey(_hwnd, id, 0, 0x63);
            RegisterHotKey(_hwnd, id, 0, 0x64);
            RegisterHotKey(_hwnd, id, 0, 0x65);
            RegisterHotKey(_hwnd, id, 0, 0x66);
            RegisterHotKey(_hwnd, id, 0, 0x67);
            RegisterHotKey(_hwnd, id, 0, 0x68);
            RegisterHotKey(_hwnd, id, 0, 0x69);

            ComponentDispatcher.ThreadPreprocessMessage += OnThreadPreprocessMessage;
        }

        private void OnThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message != WM_HOTKEY)
            {
                return;
            }

            var key = (((int)msg.lParam >> 16) & 0xFFFF);

            if (key == 0x60) // Num0
            {
                Clear();
            }
            else if (key == 0x61) // Num1
            {
                Undo();
            }
            else if (key == 0x62) // Num2
            {
                SetDrawingMode(DrawingMode.NONE);
            }
            else if (key == 0x63) // Num3
            {
                if (_mode != DrawingMode.PEN)
                {
                    SetDrawingMode(DrawingMode.PEN);
                }
                else
                {
                    SetDrawingMode(DrawingMode.NONE);
                }
            }
            else if (key == 0x64) // Num4
            {
                if (_mode != DrawingMode.HIGHLIGHT)
                {
                    SetDrawingMode(DrawingMode.HIGHLIGHT);
                }
                else
                {
                    SetDrawingMode(DrawingMode.NONE);
                }
            }
            else if (key == 0x65) // Num5
            {
                if (_mode != DrawingMode.ARROW)
                {
                    SetDrawingMode(DrawingMode.ARROW);
                }
                else
                {
                    SetDrawingMode(DrawingMode.NONE);
                }
            }
            else if (key == 0x66) // Num6
            {
                if (_mode != DrawingMode.RECT)
                {
                    SetDrawingMode(DrawingMode.RECT);
                }
                else
                {
                    SetDrawingMode(DrawingMode.NONE);
                }
            }
            else if (key == 0x67) // Num7
            {
                if (_mode != DrawingMode.ERASE)
                {
                    SetDrawingMode(DrawingMode.ERASE);
                }
                else
                {
                    SetDrawingMode(DrawingMode.NONE);
                }
            }
            else if (key == 0x68) // Num8
            {
                if (_brushColor == BrushColor.WHITE)
                {
                    SetColor(BrushColor.BLACK);
                }
                else if (_brushColor == BrushColor.BLACK)
                {
                    SetColor(BrushColor.RED);
                }
                else if (_brushColor == BrushColor.RED)
                {
                    SetColor(BrushColor.GREEN);
                }
                else if (_brushColor == BrushColor.GREEN)
                {
                    SetColor(BrushColor.BLUE);
                }
                else if (_brushColor == BrushColor.BLUE)
                {
                    SetColor(BrushColor.YELLOW);
                }
                else if (_brushColor == BrushColor.YELLOW)
                {
                    SetColor(BrushColor.CYAN);
                }
                else if (_brushColor == BrushColor.CYAN)
                {
                    SetColor(BrushColor.MAGENTA);
                }
                else if (_brushColor == BrushColor.MAGENTA)
                {
                    SetColor(BrushColor.WHITE);
                }
            }
            else if (key == 0x69) // Num9
            {
                if (_brushSize == BrushSize.S)
                {
                    SetSize(BrushSize.M);
                }
                else if (_brushSize == BrushSize.M)
                {
                    SetSize(BrushSize.L);
                }
                else if (_brushSize == BrushSize.L)
                {
                    SetSize(BrushSize.XL);
                }
                else if (_brushSize == BrushSize.XL)
                {
                    SetSize(BrushSize.S);
                }
            }
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Reset();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Reset();
            }
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

            StylusPoint pt1 = pts[0];
            StylusPoint pt2 = pts[pts.Count - 1];

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
    }
}
