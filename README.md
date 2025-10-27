# WeatherTweaks

Simple mod that adds a few difficulty tweaks to the weather system.

Currently, vanilla weather system is completely random - it means that weather condition can exists for few days in a row, which makes it _not fun_ to play - this mod, in combination with [WeatherRegistry](https://github.com/AndreyMrovol/LethalWeatherRegistry) attempts to fix that and make it more engaging in the process.

## Features

- The first set of Weathers on a new save try to mimic default vanilla behavior, which didn't happen with any modded moons present
- Combined weathers: The ability for multiple weather conditions to be present at the same time
- Progressing weathers: The ability for weather conditions to change during the day
- (Optional) Uncertain weather mechanics: Shown forecast won't always be 100% accurate
  - `[UNKNOWN]` which hides the weather condition present.
  - `Weather?` which has a 67% of being the displayed weather and a 33% chance of being any another
  - `Weather1/Weather2` or `Combined + Weather1/Weather2`: A 50/50 chance for either weather or combination
  - Disable `UncertainWeatherEnabled` in the config to disable all.
- (Optional) Difficulty scaling with player amount and quotas completed
  - The multipliers for each player and quota are added onto each other and then multiplied with all weathers but `None`
  - You can also set a cap (default 0.8)
  - Example: 2 players (each 0.01) and 3 quotas (each 0.05) would result in a 1.17 multiplier. It can not go above 1.8.
  - Set the multiplier cap to 0 to disable all.
- (Optional) No certain weather conditions shown mode
- (Optional) Always unknown weather mode

> ℹ️ If you intend changing the WeatherToWeather/Moon Weather weights for combined and progressive weather in WeatherRegistry, this is the format: `Combined+Weather@0;Progressive>Weather@0`

## Conditions

Weather calculation operates based on weights defined in the config file for [WeatherRegistry](https://github.com/AndreyMrovol/LethalWeatherRegistry).

## License

This project is licensed under [CC BY-NC-ND 4.0](https://github.com/AndreyMrovol/LethalWeatherTweaks/blob/main/LICENSE.md) license.

## Report issues

If you encounter any issues, please report them on the [GitHub issues page](https://github.com/AndreyMrovol/LethalWeatherTweaks/issues) or in the [discord thread](https://discord.com/channels/1168655651455639582/1203871322841808906).

## Credits

This project uses [LethalCompanyTemplate](https://github.com/LethalCompany/LethalCompanyTemplate), licensed under [MIT License](https://github.com/LethalCompany/LethalCompanyTemplate/blob/main/LICENSE).

This project uses [ConsoleTables](https://github.com/khalidabuhakmeh/ConsoleTables), licensed under [MIT License](https://github.com/khalidabuhakmeh/ConsoleTables/blob/main/LICENSE).

This project uses Xilophor's [LethalNetworkAPI](https://github.com/Xilophor/LethalNetworkAPI).

This project uses mrov's [WeatherRegistry](https://thunderstore.io/c/lethal-company/p/mrov/WeatherRegistry/).

Massive thanks to Electric131, Easyidle123, anon, Clark and whole [TheMostLethalCompany Discord](https://discord.gg/themostlethalcompany) for their [feedback, testing and support](https://discord.com/channels/1180619962751144050/1201565358318956665) on the initial versions of this mod.

Thanks to everyone using my mod, reporting bugs and suggesting new features - without you I wouldn't be able to make this mod as good as it is now.
