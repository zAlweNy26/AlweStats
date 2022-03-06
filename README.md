# AlweStats

You can find it here : https://valheim.thunderstore.io/package/Padank/AlweStats/

Valheim Mod to easily see "FPS", "Ping", "Total players", "Clock", "Ship health", "Ship speed", "Wind speed", 
"Wind direction", "Days passed", "Time played", "Current Biome", "Bow ammo", "Selected arrows" in the HUD.

You can also see : 
- Growth time/percentage for bush and/or plants
- Health value/percentage for every environment element (rocks, trees, ecc...) and construction pieces
- Process time/percentage for fermenters and beehives
- Fill space/percentage for containers

The UI is separated in five blocks :
- GameStats, that contains "FPS", "Ping" and "Total players"
- WorldStats, that contains "Days passed", "Time played" and "Current biome"
- WorldClock, that contains "Clock"
- ShipStats, that contains "Ship health", "Ship speed", "Wind speed", "Wind direction"
- BowStats, that contains "Selected arrows" and "Bow ammo"

For each block you can enable or disable it and you can also set :
- Position
- Margin 
- Text color
- Text size 
- Text alignment (nor for WorldClock nor for BowStats)

For the "BowStats" section, you can set :
- Whether or not to show a bow charge bar instead of the vanilla circle that shrinks 
- The color of the bow charge bar when fully charged

For the "EnvStats" section, you can set :
- The string format for the health of any environment element
- The string format for the process status of bushes, plants, beehives and fermenters
- Whether or not to show the health separately for rock and trees
- Whether or not to show the growth separately for bushes and plants
- Whether or not to show the process status for beehives and fermenters
- Whether or not to show the remaining slots for containers

In the config file you can also choose whether or not to show :
- A "Days passed" counter in the world selection panel
- A reset button in the pause menu to reset the positions of all four blocks with their default values
- The clock in 12h or 24h format
- The current biome in the WorldStats block instead of the top-left corner in minimap 

**If you have an idea for smoething to add or any suggestion regarding the implemented features, feel free to create an issue on the GitHub repository !**

> You can move the blocks in game with your mouse ! 
> To enable the editing mode you just have to click the key chose in the config file (default: F9)
> and move the blocks where you prefer ! To save your changes, you need to press again the key.
> You can also reset all the positions and margins of the blocks with the button in the pause menu (if enabled).

> Since the default day length declared in Valheim doesn't match the real one,
> to successfully use the "Days passed" counter in the world selection panel you need to join in a world at least one time, 
> The mod will then get the day length in that world and save it in the "Alwe.stats" file (you have to do it for each world).

> As I don't know the proportions that the game uses, 
> I have established that the maximum **ship speed** is equal to **30 kts** and the maximum **wind speed** is equal to **100 km/h**,
> so the values ​​are proportioned according to this.

### Known bugs

- Health starts showing from second hit for rocks and minerocks.
- Environment element name doesn't show if you hit an element without aiming at it.
- Name doesn't show for small environment elements.
- Remaining time doesn't work properly for bushes and beehives.

### Changelog

**3.2.2**
- Fixed an error showed when you had no arrows in the inventory
- Added total players string in GameStats block when there is more than 1
- Overall adjustments

**3.2.1**
- Fixed a bug that kept showing the ShipStats block even after the ship was demolished
- Adjusted the text displayed when having the max honey in a beehive

**3.2.0**
- Fixed bug that didn't let show the ShipStats block
- Now percentages are decimal values with eventually one decimal digit
- Added status for construction pieces when hovering it
- Adde a new config value to match the new addition

**3.1.0**
- Renamed the config value "GrowStringFormat" to "ProcessStringFormat"
- Splitted the config value "BushAndPlantStatus" in "BushStatus" and "PlantStatus" to separate bushes and plants
- Added status about fermenter when hovering it
- Added status about beehive when hovering it
- Added status about container when hovering it
- Added the possibility to see remaining time in the "ProcessStringFormat" config value
- Added new config values to match the new additions

**3.0.0**
- Added bushes and plants grow string in the EnvStats section
- Fixed all values that could be rounded to integer when having 0 as digit after the decimal point
- Added a new config value to match the new addition ("GrowStringFormat")
- Renamed the config value "StringFormat" to "HealthStringFormat"
- Renamed the config value "DaysInWorldList" to "DaysInWorldsList"
- Renamed the config value "BushStatus" to "BushAndPlantStatus"
- Now you will see the growth for bushes and not the health
- Added the possibility to color the text based on the health/grow percentage in "HealthStringFormat" and "GrowStringFormat"

**2.6.0**
- Added a new block regarding the bow ammo and selected arrows
- Added the possibility to show a bow charge bar instead of the vanilla one
- Added new config values to match the new additions

**2.5.0**
- Added an health string for every environment element (rocks, minerocks, trees, bushes, ecc...)
- Added new config values to match the new additions
- Added a key to reload the plugin's config file while in game

**2.4.2**
- Fixed the ship speed that was showing 0 when revesing

**2.4.1**
- Fixed the check if the player in on a ship or not
- Fixed the ship speed with more credible values (in order to better respect the true speeds of Viking ships)

**2.4.0**
- Fixed error when exiting server
- Added possibility to remove biome from minimap top-left corner and set it in the WorldStats block

**2.3.0**
- Optimized the editing mode
- Now you don't need to close and open again the game to save your changes
- Optimized all the blocks for better readability

**2.2.0**
- Added "Ship speed" in the ShipStats block
- Optimized the check if the player is on board or not (ShipStats) 

**2.1.1**
- Added the ShipStats block in the editing mode
- Overall adjustments

**2.1.0**
- Added the ShipStats block, to see ship health, wind speed and wind direction
- Added new config values to match the new additions

**2.0.1**
- Overall fixes and optimization

**2.0.0**
- Optimized the code and made it cleaner to read
- Added an editing mode
- Added new config values to match the new additions

**1.2.0**
- Fixed "Text color", "Position" and "Margin" values to be culture invariant (so now you can set decimal values with the point)
- Added a customizable in-game clock, separated from WorldStats

**1.1.1**
- Commented out the "OnDestroy" method (it is for development only)
- Removed commented code

**1.1.0**
- Added a "Days passed" string in the world selection panel
- Added the chance to disable it in the config file

**1.0.0**
- Added GameStats, that contains "FPS" and "Ping"
- Added WorldStats, that contains "Days passed" and "Time played"