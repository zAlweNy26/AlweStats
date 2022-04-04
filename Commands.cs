using HarmonyLib;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AlweStats {
    [HarmonyPatch]
    public static class Commands {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), "InitTerminal")]
        static void InitCommands() {
            new Terminal.ConsoleCommand("alwe", "[command] [...argument/s] - General command to use all the subcommands of the AlweStats mod", (Terminal.ConsoleEventArgs args) => {
                if (args.Length >= 2) {
                    if (args[1] == "df" && File.Exists(Main.statsFilePath)) {
                        File.Delete(Main.statsFilePath);
                        args.Context.AddString("The AlweStats.json file was deleted successfully !");
                    } else if (args[1] == "cfp") {
                        if (args.Length >= 3) {
                            if (!File.Exists(Main.statsFilePath)) return;
                            List<WorldInfo> worlds = JsonConvert.DeserializeObject<List<WorldInfo>>(
                                File.ReadAllText(Main.statsFilePath),
                                new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } }
                            );
                            if (worlds != null) {
                                WorldInfo world = worlds.FirstOrDefault(w => w.worldName.ToLower() == args[2]);
                                if (world != null) {
                                    world.removedPins.Clear();
                                    string worldsLines = JsonConvert.SerializeObject(
                                        worlds, 
                                        Formatting.Indented, 
                                        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore }
                                    );
                                    File.WriteAllText(Main.statsFilePath, worldsLines);
                                    args.Context.AddString($"Removed all the pins from the AlweStats.json file for {args[2]} !");
                                }
                            }
                        } else args.Context.AddString("You have to specify a world to execute this command !");
                    } else args.Context.AddString("You have to specify a valid command !");
                } else {
                    args.Context.AddString("List of valid subcommands :\n" + 
                        "cfp [world] - Remove all the pins from the AlweStats.json file for a specific world\n" +
                        "df - Clear the entire AlweStats.json file by deleting it"
                    );
                }
            });
        }
    }
}