using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Tolltech.TollEnnobler.SolutionFixers
{
    public interface IFixer
    {
        FixerGroup Group { get; }
        string Name { get; }
        int Order { get; }
        void Fix(Document document, DocumentEditor documentEditor);
    }
}