using Godot;

namespace Menu
{
  public partial class MainMenu : Node2D
  {
    private ConfigFile config = new();

    private Control UI;
    private Button smallButton;
    private Button mediumButton;
    private Button largeButton;
    private Button startButton;

    public override void _Ready()
    {
      config.SetValue("settings", "ui_scale", 1);

      UI = GetNode<CanvasLayer>("CanvasLayer").GetNode<Control>("UI");
      smallButton = GetNode<Button>("%SmallButton");
      mediumButton = GetNode<Button>("%MediumButton");
      largeButton = GetNode<Button>("%LargeButton");
      startButton = GetNode<Button>("%StartButton");

      smallButton.ButtonUp += () => SetUIScale(1);
      mediumButton.ButtonUp += () => SetUIScale(1.5f);
      largeButton.ButtonUp += () => SetUIScale(2f);

      startButton.ButtonUp += StartGame;
    }

    private void SetUIScale(float scale)
    {
      config.SetValue("settings", "ui_scale", scale);
      UI.Scale = new Vector2(scale, scale);
    }

    private void StartGame()
    {
      config.Save("user://settings.cfg");

      var nextLevel = ResourceLoader.Load<PackedScene>("res://scenes/game/level/Level_0_0.tscn");
      GetTree().ChangeSceneToPacked(nextLevel);
    }
  }
}