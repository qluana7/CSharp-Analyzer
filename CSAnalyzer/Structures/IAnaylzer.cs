using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAnalyzer.Structures
{
    public interface IAnaylzer
    {
        public Task<AnaylzeResult> Evaluate(string code);
        public AnaylzeResult Compile(string code);
    }

    public struct AnaylzeResult
    {
        public AnaylzeResult(string result, bool exception)
        {
            Result = result;
            Exception = exception;
        }

        public string Result { get; }
        public bool Exception { get; }
    }
}
