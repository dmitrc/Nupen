using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Nupen
{
    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = (-20);

        private IntPtr _hwnd;
        private int _extendedStyle;

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
                }
                else
                {
                    SetWindowLongPtr(_hwnd, GWL_EXSTYLE, _extendedStyle);
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

            CanClickThrough = true;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Left = 0;
            Top = 0;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
        }
    }
}
