using System.Collections.Generic;
using System.Linq;

namespace WeatherTweaks
{
  internal class CombinedEclipsedFlooded : Modules.Types.CombinedWeatherType
  {
    public CombinedEclipsedFlooded()
      : base("Eclipsed + Flooded", [LevelWeatherType.Eclipsed, LevelWeatherType.Flooded]) { }
  }

  internal class CombinedFoggyRainy : Modules.Types.CombinedWeatherType
  {
    public CombinedFoggyRainy()
      : base("Foggy + Rainy", [LevelWeatherType.Foggy, LevelWeatherType.Rainy]) { }
  }

  internal class CombinedEclipsedRainy : Modules.Types.CombinedWeatherType
  {
    public CombinedEclipsedRainy()
      : base("Eclipsed + Rainy", [LevelWeatherType.Eclipsed, LevelWeatherType.Rainy]) { }
  }

  internal class CombinedStormyRainy : Modules.Types.CombinedWeatherType
  {
    public CombinedStormyRainy()
      : base("Stormy + Rainy", [LevelWeatherType.Stormy, LevelWeatherType.Rainy]) { }
  }

  internal class CombinedStormyFlooded : Modules.Types.CombinedWeatherType
  {
    public CombinedStormyFlooded()
      : base("Stormy + Flooded", [LevelWeatherType.Stormy, LevelWeatherType.Flooded])
    {
      // weightModify = 10f;
    }
  }

  internal class CombinedFoggyFlooded : Modules.Types.CombinedWeatherType
  {
    public CombinedFoggyFlooded()
      : base("Foggy + Flooded", [LevelWeatherType.Foggy, LevelWeatherType.Flooded]) { }
  }

  internal class CombinedFoggyEclipsed : Modules.Types.CombinedWeatherType
  {
    public CombinedFoggyEclipsed()
      : base("Foggy + Eclipsed", [LevelWeatherType.Foggy, LevelWeatherType.Eclipsed]) { }
  }

  internal class CombinedStormyRainyEclipsed : Modules.Types.CombinedWeatherType
  {
    public CombinedStormyRainyEclipsed()
      : base("Stormy + Rainy + Eclipsed", [LevelWeatherType.Stormy, LevelWeatherType.Rainy, LevelWeatherType.Eclipsed]) { }
  }

  internal class CombinedStormyRainyFlooded : Modules.Types.CombinedWeatherType
  {
    public CombinedStormyRainyFlooded()
      : base("Stormy + Rainy + Flooded", [LevelWeatherType.Stormy, LevelWeatherType.Rainy, LevelWeatherType.Flooded]) { }
  }

  // internal class SuperFoggy : Modules.Types.CombinedWeatherType
  // {
  //   public SuperFoggy()
  //     : base("Super Foggy", [LevelWeatherType.Foggy, LevelWeatherType.DustClouds], 0.3f) { }
  // }

  internal class CombinedMadness : Modules.Types.CombinedWeatherType
  {
    public CombinedMadness()
      : base(
        "Madness",
        [LevelWeatherType.Foggy, LevelWeatherType.Eclipsed, LevelWeatherType.Rainy, LevelWeatherType.Stormy, LevelWeatherType.Flooded],
        0.02f
      ) { }
  }

  internal class ProgressingNoneFoggy : Modules.Types.ProgressingWeatherType
  {
    public ProgressingNoneFoggy()
      : base(
        "None > Foggy",
        LevelWeatherType.None,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.25f,
            Chance = 0.8f,
            Weather = LevelWeatherType.Foggy
          },
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.75f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Foggy
          }
        ]
      ) { }
  }

  internal class ProgressingNoneStormy : Modules.Types.ProgressingWeatherType
  {
    public ProgressingNoneStormy()
      : base(
        "None > Stormy",
        LevelWeatherType.None,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.35f,
            Chance = 0.35f,
            Weather = LevelWeatherType.Stormy
          },
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.75f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Stormy
          }
        ]
      ) { }
  }

  internal class ProgressingEclipsedFoggy : Modules.Types.ProgressingWeatherType
  {
    public ProgressingEclipsedFoggy()
      : base(
        "Eclipsed > Foggy",
        LevelWeatherType.Eclipsed,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.40f,
            Chance = 0.5f,
            Weather = LevelWeatherType.Foggy
          },
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.85f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Foggy
          }
        ]
      ) { }
  }

  internal class ProgressingFoggyNone : Modules.Types.ProgressingWeatherType
  {
    public ProgressingFoggyNone()
      : base(
        "Foggy > None",
        LevelWeatherType.Foggy,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.45f,
            Chance = 1f,
            Weather = LevelWeatherType.None
          }
        ]
      ) { }
  }

  internal class ProgressingHiddenEclipsed : Modules.Types.ProgressingWeatherType
  {
    public ProgressingHiddenEclipsed()
      : base(
        "Eclipsed > None",
        LevelWeatherType.Eclipsed,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.66f,
            Chance = 1f,
            Weather = LevelWeatherType.None
          }
        ]
      ) { }
  }

  internal class ProgressingStormyRainy : Modules.Types.ProgressingWeatherType
  {
    public ProgressingStormyRainy()
      : base(
        "Stormy > Rainy",
        LevelWeatherType.Stormy,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.55f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Rainy
          }
        ]
      ) { }
  }

  internal class ProgressingRainyEclipsed : Modules.Types.ProgressingWeatherType
  {
    public ProgressingRainyEclipsed()
      : base(
        "Rainy > Eclipsed",
        LevelWeatherType.Rainy,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.66f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Eclipsed
          }
        ]
      ) { }
  }

  internal class ProgressingMadness : Modules.Types.ProgressingWeatherType
  {
    public ProgressingMadness()
      : base(
        ">Madness>",
        LevelWeatherType.Eclipsed,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.10f,
            Chance = 0.5f,
            Weather = LevelWeatherType.Flooded
          },
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.40f,
            Chance = 0.5f,
            Weather = LevelWeatherType.Foggy
          },
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.65f,
            Chance = 1f,
            Weather = LevelWeatherType.Stormy
          }
        ],
        weightModify: 0.1f
      ) { }
  }

  internal class ProgressingTesting : Modules.Types.ProgressingWeatherType
  {
    public ProgressingTesting()
      : base(
        "> Testing >",
        LevelWeatherType.Eclipsed,
        [
          new Definitions.Types.ProgressingWeatherEntry
          {
            DayTime = 0.20f,
            Chance = 1f,
            Weather = LevelWeatherType.None
          },
        ],
        weightModify: 500f
      ) { }
  }
}
