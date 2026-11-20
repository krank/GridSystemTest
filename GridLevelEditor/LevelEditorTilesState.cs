using GridSystem;
using Raylib_cs;

namespace GridLevelEditor;

public class LevelEditorTilesState(LevelEditor levelEditor) : LevelEditorState(levelEditor, "Tile edit")
{
  public override void Draw()
  {
    _levelEditor.DrawSelected();
    _levelEditor.DrawHovered();
  }

  public override void DrawUI() { }

  public override void HandleHoveredTile() { }

  public override void HandleKeys()
  {
    if (Raylib.IsKeyPressed(KeyboardKey.F))
    {
      foreach (GridTile tile in _levelEditor.SelectedTiles) tile.HasFloor = !tile.HasFloor;
    }
    if (Raylib.IsKeyPressed(KeyboardKey.D))
    {
      _levelEditor.SelectedTiles.Clear();
    }
  }

  public override void HandleMouseClicks(bool left, bool right)
  {
    if (left && _levelEditor.HoveredTile != null)
    {
      if (_levelEditor.SelectedTiles.Contains(_levelEditor.HoveredTile))
      {
        _levelEditor.SelectedTiles.Remove(_levelEditor.HoveredTile);
      }
      else
      {
        _levelEditor.SelectedTiles.Add(_levelEditor.HoveredTile);
      }
    }
  }
}
