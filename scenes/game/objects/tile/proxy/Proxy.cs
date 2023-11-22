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
  public partial class Proxy : Node2D
  {
    [Export]
    public Tile ExitTile;
    [Export]
    private bool useAlternativeVisual = false;

    public static readonly bool IsPlayerControlled = false;
    public static readonly bool ExpandPreviousPath = false;

    private Level level;
    private Tile rootTile;
    private Sprite2D portal;
    private Sprite2D portalAlt;
    private CpuParticles2D particles;
    private CpuParticles2D particlesAlt;
    private AnimationPlayer animationPlayer;
    private StringName animationToPlay = "shake";

    public override void _Ready()
    {
      rootTile = GetParent<Tile>();
      level = GetTree().Root.GetNode<Level>("Level");
      portal = GetNode<Sprite2D>("Portal");
      portalAlt = GetNode<Sprite2D>("PortalAlt");
      particles = GetNode<CpuParticles2D>("Particles");
      particlesAlt = GetNode<CpuParticles2D>("ParticlesAlt");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      level.CurrentTileUpdated += OnCurrentTileUpdated;

      if (ExitTile == null)
      {
        GD.PrintErr($"Proxy on tile {rootTile.Name} doesn't have an exit.");
      }

      if (useAlternativeVisual) SetupAlternativeVisual();
    }

    public bool Dock()
    {
      return true;
    }

    public bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
    }

    public bool IsSelectableFromDirection(Direction _direction)
    {
      return true;
    }

    public bool CanUndockInDirection(Direction _direction)
    {
      return true;
    }

    public Direction? GetForcedDirection()
    {
      return null;
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
      if (previousTile == null || tile != rootTile) return;

      animationPlayer.Play(animationToPlay);

      // Case 1: this Proxy was selected by the player
      if (previousTile != ExitTile)
      {
        ExitTile.Select(rootTile, direction);
        return;
      }

      // Case 2: this Proxy was selected by its ExitTile
      level.TriggerInputInDirection(direction);
    }
  }
}