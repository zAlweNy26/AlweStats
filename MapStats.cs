using UnityEngine;
using HarmonyLib;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AlweStats {
    [HarmonyPatch]
    public static class MapStats {
        private static Block mapBlock = null;
        private static readonly Dictionary<string, int> locObjsReal = new() {
            { "SunkenCrypt", "TrophySkeletonPoison".GetStableHashCode() },
            { "TrollCave", "TrophyFrostTroll".GetStableHashCode() },
            { "FireHole", "TrophySurtling".GetStableHashCode() },
            { "Crypt", "TrophySkeleton".GetStableHashCode() }
        };
        private static readonly Dictionary<string, int> zdoObjsReal = new() {
            { "Cart", "Cart".GetStableHashCode() },
            { "Raft", "Raft".GetStableHashCode() },
            { "Karve", "Karve".GetStableHashCode() },
            { "Viking Ship", "VikingShip".GetStableHashCode() },
            { "Wood Portal", "portal_wood".GetStableHashCode() },
            { "Stone Portal", "portal".GetStableHashCode() }
        };
        private static Dictionary<string, int> zdoObjs = new(), locObjs = new();
        private static GameObject cursorObj = null, exploredObj = null, bedObj = null, shipObj = null, portalObj = null;
        private static Dictionary<ZDO, Minimap.PinData> zdoPins = new();
        private static Dictionary<Vector3, Minimap.PinData> locPins = new();
        private static List<ZDO> shipsFound = new(), portalsFound = new();
        private static long exploredTotal = 0, mapSize = 0;
        private static bool zdoCheck;

        public static Block Start() {
            Minimap map = Minimap.instance;

            if (Main.enableMapStats.Value) {
                mapBlock = new Block(
                    "MapStats",
                    Main.mapStatsColor.Value,
                    Main.mapStatsSize.Value,
                    Main.mapStatsPosition.Value,
                    Main.mapStatsMargin.Value,
                    Main.mapStatsAlign.Value
                );
            }
            if (Main.showCursorCoordinates.Value) {
                GameObject original = map.m_biomeNameLarge.gameObject;
                cursorObj = UnityEngine.Object.Instantiate(original, original.transform);
                cursorObj.name = "CursorCoordinates";
                cursorObj.transform.SetParent(original.transform.parent);
                cursorObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                cursorObj.GetComponent<Text>().fontSize = Main.largeMapInfoSize.Value;
                RectTransform cursorRect = cursorObj.GetComponent<RectTransform>();
                cursorRect.anchorMin = cursorRect.anchorMax = cursorRect.pivot = Vector2.zero;
                cursorRect.anchoredPosition = new Vector2(15f, 5f);
                cursorObj.SetActive(true);
            }
            if (Main.showExploredPercentage.Value) {
                GameObject original = map.m_biomeNameLarge.gameObject;
                exploredObj = UnityEngine.Object.Instantiate(original, original.transform);
                exploredObj.name = "ExploredPercentage";
                exploredObj.transform.SetParent(original.transform.parent);
                exploredObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                exploredObj.GetComponent<Text>().fontSize = Main.largeMapInfoSize.Value;
                RectTransform exploredRect = exploredObj.GetComponent<RectTransform>();
                exploredRect.anchorMin = exploredRect.anchorMax = exploredRect.pivot = Vector2.up;
                exploredRect.anchoredPosition = new Vector2(15f, -5f);
                exploredObj.SetActive(true);
            }
            if (Main.enableBedStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                bedObj = UnityEngine.Object.Instantiate(template, template.transform);
                bedObj.transform.SetParent(template.transform.parent);
                bedObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                bedObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_bed");
                bedObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                bedObj.SetActive(false);
            }
            if (Main.enableShipStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                shipObj = UnityEngine.Object.Instantiate(template, template.transform);
                shipObj.transform.SetParent(template.transform.parent);
                shipObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                shipObj.transform.Find("Name").GetComponent<Text>().text = "Ship";
                shipObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                shipObj.SetActive(false);
            }
            if (Main.enablePortalStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                portalObj = UnityEngine.Object.Instantiate(template, template.transform);
                portalObj.transform.SetParent(template.transform.parent);
                portalObj.GetComponentInChildren<Image>().sprite = map.m_windMarker.GetComponent<Image>().sprite;
                portalObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_portal");
                portalObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                portalObj.SetActive(false);
            }
            return mapBlock;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Start")]
        static void MinimapStart(Minimap __instance) {
            /*if (Main.showCustomMinimap.Value) {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string name = Enumerable.Single(assembly.GetManifestResourceNames(), str => str.EndsWith("CircleMask.png"));
                Stream stream = assembly.GetManifestResourceStream(name);
                byte[] byteArray;
                using (Stream resFilestream = assembly.GetManifestResourceStream(name)) {
                    byteArray = new byte[resFilestream.Length];
                    resFilestream.Read(byteArray, 0, byteArray.Length);
                }
                Texture2D bmp = new Texture2D(__instance.m_textureSize, __instance.m_textureSize, TextureFormat.RGBA32, false);
                bmp.name = "CircleTexture";
                bmp.LoadImage(byteArray);
                Sprite spt = Sprite.Create(bmp, new Rect(0, 0, __instance.m_textureSize, __instance.m_textureSize), Vector2.zero);
                spt.name = "CircleSprite";

                GameObject newSmall = new("MaskedMap");
                newSmall.transform.SetParent(__instance.m_smallRoot.transform);
                RectTransform newRect = newSmall.AddComponent<RectTransform>();
                RectTransform oldRect = __instance.m_mapSmall.GetComponent<RectTransform>();
                newRect.anchorMin = oldRect.anchorMin;
                newRect.anchorMax = oldRect.anchorMax;
                newRect.pivot = oldRect.pivot;
                newRect.anchoredPosition = oldRect.anchoredPosition;
                newRect.position = oldRect.position;
                newRect.sizeDelta = oldRect.sizeDelta;
                __instance.m_mapSmall.transform.SetParent(newSmall.transform);
                __instance.m_mapSmall.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                UnityEngine.Object.Destroy(__instance.m_mapSmall.GetComponent<RectMask2D>());
                __instance.m_mapImageSmall.maskable = true;
                newSmall.AddComponent<Image>().sprite = spt;
                newSmall.AddComponent<Mask>().showMaskGraphic = false;
                newSmall.SetActive(true);

                //__instance.m_mapSmall.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                //__instance.m_mapImageSmall.maskable = true;
                //__instance.m_smallRoot.GetComponent<Image>().sprite = spt;
                //__instance.m_smallRoot.GetComponent<Image>().preserveAspect = true;
                //__instance.m_smallRoot.AddComponent<Mask>().showMaskGraphic = false;
            }*/
            if (Main.enableRotatingMinimap.Value) {
                __instance.m_pinRootSmall.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_smallMarker.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_smallShipMarker.transform.SetParent(__instance.m_smallRoot.transform);
                __instance.m_windMarker.transform.SetParent(__instance.m_smallRoot.transform);
            }
            exploredTotal = 0;
            mapSize = __instance.m_explored.Length;
            float newSmallSize = __instance.m_smallMarker.sizeDelta.x * Main.playerMarkerScale.Value;
            float newLargeSize = __instance.m_largeMarker.sizeDelta.x * Main.playerMarkerScale.Value;
            __instance.m_smallMarker.sizeDelta = new(newSmallSize, newSmallSize);
            __instance.m_largeMarker.sizeDelta = new(newLargeSize, newLargeSize);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Update")]
        static void MinimapUpdate(Minimap __instance) {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer == null) return;
            Vector2 mousePos = Input.mousePosition;
            RectTransform mapRect = __instance.m_largeRoot.GetComponent<RectTransform>();
            int totEffects = Hud.instance.m_statusEffects.Count;
            Vector3 playerPos3 = localPlayer.transform.position;
            Vector2 playerPos = new(playerPos3.x, playerPos3.z);
            Transform cameraTransform = Utils.GetMainCamera().transform;

            LoadLocations();
            LoadZDOs();

            foreach (KeyValuePair<ZDO, Minimap.PinData> pin in zdoPins) {
                if ((pin.Key.GetPrefab() == "portal_wood".GetStableHashCode() || pin.Key.GetPrefab() == "portal".GetStableHashCode())
                    && Utilities.CheckForValue("4", Main.showPinsTitles.Value)) pin.Value.m_name = pin.Key.GetString("tag", "");
                pin.Value.m_pos = pin.Key.GetPosition();            
            }

            if (Main.enableBedStatus.Value && bedObj != null) {
                PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
                if (playerProfile.HaveCustomSpawnPoint()) {
                    Vector2 spawnPos = new(playerProfile.GetCustomSpawnPoint().x, playerProfile.GetCustomSpawnPoint().z);
                    float distance = Vector2.Distance(playerPos, spawnPos);
                    string distanceText = distance < 1000f ? $"{distance:0.#} m" : $"{(distance / 1000f):0.#} km";
                    Vector2 cameraForward = new(cameraTransform.forward.x, cameraTransform.forward.z);
                    Vector2 cameraRight = new(cameraTransform.right.x, cameraTransform.right.z);
                    float forwardAngle = Vector2.Angle(spawnPos - playerPos, cameraForward);
                    float rightAngle = Vector2.Angle(spawnPos - playerPos, cameraRight);
                    Quaternion objRotation = Quaternion.Euler(0f, 0f, rightAngle <= 90f ? 360f - forwardAngle : forwardAngle);
                    bedObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - totEffects * Hud.instance.m_statusEffectSpacing, 0f);
                    bedObj.transform.Find("Icon").rotation = objRotation;
                    bedObj.transform.Find("TimeText").GetComponent<Text>().text = distanceText;
                    bedObj.SetActive(true);
                } else bedObj.SetActive(false);
            }
            if (Main.enablePortalStatus.Value && portalObj != null) {
                int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0);
                SetElementStatus(portalObj, portalsFound, playerPos, cameraTransform, space);
            }
            if (Main.enableShipStatus.Value && shipObj != null) {
                int space = totEffects + (bedObj != null ? (bedObj.activeSelf ? 1 : 0) : 0) + (shipObj != null ? (shipObj.activeSelf ? 1 : 0) : 0);
                SetElementStatus(shipObj, shipsFound, playerPos, cameraTransform, space);
            }
            if (Main.enableMapStats.Value && mapBlock != null) {
                RaycastHit hit;
                Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit);
                mapBlock.SetText(String.Format(
                    Main.worldCoordinatesFormat.Value,
                    $"{playerPos3.x:0.#}", $"{playerPos3.y:0.#}", $"{playerPos3.z:0.#}",
                    $"{hit.point.x:0.#}", $"{hit.point.y:0.#}", $"{hit.point.z:0.#}"
                ));
            }
            if (Main.showCursorCoordinates.Value && cursorObj != null && RectTransformUtility.RectangleContainsScreenPoint(mapRect, mousePos)) {
                Vector3 cursor = __instance.ScreenToWorldPoint(mousePos);
                cursorObj.GetComponent<Text>().text = String.Format(
                    Main.mapCoordinatesFormat.Value,
                    $"{cursor.x:0.#}", $"{cursor.z:0.#}"
                );
            }
            if (Main.enableRotatingMinimap.Value) {
                Vector3 playerAngles = localPlayer.m_eye.transform.rotation.eulerAngles;
                __instance.m_mapImageSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
                __instance.m_pinRootSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
                for (int i = 0; i < __instance.m_pinRootSmall.childCount; i++)
                    __instance.m_pinRootSmall.transform.GetChild(i).transform.rotation = Quaternion.identity;
                if (__instance.m_mode == Minimap.MapMode.Small) {
                    __instance.m_smallMarker.rotation = Quaternion.identity;
                    Vector3 windAngles = Quaternion.LookRotation(EnvMan.instance.GetWindDir()).eulerAngles;
                    __instance.m_windMarker.rotation = Quaternion.Euler(0f, 0f, playerAngles.y - windAngles.y);
                    Ship controlledShip = localPlayer.GetControlledShip();
                    if (controlledShip) {
                        __instance.m_smallShipMarker.gameObject.SetActive(true);
                        __instance.m_smallShipMarker.transform.rotation = Quaternion.Euler(0f, 0f, controlledShip.GetShipYawAngle());
                    } else __instance.m_smallShipMarker.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Location), "Awake")]
        static void ShowDungeonsPins(Location __instance) {
            LoadLocations();
            Location location = __instance.GetComponent<Location>();
            Minimap map = Minimap.instance;
            if (location != null && map != null && (Utilities.CheckForValue("1", Main.showCustomPins.Value) || 
                Utilities.CheckForValue("2", Main.showCustomPins.Value) || Utilities.CheckForValue("3", Main.showCustomPins.Value))) {
                Vector3 locPos = location.transform.position;
                string locName = location.name.ToLower();
                if (!locPins.ContainsKey(locPos) && locObjs.Any(p => locName.Contains(p.Key.ToLower()))) {
                    KeyValuePair<string, int> pair = locObjs.Where(p => locName.Contains(p.Key.ToLower())).FirstOrDefault();
                    string dungeonType = pair.Key.Contains("Crypt") ? "2" : (pair.Key.Contains("TrollCave") ? "1" : "3");
                    string pinTitle = Utilities.CheckForValue(dungeonType, Main.showPinsTitles.Value) ? 
                        Regex.Replace(pair.Key, "([a-z])([A-Z])", "$1 $2") : "";
                    Minimap.PinData customPin = map.AddPin(locPos, Minimap.PinType.Death, pinTitle, false, false);
                    customPin.m_icon = GetSprite(pair.Value, false);
                    customPin.m_doubleSize = Utilities.CheckForValue(dungeonType, Main.biggerPins.Value);
                    locPins.Add(locPos, customPin);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZNetScene), "AddInstance")]
        static void PatchCreateZDO(ZNetScene __instance, ZDO zdo) {
            LoadZDOs();
            Minimap map = Minimap.instance;
            int prefabHash = zdo.GetPrefab();
            if (map != null && zdo.IsValid() && zdoObjs.Any(p => prefabHash == p.Value)) SetElementPin(map, zdo);
            if (zdo.IsValid() && portalObj != null && (prefabHash == "portal_wood".GetStableHashCode() || 
                prefabHash == "portal".GetStableHashCode())) {
                //Debug.Log($"ZDO aggiunto a portalsFound !");
                portalsFound.Add(zdo);
            } else if (zdo.IsValid() && shipObj != null && (prefabHash == "VikingShip".GetStableHashCode() || 
                prefabHash == "Raft".GetStableHashCode() || prefabHash == "Karve".GetStableHashCode())) {
                //Debug.Log($"ZDO aggiunto a shipsFound !");
                shipsFound.Add(zdo);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ZNetScene), "Destroy")]
        static bool PatchDestroyZDO(ZNetScene __instance, GameObject go) {
            if (!Utilities.CheckForValue("0", Main.showCustomPins.Value) 
                && !Main.enablePortalStatus.Value && !Main.enableShipStatus.Value) return true;
            ZNetView component = go.GetComponent<ZNetView>();
            if (component && component.GetZDO() != null) {
                Minimap map = Minimap.instance;
                ZDO zdo = component.GetZDO();
                if (map && zdoObjs.Any(p => zdo.GetPrefab() == p.Value)) {
                    Minimap.PinData customPin;
                    if (zdoPins.TryGetValue(zdo, out customPin)) {
                        //Debug.Log($"ZDO rimosso da zdoPins !");
                        map.RemovePin(customPin);
                        zdoPins.Remove(zdo);
                    }
                }
                if (map && locObjs.Any(p => zdo.GetPrefab() == p.Value)) {
                    Minimap.PinData customPin;
                    if (locPins.TryGetValue(zdo.GetPosition(), out customPin)) {
                        //Debug.Log($"ZDO rimosso da locPins !");
                        map.RemovePin(customPin);
                        locPins.Remove(zdo.GetPosition());
                    }
                }
                if (shipsFound.Contains(zdo)) {
                    //Debug.Log($"ZDO rimosso da shipsFound !");
                    shipsFound.Remove(zdo);
                }
                if (portalsFound.Contains(zdo)) {
                    //Debug.Log($"ZDO rimosso da portalsFound !");
                    portalsFound.Remove(zdo);
                }
                component.ResetZDO();
                __instance.m_instances.Remove(zdo);
                if (zdo.IsOwner()) ZDOMan.instance.DestroyZDO(zdo);
            }
            UnityEngine.Object.Destroy(go);
            return false;
        }

        private static void SetElementStatus(GameObject element, List<ZDO> list, Vector2 playerPos, Transform camera, int space) {
            if (list.Count > 0) {
                List<float> distances = list.Select(p => {
                    Vector3 p3 = p.GetPosition();
                    return Vector2.Distance(playerPos, new Vector2(p3.x, p3.z));
                }).ToList();
                float closer = distances.Min();
                string distance = closer < 1000f ? $"{closer:0.#} m" : $"{(closer / 1000f):0.#} km";
                ZDO closerZDO = list[distances.IndexOf(closer)];
                Vector2 closerPos = new(closerZDO.GetPosition().x, closerZDO.GetPosition().z);
                if (list.All(shipsFound.Contains)) {
                    string prefabName = zdoObjs.Where(p => p.Value == closerZDO.GetPrefab()).FirstOrDefault().Key;
                    element.transform.Find("Name").GetComponent<Text>().text = prefabName;
                }
                Vector2 cameraForward = new(camera.forward.x, camera.forward.z);
                Vector2 cameraRight = new(camera.right.x, camera.right.z);
                float forwardAngle = Vector2.Angle(closerPos - playerPos, cameraForward);
                float rightAngle = Vector2.Angle(closerPos - playerPos, cameraRight);
                Quaternion objRotation = Quaternion.Euler(0f, 0f, rightAngle <= 90f ? 360f - forwardAngle : forwardAngle);
                element.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - space * Hud.instance.m_statusEffectSpacing, 0f);
                element.transform.Find("Icon").rotation = objRotation;
                element.transform.Find("TimeText").GetComponent<Text>().text = distance;
                element.SetActive(true);
            } else element.SetActive(false);
        }
        
        private static void SetElementPin(Minimap map, ZDO zdo) {
            string number = "0";
            if (zdo.GetPrefab() == "Cart".GetStableHashCode()) number = "6";
            else if (zdo.GetPrefab() == "Raft".GetStableHashCode() || zdo.GetPrefab() == "Karve".GetStableHashCode()
                || zdo.GetPrefab() == "VikingShip".GetStableHashCode()) number = "5";
            else if (zdo.GetPrefab() == "portal_wood".GetStableHashCode() 
                || zdo.GetPrefab() == "portal".GetStableHashCode()) number = "4";
            Minimap.PinData customPin;
            if (!zdoPins.TryGetValue(zdo, out customPin)) {
                string zdoName = zdoObjs.Where(p => p.Value == zdo.GetPrefab()).FirstOrDefault().Key;
                string pinTitle = Utilities.CheckForValue(number, Main.showPinsTitles.Value) ? zdoName : "";
                customPin = map.AddPin(zdo.GetPosition(), Minimap.PinType.Death, pinTitle, false, false);
                customPin.m_icon = GetSprite(zdo.GetPrefab(), true);
                customPin.m_doubleSize = Utilities.CheckForValue(number, Main.biggerPins.Value);
                zdoPins.Add(zdo, customPin);
                //Debug.Log($"ZDO aggiunto a zdoPins !");
            }
        }

        private static Sprite GetSprite(int nameHash, bool isPiece) {
            if (isPiece) {
                GameObject hammerObj = ObjectDB.instance.m_itemByHash["Hammer".GetStableHashCode()];
                if (!hammerObj) return null;
                ItemDrop hammerDrop = hammerObj.GetComponent<ItemDrop>();
                if (!hammerDrop) return null;
                PieceTable hammerPieceTable = hammerDrop.m_itemData.m_shared.m_buildPieces;
                foreach (GameObject piece in hammerPieceTable.m_pieces) {
                    Piece p = piece.GetComponent<Piece>();
                    if (p.name.GetStableHashCode() == nameHash) return p.m_icon;
                }
            } else {
                GameObject itemObj;
                ObjectDB.instance.m_itemByHash.TryGetValue(nameHash, out itemObj);
                if (!itemObj) return null;
                ItemDrop itemDrop = itemObj.GetComponent<ItemDrop>();
                if (!itemDrop) return null;
                return itemDrop.m_itemData.GetIcon();
            }
            return null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "GetSprite")]
        static void PatchGetSprite(ref Sprite __result, Minimap.PinType type) {
            if (Main.replaceBedPinIcon.Value && type == Minimap.PinType.Bed) __result = GetSprite("bed".GetStableHashCode(), true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Explore", new Type[] { typeof(int), typeof(int)})]
        static void PatchExplore(ref bool __result, Minimap __instance) {
            if (Main.showExploredPercentage.Value && exploredObj != null && __result) {
                exploredTotal += 1;
                float exploredYouPercentage = exploredTotal * 100f / mapSize;
                exploredObj.GetComponent<Text>().text = $"Explored : {exploredYouPercentage:0.##} %";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Reset")]
        static void PatchReset(Minimap __instance) {
            if (Main.showExploredPercentage.Value && exploredObj != null) {
                exploredTotal = 0;
                float exploredYouPercentage = exploredTotal * 100f / mapSize;
                exploredObj.GetComponent<Text>().text = $"Explored : {exploredYouPercentage:0.##} %";
            }
        }

        private static void LoadLocations() {
            Minimap map = Minimap.instance;
            if ((Utilities.CheckForValue("1", Main.showCustomPins.Value) || Utilities.CheckForValue("2", Main.showCustomPins.Value) 
                || Utilities.CheckForValue("3", Main.showCustomPins.Value)) && locPins.Count == 0 && map != null) {
                List<ZoneSystem.LocationInstance> locations = Enumerable.ToList<ZoneSystem.LocationInstance>(ZoneSystem.instance.GetLocationList());
                foreach (ZoneSystem.LocationInstance loc in locations.Where(l => l.m_placed == true)) {
                    //Debug.Log($"Location {loc.m_location.m_prefabName} | {loc.m_position} | {loc.m_placed}");
                    string prefabName = loc.m_location.m_prefabName.ToLower();
                    if (!locPins.ContainsKey(loc.m_position) && locObjs.Any(p => prefabName.Contains(p.Key.ToLower()))) {
                        KeyValuePair<string, int> pair = locObjs.Where(p => prefabName.Contains(p.Key.ToLower())).FirstOrDefault();
                        string dungeonType = pair.Key.Contains("Crypt") ? "2" : (pair.Key.Contains("TrollCave") ? "1" : "3");
                        string pinTitle = Utilities.CheckForValue(dungeonType, Main.showPinsTitles.Value) ? 
                            Regex.Replace(pair.Key, "([a-z])([A-Z])", "$1 $2") : "";
                        Minimap.PinData customPin = map.AddPin(loc.m_position, Minimap.PinType.Death, pinTitle, false, false);
                        customPin.m_icon = GetSprite(pair.Value, false);
                        customPin.m_doubleSize = Utilities.CheckForValue(dungeonType, Main.biggerPins.Value);
                        locPins.Add(loc.m_position, customPin);
                    }
                }
                Debug.Log($"Loaded {locPins.Count} locations pins");
            }
        }

        private static void LoadZDOs() {
            Minimap map = Minimap.instance;
            if ((!Utilities.CheckForValue("0", Main.showCustomPins.Value) || Main.enablePortalStatus.Value 
                || Main.enableShipStatus.Value) && !zdoCheck && map != null) {
                if (ZDOMan.instance.m_objectsByID.Count > 0) zdoCheck = true;
                foreach (ZDO zdoByID in ZDOMan.instance.m_objectsByID.Values.ToList()) {
                    int prefabHash = zdoByID.GetPrefab();
                    if (zdoObjs.Any(p => p.Value == prefabHash)) SetElementPin(map, zdoByID);
                    if (portalObj != null && (prefabHash == "portal_wood".GetStableHashCode() || 
                        prefabHash == "portal".GetStableHashCode())) {
                        //Debug.Log($"ZDO aggiunto a portalsFound !");    
                        portalsFound.Add(zdoByID);
                    } else if (shipObj != null && (prefabHash == "VikingShip".GetStableHashCode() || 
                        prefabHash == "Raft".GetStableHashCode() || prefabHash == "Karve".GetStableHashCode())) {
                        //Debug.Log($"ZDO aggiunto a shipsFound !");
                        shipsFound.Add(zdoByID);
                    }
                }
                Debug.Log($"Loaded {zdoPins.Count} zdos pins");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZNet), "LoadWorld")]
        static void PatchLoadWorld(ZNet __instance) {
            zdoCheck = false;
            zdoPins.Clear();
            locPins.Clear();
            shipsFound.Clear();
            portalsFound.Clear();
            locObjs = locObjsReal.Keys.ToDictionary(k => k, v => locObjsReal[v]);
            zdoObjs = zdoObjsReal.Keys.ToDictionary(k => k, v => zdoObjsReal[v]);
            if (!Utilities.CheckForValue("1", Main.showCustomPins.Value)) locObjs.Remove("TrollCave");
            if (!Utilities.CheckForValue("2", Main.showCustomPins.Value)) {
                locObjs.Remove("SunkenCrypt");
                locObjs.Remove("Crypt");
            }
            if (!Utilities.CheckForValue("3", Main.showCustomPins.Value)) locObjs.Remove("FireHole");
            if (!Utilities.CheckForValue("4", Main.showCustomPins.Value)) {
                zdoObjs.Remove("Wood Portal");
                zdoObjs.Remove("Stone Portal");
            }
            if (!Utilities.CheckForValue("5", Main.showCustomPins.Value)) {
                zdoObjs.Remove("Raft");
                zdoObjs.Remove("Karve");
                zdoObjs.Remove("Viking Ship");
            }
            if (!Utilities.CheckForValue("6", Main.showCustomPins.Value)) zdoObjs.Remove("Cart");
        }
    }
}
