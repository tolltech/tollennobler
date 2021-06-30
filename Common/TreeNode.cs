using System.Collections.Generic;
using System.Linq;

namespace Tolltech.Common
{
    public class TreeNode<TNode>
    {
        public TreeNode()
        {
            Children = new TreeNode<TNode>[0];
        }

        public TNode Node { get; set; }

        public TreeNode<TNode>[] Children { get; set; }

        public TreeNode<TNode>[] Flatten()
        {
            return InnerFlatten().ToArray();
        }

        private IEnumerable<TreeNode<TNode>> InnerFlatten()
        {
            yield return this;
            foreach (var child in Children.SelectMany(x => x.InnerFlatten()))
            {
                yield return child;
            }
        }

        public override string ToString()
        {
            return $"TreeNode of {typeof(TNode).Name} {Node}";
        }
    }
}