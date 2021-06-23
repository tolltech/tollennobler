using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.Ennobler.SolutionFixers
{
    public interface IFixer
    {
        string Name { get; }
        int Order { get; }
        Task FixAsync(Document document, DocumentEditor documentEditor);
    }
}
