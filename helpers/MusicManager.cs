using Godot;

namespace Helpers
{
  public partial class MusicManager : Node
  {
    public override void _Ready()
    {
      var musicPlayer = ResourceLoader.Load<PackedScene>("res://scenes/music/MusicPlayer/MusicPlayer.tscn").Instantiate();
      AddChild(musicPlayer);
    }
  }
}