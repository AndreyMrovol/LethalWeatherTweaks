using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WeatherTweaks
{
  public static class SavedWeatherData
  {
    [JsonObject(MemberSerialization.OptIn)]
    public class WeatherData
    {
      [JsonProperty]
      public string Name { get; set; }

      [JsonProperty]
      public LevelWeatherType Weather { get; set; }

      [JsonProperty]
      public int Day { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DayWeatherData
    {
      [JsonProperty]
      public List<WeatherData> WeatherData { get; set; }
    }

    public static void SaveCurrentWeatherData(int day)
    {
      Plugin.logger.LogDebug("SaveCurrentWeatherData called.");
      Plugin.logger.LogDebug($"Day {day} weather:");

      var dayWeather = new Dictionary<string, LevelWeatherType>();

      foreach (KeyValuePair<string, LevelWeatherType> entry in dayWeather)
      {
        Plugin.logger.LogDebug($"planet: {entry.Key}, weather: {entry.Value}");
      }

      var dayWeatherData = new DayWeatherData();
      dayWeatherData.WeatherData = new List<WeatherData>();

      foreach (KeyValuePair<string, LevelWeatherType> entry in dayWeather)
      {
        var weatherData = new WeatherData();
        weatherData.Name = entry.Key;
        weatherData.Weather = entry.Value;
        weatherData.Day = day;

        dayWeatherData.WeatherData.Add(weatherData);
      }

      var json = JsonConvert.SerializeObject(dayWeatherData);
      ES3.Save<string>($"day{day}Weather", json, GameNetworkManager.Instance.currentSaveFileName);
    }

    public static List<WeatherData> GetWeather(int day)
    {
      Plugin.logger.LogDebug("GetWeather called.");

      var json = ES3.Load<string>($"day{day}Weather", GameNetworkManager.Instance.currentSaveFileName);
      var dayWeatherData = JsonConvert.DeserializeObject<DayWeatherData>(json);

      return dayWeatherData.WeatherData;
    }

    public static LevelWeatherType GetPlanetWeather(int day, string moonName)
    {
      Plugin.logger.LogDebug("GetPlanetWeather called.");

      var dayWeatherData = GetWeather(day);
      var planetWeather = dayWeatherData.FirstOrDefault(x => x.Name == moonName);

      if (planetWeather != null)
      {
        return planetWeather.Weather;
      }
      else
      {
        return LevelWeatherType.None;
      }
    }
  }
}
