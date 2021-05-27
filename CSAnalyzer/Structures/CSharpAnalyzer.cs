using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSAnalyzer.Structures
{
    public class CSharpAnalyzer : IAnaylzer
    {
        public Editor Main { get; }

        public CSharpAnalyzer(Editor editor)
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

        public async Task<AnaylzeResult> Evaluate(string cs)
        {
            cs = GetImports(cs);
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
                    return new AnaylzeResult(result.ReturnValue.ToString(), false);
                else
                    return new AnaylzeResult("No result was returned.", false);
            }
            catch (Exception ex)
            {
                return new AnaylzeResult(string.Concat(ex.GetType().ToString(), ex.Message), true);
            }
        }

        public AnaylzeResult Compile(string cs)
        {
            cs = GetImports(cs);

            var globals = new Variables(Main);

            var sopts = ScriptOptions.Default;
            sopts = sopts.WithImports(ImportsList);
            sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var script = CSharpScript.Create(cs, sopts, typeof(Variables));
            var c = script.Compile();


            var builder = new StringBuilder();
            for (int i = 0; i < c.Length; i++)
                builder.AppendLine(c[i].ToString());

            if (c.Any(l => l.Severity == DiagnosticSeverity.Error))
                return new AnaylzeResult(builder.ToString(), true);
            else
                return new AnaylzeResult(builder.ToString(), false);
        }

        private static string GetImports(string cs)
        {
            var matches = Regex.Matches(cs, @"\#include\s\<[\w\-_\:\/\.]+\>");

            for (int i = 0; i < matches.Count; i++)
            {
                var s = matches[i].Value[10..^1];

                cs = cs.Replace(matches[i].Value, File.ReadAllText(s));
            }

            return cs;
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
