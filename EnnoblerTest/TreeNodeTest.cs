using FluentAssertions;
using NUnit.Framework;
using Tolltech.Common;

namespace Tolltech.EnnoblerTest
{
    public class TreeNodeTest : TestBase
    {
        private static readonly TreeNode<int> tree = new TreeNode<int>
        {
            Node = 5,
            Children = new[]
            {
                new TreeNode<int>
                {
                    Node = 3
                },
                new TreeNode<int>
                {
                    Node = 1,
                    Children = new[]
                    {
                        new TreeNode<int>
                        {
                            Node = 4
                        },
                        new TreeNode<int>
                        {
                            Node = 8,
                            Children = new[]
                            {
                                new TreeNode<int>
                                {
                                    Node = 9
                                }
                            }
                        },
                    }
                },
                new TreeNode<int>
                {
                    Node = 7,
                    Children = new[]
                    {
                        new TreeNode<int>
                        {
                            Node = 2,
                            Children = new[]
                            {
                                new TreeNode<int>
                                {
                                    Node = 6
                                }
                            }
                        },
                    }
                },
            }
        };


        [Test]
        [TestCase(9, new[] {5, 1, 8, 9})]
        [TestCase(6, new[] {5, 7, 2, 6})]
        [TestCase(5, new[] {5})]
        [TestCase(1, new[] {5, 1})]
        [TestCase(7, new[] {5, 7})]
        public void TestFastestWay(int target, int[] expected)
        {
            tree.GetFastestWay(x => x == target).Should().BeEquivalentTo(expected);
        }
    }
}