# WeatherTweaks

Simple mod that adds a few tweaks to the weather system.

Currently, vanilla weather system is completely random - it means that weather condition can exists for few days in a row, which makes it _not very fun_ to play - this mod aims to fix that.

i'll make a proper documentation, i swear

## Features

- More advanced weather picking system
- Fully synchronized weather between host and clients
- Config file with weighted system
- Fully configurable
- Weathers on a new save try to mimic default vanilla behavior, which didn't happen with any modded moons present
- Uncertain weather mechanic: shown forecast won't always be 100% accurate
- (optional) No certain weather conditions shown mode
- (optional) Always unknown weather mode

## Conditions

Weather calculation operates based on weights defined in the config file.

## Building

This project uses [LethalLevelLoader](https://github.com/IAmBatby/LethalLevelLoader), which needs to be imported manually into the project's `lib` directory.

## Credits

This project uses [LethalCompanyTemplate](https://github.com/LethalCompany/LethalCompanyTemplate), licensed under [MIT License](https://github.com/LethalCompany/LethalCompanyTemplate/blob/main/LICENSE).

This project uses [ConsoleTables](https://github.com/khalidabuhakmeh/ConsoleTables), licensed under [MIT License](https://github.com/khalidabuhakmeh/ConsoleTables/blob/main/LICENSE).

This project uses Xilophor's [LethalNetworkAPI](https://github.com/Xilophor/LethalNetworkAPI).

This project uses IAmBatby's [LethalLevelLoader](https://github.com/IAmBatby/LethalLevelLoader).

Massive thanks to Electric131 and Easyidle123 for their feedback and support.
