namespace Chickensoft.GameTools.Tests.Nodes;

using System;
using System.Collections;
using Chickensoft.GameTools.Nodes;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class NodeAncestorEnumeratorTest : TestClass
{
  public NodeAncestorEnumeratorTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public void SetupAll() { }

  [CleanupAll]
  public void CleanupAll() { }

  [Test]
  public void InvalidBeforeFirstAdvance()
  {
    using var enumerator = new NodeAncestorEnumerator<Node>(TestScene);
    Should.Throw<InvalidOperationException>(() => enumerator.Current);
  }

  [Test]
  public void MoveNextIsTrueWhenAnAncestorRemains()
  {
    using var enumerator = new NodeAncestorEnumerator<Node>(TestScene);
    enumerator.MoveNext().ShouldBeTrue(); // gets Window
  }

  [Test]
  public void CurrentIsCorrectAfterAdvance()
  {
    using var enumerator = new NodeAncestorEnumerator<Node>(TestScene);
    enumerator.MoveNext();
    enumerator.Current.ShouldBe(TestScene.GetParent());
  }

  [Test]
  public void EnumerableCurrentIsSameAsCurrent()
  {
    using var enumerator = new NodeAncestorEnumerator<Node>(TestScene);
    enumerator.MoveNext();
    ((IEnumerator)enumerator).Current.ShouldBe(enumerator.Current);
  }

  [Test]
  public void MoveNextIsFalseWhenNoAncestorRemains()
  {
    using var enumerator = new NodeAncestorEnumerator<Node>(TestScene);
    enumerator.MoveNext(); // gets Window
    enumerator.MoveNext().ShouldBeFalse();
  }

  [Test]
  public void SkipsAncestorNotOfType()
  {
    using var enumerator = new NodeAncestorEnumerator<Node2D>(TestScene);
    enumerator.MoveNext().ShouldBeFalse();
  }

  [Test]
  public void InvalidWhenNoAncestorRemains()
  {
    using var enumerator = new NodeAncestorEnumerator<Node>(TestScene);
    enumerator.MoveNext(); // gets Window
    enumerator.MoveNext();
    Should.Throw<InvalidOperationException>(() => enumerator.Current);
  }

  [Test]
  public void Resets()
  {
    using var enumerator = new NodeAncestorEnumerator<Node>(TestScene);
    enumerator.MoveNext(); // gets Window
    enumerator.MoveNext();
    enumerator.Reset();
    enumerator.MoveNext().ShouldBeTrue();
    enumerator.Current.ShouldBe(TestScene.GetParent());
  }
}

public class NodeAncestorEnumerableTest : TestClass
{
  public NodeAncestorEnumerableTest(Node testScene) : base(testScene) { }

  [SetupAll]
  public void SetupAll() { }

  [CleanupAll]
  public void CleanupAll() { }

  [Test]
  public void GetsEnumeratorOverNodeAncestors()
  {
    var enumerable = new NodeAncestorEnumerable<Node>(TestScene);
    using var enumerator = enumerable.GetEnumerator();
    enumerator.MoveNext().ShouldBeTrue();
    enumerator.Current.ShouldBe(TestScene.GetParent());
  }
}
