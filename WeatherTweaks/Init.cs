using WeatherTweaks;

namespace WeatherTweaks
{
  public class Init
  {
    public static void InitMethod()
    {
      new CombinedEclipsedFlooded();
      new CombinedFoggyRainy();
      new CombinedStormyFlooded();
      new CombinedStormyRainy();
      new CombinedEclipsedRainy();
      new CombinedFoggyFlooded();
      new CombinedFoggyEclipsed();
      new CombinedStormyRainyEclipsed();
      new CombinedStormyRainyFlooded();
      new CombinedStormyRainyFloodedEclipsed();

      new ProgressingNoneFoggy();
      new ProgressingNoneStormy();
      new ProgressingEclipsedFoggy();
      new ProgressingFoggyNone();
      new ProgressingHiddenEclipsed();
      new ProgressingStormyRainy();
      new ProgressingRainyEclipsed();
    }
  }
}
