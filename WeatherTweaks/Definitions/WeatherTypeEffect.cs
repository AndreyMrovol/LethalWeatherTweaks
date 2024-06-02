using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WeatherAPI;
using WeatherTweaks.Definitions;

namespace WeatherTweaks.Definitions
{
  public enum CustomWeatherType
  {
    Normal,
    Combined,
    Progressing
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class WeatherType
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public virtual Weather Weather { get; set; }

    public LevelWeatherType weatherType;

    [JsonProperty]
    public CustomWeatherType Type;

    public virtual bool CanWeatherBeApplied(SelectableLevel level)
    {
      return true;
    }

    public virtual float weightModify { get; set; } = 1f;

    public WeatherType(string name, CustomWeatherType type)
    {
      Plugin.logger.LogWarning($"Creating WeatherType: {name}");

      Name = name;
      Type = type;
    }
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class RegisteredWeatherType
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public LevelWeatherType weatherType;

    public CustomWeatherType Type;

    public float WeightModify = 1f;
  }
}
