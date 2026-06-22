namespace Chickensoft.GameTools.Nodes;

using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

/// <summary>
/// An enumerator supporting enumeration over all ancestors of a Godot node that
/// satisfy a particular type.
/// </summary>
/// <typeparam name="T">
/// The type of ancestor to match while enumerating.
/// </typeparam>
public struct NodeAncestorEnumerator<T> : IEnumerator<T> where T : Node
{
  private readonly Node _node;
  private Node? _ancestor;
  private T? _current;

  /// <summary>
  /// Constructs a new enumerator over a node's ancestors.
  /// </summary>
  /// <param name="node">The node whose ancestors will be enumerated.</param>
  public NodeAncestorEnumerator(Node node)
  {
    _node = node;
    _ancestor = node;
    _current = null;
  }

  /// <summary>
  /// The current ancestor to which the enumerator points.
  /// </summary>
  public T Current
  {
    get
    {
      if (_ancestor == _node)
      {
        throw new InvalidOperationException("Not ready");
      }
      if (_current is null)
      {
        throw new InvalidOperationException("Past last ancestor");
      }
      return _current;
    }
  }

  object IEnumerator.Current => Current;

  /// <inheritdoc/>
  public void Dispose() { }

  /// <summary>
  /// Advances the enumerator to the next ancestor node of the specified type.
  /// </summary>
  /// <returns>
  /// True if an ancestor with the specified type was found, false otherwise
  /// (indicating the enumerator is finished).
  /// </returns>
  public bool MoveNext()
  {
    while (_ancestor is not null)
    {
      _ancestor = _ancestor.GetParent();
      if (_ancestor is T current)
      {
        _current = current;
        break;
      }
    }
    if (_ancestor is null)
    {
      _current = null;
    }
    return _ancestor is not null;
  }

  /// <summary>
  /// Resets the enumerator to the start of the node's ancestors.
  /// </summary>
  public void Reset()
  {
    _ancestor = _node;
    _current = null;
  }
}

/// <summary>
/// An enumerable type supporting enumeration over all ancestors of a Godot node
/// that satisfy a particular type.
/// </summary>
/// <typeparam name="T">
/// The type of ancestor to match while enumerating.
/// </typeparam>
public readonly struct NodeAncestorEnumerable<T>
  where T : Node
{
  private readonly Node _node;

  /// <summary>
  /// Constructs a new enumerable over a node's ancestors of type
  /// <typeparamref name="T"/>.
  /// </summary>
  /// <param name="node">The node whose ancestors will be enumerated.</param>
  public NodeAncestorEnumerable(Node node)
  {
    _node = node;
  }

  /// <summary>
  /// Returns an enumerator that iterates through a Godot node's ancestors of
  /// type <typeparamref name="T"/>.
  /// </summary>
  /// <returns>
  /// An enumerator that can be used to iterate through a node's ancestors of
  /// type <typeparamref name="T"/>.
  /// </returns>
  public readonly NodeAncestorEnumerator<T> GetEnumerator() => new(_node);
}
