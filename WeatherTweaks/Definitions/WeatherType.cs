using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace WeatherTweaks.Definitions
{
  public enum Type
  {
    Clear,
    Vanilla,
    Modded,
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class Weather
  {
    [JsonProperty]
    public string Name;

    [JsonProperty]
    public LevelWeatherType VanillaWeatherType = LevelWeatherType.None;

    [JsonProperty]
    public Type Type = Type.Modded;

    [JsonIgnore]
    public List<LevelWeatherVariables> WeatherVariables;

    [JsonIgnore]
    public WeatherEffect Effect;

    public float ScrapAmountMultiplier = 1;
    public float ScrapValueMultiplier = 1;

    [JsonIgnore]
    public Color Color;
    public int DefaultWeight = 50;

    [JsonIgnore]
    public AnimationClip AnimationClip;

    public Weather(string name, WeatherEffect effect)
    {
      Name = name;
      Effect = effect;

      Variables.RegisteredWeathers.Add(this);
    }
  }

  public class LevelWeatherVariables
  {
    public SelectableLevel Level;

    public int WeatherVariable1;
    public int WeatherVariable2;
  }

  public class LevelWeather : LevelWeatherVariables
  {
    public Weather Weather;
    public LevelWeatherVariables Variables;
  }

  public class WeatherEffect
  {
    [JsonIgnore]
    public GameObject EffectObject;

    [JsonIgnore]
    public GameObject WorldObject;

    private bool _effectEnabled;

    public string SunAnimatorBool;

    public bool EffectEnabled
    {
      get { return _effectEnabled; }
      set
      {
        EffectObject?.SetActive(value);
        WorldObject?.SetActive(value);

        _effectEnabled = value;
      }
    }

    public void DisableEffect(bool permament = false)
    {
      if (permament)
      {
        EffectEnabled = false;
      }
      else
      {
        EffectObject?.SetActive(false);
      }
    }

    public int DefaultVariable1;
    public int DefaultVariable2;

    public WeatherEffect(GameObject effectObject, GameObject worldObject)
    {
      EffectObject = effectObject;
      WorldObject = worldObject;
    }
  }
}
