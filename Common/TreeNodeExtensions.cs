using System.Collections.Generic;
using System.Linq;

namespace Tolltech.Common
{
    public static class TreeNodeExtensions
    {
        public static IEnumerable<(TreeNode<T> Node, int Level)> Dfs<T>(this TreeNode<T> root)
        {
            return dfs(root, 0);
        }

        private static IEnumerable<(TreeNode<T> Node, int Level)> dfs<T>(TreeNode<T> node, int level)
        {
            yield return (node, level);
            var childNodes = node.Children.SelectMany(x => dfs(x, level + 1));
            foreach (var childNode in childNodes)
            {
                yield return childNode;
            }
        }
    }
}