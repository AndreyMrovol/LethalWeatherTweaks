using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WeatherRegistry;
using WeatherRegistry.Definitions;
using WeatherTweaks.Definitions;

namespace WeatherTweaks
{
  internal class WeatherCalculation
  {
    internal static Dictionary<string, LevelWeatherType> previousDayWeather = [];
    internal static SelectableLevel CompanyMoon => MrovLib.LevelHelper.CompanyMoon;

    internal static FirstDayWeathersAlgorithm firstDayWeathersAlgorithm = new();
    internal static WeatherTweaksWeatherAlgorithm weatherTweaksWeatherAlgorithm = new();

    internal class WeatherTweaksWeatherAlgorithm : WeatherSelectionAlgorithm
    {
      public override Dictionary<SelectableLevel, LevelWeatherType> SelectWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
      {
        Plugin.logger.LogMessage("SetWeathers called.");

        if (!StartOfRound.Instance.IsHost)
        {
          Plugin.logger.LogMessage("Not a host, cannot generate weather!");
          return null;
        }

        previousDayWeather.Clear();

        System.Random random = GetRandom(startOfRound);

        Dictionary<SelectableLevel, Weather> currentWeather = [];

        List<LevelWeatherType> VanillaWeatherTypes = MrovLib.Defaults.VanillaWeathers;

        List<SelectableLevel> levels = Variables.GetGameLevels();

        int day = startOfRound.gameStats.daysSpent;
        int quota = TimeOfDay.Instance.timesFulfilledQuota;
        int dayInQuota = day % 3;

        if (day == 0 && ConfigManager.FirstDaySpecial.Value)
        {
          return WeatherCalculation.firstDayWeathersAlgorithm.SelectWeathers(connectedPlayersOnServer, startOfRound);
        }

        float lengthMultiplier = quota * ConfigManager.GameLengthMultiplier.Value;
        float playerMultiplier = StartOfRound.Instance.livingPlayers * ConfigManager.GamePlayersMultiplier.Value;

        float difficultyMultiplier = lengthMultiplier + playerMultiplier;
        Plugin.logger.LogDebug($"Difficulty multiplier: {difficultyMultiplier}");

        foreach (SelectableLevel level in levels)
        {
          previousDayWeather[level.PlanetName] = level.currentWeather;

          if (ConfigManager.AlwaysClear.Value)
          {
            Plugin.logger.LogDebug("AlwaysClear is true, setting weather to None");
            currentWeather[level] = Variables.NoneWeather;
            continue;
          }

          if (level.overrideWeather)
          {
            Plugin.logger.LogDebug($"Override weather present, changing weather to {level.overrideWeatherType}");
            currentWeather[level] = Variables.WeatherTweaksTypes.Find(x =>
              x.VanillaWeatherType == level.overrideWeatherType && x.CustomType == CustomWeatherType.Normal
            );
            continue;
          }

          // the weather should be more random by making it less random:

          // possible weathers taken from level.randomWeathers
          // use random for seeded randomness

          Plugin.logger.LogDebug("-------------");
          Plugin.logger.LogDebug($"{level.PlanetName}");
          Plugin.logger.LogDebug($"previousDayWeather: {previousDayWeather[level.PlanetName]}");

          currentWeather[level] = Variables.NoneWeather;

          // get the weighted list of weathers
          MrovLib.WeightHandler<Weather> weights = Variables.GetPlanetWeightedList(level);
          var weather = weights.Random();

          currentWeather[level] = weather;

          Plugin.logger.LogDebug($"Selected weather: {weather.Name}");
          try
          {
            Plugin.logger.LogDebug(
              $"Chance for that was {weights.Get(weather)} / {weights.Sum} ({(float)weights.Get(weather) / weights.Sum * 100}%)"
            );
          }
          catch { }

          // currentWeather[level.PlanetName] = currentWeather[level.PlanetName];
        }

        Plugin.logger.LogDebug("-------------");

        Dictionary<SelectableLevel, LevelWeatherType> selectedWeathersLevel = [];
        foreach (var selectedWeather in currentWeather)
        {
          selectedWeathersLevel[selectedWeather.Key] = selectedWeather.Value.VanillaWeatherType;
        }
        return selectedWeathersLevel;
      }
    }

    internal class FirstDayWeathersAlgorithm : WeatherSelectionAlgorithm
    {
      public List<string> PlanetsWithoutWeather { get; set; }

      public override System.Random GetRandom(StartOfRound startOfRound)
      {
        System.Random random = new(startOfRound.randomMapSeed);
        int seed = ConfigManager.FirstDaySeed.Value;

        if (ConfigManager.FirstDayRandomSeed.Value)
        {
          seed = random.Next(0, 10000000);
        }

        return new System.Random(seed);
      }

