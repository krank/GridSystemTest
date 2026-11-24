using System.Diagnostics;
using System.Text.Json;
using GridSystem;

namespace GridLevelEditor;

public class LevelEditorData(LevelEditor levelEditor)
{
  public string Filename { get; set; } = "grid.json";
  private LevelEditor _levelEditor = levelEditor;

  public void SaveToFile()
  {
    File.WriteAllText(Filename, JsonSerializer.Serialize(_levelEditor.Grid));
    Debug.Print("Saved");
  }

  public void LoadFromFile()
  {
    if (File.Exists(Filename))
    {
      string json = File.ReadAllText(Filename);
      Grid? newGrid = JsonSerializer.Deserialize<Grid>(json);
      if (newGrid != null)
      {
        _levelEditor.Gd.Grid = newGrid;
        // _graph = GGC.GridGraphConverter.ConvertGridToGraph(Grid.GetLevel(0));
        Debug.Print("Loaded");
      }
    }
  }

}
