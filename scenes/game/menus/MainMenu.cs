using Godot;
using Helpers;
using UI;

namespace Menu
{
  public partial class MainMenu : Node2D
  {
    [Signal]
    public delegate void MainMenuResumePlayEventHandler();

    private ConfigFile config = new();
    private UserSettings userSettings;
    private Control UI;
    private LevelSelector levelSelector;
    private KamiButton resumePlayButton;
    private KamiButton settingsButton;
    private KamiButton smallButton;
    private KamiButton mediumButton;
    private KamiButton largeButton;
    private int selectLevelLabelFontSize = 0;
    private int levelButtonFontSize = 0;
    private int? currentLevel = null;

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      userSettings = GetNodeOrNull<UserSettings>("/root/UserSettings");

      UI = GetNode<CanvasLayer>("CanvasLayer").GetNode<Control>("UI");
      levelSelector = GetNode<LevelSelector>("%LevelSelector");
      resumePlayButton = GetNode<KamiButton>("%ResumePlayButton");
      settingsButton = GetNode<KamiButton>("%SettingsButton");
      smallButton = GetNode<KamiButton>("%SmallButton");
      mediumButton = GetNode<KamiButton>("%MediumButton");
      largeButton = GetNode<KamiButton>("%LargeButton");

      resumePlayButton.Visible = false;

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

    public void InitFromLevel(int currentLevel)
    {
      this.currentLevel = currentLevel;

      resumePlayButton.Visible = true;
      resumePlayButton.ButtonUp += ResumePlay;

      GetNode("LevelBackground").QueueFree();

      levelSelector.InitFromLevel(currentLevel, ResumePlay);
    }

    public void ShowUI()
    {
      UI.Visible = true;
    }

    public void HideUI()
    {
      UI.Visible = false;
    }

    public void ResumePlay()
    {
      if (currentLevel != null)
      {
        EmitSignal(SignalName.MainMenuResumePlay);
      }
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