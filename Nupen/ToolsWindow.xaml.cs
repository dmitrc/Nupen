using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Nupen
{
    public partial class ToolsWindow : Window
    {
        private Style _defaultButtonStyle;
        private Style _activeButtonStyle;

        private Style _defaultColorButtonStyle;
        private Style _activeColorButtonStyle;

        private MainWindow? _mainWindow => Owner as MainWindow;

        public ToolsWindow()
        {
            InitializeComponent();

            _defaultButtonStyle = (Style)FindResource("DefaultButtonStyle");
            _activeButtonStyle = (Style)FindResource("ActiveButtonStyle");

            _defaultColorButtonStyle = (Style)FindResource("DefaultColorButtonStyle");
            _activeColorButtonStyle = (Style)FindResource("ActiveColorButtonStyle");
        }

        public void UpdateDrawingModeButtonStates(DrawingMode mode)
        {
            noneButton.Style = mode == DrawingMode.NONE ? _activeButtonStyle : _defaultButtonStyle;
            penButton.Style = mode == DrawingMode.PEN ? _activeButtonStyle : _defaultButtonStyle;
            highlightButton.Style = mode == DrawingMode.HIGHLIGHT ? _activeButtonStyle : _defaultButtonStyle;
            arrowButton.Style = mode == DrawingMode.ARROW ? _activeButtonStyle : _defaultButtonStyle;
            rectButton.Style = mode == DrawingMode.RECT ? _activeButtonStyle : _defaultButtonStyle;
            eraseButton.Style = mode == DrawingMode.ERASE ? _activeButtonStyle : _defaultButtonStyle;
        }

        public void UpdateColorButtonStates(Color color)
        {
            var colorButtons = grid.Children.OfType<Button>().Where(x => (string)x.Tag == "ColorButton").ToList();
            foreach (var button in colorButtons)
            {
                if (button.Background is SolidColorBrush brush
                    && brush.Color.ToString() == color.ToString())
                {
                    button.Style = _activeColorButtonStyle;
                }
                else
                {
                    button.Style = _defaultColorButtonStyle;
                }
            }
        }

        public void UpdateSizeButtonStates(BrushSize size)
        {
            smallButton.Style = size == BrushSize.S ? _activeButtonStyle : _defaultButtonStyle;
            mediumButton.Style = size == BrushSize.M ? _activeButtonStyle : _defaultButtonStyle;
            largeButton.Style = size == BrushSize.L ? _activeButtonStyle : _defaultButtonStyle;
            extraLargeButton.Style = size == BrushSize.XL ? _activeButtonStyle : _defaultButtonStyle;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnEraseAllButtonClick(object sender, RoutedEventArgs e)
        {
            _mainWindow?.Clear();
        }

        private void OnExitButtonClick(object sender, RoutedEventArgs e)
        {
            _mainWindow?.Close();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _mainWindow?.Reset();
            }
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow?.Reset();
        }

        private void OnColorButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button
                && button.Background is SolidColorBrush brush)
            {
                _mainWindow?.SetColor(brush.Color);
            }
        }

        private void OnSizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == smallButton)
            {
                _mainWindow?.SetSize(BrushSize.S);
            }
            else if (sender == mediumButton)
            {
                _mainWindow?.SetSize(BrushSize.M);
            }
            else if (sender == largeButton)
            {
                _mainWindow?.SetSize(BrushSize.L);
            }
            else if (sender == extraLargeButton)
            {
                _mainWindow?.SetSize(BrushSize.XL);
            }
        }

        private void OnModeButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender == noneButton)
            {
                _mainWindow?.SetDrawingMode(DrawingMode.NONE);
            }
            else if (sender == penButton)
            {
                _mainWindow?.SetDrawingMode(DrawingMode.PEN);
            }
            else if (sender == arrowButton)
            {
                _mainWindow?.SetDrawingMode(DrawingMode.ARROW);
            }
            else if (sender == rectButton)
            {
                _mainWindow?.SetDrawingMode(DrawingMode.RECT);
            }
            else if (sender == eraseButton)
            {
                _mainWindow?.SetDrawingMode(DrawingMode.ERASE);
            }
            else if (sender == highlightButton)
            {
                _mainWindow?.SetDrawingMode(DrawingMode.HIGHLIGHT);
            }
        }
    }
}
