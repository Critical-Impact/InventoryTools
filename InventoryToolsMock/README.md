## Mocked

This project allows you to run significant parts of the plugin from within an external window. 
No active instance of the game is required to run it though the game's data files are required to be referenced.
It has no way of connecting to the game and acts purely as a tool to aid in development.

### Setup

You need to pass in 2 paths as arguments to the executable, the game data path, the config directory.
I would highly recommend making a copy of your configuration and putting it in an alternate directory.

**Examples:**

"C:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack"  
"C:/Users/YourName/AppData/Roaming/XIVLauncher/pluginConfigs"  

You may also pass in the config file and inventories file as the 3rd and 4th parameters respectively if you wish to target specific json files.
