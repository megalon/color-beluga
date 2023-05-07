using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Color_Beluga
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        private Dictionary<string, System.Drawing.Color> ColorNames;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        const int VK_ALT = 0x12;
        const int VK_LCONTROL = 0xA2;

        private DispatcherTimer _timer;

        private int _imageSize;
        private POINT _cursorPos;

        public MainWindow()
        {
            InitializeComponent();

            LoadTheme();

            _imageSize = 4;

            ColorNames = new Dictionary<string, System.Drawing.Color>();

            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                System.Windows.Media.Color mediaColor = (System.Windows.Media.Color)color.GetValue(null);
                System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);

                if (color.Name.Equals("Transparent"))
                {
                    ColorNames["White"] = drawingColor;
                } else
                {
                    ColorNames[color.Name] = drawingColor;
                }
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(Settings.Default.RefreshRate);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public void SetTimerInterval(double ms)
        {
            if (ms < 0) ms = 0;

            _timer.Stop();
            _timer.Interval = TimeSpan.FromMilliseconds(ms);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            short keyState = GetAsyncKeyState(VK_ALT);

            // Check if the key is being pressed
            if ((keyState & 0x8000) != 0)
            {
                GetCursorPos(out _cursorPos);
            }

            UpdateClonedPixelsImage();
            TopmostCheck();
        }

        private void TopmostCheck()
        {
            if (!this.Topmost)
            {
                this.Topmost = true;
            }
        }

        private System.Drawing.Color GetColorUnderCursor()
        {
            GetCursorPos(out POINT cursorPos);
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, cursorPos.X, cursorPos.Y);
            ReleaseDC(IntPtr.Zero, hdc);

            int r = (int)(pixel & 0x000000FF);
            int g = (int)((pixel & 0x0000FF00) >> 8);
            int b = (int)((pixel & 0x00FF0000) >> 16);

            return System.Drawing.Color.FromArgb(r, g, b);
        }

        private void UpdateColorInfo(System.Drawing.Color color)
        {
            ColorInfo.Text = $"R: {color.R} G: {color.G} B: {color.B}";

            string closestColorName = GetClosestColorName(color);

            closestColorName = Regex.Replace(closestColorName, @"(\p{Lu})(\p{Ll})", " $1$2");

            // Remove the leading space
            if (closestColorName.Length > 0 && closestColorName[0] == ' ')
            {
                closestColorName = closestColorName[1..];
            }

            ColorName.Text = closestColorName;
        }

        private string GetClosestColorName(System.Drawing.Color color)
        {
            double minDistance = double.MaxValue;
            string closestColorName = "";
            foreach (KeyValuePair<string, System.Drawing.Color> namedColor in ColorNames)
            {
                double distance = Math.Sqrt(
                    Math.Pow(namedColor.Value.R - color.R, 2) +
                    Math.Pow(namedColor.Value.G - color.G, 2) +
                    Math.Pow(namedColor.Value.B - color.B, 2)
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestColorName = namedColor.Key;
                }
            }

            return closestColorName;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ApplyThemes(this.Resources.MergedDictionaries);
            settingsWindow.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public Collection<ResourceDictionary> SwitchTheme(string theme)
        {
            ResourceDictionary newTheme;

            if (theme == "Dark")
            {
                newTheme = new ResourceDictionary
                {
                    Source = new Uri("DarkTheme.xaml", UriKind.Relative)
                };
            }
            else
            {
                newTheme = new ResourceDictionary
                {
                    Source = new Uri("LightTheme.xaml", UriKind.Relative)
                };
            }

            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(newTheme);
            return this.Resources.MergedDictionaries;
        }

        private void LoadTheme()
        {
            SwitchTheme(Settings.Default.Theme);
        }

        private void UpdateClonedPixelsImage()
        {
            // Convert the WPF point to a System.Drawing.Point
            System.Drawing.Point screenPoint = new System.Drawing.Point((int)_cursorPos.X, (int)_cursorPos.Y);

            int size = _imageSize;

            using (Bitmap screenshot = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(screenPoint.X - (size / 2), screenPoint.Y - (size / 2), 0, 0, new System.Drawing.Size(size, size), CopyPixelOperation.SourceCopy);
                }

                Bitmap blurredScreenshot = Utils.ApplyGaussianBlur(screenshot);

                // Convert the System.Drawing.Bitmap to a WPF BitmapImage.
                BitmapImage bitmapImage;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    blurredScreenshot.Save(memoryStream, ImageFormat.Bmp);
                    memoryStream.Position = 0;
                    bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Necessary for cross-thread operations.
                }

                // Update the Image control
                ClonedPixelsImage.Source = bitmapImage;

                // Update the color text
                UpdateColorInfo(Utils.GetMedianColorOfBitmap(blurredScreenshot));
            }
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            // Make the image bigger to zoom out
            int maxDimension = 64;
            _imageSize *= 2;

            if (_imageSize > maxDimension)
            {
                _imageSize = maxDimension;
            }
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            // Make the image smaller to zoom in
            _imageSize /= 2;

            if (_imageSize < 1)
            {
                _imageSize = 1;
            }
        }
    }
}
