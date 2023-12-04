
using System.Linq;
using Game;
using Godot;

namespace UI
{
  public partial class LevelUI : CanvasLayer
  {
    private ConfigFile config = new();
    private Control helperControl;
    private Level level;
    private Label levelTitleLabel;
    private Label loadingLabel;
    private BoxContainer loadingOverContainer;
    private Label activeCoresLabel;
    private Label remainingCoresLabel;
    private BoxContainer computationsContainer;
    private Label actualComputationsLabel;
    private Label optimalComputationsLabel;
    private string[] loadingFrames = new string[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      level = GetParent<Level>();
      level.GameStart += OnGameStart;
      level.GameWon += OnGameWon;
      level.PlayerMoved += OnPlayerMoved;
      level.CurrentTileUpdated += OnCurrentTileUpdated;

      levelTitleLabel = GetNode<Label>("%LevelTitleLabel");
      loadingLabel = GetNode<Label>("%LoadingLabel");
      loadingOverContainer = GetNode<BoxContainer>("%LoadingOverContainer");
      activeCoresLabel = GetNode<Label>("%ActiveCoresLabel");
      remainingCoresLabel = GetNode<Label>("%RemainingCoresLabel");
      actualComputationsLabel = GetNode<Label>("%ActualComputationsLabel");
      optimalComputationsLabel = GetNode<Label>("%OptimalComputationsLabel");
      computationsContainer = GetNode<BoxContainer>("%ComputationsContainer");

      var backToMenuButton = GetNode<Button>("%BackToMenuButton");
      backToMenuButton.ButtonUp += BackToMainMenu;

      var uiScale = (float)config.GetValue("settings", "ui_scale");

      if (uiScale != 1)
      {
        var levelTitleLabels = GetNode("%LevelTitleContainer").GetChildren().OfType<Label>().ToArray();

        foreach (Label label in levelTitleLabels)
        {
          var fontSize = label.Get("theme_override_font_sizes/font_size");
          label.Set("theme_override_font_sizes/font_size", (int)fontSize * uiScale);
        }

        var backToMenuButtonFontSize = backToMenuButton.Get("theme_override_font_sizes/font_size");
        backToMenuButton.Set("theme_override_font_sizes/font_size", (int)backToMenuButtonFontSize * uiScale);

        var computationLabel = GetNode<Label>("%ComputationsLabel");
        var computationLabelFontSize = computationLabel.Get("theme_override_font_sizes/font_size");
        computationLabel.Set("theme_override_font_sizes/font_size", (int)computationLabelFontSize * uiScale);

        var computationsValuesLabel = computationsContainer.GetChildren().OfType<Label>().ToArray();

        foreach (Label label in computationsValuesLabel)
        {
          var fontSize = label.Get("theme_override_font_sizes/font_size");
          label.Set("theme_override_font_sizes/font_size", (int)fontSize * uiScale);
        }
      }
    }

    private void Init()
    {
      SetInitialLabelsText();
    }

    private void SetInitialLabelsText()
    {
      levelTitleLabel.Text = level.LevelTitle;
      // levelTitleLabel.Text = level.LevelTitle + " " + level.LevelSubtitle;
      loadingLabel.Text = "";
      actualComputationsLabel.Text = "0";
      optimalComputationsLabel.Text = level.OptimalScore.ToString();
    }

    private void SetCoresText()
    {
      if (loadingLabel.Text.Length > 3) loadingLabel.Text = "•";

      var activeCoresCount = level.GetActiveCoresCount();
      var coresCount = level.GetCoresCount();

      SetCoresLabel(activeCoresLabel, activeCoresCount);
      SetCoresLabel(remainingCoresLabel, coresCount - activeCoresCount);
    }

    private static void SetCoresLabel(Label label, int coresCount)
    {
      var cores = "";
      for (int i = 0; i < coresCount; i++)
      {
        cores += "◉ ";
      }

      label.Text = cores.Trim();
      label.Visible = coresCount > 0;
    }

    private void SetComputationsText()
    {
      loadingLabel.Text = loadingFrames[level.Score % loadingFrames.Length];
      actualComputationsLabel.Text = level.Score.ToString();

      if (level.Score > level.OptimalScore) computationsContainer.Modulate = new Color(0.75f, 0, 0, 0.75f);
    }

    private void OnGameStart()
    {
      Init();
    }

    private void OnGameWon()
    {
      loadingLabel.Visible = false;
      loadingOverContainer.Visible = true;
    }

    private void OnPlayerMoved(int _score)
    {
      SetComputationsText();
    }

    private void OnCurrentTileUpdated(Tile _currentTile, Tile _previousTile, Direction _direction)
    {
      SetCoresText();
    }

    private void BackToMainMenu()
    {
      var mainMenu = ResourceLoader.Load<PackedScene>("res://scenes/game/menus/MainMenu.tscn");
      GetTree().ChangeSceneToPacked(mainMenu);
    }
  }
}