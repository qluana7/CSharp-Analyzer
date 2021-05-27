using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace CSAnalyzer.Structures
{
    class PythonErrorListener : ErrorListener
    {
        public IList<(ScriptSource Source, string Message, SourceSpan Span, int ErrorCode, Severity Serverity)>
            ErrorList = new List<(ScriptSource Source, string Message, SourceSpan Span, int ErrorCode, Severity Serverity)>();

        public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity)
        {
            ErrorList.Add((source, message, span, errorCode, severity));
        }
    }
}
