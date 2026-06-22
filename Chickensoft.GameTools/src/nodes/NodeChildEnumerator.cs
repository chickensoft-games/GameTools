namespace Chickensoft.GameTools.Nodes;

using System.Collections;
using System.Collections.Generic;
using Godot;

// From https://github.com/godotengine/godot-proposals/issues/12404
/// <summary>
/// An enumerator supporting enumeration over all children of a Godot node that
/// satisfy a particular type.
/// </summary>
/// <typeparam name="T">The type of child to match while enumerating.</typeparam>
/// <remarks>
/// It is not safe to modify the node's child list while enumerating the
/// children with this enumerator (for instance, using
/// <see cref="Node.AddChild(Node, bool, Node.InternalMode)"/> or
/// <see cref="Node.RemoveChild(Node)"/>).
/// </remarks>
public struct NodeChildEnumerator<T> : IEnumerator<T> where T : Node
{
  private readonly int _count;
  private readonly Node _node;
  private int _index;

  /// <summary>
  /// Constructs a new enumerator over a node's children.
  /// </summary>
  /// <param name="node">The node whose children will be enumerated.</param>
  public NodeChildEnumerator(Node node)
  {
    _node = node;
    _count = _node.GetChildCount();
    _index = -1;
  }

  /// <summary>
  /// The current child to which the enumerator points.
  /// </summary>
  public T Current { get; private set; } = null!;

  readonly object IEnumerator.Current => Current;

  /// <inheritdoc/>
  public readonly void Dispose() { }

  /// <summary>
  /// Advances the enumerator to the next child node of the specified type.
  /// </summary>
  /// <returns>
  /// True if a child with the specified type was found, false otherwise
  /// (indicating the enumerator is finished).
  /// </returns>
  public bool MoveNext()
  {
    while (++_index < _count)
    {
      if (_node.GetChild(_index) is T child)
      {
        Current = child;
        return true;
      }
    }

    Current = null!;
    return false;
  }

  /// <summary>
  /// Resets the enumerator to the start of the child list.
  /// </summary>
  public void Reset()
  {
    _index = -1;
    Current = null!;
  }
}

/// <summary>
/// An enumerable type supporting enumeration over all children of a Godot node
/// that satisfy a particular type.
/// </summary>
/// <typeparam name="T">The type of child to match while enumerating.</typeparam>
public readonly struct NodeChildEnumerable<T>
  where T : Node
{
  private readonly Node _node;

  /// <summary>
  /// Constructs a new enumerable over a node's children of type
  /// <typeparamref name="T"/>.
  /// </summary>
  /// <param name="node">The node whose children will be enumerated.</param>
  public NodeChildEnumerable(Node node)
  {
    _node = node;
  }

  /// <summary>
  /// Returns an enumerator that iterates through a Godot node's children of
  /// type <typeparamref name="T"/>.
  /// </summary>
  /// <returns>
  /// An enumerator that can be used to iterate through a node's children of
  /// type <typeparamref name="T"/>.
  /// </returns>
  public NodeChildEnumerator<T> GetEnumerator() => new(_node);
}
