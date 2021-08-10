using CircularReveal.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CircularReveal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Themes theme = Themes.Light;
        private TimeSpan duration = TimeSpan.FromMilliseconds(700);

        private readonly Dictionary<Themes, ResourceDictionary> themes;
        private readonly Dictionary<Themes, object> geometries;

        public MainWindow()
        {
            InitializeComponent();
            themes = new()
            {
                { Themes.Light, Application.Current.Resources.MergedDictionaries[0] },
                { Themes.Dark, new() { Source = new("pack://application:,,,/Resources/Themes/DarkTheme.xaml") } }
            };

            geometries = new()
            {
                { Themes.Light, mPath.Data },
                { Themes.Dark, FindResource("MoonGeometry") }
            };
        }
        private async void mButton_Click(object sender, RoutedEventArgs e)
        {
            var w = mGrid.ActualWidth;
            var h = mGrid.ActualHeight;
            DoubleAnimation rxAnimation, ryAnimation;
            RVGrid.Visibility = Visibility.Visible;
            var ellipseGeometry = RVImage.Clip as EllipseGeometry;
            Point center = mButton.TranslatePoint(new Point(mButton.ActualWidth / 2, mButton.ActualHeight / 2), mGrid);
            ellipseGeometry.Center = center;
            float radius = (float)Hypot(Math.Max(w - center.X, center.X), Math.Max(h - center.Y, center.Y));
            ToggleTheme();
            if (theme == Themes.Dark)
            {
                mPath.Data = (PathGeometry)geometries[Themes.Dark];
                RVGrid.Background = new ImageBrush(Render(mGrid));
                rxAnimation = new(mButton.ActualWidth / 2, radius, duration);
                ryAnimation = new(mButton.ActualHeight / 2, radius, duration);
                await Task.Delay(100);
                RVImage.Source = Render(mGrid);
            }
            else
            {
                RVImage.Source = Render(mGrid);
                rxAnimation = new(radius, mButton.ActualWidth / 2, duration);
                ryAnimation = new(radius, mButton.ActualHeight / 2, duration);
            }
            rxAnimation.Completed += (s, e) =>
            {
                RVGrid.Visibility = Visibility.Collapsed;
                RVGrid.Background = null;
                RVImage.Source = null;
                if (theme == Themes.Light)
                {
                    mPath.Data = (PathGeometry)geometries[Themes.Light];
                }
            };
            ellipseGeometry.BeginAnimation(EllipseGeometry.RadiusXProperty, rxAnimation);
            ellipseGeometry.BeginAnimation(EllipseGeometry.RadiusYProperty, ryAnimation);
        }
        private void ToggleTheme()
        {
            theme ^= Themes.Dark;
            themeText.Text = theme.ToString();
            Application.Current.Resources.MergedDictionaries[0] = themes[theme];
        }
        public static BitmapSource Render(FrameworkElement visual)
        {
            RenderTargetBitmap bitmap = new((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }
        public static double Hypot(double val1, double val2)
        {
            return Math.Sqrt((val1 * val1) + (val2 * val2));
        }
    }
}
