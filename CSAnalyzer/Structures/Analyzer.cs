using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAnalyzer
{
    public class Analyzer
    {
        public Editor Main { get; }

        public Analyzer(Editor editor)
        {
            Main = editor;
        }

        public string[] ImportsList { get; set; } =
            {
                "System",
                "System.Collections.Generic",
                "System.IO",
                "System.Linq",
                "System.Text",
                "System.Net",
                "System.Threading.Tasks",
                "System.Diagnostics"
            };

        public async Task EvalCS(string cs)
        {
            try
            {
                var globals = new Variables(Main);

                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports(ImportsList);
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                var script = CSharpScript.Create(cs, sopts, typeof(Variables));
                script.Compile();
                var result = await script.RunAsync(globals).ConfigureAwait(false);

                if (result != null && result.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    Main.ResultTextBox.Text = result.ReturnValue.ToString();
                else
                    Main.ResultTextBox.Text = "No result was returned.";

                Main.StatusTextBlock.Text = "Status: Success";
                Main.AnalyzerTab.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                Main.ResultTextBox.Text = string.Concat(ex.GetType().ToString(), ex.Message);
                Main.StatusTextBlock.Text = "Status: Error";
                Main.AnalyzerTab.SelectedIndex = 1;
            }
        }

        public void Compile(string cs)
        {
             var globals = new Variables(Main);

            var sopts = ScriptOptions.Default;
            sopts = sopts.WithImports(ImportsList);
            sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(cs, sopts, typeof(Variables));
            var c = script.Compile();

            if (c.Length > 0)
            {
                var builder = new StringBuilder();
                for (int i = 0; i < c.Length; i++)
                    builder.AppendLine(c[i].ToString());

                Main.CompileTextBox.Text = builder.ToString();
                Main.StatusTextBlock.Text = "Status: Error";
            }
            else
                Main.StatusTextBlock.Text = "Status: Success";

            Main.AnalyzerTab.SelectedIndex = 0;
        }

        public class Variables
        {
            public Editor MainPage { get; set; }

            public Variables(Editor page)
            {
                MainPage = page;
            }
        }
    }
}
