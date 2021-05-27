using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace CSAnalyzer
{
    public class CSCompletionData : ICompletionData
    {
        public CSCompletionData(string s)
        {
            this.Text = s;
        }

        public CSCompletionData(string s, string desc) : this(s)
        {
            this.Description = desc;
        }

        public CSCompletionData(string s, string desc, ImageSource ima) : this(s, desc)
        {
            this.Image = ima;
        }

        public CSCompletionData(string s, string desc, ImageSource ima, double pri, object cont = null) : this(s, desc, ima)
        {
            this.Content = cont;
            this.Priority = pri;
        }

        public ImageSource Image { get; }

        public string Text { get; }

        public object Content { get; }

        public object Description { get; }

        public double Priority { get; set; }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
