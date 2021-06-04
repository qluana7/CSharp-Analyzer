using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml;
using FindReplace;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using CSAnalyzer.Structures;

namespace CSAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Editor : Window
    {
        public IAnaylzer Analyzer { get; set; }

        public Dictionary<string, string> RecentDictionary { get; set; }

        public Status CurrentStatus { get; set; }

        public string CurrentFile { get; set; }
        public Language CurrentLanguage { get; set; }

        public Dictionary<string, IHighlightingDefinition> HighlightingDictionary { get; set; }

        private bool isEdited;
        public bool IsEdited
        {
            get { return isEdited; }
            set
            {
                isEdited = value;

                if (CurrentFile == null)
                    return;

                if (value)
                {
                    if (FileTextBox.Text[^1] != '*')
                        FileTextBox.Text += "*";
                }
                else
                {
                    if (FileTextBox.Text[^1] == '*')
                        FileTextBox.Text = FileTextBox.Text[0..^1];
                }
            }
        }

        private bool IsDispose { get; set; }

        public Editor()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            Analyzer = new CSharpAnalyzer(this);
            RecentDictionary = new Dictionary<string, string>();
            HighlightingDictionary = new Dictionary<string, IHighlightingDefinition>();

            TextEditor.TextChanged += (sender, e) =>
            {
                if (IsDispose)
                    return;

                ToolUndoButton.IsEnabled = TextEditor.CanUndo;
                ToolRedoButton.IsEnabled = TextEditor.CanRedo;

                IsEdited = true;
            };

            CurrentFile = null;
            IsEdited = false;
            ToolUndoButton.IsEnabled = false;
            ToolRedoButton.IsEnabled = false;
            ChangeIsEnabled(false);

            Console.OpenStandardInput();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1);

            string[] syntaxs =
            {
                "CSharp"
            };

            for (int i = 0; i < syntaxs.Length; i++)
            {
                XshdSyntaxDefinition xshd;

                using (XmlReader reader = XmlReader.Create(@$"Highlighting\{syntaxs[i]}Syntax.xshd"))
                {
                    xshd = HighlightingLoader.LoadXshd(reader);
                }

                HighlightingDictionary.Add(syntaxs[i], HighlightingLoader.Load(xshd, null));
            }

            TextEditor.SyntaxHighlighting = HighlightingDictionary["CSharp"];

            Keyboard.AddKeyDownHandler(this, Editor_KeyDown);

            //TextEditor.TextArea.TextEntering += TextArea_TextEntering;
            //TextEditor.TextArea.TextEntered += TextArea_TextEntered;

            ChangeStatus(Status.Ready);
        }

        public void ChangeStatus(Status sta, string str = null)
        {
            StatusImage.Source = (BitmapImage)Resources["Status_" + sta.ToString()];
            StatusTextBlock.Text = "Status: " + (str ?? sta.ToString());
            CurrentStatus = sta;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (btn.Uid == "Minimum")
                WindowState = WindowState.Minimized;
            else if (btn.Uid == "Maximum")
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
                FileExit_Click(sender, e);
        }

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            var mod = Keyboard.Modifiers;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                #region FileItems
                if (e.Key == Key.N)
                    FileNew_Click(null, null);
                else if (e.Key == Key.O)
                    FileOpen_Click(null, null);
                else if (e.Key == Key.S)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        FileSaveAs_Click(null, null);
                    else
                        FileSave_Click(null, null);
                }
                else if (e.Key == Key.W)
                    FileClose_Click(null, null);
                else if (e.Key == Key.X)
                    FileExit_Click(null, null);
                #endregion

                #region EditItem
                else if (e.Key == Key.G)
                    EditLineMove_Click(null, null);
                #endregion

                #region RunItem
                else if (e.Key == Key.M)
                    RunCompile_Click(null, null);
                else if (e.Key == Key.B)
                {
                    var m = Keyboard.Modifiers ^ ModifierKeys.Control;
                    if (m == ModifierKeys.None)
                        RunRun_Click(null, null);
                    else if (m == ModifierKeys.Shift)
                        RunSaveAndRun_Click(null, null);
                    else if (m == ModifierKeys.Alt)
                        RunSaveAsAndRun_Click(null, null);
                }
                #endregion
            }

            if ((sender as Control).Name == "TextEditor")
            {
                if (mod != ModifierKeys.None && mod != ModifierKeys.Shift)
                    e.Handled = true;
            }            
        }

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            Action<object, RoutedEventArgs> func = btn.Uid switch
            {
                "Undo" => EditUndo_Click,
                "Redo" => EditRedo_Click,
                "New" => FileNew_Click,
                "Open" => FileOpen_Click,
                "Save" => FileSave_Click,
                "SaveAs" => FileSaveAs_Click,
                "Run" => RunRun_Click,
                _ => throw new InvalidOperationException()
            };

            func(null, null);
        }

        private void TextEditor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char[] spe =
            {
                '}', ')', ']', '"', '\''
            };

            if (e.Text == 
                ((TextEditor.Text.Length < TextEditor.CaretOffset + 1) 
                ? string.Empty : TextEditor.Text[TextEditor.CaretOffset].ToString())
                && spe.Contains(e.Text.FirstOrDefault()))
            {
                e.Handled = true;
                TextEditor.CaretOffset++;
                return;
            }
            else if (TextEditor.Text.Length >= TextEditor.CaretOffset + 1)
            {
                if (TextEditor.Text[TextEditor.CaretOffset] != ' ')
                    return;
            }

            TextEditor.SelectedText = string.Empty;

            var s = e.Text switch
            {
                "{" => new Tuple<string, int>(" }", 1),
                "(" => new Tuple<string, int>(")", 0),
                "[" => new Tuple<string, int>("]", 0),
                "\"" => new Tuple<string, int>("\"", 0),
                "'" => new Tuple<string, int>("'", 0),
                _ => null
            };

            if (s == null)
                return;

            var co = TextEditor.CaretOffset;
            TextEditor.Text = TextEditor.Text.Insert(TextEditor.CaretOffset, e.Text + s.Item1);
            e.Handled = true;
            TextEditor.CaretOffset = co + s.Item2 + 1;
        }

        private void SelectWord()
        {
            int cursorPosition = TextEditor.SelectionStart;
            int nextSpace = TextEditor.Text.IndexOf(' ', cursorPosition);
            int selectionStart = 0;
            string trimmedString;
            if (nextSpace != -1)
            {
                trimmedString = TextEditor.Text.Substring(0, nextSpace);
            }
            else
            {
                trimmedString = TextEditor.Text;
            }


            if (trimmedString.LastIndexOf(' ') != -1)
            {
                selectionStart = 1 + trimmedString.LastIndexOf(' ');
                trimmedString = trimmedString.Substring(1 + trimmedString.LastIndexOf(' '));
            }

            TextEditor.SelectionStart = selectionStart;
            TextEditor.SelectionLength = trimmedString.Length;
        }

        #region MenuStrip
        private async Task<bool> CheckSave()
        {
            if (IsEdited)
            {
                var r = MessageBox.Show("Do you want to save?", "Save?", MessageBoxButton.YesNoCancel);
                if (r == MessageBoxResult.Yes)
                {
                    var save = new System.Windows.Forms.SaveFileDialog();
                    if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        await System.IO.File.WriteAllTextAsync(save.FileName, TextEditor.Text);
                        return true;
                    }
                    else
                        return false;
                }
                else if (r == MessageBoxResult.Cancel)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        private void ChangeIsEnabled(bool b)
        {
            dynamic[] controls =
            {
                MenuFileSave,
                MenuFileSaveAs,
                MenuFileClose,
                ToolSaveButton,
                ToolSaveAsButton,
                ToolBuildButton,
                MenuEdit,
                MenuRun
            };

            for (int i = 0; i < controls.Length; i++)
                controls[i].IsEnabled = b;
        }

        private void RecordRecent(string path)
        {
            if (MenuFileRecent.Items.Count == 1 && !(MenuFileRecent.Items[0] as MenuItem).IsEnabled)
                MenuFileRecent.Items.Clear();

            var name = System.IO.Path.GetFileName(path);
            if (RecentDictionary.ContainsKey(name))
            {
                RecentDictionary.Remove(name);
                RecentDictionary.Add(name, path);
            }
            else
            {
                RecentDictionary.Add(name, path);
            }

            MenuFileRecent.Items.Clear();

            var d = new Dictionary<string, string>(RecentDictionary).ToArray();
            Array.Reverse(d);

            for (int i = 0; i < d.Length; i++)
                Add(d[i].Key, d[i].Value, i + 1);

            void Add(string n, string p, int num)
            {
                var menu = new MenuItem() { Uid = n, Header = $"{num}. {p}" };
                menu.Click += async (sender, e) =>
                {
                    if (!await CheckSave())
                        return;

                    ChangeStatus(Status.Loading);

                    MenuItem obj = sender as MenuItem;
                    var fname = string.Join('.', obj.Header.ToString().Split('.')[1..]).Trim();

                    IsDispose = false;
                    var t = await System.IO.File.ReadAllTextAsync(fname);
                    FileTextBox.Text = System.IO.Path.GetFileName(fname);
                    TextEditor.Text = t;
                    CurrentFile = fname;
                    EditGrid.Visibility = Visibility.Visible;
                    ChangeIsEnabled(true);

                    ChangeStatus(Status.Ready);
                };
                MenuFileRecent.Items.Add(menu);
            }
        }

        private async void FileNew_Click(object sender, RoutedEventArgs e)
        {
            if (!await CheckSave())
                return;

            IsDispose = true;
            IsEdited = false;
            FileTextBox.Text = $@"unnamed.{CurrentLanguage switch
                {
                    Structures.Language.CSharp => "cs",
                    Structures.Language.Python => "py"
                }
            }";
            TextEditor.Text = string.Empty;
            CurrentFile = string.Empty;
            EditGrid.Visibility = Visibility.Visible;
            ChangeIsEnabled(true);

            IsDispose = false;
        }

        private async void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            if (!await CheckSave())
                return;

            ChangeStatus(Status.Loading);

            var open = new System.Windows.Forms.OpenFileDialog()
            {
                Multiselect = false
            };

            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsDispose = true;
                var t = await System.IO.File.ReadAllTextAsync(open.FileName);
                FileTextBox.Text = System.IO.Path.GetFileName(open.FileName);
                TextEditor.Text = t;
                CurrentFile = open.FileName;
                EditGrid.Visibility = Visibility.Visible;
                ChangeIsEnabled(true);

                RecordRecent(open.FileName);

                IsDispose = false;
            }

            ChangeStatus(Status.Ready);
        }

        private async void FileClose_Click(object sender, RoutedEventArgs e)
        {
            if (!await CheckSave())
                return;

            IsDispose = true;
            IsEdited = false;
            TextEditor.Text = string.Empty;
            CurrentFile = null;
            EditGrid.Visibility = Visibility.Hidden;
            FileTextBox.Text = string.Empty;
            ToolUndoButton.IsEnabled = false;
            ToolRedoButton.IsEnabled = false;
            ChangeIsEnabled(false);
        }

        private async void FileSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentFile))
            {
                FileSaveAs_Click(sender, e);
                return;
            }

            await System.IO.File.WriteAllTextAsync(CurrentFile, TextEditor.Text);
            IsEdited = false;
            RecordRecent(CurrentFile);
        }

        private async void FileSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var open = new System.Windows.Forms.SaveFileDialog();

            if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await System.IO.File.WriteAllTextAsync(open.FileName, TextEditor.Text);
                IsEdited = false;
                CurrentFile = open.FileName;
                FileTextBox.Text = System.IO.Path.GetFileName(open.FileName);
                RecordRecent(open.FileName);
            }
        }

        private async void FileExit_Click(object sender, RoutedEventArgs e)
        {
            if (!await CheckSave())
                return;
                
            Application.Current.Shutdown();
        }

        private void EditUndo_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Undo();
        }

        private void EditRedo_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Redo();
        }

        private void EditCut_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Cut();
        }

        private void EditCopy_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Copy();
        }

        private void EditPaste_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Paste();
        }
        
        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Delete();
        }

        private void EditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.SelectAll();
        }

        private void EditFind_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FindReplaceDialog(new FindReplaceMgr()
            {
                CurrentEditor = new TextEditorAdapter(TextEditor),
                ShowSearchIn = false,
                OwnerWindow = this
            }, Options.Find);

            dialog.Show();
        }

        private void EditReplace_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FindReplaceDialog(new FindReplaceMgr()
            {
                CurrentEditor = new TextEditorAdapter(TextEditor),
                ShowSearchIn = false,
                OwnerWindow = this
            }, Options.Replace);

            dialog.Show();
        }

        private void EditLineMove_Click(object sender, RoutedEventArgs e)
        {
            var move = new LineMoveDialog(TextEditor.Document, TextEditor.CaretOffset);
            if (move.ShowDialog().Value)
                if (int.TryParse(move.Line, out int r))
                    TextEditor.CaretOffset = TextEditor.Document.GetOffset(new TextLocation(r, 0));
        }

        private void RunCompile_Click(object sender, RoutedEventArgs e)
        {
            CompileTextBox.Text = string.Empty;
            ChangeStatus(Status.Build, "Compiling...");
            var com = Analyzer.Compile(TextEditor.Text);

            CompileTextBox.Text = com.Result;

            if (!com.Exception)
                StatusTextBlock.Text = "Status: Success";
            else
                StatusTextBlock.Text = "Status: Error";

            AnalyzerTab.SelectedIndex = 0;
        }

        private async void RunRun_Click(object sender, RoutedEventArgs e)
        {
            CompileTextBox.Text = string.Empty;
            ResultTextBox.Text = string.Empty;
            ChangeStatus(Status.Build, "Running");
            var com = await Analyzer.Evaluate(TextEditor.Text);
            ResultTextBox.Text = com.Result;

            if (com.Exception)
                StatusTextBlock.Text = "Status: Error";
            else
                StatusTextBlock.Text = "Status: Success";

            AnalyzerTab.SelectedIndex = 1;
        }

        private void RunSaveAndRun_Click(object sender, RoutedEventArgs e)
        {
            FileSave_Click(null, null);
            RunRun_Click(null, null);
        }

        private void RunSaveAsAndRun_Click(object sender, RoutedEventArgs e)
        {
            FileSaveAs_Click(null, null);
            RunRun_Click(null, null);
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileClose_Click(sender, e);
        }
    }

    public class TabItems
    {
        public string Header;
    }
}
