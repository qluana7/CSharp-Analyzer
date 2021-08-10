using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSAnalyzer
{
    /// <summary>
    /// Properties.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PropertiesDialog : Window
    {
        public bool AOD { get; private set; }

        public PropertiesDialog()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CancelButton_Click(null, null);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void General_AodCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AOD = true;
        }

        private void General_AodCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AOD = false;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var s = e.AddedItems[0].ToString();

            var panel = s switch
            {
                "General" => GeneralStackPanel,
                "Colors" => ColorsStackPanel,
                _ => throw new InvalidOperationException("Unknown ListBox Selection.")
            };

            var panels = SettingGrid.Children.Cast<UIElement>().Where(l => l is StackPanel && l.Uid == "SettingPanel");

            for (int i = 0; i < panels.Count(); i++)
                panels.ElementAt(i).Visibility = Visibility.Collapsed;

            panel.Visibility = Visibility.Visible;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;

            Action act = rect.Name switch
            {
                "Comment" => CommentRect_Click,
                "String" => StringRect_Click,
                "Char" => CharRect_Click,
                _ => throw new InvalidCastException("Color Selection Error")
            };
        }

        /*
        private string GetColor(string color)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();

        }
        */

        private void CommentRect_Click()
        {
            
        }

        private void StringRect_Click()
        {

        }

        private void CharRect_Click()
        {

        }
    }
}
