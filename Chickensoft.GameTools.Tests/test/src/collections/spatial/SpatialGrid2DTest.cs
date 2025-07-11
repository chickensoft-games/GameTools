namespace Chickensoft.GameTools.Tests.Collections.Spatial;

using System;
using System.Collections.Generic;
using Chickensoft.GameTools.Collections.Spatial;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public class SpatialGrid2DTest(Node testScene) : TestClass(testScene) {
  private readonly Rect2 _area = new(-1, -1, 2, 2);
  private readonly int _size = 2;

  [Test]
  public void Initializes() {
    var grid = new SpatialGrid2D<int>(_size, _area);

    grid.Size.ShouldBe(_size);
    grid.Area.ShouldBe(_area);
    grid.CellSize.ShouldBe(_area.Size.X / _size);
  }

  [Test]
  public void FailsToInitializeIfAreaIsNotSquare() {
    var area = new Rect2(-1, -1, 2, 3);
    Should.Throw<ArgumentException>(() => new SpatialGrid2D<int>(_size, area));
  }

  [Test]
  public void AddContainsRemove() {
    var grid = new SpatialGrid2D<string>(_size, _area);
    grid.Contains("a").ShouldBeFalse();
    grid.Add("a", new Vector2(0, 0)).ShouldBeTrue();
    grid.Contains("a").ShouldBeTrue();
    // Adding again moves the object and returns false
    grid.Add("a", new Vector2(0, 0)).ShouldBeFalse();
    grid.Remove("a").ShouldBeTrue();
    grid.Contains("a").ShouldBeFalse();
    // Removing non-existent returns false
    grid.Remove("a").ShouldBeFalse();
  }

  [Test]
  public void MoveUpdatesPosition() {
    var grid = new SpatialGrid2D<string>(_size, _area);
    var pos1 = new Vector2(-0.5f, -0.5f);
    var pos2 = new Vector2(0.5f, 0.5f);
    grid.Add("obj", pos1).ShouldBeTrue();
    // Should find at original spot
    grid.FindNearest(pos1, 0.1f).ShouldBe("obj");
    grid.Move("obj", pos2);
    // Old area no longer contains it within small radius
    grid.FindNearest(pos1, 0.1f).ShouldBeNull();
    // New area contains it
    grid.FindNearest(pos2, 0.1f).ShouldBe("obj");
  }

  [Test]
  public void MoveAddsNewObjectIfNotAlreadyInGrid() {
    var grid = new SpatialGrid2D<string>(_size, _area);
    var pos = new Vector2(0, 0);
    // Moving an object not in the grid should add it
    grid.Move("newObj", pos);
    grid.Contains("newObj").ShouldBeTrue();
    // Should find it at the position
    grid.FindNearest(pos, 0.1f).ShouldBe("newObj");
  }

  [Test]
  public void FindNearestAndWithin() {
    var grid = new SpatialGrid2D<int>(_size, _area);
    var posA = new Vector2(-0.5f, -0.5f);
    var posB = new Vector2(0.5f, 0.5f);
    grid.Add(1, posA);
    grid.Add(2, posB);

    // Nearest to a point near A
    grid.FindNearest(new Vector2(-0.4f, -0.4f), 1f).ShouldBe(1);
    // Nearest to a point near B
    grid.FindNearest(new Vector2(0.4f, 0.4f), 1f).ShouldBe(2);
    // No object within very small radius
    grid.FindNearest(new Vector2(0, 0), 0.1f).ShouldBe(0);

    // FindWithin should include both within distance 1
    var list = new List<int>();
    grid.FindWithin(new Vector2(0, 0), 1f, list);
    list.ShouldContain(1);
    list.ShouldContain(2);
    list.Count.ShouldBe(2);

    // Smaller radius yields empty list
    list.Clear();
    grid.FindWithin(new Vector2(0, 0), 0.1f, list);
    list.Count.ShouldBe(0);
  }

  [Test]
  public void ClearEmptiesGrid() {
    var grid = new SpatialGrid2D<int>(_size, _area);
    grid.Add(1, new Vector2(0, 0));
    grid.Add(2, new Vector2(0, 0));
    grid.Clear();
    // After clear, no objects remain
    grid.Contains(1).ShouldBeFalse();
    grid.Contains(2).ShouldBeFalse();
    grid.FindNearest(new Vector2(0, 0), 1f).ShouldBe(0);
    var list = new List<int> { 1, 2 };
    grid.FindWithin(new Vector2(0, 0), 1f, list);
    list.Count.ShouldBe(0);
  }

  [Test]
  public void HandlesPositionsOutsideArea() {
    var grid = new SpatialGrid2D<int>(_size, _area);
    // Position far outside the area should be clamped to edge cells
    var outsidePos = new Vector2(10, 10);
    grid.Add(42, outsidePos).ShouldBeTrue();
    // Searching near the far corner should find it
    grid.FindNearest(new Vector2(1, 1), 15).ShouldBe(42);

    // Verify it's in the correct edge cell
    var cell = new Vector2(
      grid.ComputeCellX(outsidePos),
      grid.ComputeCellY(outsidePos)
    );

    cell.ShouldBe(new Vector2(_size - 1, _size - 1));
  }
}
