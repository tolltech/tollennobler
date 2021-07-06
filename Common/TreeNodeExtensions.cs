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

        public static IEnumerable<TreeNode<T>> RightWays<T>(this TreeNode<T> root)
        {
            var current = root;

            while (true)
            {
                yield return current;
                current = current.Children.Length > 0 ? current.Children.Last() : null;

                if (current == null)
                {
                    break;
                }
            }
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