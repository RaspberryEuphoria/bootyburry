using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using UI;

namespace Menu
{
  public partial class LevelSelector : BoxContainer
  {
    [GeneratedRegex(@"Level_\d_\d")]
    private static partial Regex LevelRegex();
    [GeneratedRegex("\\d+")]
    private static partial Regex DigitRegex();
    private StringName levelsFolder = "res://scenes/game/level";
    // private IEnumerable<string> levels = new List<string> { };

    private BoxContainer worldTemplate;

    public override void _Ready()
    {
      worldTemplate = GetNode<BoxContainer>("WorldTemplate");

      var levels = GetLevels();
      var worlds = new List<List<int>> { };

      levels.ToList().ForEach(level =>
      {
        var digits = DigitRegex().Matches(level);
        var (worldId, levelId) = (digits.ElementAt(0).Value.ToInt(), digits.ElementAt(1).Value.ToInt());

        if (worlds.Count < worldId + 1)
        {
          worlds.Add(new List<int> { });
        }

        var world = worlds[worldId];
        world.Add(levelId);
      });

      SetupWorlds(worlds);

      worldTemplate.QueueFree();
    }

    private IEnumerable<string> GetLevels()
    {
      var dir = DirAccess.Open(levelsFolder);
      if (dir == null)
      {
        GD.PrintErr($"Could not open directory {levelsFolder}.");
        return new List<string> { };
      }

      var levels = new List<string> { };

      dir.ListDirBegin();
      string fileName = dir.GetNext();
      while (fileName != "")
      {
        if (!dir.CurrentIsDir())
        {
          if (LevelRegex().IsMatch(fileName))
          {
            levels.Add(fileName);
          }
        }

        fileName = dir.GetNext();
      }
      dir.ListDirEnd();

      return levels;
    }

    private void SetupWorlds(List<List<int>> worlds)
    {
      for (int world = 0; world < worlds.Count; world++)
      {
        var newWorld = worldTemplate.Duplicate() as BoxContainer;
        var levelTemplate = newWorld.GetNode<KamiButton>("LevelTemplate");

        var levels = worlds[world];
        newWorld.Name = $"World{world}";

        for (int level = 0; level < levels.Count; level++)
        {
          var newLevel = levelTemplate.Duplicate() as KamiButton;
          newWorld.AddChild(newLevel);

          // local copies for closure
          var _world = world;
          var _level = level;

          newLevel.Name = $"Level {_world}_{level}";
          newLevel.Text = $"{_world}.{level}";
          newLevel.ButtonUp += () => SelectLevel(_world, _level);
        }

        AddChild(newWorld);
        levelTemplate.QueueFree();
      }
    }

    private void SelectLevel(int worldId, int levelId)
    {
      var nextLevel = ResourceLoader.Load<PackedScene>($"res://scenes/game/level/Level_{worldId}_{levelId}.tscn");
      GetTree().ChangeSceneToPacked(nextLevel);
    }
  }
}