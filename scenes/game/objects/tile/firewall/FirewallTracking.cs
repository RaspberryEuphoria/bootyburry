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

    private async void OnPlayerMoved(int _score, Direction direction)
    {
      if (level.GameState != GameState.Playing) return;

      var nextAdjacentTile = firewall.GetAdjacentTile(direction);
      if (nextAdjacentTile == null || nextAdjacentTile.Type != TileType.Empty) return;

      /**
       * We want to do the next task asynchronously in case multiple firewalls are
       * on the same row or column: if we did this synchronously, the tracking could
       * behave unexpectedly because the neighbord tiles would be updated too soon.
       */
      await ToSignal(GetTree().CreateTimer(0f), SceneTreeTimer.SignalName.Timeout);

      MoveTo(nextAdjacentTile, direction);
    }

    private void MoveTo(Tile nextAdjacentTile, Direction direction)
    {
      while (true)
      {
        var adjacentTile = nextAdjacentTile.GetAdjacentTile(direction);
        if (adjacentTile == null || adjacentTile.Type != TileType.Empty) break;
        nextAdjacentTile = adjacentTile;
      }

      if (nextAdjacentTile.Type == TileType.Empty)
      {
        nextAdjacentTile.ReplaceTerrain(TileType.Firewall);

        var nextFirewall = nextAdjacentTile.Terrain as Firewall;
        nextFirewall.IsFirewallTracking = true;
        nextFirewall.Init();

        rootTile.ReplaceTerrain(TileType.Empty);
      }
    }
  }
}