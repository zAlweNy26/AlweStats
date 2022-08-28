using HarmonyLib;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace AlweStats {
    [HarmonyPatch]
    public static class Commands {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
        static void InitCommands() {
            new Terminal.ConsoleCommand("alwe", "[command] [...argument/s] - General command to use all the subcommands of the AlweStats mod", (Terminal.ConsoleEventArgs args) => {
                if (args.Length >= 2) {
                    List<WorldInfo> worlds = Utilities.GetWorldInfos();
                    if (args[1] == "reload") {
                        Main.ReloadConfig();
                    } else if (args[1] == "df") {
                        File.Delete(Main.statsFilePath);
                        args.Context.AddString("The AlweStats.json file was deleted successfully !");
                    } else if (args[1] == "cfp") {
                        if (args.Length >= 3) {
                            WorldInfo world = worlds.FirstOrDefault(w => w.worldName.ToLower() == args[2]);
                            if (world != null) world.removedPins.Clear();
                            args.Context.AddString($"Removed all the pins from the AlweStats.json file for {args[2]} !");
                        } else args.Context.AddString("You have to specify a world to execute this command !");
                    }/* else if (args[1] == "gp") {
                        if (Minimap.instance == null || 
                            Player.m_localPlayer == null ||
                            ZNet.instance == null) args.Context.AddString("You have to join a world to execute this command !");
                        else if (args.Length >= 3) {
                            worlds = Utilities.UpdateWorldFile();
                            Minimap.PinType portalPinType = (Minimap.PinType) (Enum.GetValues(typeof(Minimap.PinType)).Length + 8);
                            List<KeyValuePair<ZDO, Minimap.PinData>> portalPins = MapStats.zdoPins.Where(p => {
                                bool isPortal = p.Value.m_type == portalPinType;
                                return isPortal && Vector3.Distance(p.Value.m_pos, Player.m_localPlayer.transform.position) <= Convert.ToInt32(args[2]);
                            }).ToList();
                            if (portalPins.Count >= 1) {
                                WorldInfo world = worlds.FirstOrDefault(w => w.worldName == ZNet.instance.GetWorldName());
                                Vector3 sumVectors = Vector3.zero, vectorsCenter = Vector3.zero;
                                List<Vector3> vectorsList = new();
                                portalPins.ForEach(p => {
                                    vectorsList.Add(p.Value.m_pos);
                                    sumVectors += p.Value.m_pos;
                                    Minimap.instance.RemovePin(p.Value);
                                });
                                vectorsCenter = sumVectors / portalPins.Count;
                                Minimap.PinData portalHubPin = Minimap.instance.AddPin(vectorsCenter, portalPinType, "Portals Hub", true, false);
                                world.portalsHubs.Add(portalHubPin, portalPins.Select(p => p.Key).ToList());
                                args.Context.AddString($"Portals hub pin grouped {portalPins.Count} successfully !"); 
                            } else args.Context.AddString("No portal was found in the radius !"); 
                        } else args.Context.AddString("You have to specify a radius to execute this command !");
                    } else if (args[1] == "ugph") {
                        if (Minimap.instance == null || 
                            Player.m_localPlayer == null ||
                            ZNet.instance == null) args.Context.AddString("You have to join a world to execute this command !");
                        else if (args.Length >= 3) {
                            worlds = Utilities.UpdateWorldFile();
                            Minimap.PinType portalPinType = (Minimap.PinType) (Enum.GetValues(typeof(Minimap.PinType)).Length + 8);
                            List<Minimap.PinData> portalPins = Minimap.instance.m_pins.Where(p => {
                                bool isPortal = p.m_type == portalPinType;
                                return isPortal && Vector3.Distance(p.m_pos, Player.m_localPlayer.transform.position) <= Convert.ToInt32(args[2]);
                            }).ToList();
                            if (portalPins.Count >= 1) {
                                List<float> distances = portalPins.Select(p => Vector3.Distance(Player.m_localPlayer.transform.position, p.m_pos)).ToList();
                                Minimap.PinData closerPortalPin = Minimap.instance.m_pins[distances.IndexOf(distances.Min())];
                                if (closerPortalPin != null) {
                                    WorldInfo world = worlds.FirstOrDefault(w => w.worldName == ZNet.instance.GetWorldName() 
                                        && w.portalsHubs.Any(pair => pair.Key == closerPortalPin));
                                    if (world != null) {
                                        KeyValuePair<Minimap.PinData, List<ZDO>> portalsHubs = world.portalsHubs.FirstOrDefault(pair => pair.Key == closerPortalPin);
                                        if (portalsHubs.Key != null) {
                                            Minimap.instance.RemovePin(portalsHubs.Key);
                                            portalsHubs.Value.ForEach(p => world.removedPins.Remove(p.GetPosition()));
                                            world.removedPins.Remove(portalsHubs.Key.m_pos);
                                            world.portalsHubs[portalsHubs.Key].Clear();
                                            portalsHubs.Value.ForEach(p => Minimap.instance.AddPin(p.GetPosition(), portalPinType, p.GetString("tag", ""), true, false));
                                            args.Context.AddString($"Portals hub pin ungrouped {portalPins.Count} successfully !"); 
                                            return;
                                        }
                                    }
                                }
                                args.Context.AddString("No portal hub was found in the radius !");
                            } else args.Context.AddString("No portal was found in the radius !"); 
                        } else args.Context.AddString("You have to specify a radius to execute this command !");
                    } */else args.Context.AddString("You have to specify a valid command !");
                    Utilities.SetWorldInfos(worlds);
                } else {
                    args.Context.AddString("List of valid subcommands :\n" + 
                        "reload - Reload the configuration file to update changes in-game" +
                        "cfp [world] - Remove all the pins saved in the AlweStats.json file for a specific world\n" +
                        "df - Clear the entire AlweStats.json file by deleting it"/*\n" +
                        "gp [radius] - Group portal pins into one in the radius from the player position\n" +
                        "ugph [radius] - Ungroup the portal hub in the radius from the player position"*/
                    );
                }
            });
        }
    }
}