using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WeatherRegistry;
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

    public virtual (float valueMultiplier, float amountMultiplier) GetMultiplierData()
    {
      return (Weather?.ScrapValueMultiplier ?? 1f, Weather?.ScrapAmountMultiplier ?? 1f);
    }

    public virtual float WeightModify { get; set; } = 1f;

    public WeatherType(string name, CustomWeatherType type)
    {
      Plugin.logger.LogWarning($"Creating WeatherType: {name}");

      Name = name;
      Type = type;
    }
  }

  [JsonObject(MemberSerialization.OptIn)]
  internal class WeatherMultiplierData(LevelWeatherType weatherType, float valueMultiplier, float spawnMultiplier)
  {
    [JsonProperty]
    public LevelWeatherType weatherType = weatherType;

    [JsonProperty]
    public float valueMultiplier = valueMultiplier;

    [JsonProperty]
    public float spawnMultiplier = spawnMultiplier;
  }
}
