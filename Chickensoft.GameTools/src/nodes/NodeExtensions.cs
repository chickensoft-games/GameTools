namespace Chickensoft.GameTools.Nodes;

using System.Diagnostics.CodeAnalysis;
using Godot;

/// <summary>
/// Contains extension methods for <see cref="Node"/>.
/// </summary>
public static class NodeExtensions
{
  /// <summary>
  /// Obtain an allocation-free enumerable over a node's children.
  /// </summary>
  /// <param name="node">The node whose children should be enumerated.</param>
  /// <returns>The enumerable for <paramref name="node"/>'s children.</returns>
  /// <remarks>
  /// It is not safe to modify <paramref name="node"/>'s child list while using
  /// the enumerable.
  /// </remarks>
  public static NodeChildEnumerable<Node> GetChildEnumerable(this Node node) =>
    new(node);

  /// <summary>
  /// Obtain an allocation-free enumerable over a node's children of type
  /// <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">
  /// The type of child to be included in the enumeration.
  /// </typeparam>
  /// <param name="node">The node whose children should be enumerated.</param>
  /// <returns>The enumerable for <paramref name="node"/>'s children.</returns>
  /// <remarks>
  /// It is not safe to modify <paramref name="node"/>'s child list while using
  /// the enumerable.
  /// </remarks>
  public static NodeChildEnumerable<T> GetChildEnumerable<T>(this Node node)
    where T : Node =>
      new(node);

  /// <summary>
  /// Obtain an allocation-free enumerable over a node's ancestors.
  /// </summary>
  /// <param name="node">The node whose ancestors should be enumerated.</param>
  /// <returns>The enumerable for <paramref name="node"/>'s ancestors.</returns>
  /// <remarks>
  /// It is not safe to modify <paramref name="node"/>'s ancestor hierarchy
  /// while using the enumerable.
  /// </remarks>
  public static NodeAncestorEnumerable<Node> GetAncestorEnumerable(
    this Node node
  ) =>
    new(node);

  /// <summary>
  /// Obtain an allocation-free enumerable over a node's ancestors of type
  /// <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">
  /// The type of ancestor to be included in the enumeration.
  /// </typeparam>
  /// <param name="node">The node whose ancestors should be enumerated.</param>
  /// <returns>The enumerable for <paramref name="node"/>'s ancestors.</returns>
  /// <remarks>
  /// It is not safe to modify <paramref name="node"/>'s ancestor hierarchy
  /// while using the enumerable.
  /// </remarks>
  public static NodeAncestorEnumerable<T> GetAncestorEnumerable<T>(
    this Node node
  ) where T : Node =>
    new(node);

  /// <summary>
  /// Obtain the nearest ancestor of type <typeparamref name="T"/> in a node's
  /// ancestor hierarchy, if any.
  /// </summary>
  /// <typeparam name="T">
  /// The type to check in the ancestors of <paramref name="node"/>.
  /// </typeparam>
  /// <param name="node">The node whose ancestors should be checked.</param>
  /// <param name="ancestor">
  /// The nearest ancestor of <paramref name="node"/> whose type satisfies
  /// <typeparamref name="T"/>, if one exists; else null.
  /// </param>
  /// <returns>
  /// True if any ancestor of <paramref name="node"/> is of type
  /// <typeparamref name="T"/>; else false.
  /// </returns>
  public static bool FindAncestor<T>(
    this Node node,
    [NotNullWhen(true)] out T? ancestor
  ) where T : Node
  {
    ancestor = null;
    foreach (var a in node.GetAncestorEnumerable<T>())
    {
      ancestor = a;
      return true;
    }
    return false;
  }
}
