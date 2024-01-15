using Game;
using Godot;
using Helpers;
using Menu;

namespace UI
{
  public partial class LevelUI : CanvasLayer
  {
    [Signal]
    public delegate void IncrementZoomEventHandler();
    [Signal]
    public delegate void DecrementZoomEventHandler();


    private ConfigFile config = new();
    private Control helperControl;
    private MainMenu mainMenu;
    private Level level;
    private Label currentMovesLabel;
    private KamiLabel optimalMovesLabel;
    private KamiButton retryButton;
    private string[] loadingFrames = new string[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

    private PanelContainer scorePanel;
    private StringName panelContainerDisabledVariation = "PanelContainerDisabled";
    private MarginContainer hud;

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      level = GetParent<Level>();
      level.GameStart += OnGameStart;
      level.PlayerMoved += OnPlayerMoved;

      var backToMenuButton = GetNode<Button>("%BackToMenuButton");
      backToMenuButton.ButtonUp += OpenMainMenu;

      var retryButton = GetNode<KamiButton>("%RetryButton");
      retryButton.ButtonUp += level.Retry;

      scorePanel = GetNode<PanelContainer>("%ScorePanel");
      currentMovesLabel = GetNode<Label>("%CurrentMovesLabel");
      optimalMovesLabel = GetNode<KamiLabel>("%OptimalMovesLabel");
      hud = GetNode<MarginContainer>("%HUD");

      var levelIdLabel = GetNode<Label>("%LevelIdLabel");
      levelIdLabel.Text = level.Id.ToString();
    }

    private void Init()
    {
      optimalMovesLabel.Text = level.OptimalScore.ToString();
      currentMovesLabel.Text = "0";
    }

    private void OnGameStart()
    {
      Init();
    }

    private void DisableScore()
    {
      scorePanel.ThemeTypeVariation = panelContainerDisabledVariation;

      GetNode<TextureRect>("%TrophyIcon").Modulate = KamiColors.GetDisabled();
    }

    private void OpenMainMenu()
    {
      if (mainMenu == null)
      {
        mainMenu = ResourceLoader.Load<PackedScene>("res://scenes/game/menus/MainMenu.tscn").Instantiate<MainMenu>();
        AddChild(mainMenu);

        mainMenu.InitFromLevel(level.Id);
        mainMenu.MainMenuResumePlay += OnMainMenuResumePlay;
      }

      Hide();
      level.Hide();
      mainMenu.ShowUI();
    }

    private void OnMainMenuResumePlay()
    {
      Show();
      level.Show();
      mainMenu.HideUI();
    }

    private void OnPlayerMoved(int _score, Direction _direction)
    {
      var currentValue = currentMovesLabel.Text.ToInt();
      currentValue++;
      currentMovesLabel.Text = currentValue.ToString();

      if (currentValue > level.OptimalScore) DisableScore();
    }
  }
}