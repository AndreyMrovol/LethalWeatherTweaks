using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  public enum CustomWeatherType
  {
    Vanilla,
    Custom,
    Combined
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class WeatherType(string name, LevelWeatherType weatherType, List<LevelWeatherType> weathers, CustomWeatherType type)
  {
    [JsonProperty]
    public string Name = name;

    // public RandomWeatherWithVariables Weather;
    [JsonProperty]
    public LevelWeatherType weatherType = weatherType;

    [JsonIgnore]
    public List<WeatherEffect> Effects;

    [JsonProperty]
    public List<LevelWeatherType> Weathers = weathers;

    [JsonProperty]
    public CustomWeatherType Type = type;
  }

  // [JsonObject(MemberSerialization.OptIn)]
  // public class ExtendedWeatherEffect : WeatherEffect
  // {
  //   [JsonProperty]
  //   public int variable1 = 1;

  //   [JsonProperty]
  //   public int variable2 = 1;
  // }

  // public class CustomWeatherEffect : WeatherEffect
  // {
  //   public int variable1 = 1;
  //   public int variable2 = 1;
  // }
}
