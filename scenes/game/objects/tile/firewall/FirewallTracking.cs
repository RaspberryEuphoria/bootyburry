using Godot;

namespace Game
{
  public partial class FirewallTracking : Node2D
  {
    private Level level;
    private Tile rootTile;
    private Firewall firewall;

    public override void _Ready()
    {
      level = GetTree().Root.GetNode<Level>("Level");
      firewall = GetParent<Firewall>();
      rootTile = firewall.GetParent<Tile>();

      level.PlayerMoved += OnPlayerMoved;
    }

    public override void _ExitTree()
    {
      level.PlayerMoved -= OnPlayerMoved;
    }

    public void OnPlayerMoved(int _score, Direction direction)
    {
      if (level.GameState != GameState.Playing) return;

      var nextEmptyTile = firewall.GetAdjacentTile(direction);
      if (nextEmptyTile == null || nextEmptyTile.Type != TileType.Empty) return;

      while (true)
      {
        var nextTile = nextEmptyTile.GetAdjacentTile(direction);
        if (nextTile == null || nextTile.Type != TileType.Empty) break;
        nextEmptyTile = nextTile;
      }

      if (nextEmptyTile.Type == TileType.Empty)
      {
        nextEmptyTile.UpdateTerrainDuringGameplay(TileType.Firewall);
        var nextFirewall = nextEmptyTile.Terrain as Firewall;
        nextFirewall.IsFirewallTracking = true;

        rootTile.UpdateTerrainDuringGameplay(TileType.Empty);
      }
    }
  }
}