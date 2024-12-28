namespace WeatherTweaks
{
  public static class DisableAllWeathers
  {
    internal static void DisableAllWeather()
    {
      ChangeMidDay.Reset();

      if (StartOfRound.Instance.IsHost)
      {
        NetworkedConfig.SetProgressingWeatherEntry(null);
      }
    }
  }
}
