using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Tolltech.Common
{
    public static class TreeNodeExtensions
    {
        public static IEnumerable<(TreeNode<T> Node, int Level)> Dfs<T>(this TreeNode<T> root)
        {
            return dfs(root, 0);
        }

        [NotNull]
        public static T[] GetFastestWay<T>(this TreeNode<T> root, [NotNull] Func<T, bool> target)
        {
            var path = new LinkedList<T>();
            if (search(root, target, path))
            {
                return path.ToArray();
            }

            return Array.Empty<T>();
        }

        private static bool search<T>(TreeNode<T> root, [NotNull] Func<T, bool> target, [NotNull] LinkedList<T> path)
        {
            path.AddLast(root.Node);
            if (target(root.Node))
            {
                return true;
            }

            foreach (var child in root.Children)
            {
                if (search(child, target, path))
                {
                    return true;
                }
            }
            path.RemoveLast();

            return false;
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