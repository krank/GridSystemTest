namespace GridSystemTest.LevelEditor;

// TODO: Make the IsoGridRendererRaylib a lot less specific
public abstract class LevelEditorState(LevelEditor levelEditor, string name)
{
  protected LevelEditor _levelEditor = levelEditor;
  public string Name { get; init; } = name;

  public abstract void DrawUI();
  public abstract void Draw();

  public abstract void HandleKeys();
  public abstract void HandleHoveredTile();

  public abstract void HandleMouseClicks(bool left, bool right);
}