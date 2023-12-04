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
    private int selectLevelLabelFontSize = 0;
    private int levelButtonFontSize = 0;

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      UI = GetNode<CanvasLayer>("CanvasLayer").GetNode<Control>("UI");
      smallButton = GetNode<Button>("%SmallButton");
      mediumButton = GetNode<Button>("%MediumButton");
      largeButton = GetNode<Button>("%LargeButton");

      smallButton.ButtonUp += () => SetUIScale(1, smallButton);
      mediumButton.ButtonUp += () => SetUIScale(1.5f, mediumButton);
      largeButton.ButtonUp += () => SetUIScale(2f, largeButton);

      var scale = (float)config.GetValue("settings", "ui_scale");
      SetUIScale(scale, scale switch
      {
        1 => smallButton,
        1.5f => mediumButton,
        2f => largeButton,
        _ => smallButton
      });
    }

    private void SetUIScale(float scale, Button button)
    {
      config.SetValue("settings", "ui_scale", scale);
      config.Save("user://settings.cfg");

      smallButton.Modulate = Colors.White;
      mediumButton.Modulate = Colors.White;
      largeButton.Modulate = Colors.White;

      button.Modulate = new Color(0.565f, 0.984f, 0.424f);

      var selectLevelLabel = GetNode<Label>("%LevelSelectorLabel");
      if (selectLevelLabelFontSize == 0)
      {
        selectLevelLabelFontSize = (int)selectLevelLabel.Get("theme_override_font_sizes/font_size");
      }
      selectLevelLabel.Set("theme_override_font_sizes/font_size", selectLevelLabelFontSize * scale);

      var levelSelectorWorlds = GetNode("%LevelSelector").GetChildren();

      foreach (Node world in levelSelectorWorlds)
      {
        var levelButtons = world.GetChildren();

        foreach (Node levelButton in levelButtons)
        {
          if (levelButtonFontSize == 0) levelButtonFontSize = (int)levelButton.Get("theme_override_font_sizes/font_size");
          levelButton.Set("theme_override_font_sizes/font_size", levelButtonFontSize * scale);
        }
      }
    }
  }
}