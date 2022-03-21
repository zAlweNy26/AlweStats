using UnityEngine;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AlweStats {
    [HarmonyPatch]
    public static class MapStats {
        private static Block mapBlock = null;
        private static GameObject cursorObj = null, exploredObj = null, bedObj = null, portalObj = null;

        public static Block Start() {
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
            if (Main.showCursorCoordinatesInMap.Value) {
                GameObject original = Minimap.instance.m_biomeNameLarge.gameObject;
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
                GameObject original = Minimap.instance.m_biomeNameLarge.gameObject;
                exploredObj = UnityEngine.Object.Instantiate(original, original.transform);
                exploredObj.name = "ExploredPercentage";
                exploredObj.transform.SetParent(original.transform.parent);
                exploredObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                exploredObj.GetComponent<Text>().fontSize = Main.largeMapInfoSize.Value;
                RectTransform exploredRect = exploredObj.GetComponent<RectTransform>();
                exploredRect.anchorMin = exploredRect.anchorMax = exploredRect.pivot = new Vector2(0f, 1f);
                exploredRect.anchoredPosition = new Vector2(15f, -5f);
                exploredObj.SetActive(true);
            }
            if (Main.enableBedStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                bedObj = UnityEngine.Object.Instantiate(template, template.transform);
                bedObj.transform.SetParent(template.transform.parent);
                bedObj.GetComponentInChildren<Image>().sprite = Minimap.instance.m_windMarker.GetComponent<Image>().sprite;
                bedObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_bed");
                bedObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                bedObj.SetActive(false);
            }
            if (Main.enablePortalStatus.Value) {
                GameObject template = Hud.instance.m_statusEffectTemplate.gameObject;
                portalObj = UnityEngine.Object.Instantiate(template, template.transform);
                portalObj.transform.SetParent(template.transform.parent);
                portalObj.GetComponentInChildren<Image>().sprite = Minimap.instance.m_windMarker.GetComponent<Image>().sprite;
                portalObj.transform.Find("Name").GetComponent<Text>().text = Localization.instance.Localize("$piece_portal");
                portalObj.transform.Find("TimeText").GetComponent<Text>().text = "0 m";
                portalObj.SetActive(false);
            }
            return mapBlock;
        }

        public static void Update() {
            Minimap map = Minimap.instance;
            Player localPlayer = Player.m_localPlayer;
            Vector2 mousePos = Input.mousePosition;
            RectTransform mapRect = map.m_largeRoot.GetComponent<RectTransform>();
            float totEffects = (float) Hud.instance.m_statusEffects.Count;
            if (Main.enableBedStatus.Value && bedObj != null && localPlayer != null) {
                PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
                if (playerProfile.HaveCustomSpawnPoint()) {
                    bedObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - totEffects * Hud.instance.m_statusEffectSpacing, 0f);
                    Vector3 cameraPos = localPlayer.m_eye.transform.position;
                    Vector3 spawnPos = playerProfile.GetCustomSpawnPoint();
                    float distance = Vector3.Distance(localPlayer.transform.position, spawnPos);
                    string pointDistance = distance < 1000f ? $"{distance:0.#} m" : $"{(distance / 1000f):0.#} km";
                    float forwardAngle = Vector3.Angle(spawnPos - cameraPos, localPlayer.m_eye.transform.forward);
                    float rightAngle = Vector3.Angle(spawnPos - cameraPos, localPlayer.m_eye.transform.right);
                    bedObj.transform.Find("Icon").rotation = Quaternion.Euler(0f, 0f, rightAngle <= 90f ? 360f - forwardAngle : forwardAngle);
                    bedObj.transform.Find("TimeText").GetComponent<Text>().text = pointDistance;
                    bedObj.SetActive(true);
                } else bedObj.SetActive(false);
            }
            if (Main.enablePortalStatus.Value && portalObj != null && localPlayer != null) {
                List<ZDO> portalsList = new();
                ZDOMan.instance.GetAllZDOsWithPrefab(Game.instance.m_portalPrefab.name, portalsList);
                if (portalsList.Count > 0) {
                    float space = totEffects + (Main.enableBedStatus.Value ? 1 : 0);
                    portalObj.GetComponent<RectTransform>().anchoredPosition = new Vector3(-4f - space * Hud.instance.m_statusEffectSpacing, 0f);
                    Vector3 cameraPos = localPlayer.m_eye.transform.position;
                    List<float> distances = portalsList.Select(p => Vector3.Distance(cameraPos, p.GetPosition())).ToList();
                    float closer = distances.Min();
                    Vector3 portalPos = portalsList[distances.IndexOf(closer)].GetPosition();
                    closer -= 1.75f;
                    string closerDistance = closer < 1000f ? $"{closer:0.#} m" : $"{(closer / 1000f):0.#} km";
                    float forwardAngle = Vector3.Angle(portalPos - cameraPos, localPlayer.m_eye.transform.forward);
                    float rightAngle = Vector3.Angle(portalPos - cameraPos, localPlayer.m_eye.transform.right);
                    portalObj.transform.Find("Icon").rotation = Quaternion.Euler(0f, 0f, rightAngle <= 90f ? 360f - forwardAngle : forwardAngle);
                    portalObj.transform.Find("TimeText").GetComponent<Text>().text = closerDistance;
                    portalObj.SetActive(true);
                } else portalObj.SetActive(false);
            }
            if (Main.enableMapStats.Value && localPlayer != null && mapBlock != null) {
                Vector3 player = localPlayer.transform.position;
                RaycastHit hit;
                Physics.Raycast(Utils.GetMainCamera().transform.position, Utils.GetMainCamera().transform.forward, out hit);
                mapBlock.SetText(String.Format(
                    Main.worldCoordinatesFormat.Value,
                    $"{player.x:0.#}", $"{player.y:0.#}", $"{player.z:0.#}",
                    $"{hit.point.x:0.#}", $"{hit.point.y:0.#}", $"{hit.point.z:0.#}"
                ));
            }
            if (Main.showCursorCoordinatesInMap.Value && cursorObj != null &&
                RectTransformUtility.RectangleContainsScreenPoint(mapRect, mousePos)) {
                Vector3 cursor = map.ScreenToWorldPoint(mousePos);
                cursorObj.GetComponent<Text>().text = String.Format(
                    Main.mapCoordinatesFormat.Value,
                    $"{cursor.x:0.#}", $"{cursor.z:0.#}"
                );
            }
            if (Main.showExploredPercentage.Value && exploredObj != null) {
                float exploredYouPercentage = map.m_explored.Where(b => b).Count() * 100f / map.m_explored.Length;
                //float exploredOthersPercentage = map.m_exploredOthers.Where(b => b).Count() * 100f / map.m_exploredOthers.Length;
                exploredObj.GetComponent<Text>().text = $"Explored : {exploredYouPercentage:0.##} %";
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Awake")]
        private static void MinimapStart(Minimap __instance) {
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
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), "Update")]
        private static void RotateMinimap(Minimap __instance) {
            if (!Main.enableRotatingMinimap.Value) return;
            Player player = Player.m_localPlayer;
            if (player == null) return;
            Vector3 playerAngles = player.m_eye.transform.rotation.eulerAngles;
            __instance.m_mapImageSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
            __instance.m_pinRootSmall.transform.rotation = Quaternion.Euler(0f, 0f, playerAngles.y);
            for (int i = 0; i < __instance.m_pinRootSmall.childCount; i++)
                __instance.m_pinRootSmall.transform.GetChild(i).transform.rotation = Quaternion.identity;
            if (__instance.m_mode == Minimap.MapMode.Small) {
				__instance.m_smallMarker.rotation = Quaternion.identity;
                Vector3 windAngles = Quaternion.LookRotation(EnvMan.instance.GetWindDir()).eulerAngles;
                __instance.m_windMarker.rotation = Quaternion.Euler(0f, 0f, playerAngles.y - windAngles.y);
				Ship controlledShip = player.GetControlledShip();
				if (controlledShip) {
					__instance.m_smallShipMarker.gameObject.SetActive(true);
					__instance.m_smallShipMarker.transform.rotation = Quaternion.Euler(0f, 0f, controlledShip.GetShipYawAngle());
				} else __instance.m_smallShipMarker.gameObject.SetActive(false);
			}
        }
    }
}
