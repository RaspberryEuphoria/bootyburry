using Godot;
using Helpers;
using UI;

namespace Menu
{
  public partial class MainMenu : Node2D
  {
    private ConfigFile config = new();

    private UserSettings userSettings;
    private Control UI;
    private KamiButton smallButton;
    private KamiButton mediumButton;
    private KamiButton largeButton;
    private int selectLevelLabelFontSize = 0;
    private int levelButtonFontSize = 0;

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      userSettings = GetNodeOrNull<UserSettings>("/root/UserSettings");

      UI = GetNode<CanvasLayer>("CanvasLayer").GetNode<Control>("UI");
      smallButton = GetNode<KamiButton>("%SmallButton");
      mediumButton = GetNode<KamiButton>("%MediumButton");
      largeButton = GetNode<KamiButton>("%LargeButton");

      smallButton.ButtonUp += () => SetUIScale(0.75f, smallButton);
      mediumButton.ButtonUp += () => SetUIScale(1f, mediumButton);
      largeButton.ButtonUp += () => SetUIScale(1.25f, largeButton);

      var scale = userSettings.UIScale;
      SetUIScale(scale, scale switch
      {
        0.75f => smallButton,
        1f => mediumButton,
        1.25f => largeButton,
        _ => smallButton
      });
    }

    private void SetUIScale(float scale, KamiButton button)
    {
      userSettings?.SetUIScale(scale);

      smallButton.SetColorType(KamiColors.ColorType.Dark);
      mediumButton.SetColorType(KamiColors.ColorType.Dark);
      largeButton.SetColorType(KamiColors.ColorType.Dark);

      button.SetColorType(KamiColors.ColorType.Primary);
    }
  }
}