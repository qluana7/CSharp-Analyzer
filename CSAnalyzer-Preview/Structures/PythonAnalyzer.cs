using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace CSAnalyzer.Structures
{
    public class PythonAnalyzer : IAnaylzer
    {
        ScriptEngine Engine;
        ScriptScope Scope;

        public PythonAnalyzer()
        {
            Engine = Python.CreateEngine();
            Scope = Engine.CreateScope();

            var collection = Engine.GetSearchPaths();
            collection.Add(@"C:\Python27");
            collection.Add(@"C:\Python27\Lib");
            collection.Add(@"C:\Python27\Lib\site-packages");
        }

        public async Task<AnaylzeResult> Evaluate(string py)
        {
            var script = Engine.CreateScriptSourceFromString(py);

            try
            {
                if (!py.Contains("def main():"))
                    throw new EntryPointNotFoundException("Cannot found entry point.Add main() method.");

                await Task.Delay(0);
                script.Execute(Scope);
                var main = Scope.GetVariable("main");

                return new AnaylzeResult(main().ToString(), false);
            }
            catch (Exception ex)
            {
                return new AnaylzeResult(ex.ToString(), true);
            }
        }

        public AnaylzeResult Compile(string py)
        {
            PythonErrorListener listener = new PythonErrorListener();
            var script = Engine.CreateScriptSourceFromString(py);
            script.Compile(listener);

            if (!py.Contains("def main():"))
                listener.ErrorReported(script, "Cannot found entry point. Add main() method.", SourceSpan.Invalid, -1, Severity.Error);

            var c = listener.ErrorList;

            var builder = new StringBuilder();
            for (int i = 0; i < c.Count; i++)
            {
                var v = c[i];
                string s = $"({v.Span.Start}): {v.Serverity} {v.ErrorCode}: {v.Message}\n";
                builder.Append(s);
            }

            if (c.Any(l => l.Serverity == Severity.Error || l.Serverity == Severity.FatalError))
                return new AnaylzeResult(builder.ToString(), true);
            else
                return new AnaylzeResult(builder.ToString(), false);
        }
    }
}
