namespace Chickensoft.GameTools.Tests.Nodes;

using System.Collections;
using Chickensoft.GameTools.Nodes;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class NodeChildEnumeratorTest : TestClass
{
  private Node _child = null!;

  public NodeChildEnumeratorTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public void SetupAll()
  {
    _child = new Node();
    TestScene.AddChild(_child);
  }

  [CleanupAll]
  public void CleanupAll()
  {
    TestScene.RemoveChild(_child);
    _child.Free();
    _child = null!;
  }

  [Test]
  public void InvalidBeforeFirstAdvance()
  {
    using var enumerator = new NodeChildEnumerator<Node>(TestScene);
    enumerator.Current.ShouldBeNull();
  }

  [Test]
  public void MoveNextIsTrueWhenChildRemains()
  {
    using var enumerator = new NodeChildEnumerator<Node>(TestScene);
    enumerator.MoveNext().ShouldBeTrue();
  }

  [Test]
  public void CurrentIsCorrectAfterAdvance()
  {
    using var enumerator = new NodeChildEnumerator<Node>(TestScene);
    enumerator.MoveNext();
    enumerator.Current.ShouldBe(_child);
  }

  [Test]
  public void EnumerableCurrentIsSameAsCurrent()
  {
    using var enumerator = new NodeChildEnumerator<Node>(TestScene);
    enumerator.MoveNext();
    ((IEnumerator)enumerator).Current.ShouldBe(enumerator.Current);
  }

  [Test]
  public void MoveNextIsFalseWhenNoChildRemains()
  {
    using var enumerator = new NodeChildEnumerator<Node>(TestScene);
    enumerator.MoveNext();
    enumerator.MoveNext().ShouldBeFalse();
  }

  [Test]
  public void SkipsChildNotOfType()
  {
    using var enumerator = new NodeChildEnumerator<Node2D>(TestScene);
    enumerator.MoveNext().ShouldBeFalse();
  }

  [Test]
  public void InvalidWhenNoChildRemains()
  {
    using var enumerator = new NodeChildEnumerator<Node>(TestScene);
    enumerator.MoveNext();
    enumerator.MoveNext();
    enumerator.Current.ShouldBeNull();
  }

  [Test]
  public void Resets()
  {
    using var enumerator = new NodeChildEnumerator<Node>(TestScene);
    enumerator.MoveNext();
    enumerator.MoveNext();
    enumerator.Reset();
    enumerator.MoveNext().ShouldBeTrue();
    enumerator.Current.ShouldBe(_child);
  }
}

public class NodeChildEnumerableTest : TestClass
{
  private Node _child = default!;

  public NodeChildEnumerableTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public void SetupAll()
  {
    _child = new Node();
    TestScene.AddChild(_child);
  }

  [CleanupAll]
  public void CleanupAll()
  {
    TestScene.RemoveChild(_child);
    _child.Free();
    _child = null!;
  }

  [Test]
  public void GetsEnumeratorOverNodeChildren()
  {
    var enumerable = new NodeChildEnumerable<Node>(TestScene);
    using var enumerator = enumerable.GetEnumerator();
    enumerator.MoveNext().ShouldBeTrue();
    enumerator.Current.ShouldBe(_child);
  }
}
