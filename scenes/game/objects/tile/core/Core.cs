using System;
using System.Linq;
using Godot;

namespace Game
{
  public partial class Core : TileTerrain
  {
    [Export]
    private bool isCoreEnabledOnStart = false;
    [Export]
    private bool isCoreGlitched = false;
    [Export]
    private Texture2D glitchTexture;

    private Tile _rootTile;
    public override Tile RootTile
    {
      get => _rootTile;
      set
      {
        _rootTile = value;
      }
    }
    public override bool IsPlayerControlled { get => true; }
    public override bool ExpandPreviousPath { get => false; }

    private AnimationPlayer animationPlayer;
    private InnerCore innerCore;
    private Selector[] selectors;

    private StringName enableAnimation = "enable";
    private StringName disableAnimation = "disable";

    public override void _Ready()
    {
      RootTile = GetParent<Tile>();
      selectors = GetChildren().OfType<Selector>().ToArray();
      innerCore = GetNode<InnerCore>("InnerCore");
      animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

      RootTile.TileSelected += OnTileSelected;

      for (int i = 0; i < selectors.Length; i++)
      {
        RootTile.TileSelected += selectors[i].OnTileSelected;
        RootTile.TileUnselected += selectors[i].OnTileUnselected;
      }

      if (isCoreEnabledOnStart)
      {
        innerCore.Toggle();
        animationPlayer.Play("enable");
      }

      if (isCoreGlitched)
      {
        var outerCircle = GetNode<Sprite2D>("OuterCircle");
        outerCircle.Texture = glitchTexture;
      }
    }

    public override bool IsBlockedFromDirection(Direction _direction)
    {
      return false;
    }

    public override bool IsSelectableFromDirection(Direction direction)
    {
      return true;
    }

    public override bool CanUndockInDirection(Direction _direction)
    {
      return true;
    }

    private void TriggerGlitch()
    {
      var topCoreTile = GetNextCoreTileInDirection(Direction.Up);
      var rightCore = GetNextCoreTileInDirection(Direction.Right);
      var bottomCore = GetNextCoreTileInDirection(Direction.Down);
      var leftCore = GetNextCoreTileInDirection(Direction.Left);

      topCoreTile?.Toggle();
      rightCore?.Toggle();
      bottomCore?.Toggle();
      leftCore?.Toggle();
    }

    public bool IsEnabled()
    {
      return innerCore.IsEnabled();
    }

    public override void Toggle()
    {
      innerCore.Toggle();
      animationPlayer.Play(innerCore.IsEnabled() ? enableAnimation : disableAnimation);
    }

    public void OnTileSelected(Tile _tile, Tile _previousTile, Direction _direction)
    {
      Toggle();

      if (isCoreGlitched)
      {
        TriggerGlitch();
      }
    }
  }
}