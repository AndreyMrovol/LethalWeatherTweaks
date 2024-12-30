namespace WeatherTweaks
{
  internal class CombinedEclipsedFlooded : Definitions.CombinedWeatherType
  {
    public CombinedEclipsedFlooded()
      : base("Eclipsed + Flooded", [LevelWeatherType.Eclipsed, LevelWeatherType.Flooded]) { }
  }

  internal class CombinedFoggyRainy : Definitions.CombinedWeatherType
  {
    public CombinedFoggyRainy()
      : base("Foggy + Rainy", [LevelWeatherType.Foggy, LevelWeatherType.Rainy]) { }
  }

  internal class CombinedEclipsedRainy : Definitions.CombinedWeatherType
  {
    public CombinedEclipsedRainy()
      : base("Eclipsed + Rainy", [LevelWeatherType.Eclipsed, LevelWeatherType.Rainy]) { }
  }

  internal class CombinedStormyRainy : Definitions.CombinedWeatherType
  {
    public CombinedStormyRainy()
      : base("Stormy + Rainy", [LevelWeatherType.Stormy, LevelWeatherType.Rainy]) { }
  }

  internal class CombinedStormyFlooded : Definitions.CombinedWeatherType
  {
    public CombinedStormyFlooded()
      : base("Stormy + Flooded", [LevelWeatherType.Stormy, LevelWeatherType.Flooded]) { }
  }

  internal class CombinedFoggyFlooded : Definitions.CombinedWeatherType
  {
    public CombinedFoggyFlooded()
      : base("Foggy + Flooded", [LevelWeatherType.Foggy, LevelWeatherType.Flooded]) { }
  }

  internal class CombinedFoggyEclipsed : Definitions.CombinedWeatherType
  {
    public CombinedFoggyEclipsed()
      : base("Foggy + Eclipsed", [LevelWeatherType.Foggy, LevelWeatherType.Eclipsed]) { }
  }

  internal class CombinedStormyRainyEclipsed : Definitions.CombinedWeatherType
  {
    public CombinedStormyRainyEclipsed()
      : base("Stormy + Rainy + Eclipsed", [LevelWeatherType.Stormy, LevelWeatherType.Rainy, LevelWeatherType.Eclipsed]) { }
  }

  internal class CombinedStormyRainyFlooded : Definitions.CombinedWeatherType
  {
    public CombinedStormyRainyFlooded()
      : base("Stormy + Rainy + Flooded", [LevelWeatherType.Stormy, LevelWeatherType.Rainy, LevelWeatherType.Flooded]) { }
  }

  internal class CombinedStormyRainyFloodedEclipsed : Definitions.CombinedWeatherType
  {
    public CombinedStormyRainyFloodedEclipsed()
      : base(
        "Stormy + Rainy + Flooded + Eclipsed",
        [LevelWeatherType.Stormy, LevelWeatherType.Rainy, LevelWeatherType.Flooded, LevelWeatherType.Eclipsed]
      ) { }
  }

  // internal class SuperFoggy : Definitions.CombinedWeatherType
  // {
  //   public SuperFoggy()
  //     : base("Super Foggy", [LevelWeatherType.Foggy, LevelWeatherType.DustClouds], 0.3f) { }
  // }

  // internal class CombinedMadness : Definitions.CombinedWeatherType
  // {
  //   public CombinedMadness()
  //     : base(
  //       "Madness",
  //       [LevelWeatherType.Foggy, LevelWeatherType.Eclipsed, LevelWeatherType.Rainy, LevelWeatherType.Stormy, LevelWeatherType.Flooded],
  //       0.02f
  //     ) { }
  // }

  internal class ProgressingNoneFoggy : Definitions.ProgressingWeatherType
  {
    public ProgressingNoneFoggy()
      : base(
        "None > Foggy",
        LevelWeatherType.None,
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.25f,
            Chance = 0.8f,
            Weather = LevelWeatherType.Foggy
          },
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.75f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Foggy
          }
        ]
      ) { }
  }

  internal class ProgressingNoneStormy : Definitions.ProgressingWeatherType
  {
    public ProgressingNoneStormy()
      : base(
        "None > Stormy",
        LevelWeatherType.None,
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.35f,
            Chance = 0.35f,
            Weather = LevelWeatherType.Stormy
          },
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.75f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Stormy
          }
        ]
      ) { }
  }

  internal class ProgressingEclipsedFoggy : Definitions.ProgressingWeatherType
  {
    public ProgressingEclipsedFoggy()
      : base(
        "Eclipsed > Foggy",
        LevelWeatherType.Eclipsed,
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.40f,
            Chance = 0.5f,
            Weather = LevelWeatherType.Foggy
          },
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.85f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Foggy
          }
        ]
      ) { }
  }

  internal class ProgressingFoggyNone : Definitions.ProgressingWeatherType
  {
    public ProgressingFoggyNone()
      : base(
        "Foggy > None",
        LevelWeatherType.Foggy,
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.45f,
            Chance = 1f,
            Weather = LevelWeatherType.None
          }
        ]
      ) { }
  }

  internal class ProgressingHiddenEclipsed : Definitions.ProgressingWeatherType
  {
    public ProgressingHiddenEclipsed()
      : base(
        "Eclipsed > None",
        LevelWeatherType.Eclipsed,
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.66f,
            Chance = 1f,
            Weather = LevelWeatherType.None
          }
        ]
      ) { }
  }

  internal class ProgressingStormyRainy : Definitions.ProgressingWeatherType
  {
    public ProgressingStormyRainy()
      : base(
        "Stormy > Rainy",
        LevelWeatherType.Stormy,
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.55f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Rainy
          }
        ]
      ) { }
  }

  internal class ProgressingRainyEclipsed : Definitions.ProgressingWeatherType
  {
    public ProgressingRainyEclipsed()
      : base(
        "Rainy > Eclipsed",
        LevelWeatherType.Rainy,
        [
          new Definitions.ProgressingWeatherEntry
          {
            DayTime = 0.66f,
            Chance = 1.0f,
            Weather = LevelWeatherType.Eclipsed
          }
        ]
      ) { }
  }

  // internal class ProgressingTesting : Definitions.ProgressingWeatherType
  // {
  //   public ProgressingTesting()
  //     : base(
  //       "> Testing >",
  //       LevelWeatherType.Eclipsed,
  //       [
  //         new Definitions.ProgressingWeatherEntry
  //         {
  //           DayTime = 0.20f,
  //           Chance = 1f,
  //           Weather = LevelWeatherType.None
  //         },
  //       ],
  //       weightModifier: 500f
  //     ) { }
  // }
}
