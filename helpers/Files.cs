using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace Helpers
{
  public partial class KamiFiles : Node
  {
    public static List<string> GetFilesFromFolder(string folder, Regex regex = null)
    {
      var dir = DirAccess.Open(folder);
      if (dir == null)
      {
        GD.PrintErr($"Could not open directory {folder}.");
        return new List<string> { };
      }

      var files = new List<string> { };

      dir.ListDirBegin();
      string fileName = dir.GetNext();
      while (fileName != "")
      {
        if (!dir.CurrentIsDir())
        {
          if (regex == null || regex.IsMatch(fileName))
          {
            files.Add(fileName);
          }
        }

        fileName = dir.GetNext();
      }
      dir.ListDirEnd();

      return files;
    }
  }
}