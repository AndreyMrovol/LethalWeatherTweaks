using System.Collections.Generic;
using System.Linq;

namespace WeatherTweaks.Modules
{
  public class FoggyPatchHandler
  {
    public static string DefaultValue = "";
    public static List<SelectableLevel> LevelsToIgnore = [];

    public static void AddLevelToIgnore(string levelName)
    {
      SelectableLevel level = MrovLib.StringResolver.ResolveStringToLevel(levelName);
      if (level != null)
      {
        AddLevelToIgnore(level);
      }
    }

    public static void AddLevelToIgnore(SelectableLevel level)
    {
      if (LevelsToIgnore != ConfigManager.FoggyIgnoreLevels.Value.ToList())
      {
        LevelsToIgnore = ConfigManager.FoggyIgnoreLevels.Value.ToList();
      }

      LevelsToIgnore.Add(level);

      ConfigManager.FoggyIgnoreLevels.SetNewLevelsToIgnore(LevelsToIgnore.ToArray());
    }
  }
}
