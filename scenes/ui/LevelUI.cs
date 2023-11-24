
using Game;
using Godot;

namespace UI
{
  public partial class LevelUI : CanvasLayer
  {
    [Export(PropertyHint.MultilineText)]
    private string helperText;


    private ConfigFile config = new();
    private Control helperControl;
    private Level level;
    private Label levelTitleLabel;
    private Label loadingLabel;
    private BoxContainer loadingOverContainer;
    private Label activeCoresLabel;
    private Label remainingCoresLabel;
    private Label actualComputationsLabel;
    private Label optimalComputationsLabel;
    private string[] loadingFrames = new string[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

    public override void _Ready()
    {
      config.Load("user://settings.cfg");

      var uiScale = (float)config.GetValue("settings", "ui_scale");
      Scale = new Vector2(uiScale, uiScale);

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
    }

    private void Init()
    {
      SetInitialLabelsText();
    }

    private void SetInitialLabelsText()
    {
      levelTitleLabel.Text = level.LevelTitle + " " + level.LevelSubtitle;
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
    }

    private void SetupHelperText()
    {
      var helpLabel = helperControl.GetNode<RichTextLabel>("%HelpLabel");
      if (helperText == null || helperText.Trim() == "")
      {
        helpLabel.QueueFree();
        return;
      }

      helpLabel.Visible = true;
      helpLabel.Text = helperText;
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
  }
}