using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Color_Beluga
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            RefreshRateTextBox.Text = "" + Settings.Default.RefreshRate;

            SetThemeComboBox(Settings.Default.Theme);

            VersionLabel.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow? mainWindow = this.Owner as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            string selectedTheme = (e.AddedItems[0] as ComboBoxItem).Content.ToString();

            Collection<ResourceDictionary> newTheme = mainWindow.SwitchTheme(selectedTheme);

            ApplyThemes(newTheme);

            Settings.Default.Theme = selectedTheme;
            Settings.Default.Save();
        }

        public void ApplyThemes(Collection<ResourceDictionary> themes)
        {
            this.Resources.MergedDictionaries.Clear();

            foreach(ResourceDictionary theme in themes)
            {
                this.Resources.MergedDictionaries.Add(theme);
            }
        }

        private void RefreshRateTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow? mainWindow = this.Owner as MainWindow;

            if (mainWindow == null)
            {
                Debug.WriteLine("Owner is null! Could not find mainWindow!");
                return;
            }

            TextBox textBox = (TextBox)sender;

            string text = textBox.Text;

            if(double.TryParse(text, out double result)) {
                mainWindow.SetTimerInterval(result);

                // Save the selected theme to the settings
                Settings.Default.RefreshRate = result;
                Settings.Default.Save();
            }
        }

        private void SetThemeComboBox(string theme)
        {
            foreach (ComboBoxItem item in ThemeComboBox.Items)
            {
                if (item.Content.ToString() == theme)
                {
                    ThemeComboBox.SelectedItem = item;
                    break;
                }
            }
        }
    }
}
