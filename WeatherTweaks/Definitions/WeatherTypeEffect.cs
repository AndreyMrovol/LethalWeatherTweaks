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
  public class WeatherType
  {
    [JsonProperty]
    public string Name;

    // public RandomWeatherWithVariables Weather;
    [JsonProperty]
    public LevelWeatherType weatherType;

    [JsonIgnore]
    public List<WeatherEffect> Effects;

    [JsonProperty]
    public List<LevelWeatherType> Weathers;

    [JsonProperty]
    public CustomWeatherType Type = CustomWeatherType.Vanilla;
  }

  // [JsonObject(MemberSerialization.OptIn)]
  // public class ExtendedWeatherEffect : WeatherEffect
  // {
  //   [JsonProperty]
  //   public int variable1 = 1;

  //   [JsonProperty]
  //   public int variable2 = 1;
  // }

  public class CustomWeatherEffect : WeatherEffect
  {
    public int variable1 = 1;
    public int variable2 = 1;
  }
}
