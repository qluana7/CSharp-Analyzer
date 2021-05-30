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
using System.Windows.Shapes;

namespace CSAnalyzer_Preview.Dialog
{
    /// <summary>
    /// CustomMessageBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        protected CustomMessageBox()
        {
            InitializeComponent();
        }

        public CustomMessageBox(string content) : this()
            => ContentTextBlock.Text = content;

        public CustomMessageBox(string content,
            string button1 = null, string button2 = null, string button3 = null) : this(content)
        {
            if (!string.IsNullOrWhiteSpace(button1))
            {
                Button1.IsEnabled = true;
                Button1.Content = button1;
            }

            if (!string.IsNullOrWhiteSpace(button2))
            {
                Button2.IsEnabled = true;
                Button2.Content = button2;
            }

            if (!string.IsNullOrWhiteSpace(button3))
            {
                Button3.IsEnabled = true;
                Button3.Content = button3;
            }
        }

        public CustomMessageBox(string caption, string content,
            string button1 = null, string button2 = null, string button3 = null)
            : this(content, button1, button2, button3)
            => CaptionTextBox.Text = caption;

        public string SelectedButton { get; private set; } = null;

        public bool IsEscaped { get; private set; } = false;
        public bool IsEnterd { get; private set; } = false;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Exit_Click(null, null);
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
                IsEnterd = true;
                Close();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            IsEscaped = true;
            Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
