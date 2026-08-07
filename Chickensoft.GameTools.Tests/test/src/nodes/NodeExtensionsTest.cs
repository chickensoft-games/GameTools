namespace Chickensoft.GameTools.Tests.Nodes;

using Chickensoft.GameTools.Nodes;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class NodeExtensionsTest : TestClass
{
  private Node _child = default!;

  public NodeExtensionsTest(Node testScene) : base(testScene) { }

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
  public void GetsEnumerableOverAncestors()
  {
    var enumerable = TestScene.GetAncestorEnumerable();
    var ancestorCount = 0;
    foreach (var ancestor in enumerable)
    {
      ancestorCount += 1;
    }
    ancestorCount.ShouldBe(1);
  }

  [Test]
  public void GetsEnumerableOverAncestorsOfType()
  {
    var enumerable = TestScene.GetAncestorEnumerable<Node2D>();
    var ancestorCount = 0;
    foreach (var ancestor in enumerable)
    {
      ancestorCount += 1;
    }
    ancestorCount.ShouldBe(0);
  }

  [Test]
  public void GetsEnumerableOverChildren()
  {
    var enumerable = TestScene.GetChildEnumerable();
    var childCount = 0;
    foreach (var ancestor in enumerable)
    {
      childCount += 1;
    }
    childCount.ShouldBe(1);
  }

  [Test]
  public void GetsEnumerableOverChildrenOfType()
  {
    var enumerable = TestScene.GetChildEnumerable<Node2D>();
    var childCount = 0;
    foreach (var ancestor in enumerable)
    {
      childCount += 1;
    }
    childCount.ShouldBe(0);
  }

  [Test]
  public void FindsAncestorOfType()
  {
    TestScene.FindAncestor<Window>(out var ancestor).ShouldBeTrue();
    ancestor.ShouldBe(TestScene.GetParent());
  }

  [Test]
  public void DoesNotFindAncestorOfType()
  {
    TestScene.FindAncestor<Node2D>(out var ancestor).ShouldBeFalse();
    ancestor.ShouldBeNull();
  }
}
