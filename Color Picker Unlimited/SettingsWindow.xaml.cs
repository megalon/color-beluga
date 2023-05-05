using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace Color_Picker_Unlimited
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow? mainWindow = this.Owner as MainWindow;
            if (mainWindow != null)
            {
                string selectedTheme = (e.AddedItems[0] as ComboBoxItem).Content.ToString();
                mainWindow.SwitchTheme(selectedTheme);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow? mainWindow = this.Owner as MainWindow;

            if (mainWindow == null)
            {
                Debug.WriteLine("Owner is null! Could not find mainWindow!");
                return;
            }

            TextBox textBox = (TextBox)sender;

            string text = textBox.Text;

            if(int.TryParse(text, out int result)) {
                mainWindow.SetTimerInterval(result);
            }
        }
    }
}
