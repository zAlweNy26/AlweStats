# AlweStats

You can find it on [ThunderStore](https://valheim.thunderstore.io/package/Padank/AlweStats/)
and [Nexus](https://www.nexusmods.com/valheim/mods/1822/).

**Give a thumbs up if you like the mod and you have a GitHub account !**

**If you have an idea for something to add or any suggestion regarding the implemented features,
feel free to create an issue on the GitHub repository !**

**You may encounter incompatibilities in case there are other mods that modify the UI,
be sure to enable either one or the other equivalent section so they don't conflict in the same function**

**The mod is compatible with "randyknapp's Minimal Status Effects", "randyknapp's Equipment and Quick Slots",
"aedenthorn's Extended Player Inventory" and "marlthon's OdinShip" mods.**

> For reasons inherent to the optimal functioning of the mod,
> everything related to the pins and elements in the status effects list (like BedStatus, PortalStatus and ShipStatus, except for WeightStatus)
> could cause a lowering of fps if the computer running the game is not very powerful.
>  
> Since the default day length declared in Valheim doesn't match the real one,
> to successfully use the "Days passed" counter in the world selection panel you need to join in a world at least one time,
> The mod will then get the day length in that world and save it in the "AlweStats.json" file (you have to do it for each world).

The UI is separated into these blocks :

- GameStats, which contains "FPS"
- WorldStats, which contains "Days passed", "Time played", "Current biome", "Weather" and "Seed"
- WorldClock, which contains "Clock" (game clock)
- SystemClock, which contains "Clock" (real-life clock)
- ServerStats, which contains "Ping", "Total players" and a list of players in range with their health percentage
- ShipStats, which contains "Ship health", "Ship speed", "Wind speed", "Wind direction"
- PlayerStats, which contains "Inventory slots", "Inventory weight", "Selected arrows" and "Bow ammo"
- MapStats, which contains "Player coordinates" and "Focus coordinates"

For each block, you can enable or disable it and you can also set :

- Position
- Margin
- Text color
- Text size (also for the cursor coordinates and the explored percentage)
- Text alignment (not for WorldClock and SystemClock)

Each block is also movable and resizable with your mouse while in-game thanks to the integrated editing mode (default key : F8).

You can reload the configuration file (default key : F9) while in-game to see changes.

There are also "EntityStats" and "EnvStats" which aren't UI blocks but can let you choose different things.

In the config file you can also :

- Change key for plugin reload and editing mode
- Change the color of the health bar for entities and tamed creatures
- Change the height of the health bar for entities
- Choose the clock format (12h or 24h)
- Replace the default bed pin icon with the icon of the bed as a building piece
- Set the background color for all the blocks
- Set padding for all the blocks
- Set the string format for the health of environment elements, construction pieces and entities
- Set the string format for the process status of bushes, plants, fireplaces, beehives and fermenters
- Set the string format for the cursor coordinates string
- Set the string format for the ShipStats, MapStats, WorldStats, ServerStats and GameStats blocks
- Set the color of the crosshair and the custom bow charge bar
- Set the scale of the player marker and the crosshair
- Toggle each section (blocks and not)
- Toggle a "Days" counter in the world selection panel
- Toggle the player statistics in the character selection
- Toggle a reset button in the pause menu to reset the positions of all the blocks with their default values
- Toggle the current biome text in the top-right corner in the minimap
- Toggle a weight fill percentage as an element in the status effects list
- Toggle the hover status separately for rock, trees, bushes, plants, beehives, fireplaces, fermenters, containers, cooking stations and smelters
- Toggle the cursor coordinates and the explored percentage in the large map
- Toggle the rotation of the minimap that follows the player camera rotation
- Toggle the distance and direction from claimed bed, closer portal and closer ship shown as elements in the status effects list
- Toggle the custom bow charge bar instead of the vanilla circle that shrinks
- Toggle pins for ships, dungeons (troll caves, mountain caves, crypts, fire holes and infested mines), carts and portals
- Toggle the title of the custom pins
- Toggle the distance between you and the ping a player does on the map

In the console or chat, now you can use these commands :

```console
> alwe (or /alwe in chat)
List of valid subcommands :
reload - Reload the configuration file to update changes in-game
cfp [world] - Remove all the pins from the AlweStats.json file for a specific world
df - Clear the entire AlweStats.json file by deleting it
```

## Things I want to do as soon as possible

- Add detailed items informations
- Add a compass
- Add a custom minimap
- Add compatibility with Project Auga
- Fix the known issues

### Known issues

- Health starts showing from the second hit for rocks and mine rocks
- Name doesn't show for small environment elements
- Remaining time doesn't work properly for beehives
- Pregnancy percentage doesn't work properly
- Config reload doesn't work properly

### Changelog

v**5.1.1**

- Fixed error when dealing with health bars of players
- Fixed bug that wasn't showing ServerStats block if you weren't the host
- Fixed error showing while connecting to a server
- Updated custom pins titles

v**5.1.0**

- Updated commands to be less error-prone
- Now custom pins are removed from the map save data on game exit
- Fixed error with smelter hover text
- Added remaining time and percentage for bathtubs
- Removed middle bar for mobs
- Added config setting "HealthBarHeight" to change the height of all the health bars
- Added config setting "HealthBarColor" to change the color of all the health bars
- Replaced seed number with seed name in WorldStats block

v**5.0.0**

- **Added new ServerStats block**
- Moved ping and total players counter from GameStats to ServerStats block
- Added dynamic list of players in range with relative stats (health, max health, percentage of health)
- Added config setting "GameStatsFormat" to format text of GameStats block
- Added config setting "ServerStatsFormat" to format text of ServerStats block
- Added config setting "WorldStatsFormat" to format text of WorldStats block
- Added config setting "RangeForPlayers" to change the range in which to scan for players
- Added weather in the "WorldStatsFormat" config setting
- Integrated the "ShowWorldSeed" config setting in the "WorldStatsFormat" one
- Integrated the "CustomShowBiome" config setting in the "WorldStatsFormat" one
- Added config setting "RemoveMinimapBiome" to remove the current biome label in the top-left corner in minimap
- Added pregnancy percentage when hovering tameable animals
- Added support for aedenthorn's "Extended Player Inventory" in the PlayerStats block
- Fixed distance text, now it shows only on the hovered entity when ShowEntityDistance is set to 1

v**4.5.0**

- **Now compatible with the Mistlands update**
- Updated game version reference to 0.212.9
- Added config setting "ShowWorldSeed" to show the world seed in the WorldStats Block
- Added map pins for the new mistlands' infested mines

v**4.4.1**

- Removed process filter to work in dedicated servers
- Fixed the bug that was reloading config file instead of saving it on world shutdown
- Set label name for those runes which have it

v**4.4.0**

- Moved "AlweStats.json" path from "plugins" folder to "config" folder
- Moved the player infos in character selection toggle in a new config setting called "PlayerInfos"
- Added y value in the cursor coordinates showed in the large map (set the setting to default to see the change)
- Added a console/chat subcommand "reload" to reload the AlweStats configuration file
- Fixed a bug that was showing an error message related to the smelter
- Added a config setting "ShowTotalOfQueue" to show the total remaining time for the entire queue or for a single item
- Added the runestones to the custom pins (you have to interact with it to add the pin)
- Fixed a bug that was causing an error if not all the custom status effects were enabled with "MinimalStatusEffects" mod enabled
- Integrated dependencies "System.Data.dll" and "System.Runtime.Serialization.dll" (so now you can delete those two files)

v**4.3.1**

- Changed configuration file name from "AlweStats.cfg" to "Alwe.AlweStats.cfg"
- Added compatibility with "MinimalStatusEffects" mod
- Added compatibility with "OdinShip" mod
- Added cooking stations and smelters status strings in the EnvStats section
- Fixed bug that was causing the minimap pins and player marker to disappear when the rotation was enabled
- Fixed ship speed using <https://valheim.fandom.com/wiki/Boats>
- Updated game version reference to 0.209.10
- Moved save type string up above the character name (to avoid overlapping in version 0.209.10)
- Moved a little to the right the "Days" counter in the worlds list (to avoid overlapping in version 0.209.10)
- Impossibility to use the "Days" counter in the world selection panel in version 0.209.10 if the world is in the steam cloud

v**4.3.0**

- Added a new block : "System Clock" that displays the real-life clock time
- Added compatibility with "MarketplaceAndServerNPCs" mod (in theory, let me know if it doesn't works)
- Added possibility to resize blocks while in-game by using CTRL + mouse wheel

v**4.2.3**

- Now the ShipStatus disappear when you are on a boat
- Added compatibility with "TargetPortal" mod, to make it works fine you have to disable portal pins in the "ShowCustomPins" config setting

v**4.2.2**

- Fixed error generated when adding dungeons pins
- Fixed a bug that wasn't saving world infos in the AlweStats.json file when it was the first time
- Added more conditions to check if game objects are valid or not
- Added more conditions before adding a pin
- Added compatibility to other mods that add custom pins
- Changed the pin icon for sunken crypts to draugr trophy image

v**4.2.1**

- Added config setting to scale the size of the crosshair
- Now you have to be closer to a dungeon before the pin is added
- Added compatibility to other mods that add custom pins

v**4.2.0**

- Fixed error shown when getting grow time of lots of surrounding plants
- Fixed bug that wasn't hiding the charging circle when the custom bow charge bar was enabled
- Added support for randyknapp's "Equipment and quick slots" in the PlayerStats block
- Added black outline to the direction arrow for Bed, Portal and Ship status
- Added config setting to change text of the ShipStats block
- Added config setting to change the color of the crosshair

v**4.1.0**

- Merged BowStats with PlayerStats to a unique block that displays inventory slots, weight, bow ammo and select arrows
- Added weight fill percentage as an element in the status effect list
- Fixed bug that wasn't correctly sizing vanilla pins when there was "0" in the "BiggerPins" config setting
- Fixed bug that wasn't correctly showing EnvStatus for each environment element

v**4.0.0**

- Replaced the Alwe.stats file with AlweStats.json to be able to save custom pins
- Fixed ShipStatus that wasn't correctly being positioned when PortalStatus wasn't active
- Added mountain caves to the custom pins
- Now you can rename, check/uncheck and remove custom pins
- Added a console/chat command named "alwe" to execute subcommands
- Added a console/chat subcommands "cfp" to remove all pins saved in the AlweStats.json file for a specific world
- Added a console/chat subcommands "df" to delete the AlweStats.json file

v**3.8.1**

- Fixed positions of block templates when dragging with a mouse
- Fixed a bug that was spamming how many pins it loaded
- Added config setting to show the distance between you and the ping a player does on the map

v**3.8.0**

- **Reduced the load of everything related to MapStats (pin, status, etc...) to maximize the performance**
- Grouped all the config settings for the status in EnvStats in one config setting (ShowEnvStatus)
- Renamed the config value "EntityDistance" to "ShowEntityDistance"
- Now with the config value "ShowEntityDistance" you can choose to show the distance with all the entities or just the one you are hovering
- Fixed bug that was showing incorrect pin title for fire holes pins
- Fixed bug that wasn't correctly rotating the wind and player markers in the minimap
- Fixed world days text in world selection panel not showing when selecting world with arrow keys
- Added config setting to replace the default bed pin icon with the icon of the bed as a building piece
- Renamed the config value "ShowCursorCoordinatesInMap" to "ShowCursorCoordinates"

v**3.7.0**

- Updated game version reference to 0.207.20
- You can change the color of the health bar for tamed animals
- Added player stats when choosing the character (kills, deaths, crafts, builds)
- Fixed world days text in world selection panel not showing when changing tab
- Fixed position of closer portal element showing too much spaced from others status effects when a bed wasn't claimed
- Added direction and distance from closer ship as an element in the status effect list
- Now the distance calculation for closer portal, closer ship and claimed bed ignores the height
- You can see the distance between you and the entity you are hovering, below the health bar
- Toggle pins for portals, ships, carts and dungeons (troll caves, crypts and fire holes)
- Toggle the title for the custom pins
- You can set the scale of the player marker icon
- You can choose to double or not the size of the custom pins
- Added new config values to match the new additions

v**3.6.0**

- Added direction and distance from closer portal as elements in the status effect list
- Added direction and distance from the claimed bed as elements in the status effect list
- Fixed the percentage color
- Centered the mouse position when moving block while in editing mode
- Added new config values to match the new additions

v**3.5.0**

- Added the possibility to enable a rotating minimap that follows the player camera rotation
- Added remaining time and percentage for fireplaces
- Added explored percentage in the top-left corner on the large map
- Fixed remaining time for bushes
- Reduced the text length for the WorldStats block
- Added new config values to match the new additions
- Added config setting for the font size of the cursor coordinates and explored percentage text in the large map

v**3.4.0**

- Added a new config value regarding the padding for all the blocks
- Fixed the flicker when the value of blocks change
- Now the environment element name will be shown even if you hit an element without aiming at it
- Now the health bar color of construction pieces is based on the health percentage
- Moved the position of the health text and bar for construction pieces
- Added outline to the text of all the blocks
- Now you can reload the config file also in the main menu
- Added a string in the MapStats block that shows the world coordinates of where the crosshair is pointing at
- Added a new config value regarding the text alignment for the MapStats block
- Removed the "ShowPlayerCoordinates" config value
- Renamed the config value "ShowCursorCoordinates" to "ShowCursorCoordinatesInMap"
- Renamed the config value "HealthStringFormat" to "HealthFormat"
- Renamed the config value "ProcessStringFormat" to "ProcessFormat"
- Renamed the config value "PlayerCoordinatesStringFormat" to "WorldCoordinatesFormat"
- Renamed the config value "CursorCoordinatesStringFormat" to "MapCoordinatesFormat"

v**3.3.0**

- Added a new config value regarding the text alignment for the BowStats block
- Added a new config value regarding the background color for all the blocks (under General > BlocksBackgroundColor)
- Fixed an error shown when deleting a construction piece
- Fixed an error that didn't make editing mode work best
- Moved "HealthStringFormat" and "ProcessStringFormat" to the "General" section
- Added a new block called EntityStats that shows health as text in the health bar when hovering living entities
- Added a new block called MapStats that shows coordinates of the player
- Added a text that shows the world coordinates of where the cursor is pointing on the large map
- Added new config values to match the new additions
- Moved the position of the health text for construction pieces

v**3.2.2**

- Fixed an error shown when you had no arrows in the inventory
- Added total players string in GameStats block when there is more than 1
- Overall adjustments

v**3.2.1**

- Fixed a bug that kept showing the ShipStats block even after the ship was demolished
- Adjusted the text displayed when having the max honey in a beehive

v**3.2.0**

- Fixed bug that didn't let show the ShipStats block
- Now percentages are decimal values with eventually one decimal digit
- Added status for construction pieces when hovering it
- Added a new config value to match the new addition

v**3.1.0**

- Renamed the config value "GrowStringFormat" to "ProcessStringFormat"
- Split the config value "BushAndPlantStatus" in "BushStatus" and "PlantStatus" to separate bushes and plants
- Added status about fermenter when hovering it
- Added status about beehive when hovering it
- Added status about container when hovering it
- Added the possibility to see remaining time in the "ProcessStringFormat" config value
- Added new config values to match the new additions

v**3.0.0**

- Added bushes and plants grow string in the EnvStats section
- Fixed all values that could be rounded to integer when having 0 as a digit after the decimal point
- Added a new config value to match the new addition ("GrowStringFormat")
- Renamed the config value "StringFormat" to "HealthStringFormat"
- Renamed the config value "DaysInWorldList" to "DaysInWorldsList"
- Renamed the config value "BushStatus" to "BushAndPlantStatus"
- Now you will see the growth for bushes and not the health
- Added the possibility to color the text based on the health/grow percentage in "HealthStringFormat" and "GrowStringFormat"

v**2.6.0**

- Added a new block regarding the bow ammo and selected arrows
- Added the possibility to show a bow charge bar instead of the vanilla one
- Added new config values to match the new additions

v**2.5.0**

- Added a health string for every environment element (rocks, mine rocks, trees, bushes, etc...)
- Added new config values to match the new additions
- Added a key to reload the plugin's config file while in-game

v**2.4.2**

- Fixed the ship speed that was showing 0 when reversing

v**2.4.1**

- Fixed the check if the player is on a ship or not
- Fixed the ship speed with more credible values (in order to better respect the true speeds of Viking ships)

v**2.4.0**

- Fixed error when exiting the server
- Added possibility to remove biome from minimap top-left corner and set it in the WorldStats block

v**2.3.0**

- Optimized the editing mode
- Now you don't need to close and open again the game to save your changes
- Optimized all the blocks for better readability

v**2.2.0**

- Added "Ship speed" in the ShipStats block
- Optimized the check if the player is on board or not (ShipStats)

v**2.1.1**

- Added the ShipStats block in the editing mode
- Overall adjustments

v**2.1.0**

- Added the ShipStats block, to see ship health, wind speed and wind direction
- Added new config values to match the new additions

v**2.0.1**

- Overall fixes and optimization

v**2.0.0**

- Optimized the code and made it cleaner to read
- Added an editing mode
- Added new config values to match the new additions

v**1.2.0**

- Fixed "Text color", "Position" and "Margin" values to be culture invariant (so now you can set decimal values with the point)
- Added a customizable in-game clock, separated from WorldStats

v**1.1.1**

- Commented out the "OnDestroy" method (it is for development only)
- Removed commented code

v**1.1.0**

- Added a "Days passed" string in the world selection panel
- Added the chance to disable it in the config file

v**1.0.0**

- Added GameStats, that contains "FPS" and "Ping"
- Added WorldStats, that contains "Days passed" and "Time played"
