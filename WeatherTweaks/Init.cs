using WeatherRegistry;
using WeatherTweaks;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  public class Init
  {
    public static void InitMethod()
    {
      new CombinedWeatherType(
        "Eclipsed + Flooded",
        [new WeatherTypeResolvable(LevelWeatherType.Eclipsed), new WeatherTypeResolvable(LevelWeatherType.Flooded)]
      );
      new CombinedWeatherType(
        "Foggy + Rainy",
        [new WeatherTypeResolvable(LevelWeatherType.Foggy), new WeatherTypeResolvable(LevelWeatherType.Rainy)]
      );
      new CombinedWeatherType(
        "Eclipsed + Rainy",
        [new WeatherTypeResolvable(LevelWeatherType.Eclipsed), new WeatherTypeResolvable(LevelWeatherType.Rainy)]
      );
      new CombinedWeatherType(
        "Stormy + Rainy",
        [new WeatherTypeResolvable(LevelWeatherType.Stormy), new WeatherTypeResolvable(LevelWeatherType.Rainy)]
      );
      new CombinedWeatherType(
        "Stormy + Flooded",
        [new WeatherTypeResolvable(LevelWeatherType.Stormy), new WeatherTypeResolvable(LevelWeatherType.Flooded)]
      );
      new CombinedWeatherType(
        "Foggy + Flooded",
        [new WeatherTypeResolvable(LevelWeatherType.Foggy), new WeatherTypeResolvable(LevelWeatherType.Flooded)]
      );
      new CombinedWeatherType(
        "Foggy + Eclipsed",
        [new WeatherTypeResolvable(LevelWeatherType.Foggy), new WeatherTypeResolvable(LevelWeatherType.Eclipsed)]
      );
      new CombinedWeatherType(
        "Stormy + Rainy + Eclipsed",
        [
          new WeatherTypeResolvable(LevelWeatherType.Stormy),
          new WeatherTypeResolvable(LevelWeatherType.Rainy),
          new WeatherTypeResolvable(LevelWeatherType.Eclipsed)
        ]
      );
      new CombinedWeatherType(
        "Stormy + Rainy + Flooded",
        [
          new WeatherTypeResolvable(LevelWeatherType.Stormy),
          new WeatherTypeResolvable(LevelWeatherType.Rainy),
          new WeatherTypeResolvable(LevelWeatherType.Flooded)
        ]
      );
      new CombinedWeatherType(
        "Stormy + Rainy + Flooded + Eclipsed",
        [
          new WeatherTypeResolvable(LevelWeatherType.Stormy),
          new WeatherTypeResolvable(LevelWeatherType.Rainy),
          new WeatherTypeResolvable(LevelWeatherType.Flooded),
          new WeatherTypeResolvable(LevelWeatherType.Eclipsed)
        ]
      );

      new ProgressingWeatherType(
        "None > Foggy",
        new WeatherTypeResolvable(LevelWeatherType.None),
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.25f,
            Chance = 0.8f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Foggy)
          },
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.75f,
            Chance = 1.0f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Foggy)
          }
        ]
      );

      new ProgressingWeatherType(
        "None > Stormy",
        new WeatherTypeResolvable(LevelWeatherType.None),
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.35f,
            Chance = 0.35f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Stormy)
          },
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.75f,
            Chance = 1.0f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Stormy)
          }
        ]
      );

      new ProgressingWeatherType(
        "Eclipsed > Foggy",
        new WeatherTypeResolvable(LevelWeatherType.Eclipsed),
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.40f,
            Chance = 0.5f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Foggy)
          },
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.85f,
            Chance = 1.0f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Foggy)
          }
        ]
      );

      new ProgressingWeatherType(
        "Foggy > None",
        new WeatherTypeResolvable(LevelWeatherType.Foggy),
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.45f,
            Chance = 1f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.None)
          }
        ]
      );

      new ProgressingWeatherType(
        "Eclipsed > None",
        new WeatherTypeResolvable(LevelWeatherType.Eclipsed),
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.66f,
            Chance = 1f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.None)
          }
        ]
      );

      new ProgressingWeatherType(
        "Stormy > Rainy",
        new WeatherTypeResolvable(LevelWeatherType.Stormy),
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.55f,
            Chance = 1.0f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Rainy)
          }
        ]
      );

      new ProgressingWeatherType(
        "Rainy > Eclipsed",
        new WeatherTypeResolvable(LevelWeatherType.Rainy),
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.66f,
            Chance = 1.0f,
            Weather = new WeatherTypeResolvable(LevelWeatherType.Eclipsed)
          }
        ]
      );

      if (Plugin.MrovWeathersCompat.IsModPresent)
      {
        new ProgressingWeatherType(
          "None > Blackout",
          new WeatherTypeResolvable(LevelWeatherType.None),
          [
            new Definitions.ProgressingWeatherEntry
            {
              DayTime = 0.66f,
              Chance = 1.0f,
              Weather = new WeatherNameResolvable("blackout")
            }
          ]
        );
      }
    }
  }
}
