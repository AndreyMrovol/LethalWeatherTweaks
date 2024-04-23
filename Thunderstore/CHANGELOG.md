# 0.5.0

- support for game version v50
- support for LLL v1.2.0
- changed Harmony patch priority to work with [Malfunctions](https://thunderstore.io/c/lethal-company/p/zealsprince/Malfunctions/)
- fixed a crash when no uncertain weathers were enabled
- fixed an issue when available weight sum was 0

# 0.4.6

- fixed GI BetterMonitors and FancyWeather

# 0.4.5

- reworked uncertain weather system to be more modular **(please re-adjust your uncertain weathers config, sorry!)**
- added GeneralImprovements compatibility

# 0.4.4

- fixed an issue with moons having no random weathers to choose from (thanks, xuxiaolan)!

# 0.4.3

- fixed an issue with modded planets not having weather

# 0.4.2

- updated LLL dependency to v1.1.4

# 0.4.1

- added `ScaleDownClear` mechanic
- fixed an issue with AlwaysUncertain

# 0.4.0

### Please re-generate your config file!

- added `MaxMultiplier` option
- fixed multiple instances of wrong thing being logged
- MapScreenInfo patch is now applied with highest priority (for compatibility with other mods)
- added `AlwaysClear` mode
- there will be no uncertain weathers first day
- first day will have less weather conditions present
- reordered whole mess of a config file

# 0.3.0

### Please re-generate your config file!

- fixed incorrectly applied multipliers
- added `MaxMultiplier` option

# 0.2.1

- LethalLevelLoader is now a dependency for easier and more compatible terminal patches

# 0.2.0

### Please re-generate your config file!

- fixed an issue with uncertain weathers not syncing between players
- added alwaysUnknown mode: weather is always unknown
- added config option for gameLength difficulty multiplier
- added config option for playersAmount difficulty multiplier

# 0.1.2

- added colored weather text to ship screen
- weathers displayed in terminal should always match UncertainWeather mechanic

# 0.1.1

- fixed difficulty multiplier being applied in reverse proportion

# 0.1.0

### Please re-generate your config file!

- added a new mechanic: uncertain weather
  - shown forecast won't always be 100% accurate
  - there are 3 levels of uncertainty:
    - uncertain (e.g. `Rainy?`)
    - probable (e.g. `Rainy/Flooded`)
    - unknown (e.g. `[UNKNOWN]`)
- added a new mechanic: no weather conditions shown mode
  - toggleable in the config file
  - uses uncertain weather
- added new weather display on ship screen
- changed how weathers are displayed in terminal (to allow uncertainty)
- fixed all planets being eclipsed on first day
- first day seed is now configurable
- added game length difficulty multiplier - the longer game goes, the lesser chance for no weather is
- changed number of moons with no weather at start
- changed all weight defaults

# 0.0.8

- changed how many planets have no weather at start

# 0.0.7

- fixed issues with DustClouds being selected as a weather

# 0.0.6

- changed the defaults in the config file
- changed logging behavior to be more readable
- fixed and issue when hosting a game

# 0.0.5

- added a config file with weighted system

# 0.0.4

- current weather is now fully synchronized with host
- split code into more readable functions
- display pretty tables in console
