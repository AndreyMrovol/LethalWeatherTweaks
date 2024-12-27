# 0.24.7

- moved `TextPostProcess` patch to `WeatherRegistry`

# 0.24.6

- fixed an issue where WeatherTweaks would override WeatherRegistry's configured scrap multipliers (thanks, `crutled`!)
- changed Blackout's display color to be more visible

# 0.24.5

- fixed an issue where _something_ tried to access game's weather too early (thanks, `moroxide`!)
- added logs for scrap multipliers going off the rails (cause i still don't know what's happening)

# 0.24.4

- added `CombinedStormyRainyFloodedEclipsed` weather
- removed `ProgressingMadness` weather
- fixed an issue where WeatherTweaks would override WeatherRegistry's vanilla algorithm

# 0.24.2

- changes to Blackout:
  - fixed errors during game startup
  - apparatus lights won't be disabled anymore
  - fixed breaker box being non-functional and throwing errors
  - reduced the range of floodlights
- (hopefully) fixed all issues with progressing weathers changing during the landing

# 0.24.1

- added ||Blackout|| weather
  - ||disables all lights on the planet and in the dungeon||
  - ||makes the floodlight work like real ones||

# 0.24.0

## Please re-generate your configs!

- changed config entry names
- added an option to generate all hidden config entries for special weathers (thanks, `dragonmcmx`!)
- updated how picked weathers are displayed
- re-introduced foggy patch ||with a twist||

# 0.23.2

- made sure WeatherTweaks weathers are registered before Registry initialization
- changed configs: `WeatherTweaksWeather` uses `DefaultWeight` instead of `WeightModify`
- changed some logs

# 0.23.1

- fixed an issue where special weathers weren't properly registered

# 0.23.0

- reworked the whole thing

<details>
  <summary><b> Changelog from earlier "Beta" version</summary>

# 0.22.0

- removed foggy patch
- removed LGU's probe integration

# 0.21.3

- fully updated LethalNetworkAPI to v3
- fixed an issue with the game crashing on lobby reloads (thank you, xilophor!)
- added `FoggyIgnoreLevels` config option: block foggy patch from applying on defined levels
- publicized `Variables.GetCurrentWeather` (thanks, loaforc)
- reimplemented combined and progressing weather multipliers

# 0.21.2

- hopefully finally fully fixed lobby reload errors

# 0.21.1

- fixed issues related to LethalNetworkAPI v3 update

# 0.21.0

- fixed the issue with combined weather effects not applying correctly (thanks: lunxara, instaplayer)
- removed some leftover files
- finally unified Combined and Progressing weathers methods to not be separate and shit

# 0.20.8

- removed all weight-related config entries, as they are now handled by WeatherRegistry
- added new config entries: `LogWeatherSelection` and `LogWeatherVariables`
- added first-day algorithm options: `FirstDaySpecial` (a toggle for current first-day algorithm) and `FirstDayRandomSeed` (for randomizing the first day seed)
- moved bunch of stuff to use MrovLib
- added full compatibility with v55

# 0.20.7

- fixed issues with MrovLib update

# 0.20.6

- (hopefully) fixed an issue with ChangeMidDay allocating absurd amounts of memory every TimeOfDay update (thanks, diffoz)
- added Cloudy weather
- moved even more things into WeatherRegistry

# 0.20.5

- fixed an issue with weighted weather list being empty (thanks, b1adewo1f)

# 0.20.4

- fixed an issue with special weathers not being disabled (thanks: mari0no1, finembelli, Lunxara)

# 0.20.3

- fixed an issue with the game adding progressing weathers to wrong moons
- updated logs
- removed unused config entries

# 0.20.2

- fixed weathers not using default WeatherRegistry values
- updated logs

# 0.20.1

- (hopefully) fixed EntranceTeleport errors
- changed logging level of some debug messages
- added LobbyCompatibility support

# 0.20.0

- Released WeatherRegistry: a new foundation library for managing all weather-related things in the game
- switched to WeatherRegistry for all weather-related things - Weather type, Effect type and more
- removed SunAnimator patches
- removed MapScreen patch
- changed Combined/Progressing weathers registration
- changed Foggy weather applying correctly with progressing weathers

# 0.14.11

- fixed LethalLib weathers causing dictionary errors

# 0.14.10

- fixed "all eclipsed" issue ([#21](https://github.com/AndreyMrovol/LethalWeatherTweaks/issues/21)) (thanks, 1410677474)

# 0.14.9

- fixed an issue with sunAnimator disabling eclipse-related animations not tied to sunAnimator itself

# 0.14.8

- disabled debug thingies (thanks, littlemssara)

# 0.14.7

- fixed the issues with missing config entries

# 0.14.6

- fixed the SunAnimator issues on [Ooblterra](https://thunderstore.io/c/lethal-company/p/Skeleton_Studios/Welcome_To_Ooblterra/) (thanks, [SkullCrusher](https://github.com/Skull220))

# 0.14.5

- fixed fog settings not being applied consistently

# 0.14.4

- fixed compatibility patch with GeneralImprovements (circular dependency)

# 0.14.3

- started doing changelog
- fixed an issue where the game would softlock if any levels had no defined randomWeathers

</details>
