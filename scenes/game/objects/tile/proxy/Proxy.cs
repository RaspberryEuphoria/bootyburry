using System.Linq;
using Godot;
using Helpers;
using UI;

namespace Game
{
  /**
   * A Proxy is a special type of Tile that is used to connect two Tiles sharing the same ProxyId.
   * 1. The player moves to the tile with Proxy A
   * 2. The Proxy A automatically selects Proxy B (through the "ExitTile" property)
   * 3. The Proxy B triggers an input with the same direction the player used
     when moving to Proxy A
   * 4. Finally, the player is stopped at the next selectable tile.
   */
  public partial class Proxy : TileTerrain
  {
    [Export]
    public int ProxyId { get; private set; } = 0;

    private Tile _rootTile;
    public override Tile RootTile
    {
      get => _rootTile;
      set
      {
        _rootTile = value;
      }
    }

    public override bool IsPlayerControlled { get => false; }
    public override bool ExpandPreviousPath { get => false; }

    private Level level;
    public Tile ExitTile { get; private set; }
    private Sprite2D portal;
    private Sprite2D portalAlt;
    private CpuParticles2D particles;
    private AnimationPlayer animationPlayer;
    private StringName animationToPlay = "shake";

    public override void _Ready()
    {
      RootTile = GetParent<Tile>();
      level = GetTree().Root.GetNode<Level>("Level");
      portal = GetNode<Sprite2D>("Portal");
      particles = GetNode<CpuParticles2D>("Particles");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      level.CurrentTileUpdated += OnCurrentTileUpdated;
    }

    /**
     * The first Proxy tile to be Initialized won't have an ExitTile yet.
     * Because of this, the Init will return early.
     * The trick is that once its corresponding ExitTile is Initialized,
     * it will call this Init method again.
     */
    public override void Init()
    {
      var proxyTiles = level.GetTiles().Where(tile => tile.IsProxy());
      if (!proxyTiles.Any()) return;

      var exitTile = proxyTiles.Where(tile => tile.Name != RootTile.Name && (tile.Terrain as Proxy).ProxyId == ProxyId).FirstOrDefault();
      if (exitTile == null) return;

      particles.Emitting = true;
      ExitTile = exitTile;

      if ((ExitTile.Terrain as Proxy).ExitTile == null)
      {
        ExitTile.Terrain.Init();
      }

      if (proxyTiles.Count() > 2) SetupIdLabel();
    }

    public override void _ExitTree()
    {
      level.CurrentTileUpdated -= OnCurrentTileUpdated;
    }

    private void AddNavigationPathToExitTile()
    {
      var navigationPath = ResourceLoader.Load<PackedScene>("res://NavigationPath.tscn").Instantiate<NavigationPath>();

      navigationPath.Init(RootTile, ExitTile);
      level.AddChild(navigationPath);
    }

    public override Tile GetNextSelectableTileInDirection(Direction direction)
    {
      return ExitTile.GetNextSelectableTileInDirection(direction);
    }

    public override Tile GetNextCoreTileInDirection(Direction direction)
    {
      return ExitTile.GetNextCoreTileInDirection(direction);
    }

    public override bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
    }

    public override bool IsSelectableFromDirection(Direction _direction)
    {
      return true;
    }

    public override bool CanUndockInDirection(Direction _direction)
    {
      return true;
    }

    private void SetupIdLabel()
    {
      var idLabel = GetNode<KamiLabel>("IdLabel");
      idLabel.Show();

      for (int i = 0; i <= ProxyId; i++)
      {
        idLabel.Text += ".";
      }
    }

    public void OnCurrentTileUpdated(Tile tile, Tile previousTile, Direction direction)
    {
      if (previousTile == null || tile != RootTile) return;

      animationPlayer.Play(animationToPlay);

      // Case 1: this Proxy was selected by the player
      if (previousTile != ExitTile)
      {
        level.DrawNavigationPath(RootTile, ExitTile, true);
        ExitTile.Select(RootTile, direction);
        return;
      }

      // Case 2: this Proxy was selected by its ExitTile
      level.TriggerInputInDirection(direction);
    }
  }
}