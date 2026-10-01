using System.Numerics;
using GridSystem;
using Raylib_cs;
using RaylibExt;
using WeightedGraph;


// WeightedGraph<string> graph = new();
// graph.Vertices.Add(new Vertex<string>("1"));
// graph.Vertices.Add(new Vertex<string>("2"));
// graph.Vertices.Add(new Vertex<string>("3"));
// graph.Vertices.Add(new Vertex<string>("4"));
// graph.Vertices.Add(new Vertex<string>("5"));
// graph.Vertices.Add(new Vertex<string>("6"));

// graph.AddEdge(graph.Vertices[0], graph.Vertices[1], 1);
// graph.AddEdge(graph.Vertices[1], graph.Vertices[2], 1);
// graph.AddEdge(graph.Vertices[2], graph.Vertices[3], 1);
// graph.AddEdge(graph.Vertices[3], graph.Vertices[4], 1);
// graph.AddEdge(graph.Vertices[4], graph.Vertices[5], 1);
// graph.AddEdge(graph.Vertices[5], graph.Vertices[1], 1);

// graph.AddEdge(graph.Vertices[2], graph.Vertices[5], 1);


// Console.WriteLine(graph.GetAllEdges().Count);
// Console.ReadLine();
// return;
Raylib.InitWindow(800, 600, "Title");
Raylib.SetTargetFPS(60);

Grid grid = new(5, 5);
grid.GetAllCoords();

grid.Get(1, 1).Blocking[(int)Direction.North] = true;
grid.Get(1, 1).Blocking[(int)Direction.East] = true;

grid.Get(3, 3).Blocking[(int)Direction.West] = true;
grid.Get(3, 3).Blocking[(int)Direction.East] = true;

WeightedGraph<GridTile> graph = grid.GetGraph();
List<WeightedEdge<GridTile>> edges = graph.GetAllEdges();

GridRendererRaylib gd = new(grid) { GridTileSize = 100 };
gd.GridOffset = Raylib.GetScreenCenter() - gd.GetPxSize() / 2;

GridRendererRaylib.GridTheme markedTheme = new()
{
  Background = Color.Blank,
  Wall = Color.Blank,
  Tile = new(0, 0, 255, 16)
};

(int, int)[] markedCoords = [(1, 1), (2, 2), (2, 3)];
Vector2[] markedCoordsPx = gd.GetTileCenterCoords(markedCoords);

Vector2[] allCenters = gd.GetTileCenterCoords();

while (!Raylib.WindowShouldClose())
{
  // GridTile? hoveredTile = gd.GetTileFromScreenPoint(Raylib.GetMousePosition());

  Raylib.BeginDrawing();
  Raylib.ClearBackground(Color.White);

  Vector2 center = Raylib.GetScreenCenter();
  Vector2 mPos = Raylib.GetMousePosition();

  gd.DrawGrid();

  foreach (WeightedEdge<GridTile> edge in edges)
  {

    Vector2[] coords = gd.GetTileCenterCoords([
      (edge.From.Value.X, edge.From.Value.Y),
      (edge.To.Value.X, edge.To.Value.Y)
    ]);

    Vector2 direction = Vector2.Normalize(coords[1] - coords[0]);

    Lines.DrawArrow(
      [coords[0] + direction * 16f,
      coords[1] + -direction * 16f],
      3, Color.Blue, legLength: 10
    );

    Lines.DrawLineLabeled(
      coords[0] + direction * 16f,
      coords[1] + -direction * 16f,
      0, Color.Blue, "" + edge.Weight, Raylib.GetFontDefault().BaseSize
    );
  }


  // if (hoveredTile != null)
  // {
  //   gd.DrawTile(hoveredTile, $"{hoveredTile.X}:{hoveredTile.Y}");
  // }

  Raylib.EndDrawing();
}