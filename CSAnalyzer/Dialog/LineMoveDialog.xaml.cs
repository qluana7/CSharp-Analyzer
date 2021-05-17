using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Document;

namespace CSAnalyzer
{
    /// <summary>
    /// LineMoveDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LineMoveDialog : Window
    {
        public string Line { get; private set; }

        public LineMoveDialog(TextDocument docu, int offset)
        {
            InitializeComponent();

            LineNumTextBlock.Text += $" (1~{docu.LineCount})";
            LineNumTextBox.Text = docu.GetLocation(offset).Line.ToString();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Line = LineNumTextBox.Text;

            if (!DialogResult.HasValue)
                DialogResult = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            DialogResult = btn.Uid switch
            {
                "Cancel" => false,
                "Ok" => true,
                _ => throw new InvalidOperationException()
            };
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
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
    }
}
