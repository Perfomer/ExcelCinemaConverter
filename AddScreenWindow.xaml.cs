using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExcelConverter
{
    public partial class AddScreenWindow : Window
    {

        public IList aswDimensionalities;
        public string aswScreenName;

        public AddScreenWindow()
        {
            InitializeComponent();
        }

        public AddScreenWindow(List<string> dimensionalities)
        {
            InitializeComponent();
            LB_Dimensionalities.ItemsSource = dimensionalities;
            if (dimensionalities.Count == 1)
            {
                LB_Dimensionalities.IsEnabled = false;
                LB_Dimensionalities.SelectAll();
            }
        }

        private void Save()
        {
            if (LB_Dimensionalities.SelectedItems.Count > 0 && TB_ScreenName.Text.Length > 0)
            {
                aswDimensionalities = LB_Dimensionalities.SelectedItems;
                aswScreenName = TB_ScreenName.Text;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Не указано название экрана или не выбраны форматы.");
            }
        }
        
        private void B_Save_Click(object sender, RoutedEventArgs e) { Save(); }

        private void B_Save_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Save();
        }
    }
}
