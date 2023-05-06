using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace Color_Picker_Unlimited
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

        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            LoadTheme();

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
            _timer.Interval = TimeSpan.FromMilliseconds(16.67);
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
            UpdateColorInfo();
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
        private void UpdateColorInfo()
        {
            System.Drawing.Color color = GetColorUnderCursor();
            ColorInfo.Text = $"R: {color.R} G: {color.G} B: {color.B}";

            ColorBox.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));

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
    }
}
