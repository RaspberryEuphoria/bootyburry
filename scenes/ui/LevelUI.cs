
using System.Linq;
using Game;
using Godot;
using Helpers;

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
    private Level level;
    private Label currentMovesLabel;
    private KamiLabel optimalMovesLabel;
    private KamiButton retryButton;
    private string[] loadingFrames = new string[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

    private PanelContainer scorePanel;
    private StringName panelContainerDisabledVariation = "PanelContainerDisabled";
    private Label currentZoomLabel;
    private DynamicCamera dynamicCamera;

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      level = GetParent<Level>();
      level.GameStart += OnGameStart;
      level.PlayerMoved += OnPlayerMoved;

      var backToMenuButton = GetNode<Button>("%BackToMenuButton");
      backToMenuButton.ButtonUp += BackToMainMenu;

      var retryButton = GetNode<KamiButton>("%RetryButton");
      retryButton.ButtonUp += level.Retry;

      scorePanel = GetNode<PanelContainer>("%ScorePanel");
      currentMovesLabel = GetNode<Label>("%CurrentMovesLabel");
      optimalMovesLabel = GetNode<KamiLabel>("%OptimalMovesLabel");

      currentZoomLabel = GetNode<Label>("%CurrentZoomLabel");

      var increaseZoomButton = GetNode<Button>("%IncreaseZoomButton");
      var decreaseZoomButton = GetNode<Button>("%DecreaseZoomButton");

      increaseZoomButton.ButtonUp += OnIncreaseZoom;
      decreaseZoomButton.ButtonUp += OnDecreaseZoom;
    }

    public override void _Process(double delta)
    {
      if (!level.IsInputAllowed()) return;

      if (Input.IsActionJustPressed("scroll_down"))
      {
        OnDecreaseZoom();
      }
      else if (Input.IsActionJustPressed("scroll_up"))
      {
        OnIncreaseZoom();
      }
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
      optimalMovesLabel.SetColorType(KamiColors.ColorType.Disabled);
      scorePanel.ThemeTypeVariation = panelContainerDisabledVariation;
    }

    private void BackToMainMenu()
    {
      var mainMenu = ResourceLoader.Load<PackedScene>("res://scenes/game/menus/MainMenu.tscn");
      GetTree().ChangeSceneToPacked(mainMenu);
    }

    private void UpdateZoomLabel()
    {
      dynamicCamera ??= level.GetNode<DynamicCamera>("DynamicCamera");

      var zoomPercentage = dynamicCamera.TargetZoom.X * 100;
      currentZoomLabel.Text = $"{zoomPercentage}%";
    }

    private void OnIncreaseZoom()
    {
      EmitSignal(SignalName.IncrementZoom);
      UpdateZoomLabel();
    }

    private void OnDecreaseZoom()
    {
      EmitSignal(SignalName.DecrementZoom);
      UpdateZoomLabel();
    }

    private void OnPlayerMoved(int _score)
    {
      var currentValue = currentMovesLabel.Text.ToInt();
      currentValue++;
      currentMovesLabel.Text = currentValue.ToString();

      if (currentValue > level.OptimalScore) DisableScore();
    }
  }
}