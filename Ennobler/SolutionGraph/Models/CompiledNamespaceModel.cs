using System.Collections.Generic;

namespace Tolltech.Ennobler.SolutionGraph.Models
{
    public class CompiledNamespaceModel
    {
        public string Name { get; set; }

        public Dictionary<string, CompiledClassModel> Classes { get; set; }
    }
}