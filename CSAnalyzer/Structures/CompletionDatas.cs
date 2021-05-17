using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace CSAnalyzer
{
    public class CompletionDatas
    {
        public Dictionary<string, CompletionData> CompletionDataDictionary { get; }

        private Dictionary<string, Type[]> Assemblys { get; }

        public CompletionDatas()
        {
            CompletionDataDictionary = new Dictionary<string, CompletionData>();
            Assemblys = new Dictionary<string, Type[]>();
        }

        public void GetCompletion(Assembly asm)
        {
            var types = asm.GetTypes().Where(l =>
            (l.IsEnum || l.IsClass || l.IsInterface || l.IsValueType) && l.IsPublic);

            var p = Path.GetFileNameWithoutExtension(asm.Location);

            Assemblys.Add(p, types.ToArray());

            for (int i = 0; i < Assemblys[p].Length; i++)
            {
                var t = Assemblys[p][i];
                CompletionDataDictionary.TryAdd(t.Name, new CompletionData()
                {
                    Name = t.Name,
                    Description = string.Empty,
                    Image = null
                });
            }
        }

        public void GetTypeOfType()
        {

        }

        public void GetCurrentAssemblyCompletion()
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies().Where(l => !l.IsDynamic).ToArray();
            for (int i = 0; i < asms.Length; i++)
                GetCompletion(asms[i]);
        }
    }

    public struct CompletionData
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public ImageSource Image { get; init; }
    }
}
