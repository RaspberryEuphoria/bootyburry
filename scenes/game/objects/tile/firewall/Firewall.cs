using Godot;

namespace Game
{
  public partial class Firewall : TileTerrain
  {
    [Export]
    public bool IsFirewallTracking = false;
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

    public override void _Ready()
    {
      RootTile = GetParent<Tile>();
      if (IsFirewallTracking) ToggleTracking();
    }

    public override void Init()
    {
      if (IsFirewallTracking) ToggleTracking();
    }

    private void ToggleTracking()
    {
      if (IsFirewallTracking)
      {
        AddTracking();
      }
      else
      {
        RemoveTracking();
      }
    }

    private void AddTracking()
    {
      var hasTracking = GetNodeOrNull<FirewallTracking>("FirewallTracking") != null;
      if (hasTracking) return;
      var firewallTracking = ResourceLoader.Load<PackedScene>("res://scenes/game/objects/tile/firewall/FirewallTracking.tscn").Instantiate();
      AddChild(firewallTracking);
    }

    private void RemoveTracking()
    {
      var firewallTracking = GetNodeOrNull<FirewallTracking>("FirewallTracking");
      firewallTracking?.QueueFree();
    }

    public override bool IsBlockedFromDirection(Direction _direction)
    {
      return true;
    }

    public override bool IsSelectableFromDirection(Direction _direction)
    {
      return false;
    }

    public override bool CanUndockInDirection(Direction _direction)
    {
      return false;
    }
  }
}