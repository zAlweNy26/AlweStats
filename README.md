# AlweStats
Valheim Mod to easily see "FPS", "Ping", "Clock", "Days passed" and "Time played".

The UI is separated in three blocks :
- GameStats, that contains "FPS" and "Ping".
- WorldStats, that contains "Days passed" and "Time played".
- WorldClock, that contains "Clock".

For each block you can enable or disable it and you can also set :
- Position
- Margin 
- Text color
- Text size 
- Text alignment.

In "WorldClock" you can also set if the time should be showed in 12h or 24h format.

You can even add a "Days passed" counter in the world selection panel. 
Since the default day length declared in Valheim doesn't match the real one, to make the mod works fine, you need to join in the world at least one time, so that it can get the day length in that world. (You have to do it for each world)

Since **2.0.0**, you can now also move the blocks in game with your mouse ! 
To enable the editing mode you just have to click the button chose in the config file (default: F9)
or click the button positioned in the pause menu.

### Changelog

**2.0.0**
- Optimized the code and made it cleaner to read
- Added an editing mode
- Added new config values to match the new addition

**1.2.0**
- Fixed "Text color", "Position" and "Margin" values to be culture invariant (so now you can set decimal values with the dot)
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