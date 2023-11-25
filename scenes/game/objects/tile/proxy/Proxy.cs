using Godot;

namespace Game
{
  /**
   * A Proxy is a special type of Tile that is used to connect two Tiles.
   * 1. The player moves to the tile with Proxy A
   * 2. The Proxy A automatically selects Proxy B (through the "ExitTile" property)
   * 3. The Proxy B triggers an input with the same direction the player used
     when moving to Proxy A
   * 4. Finally, the player is stopped at the next selectable tile.
   */
  public partial class Proxy : TileTerrain
  {
    [Export]
    public Tile ExitTile;
    [Export]
    private bool useAlternativeVisual = false;

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
    private Sprite2D portal;
    private Sprite2D portalAlt;
    private CpuParticles2D particles;
    private CpuParticles2D particlesAlt;
    private AnimationPlayer animationPlayer;
    private StringName animationToPlay = "shake";

    public override void _Ready()
    {
      RootTile = GetParent<Tile>();
      level = GetTree().Root.GetNode<Level>("Level");
      portal = GetNode<Sprite2D>("Portal");
      portalAlt = GetNode<Sprite2D>("PortalAlt");
      particles = GetNode<CpuParticles2D>("Particles");
      particlesAlt = GetNode<CpuParticles2D>("ParticlesAlt");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      level.CurrentTileUpdated += OnCurrentTileUpdated;

      if (ExitTile == null)
      {
        GD.PrintErr($"Proxy on tile {RootTile.Name} doesn't have an exit.");
      }

      if (useAlternativeVisual) SetupAlternativeVisual();
    }

    public override Tile GetNextSelectableTileInDirection(Direction direction)
    {
      return ExitTile.GetNextSelectableTileInDirection(direction);
    }

    public override Tile GetNextCoreTileInDirection(Direction direction)
    {
      return ExitTile.GetNextCoreTileInDirection(direction);
    }

    public bool Dock()
    {
      return true;
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

    private void SetupAlternativeVisual()
    {
      animationToPlay = "shake_alt";

      portal.Visible = false;
      portalAlt.Visible = true;

      particles.Visible = false;
      particlesAlt.Visible = true;
    }

    public void OnCurrentTileUpdated(Tile tile, Tile previousTile, Direction direction)
    {
      if (previousTile == null || tile != RootTile) return;

      animationPlayer.Play(animationToPlay);

      // Case 1: this Proxy was selected by the player
      if (previousTile != ExitTile)
      {
        ExitTile.Select(RootTile, direction);
        return;
      }

      // Case 2: this Proxy was selected by its ExitTile
      level.TriggerInputInDirection(direction);
    }
  }
}