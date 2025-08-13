namespace Chickensoft.GameTools.Collections.Spatial;

using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// <para>
/// A spatial grid for efficiently partitioning dynamically moving objects in
/// a 2D area.
/// </para>
/// <para>
/// Spatial grids enable fast-lookup of objects in a given area when
/// the objects are distributed somewhat uniformly across the area that
/// the grid covers. The grid and its cells are all square.
/// </para>
/// <para>
/// If objects are positioned outside the grid area, they will be kept in the
/// nearest edge cells. Try to avoid placing objects outside the grid area to
/// avoid performance problems when searching near the edge of the grid.
/// </para>
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public sealed class SpatialGrid2D<T> where T : notnull {
  private readonly HashSet<T>[] _grid;
  private readonly Dictionary<T, int> _cellsByObject = [];
  private readonly Dictionary<T, Vector2> _positionsByObject = [];

  /// <summary>Number of cells in each dimension of the grid.</summary>
  public int Size { get; }

  /// <summary>Area that the grid covers.</summary>
  public Rect2 Area { get; }

  /// <summary>
  /// Size of cells in the spatial grid. Grids cell sizes are uniform in both
  /// dimensions, so this is the size of a square cell.
  /// </summary>
  public float CellSize { get; }

  /// <summary>
  /// Creates a new spatial grid to partition dynamically moving objects
  /// in a 2D area.
  /// </summary>
  /// <param name="size">Size of the grid.</param>
  /// <param name="area">Area covered by the grid. Must be square.</param>
  public SpatialGrid2D(int size, Rect2 area) {
    if (area.Size.Aspect() != 1) {
      throw new ArgumentException(
        "SpatialGrid area must be square (aspect ratio 1:1).",
        nameof(area)
      );
    }

    Size = size;
    Area = area;

    CellSize = Mathf.Round(area.Size.X / size);

    _grid = new HashSet<T>[size * size];

    for (var c = 0; c < _grid.Length; c++) {
      _grid[c] = [];
    }
  }

  /// <summary>
  /// Find the closest object at the given position within the specified
  /// distance. This searches nearby cells that fall within distance from
  /// the given position.
  /// </summary>
  /// <param name="pos">Position.</param>
  /// <param name="distance">Distance from the position.</param>
  /// <returns>Closest object, if any.</returns>
  public T? FindNearest(Vector2 pos, float distance) {
    distance = Mathf.Abs(distance);

    // starting cell
    var cellX = ComputeCellX(pos);
    var cellY = ComputeCellY(pos);

    // number of cells involved in the search
    var dC = (int)Math.Ceiling(distance / CellSize);

    // grid search area indices
    var minX = Math.Max(0, cellX - dC);
    var maxX = Math.Min(Size - 1, cellX + dC);
    var minY = Math.Max(0, cellY - dC);
    var maxY = Math.Min(Size - 1, cellY + dC);

    T? best = default;
    var bestDistSq = float.MaxValue;
    var distSq = distance * distance;
    var maxDsq = distSq;

    for (var y = minY; y <= maxY; y++) {
      for (var x = minX; x <= maxX; x++) {
        var idx = ComputeIndex(x, y);
        foreach (var obj in _grid[idx]) {
          var objPos = _positionsByObject[obj];
          var delta = objPos - pos;

          var dsq = delta.LengthSquared();

          if (dsq > maxDsq) {
            // must fall within circular distance
            continue;
          }

          if (dsq < bestDistSq) {
            bestDistSq = dsq;
            best = obj;
          }
        }
      }
    }

    return best;
  }

  /// <summary>
  /// <para>
  /// Finds all objects within the specified distance from the given
  /// position. This searches nearby cells that fall within the specified
  /// distance of the position.
  /// </para>
  /// <para>
  /// You must provide a collection that implements
  /// <see cref="IList{T}"/> to store the results. The collection
  /// will be cleared before the search begins. C# lists resize themselves as
  /// needed, so you can minimize search allocations by just reusing the same
  /// list each time.
  /// </para>
  /// </summary>
  /// <param name="pos">Position.</param>
  /// <param name="distance">Distance from the position.</param>
  /// <param name="nearbyObjects">A collection which implements
  /// <see cref="IList{T}"/>. Objects within the given search
  /// <paramref name="distance"/> of <paramref name="pos"/> will be added to
  /// the list. The list is cleared before the search begins.</param>
  public void FindWithin(Vector2 pos, float distance, IList<T> nearbyObjects) {
    nearbyObjects.Clear();
    distance = Mathf.Abs(distance);

    // starting cell
    var cellX = ComputeCellX(pos);
    var cellY = ComputeCellY(pos);

    // number of cells involved in the search
    var dC = (int)Math.Ceiling(distance / CellSize);

    // grid search area indices
    var minX = Math.Max(0, cellX - dC);
    var maxX = Math.Min(Size - 1, cellX + dC);
    var minY = Math.Max(0, cellY - dC);
    var maxY = Math.Min(Size - 1, cellY + dC);

    var distSq = distance * distance;

    for (var y = minY; y <= maxY; y++) {
      for (var x = minX; x <= maxX; x++) {
        var idx = ComputeIndex(x, y);
        foreach (var obj in _grid[idx]) {
          var objPos = _positionsByObject[obj];
          var delta = objPos - pos;

          var dsq = delta.LengthSquared();

          if (dsq < distSq) {
            nearbyObjects.Add(obj);
          }
        }
      }
    }
  }

  /// <summary>
  /// Add an object to the spatial grid at the specified position. If the object
  /// is already in the grid, it will be moved to the new position.
  /// </summary>
  /// <param name="obj">Object to add.</param>
  /// <param name="pos">Position of the object.</param>
  /// <returns>True if the object was added, false if it is already in the grid.
  /// </returns>
  public bool Add(T obj, Vector2 pos) {
    if (Contains(obj)) {
      Move(obj, pos);
      return false;
    }

    var index = ComputeIndexFromPosition(pos);

    _grid[index].Add(obj);
    _cellsByObject[obj] = index;
    _positionsByObject[obj] = pos;

    return true;
  }

  /// <summary>
  /// Removes an object from the spatial grid. If the object is not in the grid,
  /// this will return false.
  /// </summary>
  /// <param name="obj">Object to remove.</param>
  /// <returns>True if the object was in the grid and removed, false otherwise.
  /// </returns>
  public bool Remove(T obj) {
    if (!Contains(obj)) {
      return false;
    }

    _grid[_cellsByObject[obj]].Remove(obj);
    _cellsByObject.Remove(obj);
    _positionsByObject.Remove(obj);

    return true;
  }

  /// <summary>
  /// Checks if the spatial grid contains the specified object.
  /// </summary>
  /// <param name="obj">Object to check.</param>
  /// <returns>True if the object is in the grid, false otherwise.</returns>
  public bool Contains(T obj) => _cellsByObject.ContainsKey(obj);

  /// <summary>
  /// Updates the position of an object in the spatial grid, ensuring it ends
  /// up in the correct grid cell based on its new position.
  /// </summary>
  /// <param name="obj">Object to move.</param>
  /// <param name="pos">Object's new position.</param>
  public void Move(T obj, Vector2 pos) {
    if (!_cellsByObject.ContainsKey(obj)) {
      Add(obj, pos);
    }

    _positionsByObject[obj] = pos;

    var oldIndex = _cellsByObject[obj];
    var index = ComputeIndexFromPosition(pos);

    if (oldIndex == index) {
      return;
    }

    _grid[oldIndex].Remove(obj);
    _grid[index].Add(obj);
    _cellsByObject[obj] = index;
  }

  /// <summary>
  /// Completely clears and resets the spatial grid.
  /// </summary>
  public void Clear() {
    for (var i = 0; i < _grid.Length; i++) {
      _grid[i].Clear();
    }

    _cellsByObject.Clear();
    _positionsByObject.Clear();
  }

  internal int ComputeIndexFromPosition(Vector2 pos) => ComputeIndex(
    ComputeCellX(pos),
    ComputeCellY(pos)
  );

  // items outside grid will bunch up in the edge cells
  internal int ComputeCellX(Vector2 pos) =>
    Math.Clamp((int)((pos.X - Area.Position.X) / CellSize), 0, Size - 1);

  internal int ComputeCellY(Vector2 pos) =>
    Math.Clamp((int)((pos.Y - Area.Position.Y) / CellSize), 0, Size - 1);

  internal int ComputeIndex(int x, int y) => x + (y * Size);
}
