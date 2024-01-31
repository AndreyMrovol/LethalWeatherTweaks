# WeatherTweaks

Simple mod that adds a few tweaks to the weather system.

Currently, vanilla weather system is completely random - it means that weather condition can exists for few days in a row, which makes it _not very fun_ to play - this mod aims to fix that.

## Features

- Weather condition cannot repeat
- Eclipse cannot happen more than once in a row
- Weathers on a new save try to mimic default vanilla behavior, which didn't happen with any modded moons present

## Conditions

Weather calculation operates with parameters:

- If weather was clear:
  - 50% chance for weather condition next day
- If weather was not clear:
  - 55% chance for weather to be clear next day
  - 45% chance for weather condition next day
    - **weather condition cannot repeat**
  - if there was an eclipse:
    - 85% chance for no weather next day
    - 15% chance for weather condition next day
      - as before, weather cannot be eclipsed again

## To-do

- config file
- ability to work with vanilla clients
- additional parameters:
  - chance for eclipse based on game's length
  - crew daysInRow

## Credits

This project uses [LethalCompanyTemplate](https://github.com/LethalCompany/LethalCompanyTemplate), licensed under [MIT License](https://github.com/LethalCompany/LethalCompanyTemplate/blob/main/LICENSE).

This project uses [ConsoleTables](https://github.com/khalidabuhakmeh/ConsoleTables), licensed under [MIT License](https://github.com/khalidabuhakmeh/ConsoleTables/blob/main/LICENSE).

This project uses [Xilophor's LethalNetworkAPI](https://github.com/Xilophor/LethalNetworkAPI).
