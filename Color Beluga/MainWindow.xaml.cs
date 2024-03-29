using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using ColorMine.ColorSpaces.Comparisons;
using ColorMine.ColorSpaces;

namespace Color_Beluga
{
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

        private Dictionary<System.Drawing.Color, string> ColorNames;

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

        private POINT _cursorPos;

        public MainWindow()
        {
            InitializeComponent();

            LoadTheme();

            ColorNames = new Dictionary<System.Drawing.Color, string>();

            LoadColorDataResourceJSON(Settings.Default.ColorSet);

            CheckBoxBlur.IsChecked = Settings.Default.BlurEnabled;

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
            //if ((keyState & 0x8000) != 0)
            //{
            GetCursorPos(out _cursorPos);
            //}

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
            CieDe2000Comparison comparer = new CieDe2000Comparison();

            Rgb queryColor = new Rgb { R = color.R, G = color.G, B = color.B };

            foreach (KeyValuePair<System.Drawing.Color, string> namedColor in ColorNames)
            {
                Rgb namedRgb = new Rgb { R = namedColor.Key.R, G = namedColor.Key.G, B = namedColor.Key.B };

                double distance = queryColor.Compare(namedRgb, comparer);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestColorName = namedColor.Value;
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
                    Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                };
            }
            else
            {
                newTheme = new ResourceDictionary
                {
                    Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative)
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

            int size = Settings.Default.ImageSize;

            try
            {
                using (Bitmap screenshot = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(screenPoint.X - (size / 2), screenPoint.Y - (size / 2), 0, 0, new System.Drawing.Size(size, size), CopyPixelOperation.SourceCopy);
                    }

                    if (Settings.Default.BlurEnabled)
                    {
                        UpdateImageAndColorInfo(Utils.ApplyGaussianBlur(screenshot));
                    }
                    else
                    {
                        UpdateImageAndColorInfo(screenshot);
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // Catch special case where context is lost because computer is locked, then unlocked
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateImageAndColorInfo(Bitmap screenshot)
        {
            // Convert the System.Drawing.Bitmap to a WPF BitmapImage.
            BitmapImage bitmapImage;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                screenshot.Save(memoryStream, ImageFormat.Bmp);
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
            UpdateColorInfo(Utils.GetMedianColorOfBitmap(screenshot));
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            // Make the image bigger to zoom out
            int maxDimension = 64;
            Settings.Default.ImageSize *= 2;

            if (Settings.Default.ImageSize > maxDimension)
            {
                Settings.Default.ImageSize = maxDimension;
            }
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            // Make the image smaller to zoom in
            Settings.Default.ImageSize /= 2;

            if (Settings.Default.ImageSize < 1)
            {
                Settings.Default.ImageSize = 1;
            }
        }

        private void CheckBoxBlur_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox? checkBox = sender as System.Windows.Controls.CheckBox;

            Settings.Default.BlurEnabled = (bool)checkBox.IsChecked;
        }

        public void LoadColorDataResourceJSON(string resourceName)
        {
            if (resourceName.Equals("Standard"))
            {
                resourceName = "Color_Beluga.Resources.Standard.json";
            }
            else if (resourceName.Equals("Standard (Simple Names)"))
            {
                resourceName = "Color_Beluga.Resources.Simple-Names.json";
            }

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            ColorNames.Clear();

            Dictionary<string, string> colorDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (var kvp in colorDictionary)
            {
                ColorNames.Add(ColorTranslator.FromHtml("#FF" + kvp.Key), kvp.Value);
            }
        }
    }
}