      public override Dictionary<SelectableLevel, LevelWeatherType> SelectWeathers(int connectedPlayersOnServer, StartOfRound startOfRound)
      {
        Plugin.logger.LogInfo("First day, setting predefined weather conditions");

        // from all levels, 2 cannot have a weather condition (41 Experimentation and 56 Vow)
        // if there are more than 9 levels (vanilla amount), make it 3 without weather

        List<SelectableLevel> levels = Variables.GetGameLevels();
        Dictionary<SelectableLevel, Weather> selectedWeathers = [];
        List<string> noWeatherOnStartPlanets = ["41 Experimentation", "56 Vow"];
        List<SelectableLevel> planetsToPickFrom = levels.Where(level => !noWeatherOnStartPlanets.Contains(level.PlanetName)).ToList();

        System.Random random = GetRandom(startOfRound);

        if (levels.Count > 9)
        {
          // make 1/2 (default) of them have no weather
          int planetsWithoutWeather = (int)(levels.Count * 0.5);
          Plugin.logger.LogDebug($"Planets without weather: {planetsWithoutWeather + 2}");

          for (int i = 0; i < planetsWithoutWeather; i++)
          {
            // pick a random planet
            string planetName = planetsToPickFrom[random.Next(0, planetsToPickFrom.Count)].PlanetName;

            // add it to the list of planets without weather
            noWeatherOnStartPlanets.Add(planetName);

            // remove it from the list of planets to pick from
            planetsToPickFrom.RemoveAll(level => level.PlanetName == planetName);
          }
        }
        PlanetsWithoutWeather = noWeatherOnStartPlanets;

        foreach (SelectableLevel level in Variables.GameLevels)
        {
          string planetName = level.PlanetName;
          Plugin.logger.LogDebug($"planet: {planetName}");

          if (ConfigManager.AlwaysClear.Value)
          {
            Plugin.logger.LogDebug("AlwaysClear is true, setting weather to None");
            selectedWeathers[level] = Variables.NoneWeather;
            continue;
          }

          if (level.overrideWeather)
          {
            Plugin.logger.LogDebug($"Override weather present, changing weather to {level.overrideWeatherType}");
            selectedWeathers[level] = Variables.WeatherTweaksTypes.Find(x =>
              x.VanillaWeatherType == level.overrideWeatherType && x.CustomType == CustomWeatherType.Normal
            );
            continue;
          }

          List<WeatherTweaksWeather> randomWeathers = Variables
            .GetPlanetWeatherTypes(level)
            .Where(randomWeather =>
              randomWeather.VanillaWeatherType != LevelWeatherType.None
              && randomWeather.VanillaWeatherType != LevelWeatherType.DustClouds
              && randomWeather.CustomType == CustomWeatherType.Normal
            )
            .ToList();

          // var randomWeathers = level.randomWeathers.ToList();

          var stringifiedRandomWeathers = JsonConvert.SerializeObject(randomWeathers.Select(x => x.VanillaWeatherType.ToString()).ToList());

          randomWeathers.RemoveAll(x => x.VanillaWeatherType == LevelWeatherType.Eclipsed);

          if (randomWeathers.Count == 0 || randomWeathers == null)
          {
            selectedWeathers[level] = Variables.NoneWeather;
            Plugin.logger.LogDebug($"No random weathers for {planetName}, skipping");
            continue;
          }

          if (PlanetsWithoutWeather.Contains(planetName))
          {
            selectedWeathers[level] = Variables.NoneWeather;
            Plugin.logger.LogDebug($"Skipping {planetName} (predefined)");
            continue;
          }

          // 5% chance for eclipsed
          bool shouldBeEclipsed = random.Next(0, 100) < 5;
          var selectedRandom = randomWeathers[random.Next(0, randomWeathers.Count)];

          if (shouldBeEclipsed)
          {
            Plugin.logger.LogDebug($"Setting eclipsed for {planetName}");
            // check if eclipsed is possible in randomWeathers
            if (!randomWeathers.Any(x => x.VanillaWeatherType == LevelWeatherType.Eclipsed))
            {
              Plugin.logger.LogDebug($"Eclipsed not possible for {planetName}, setting random weather");
            }
            else
            {
              selectedRandom = randomWeathers.First(x => x.VanillaWeatherType == LevelWeatherType.Eclipsed);
            }
          }

          Weather selectedWeather = Variables.WeatherTweaksTypes.Find(x =>
            x.VanillaWeatherType == selectedRandom.VanillaWeatherType && x.CustomType == CustomWeatherType.Normal
          );

          selectedWeathers[level] = selectedWeather;
          Plugin.logger.LogDebug($"Set weather for {planetName}: {selectedWeather.Name}");
        }

        if (CompanyMoon != null)
        {
          selectedWeathers[CompanyMoon] = Variables.NoneWeather;
        }

        Dictionary<SelectableLevel, LevelWeatherType> selectedWeathersLevel = [];
        foreach (var selectedWeather in selectedWeathers)
        {
          selectedWeathersLevel[selectedWeather.Key] = selectedWeather.Value.VanillaWeatherType;
        }
        return selectedWeathersLevel;
      }
    }
  }
}
