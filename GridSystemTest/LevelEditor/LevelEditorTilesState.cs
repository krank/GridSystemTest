namespace GridSystemTest.LevelEditor;

public class LevelEditorTilesState(LevelEditor levelEditor) : LevelEditorState(levelEditor, "Tile edit")
{
  public override void Draw()
  {
    _levelEditor.DrawSelected();
    _levelEditor.DrawHovered();
  }

  public override void DrawUI() { }

  public override void HandleHoveredTile() { }

  public override void HandleKeys() { }

  public override void HandleMouseClicks(bool left, bool right)
  {
    if (left)
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
