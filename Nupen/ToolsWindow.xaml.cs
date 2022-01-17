using System.Windows;
using System.Windows.Input;

namespace Nupen
{
    public partial class ToolsWindow : Window
    {
        public ToolsWindow()
        {
            InitializeComponent();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnNoneButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.SetDrawingMode(DrawingMode.NONE);
        }

        private void OnPenButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.SetDrawingMode(DrawingMode.PEN);
        }

        private void OnArrowButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.SetDrawingMode(DrawingMode.ARROW);
        }

        private void OnRectButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.SetDrawingMode(DrawingMode.RECT);
        }

        private void OnCircleButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.SetDrawingMode(DrawingMode.CIRCLE);
        }

        private void OnEraserButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.SetDrawingMode(DrawingMode.ERASE);
        }

        private void OnHighlighterButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.SetDrawingMode(DrawingMode.HIGHLIGHT);
        }

        private void OnEraseAllButtonClick(object sender, RoutedEventArgs e)
        {
            (Owner as MainWindow)?.Clear();
        }

        private void OnExitButtonClick(object sender, RoutedEventArgs e)
        {
            Owner.Close();
        }
    }
}
