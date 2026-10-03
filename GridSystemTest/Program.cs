using System.Text.Json;
using GridSystem;
using Raylib_cs;
using RaylibExt;
using WeightedGraph;

/* TODO
   - Saving, loading grids to json
   - Visual level editing (enable/disable tiles, add/remove walls)
   - GridRenderer: draw arrows and paths
*/

Raylib.InitWindow(800, 600, "Title");
Raylib.SetTargetFPS(60);

Grid grid = new(10, 7);

grid.Get(1, 1).Blocking[(int)Direction.North] = true;
grid.Get(1, 1).Blocking[(int)Direction.East] = true;

grid.Get(3, 3).Blocking[(int)Direction.West] = true;
grid.Get(3, 3).Blocking[(int)Direction.East] = true;

// File.WriteAllText("grid.json", JsonSerializer.Serialize(grid));
grid = JsonSerializer.Deserialize<Grid>(File.ReadAllText("grid.json"));

// Console.ReadLine();
// return;

WeightedGraph<GridTile> graph = GridGraphConverter.ConvertGridToGraph(grid);

List<WeightedEdge<GridTile>> edges = graph.GetAllEdges();

GridRendererRaylib gd = new(grid) { GridTileSize = 20 };
gd.FitToScreen();
gd.CenterOnScreen();

GridRendererRaylib.GridTheme markedTheme = new()
{
  Background = Color.Blank,
  Wall = Color.Blank,
  Tile = new(0, 0, 255, 16)
};

(int, int)[] markedCoords = [(1, 1), (2, 2), (2, 3)];

while (!Raylib.WindowShouldClose())
{
  Raylib.BeginDrawing();
  Raylib.ClearBackground(Color.White);

  gd.DrawGrid();

  foreach (WeightedEdge<GridTile> edge in edges)
  {
    (int x, int y)[] coords = [
      (edge.From.Value.X, edge.From.Value.Y),
      (edge.To.Value.X, edge.To.Value.Y),
      ];

    gd.DrawLine(coords, Color.Blue, 3, (from, to, thickness, color) =>
    {
      Lines.DrawArrow([from, to], thickness, color, legLength: 5);
      Lines.DrawLineLabel(from, to, Color.Black, edge.Weight + "", Raylib.GetFontDefault().BaseSize);
    }, 15);
  }

  Raylib.EndDrawing();
}