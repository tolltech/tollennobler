using System.Collections.Generic;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledClassModel
    {
        public CompiledClassModel()
        {
            Methods = new List<CompiledMethod>();
        }

        public string Name { get; set; }
        public List<CompiledMethod> Methods { get; set; }
    }
}