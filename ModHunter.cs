﻿using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModHunter
{
    /// <summary>
    /// ModHunter is a mod for Green Hell that allows a player to unlock all weapons, armor and traps.
	/// A player can get tribal weapons.
    /// It also gives the AI the possibility to swim if enabled.
    /// (only in single player mode - Use ModManager for multiplayer).
    /// Enable the mod UI by pressing Home.
    /// </summary>
    public class ModHunter : MonoBehaviour
    {
        private static ModHunter s_Instance;

        private static readonly string ModName = nameof(ModHunter);

        private bool showUI = false;

        public Rect ModHunterScreen = new Rect(10f, 170f, 450f, 150f);

        private static ItemsManager itemsManager;

        private static Player player;

        private static HUDManager hUDManager;

        private static readonly List<ItemID> m_TribalWeaponItemIDList = new List<ItemID>
                    {
                        ItemID.metal_axe,
                        ItemID.Obsidian_Bone_Blade,
                        ItemID.Tribe_Bow,
                        ItemID.Tribe_Spear
                    };
        private static List<ItemInfo> m_UnlockedHunterItemInfos = new List<ItemInfo>();
        public bool HasUnlockedHunter { get; private set; }
        public bool UseOptionF8 { get; private set; }
        public bool UseOptionAI { get; private set; }

        public bool IsModActiveForMultiplayer => FindObjectOfType(typeof(ModManager.ModManager)) != null && ModManager.ModManager.AllowModsForMultiplayer;
        public bool IsModActiveForSingleplayer => ReplTools.AmIMaster();

        public ModHunter()
        {
            useGUILayout = true;
            s_Instance = this;
        }

        public static ModHunter Get()
        {
            return s_Instance;
        }

        public void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            Localization localization = GreenHellGame.Instance.GetLocalization();
            ((HUDMessages)hUDManager.GetHUD(typeof(HUDMessages))).AddMessage(localization.Get(localizedTextKey) + "  " + localization.Get(itemID));
        }

        public void ShowHUDBigInfo(string text, string header, string textureName)
        {
            HUDManager hUDManager = HUDManager.Get();

            HUDBigInfo hudBigInfo = (HUDBigInfo)hUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData hudBigInfoData = new HUDBigInfoData
            {
                m_Header = header,
                m_Text = text,
                m_TextureName = textureName,
                m_ShowTime = Time.time
            };
            hudBigInfo.AddInfo(hudBigInfoData);
            hudBigInfo.Show(true);
        }

        private void EnableCursor(bool blockPlayer = false)
        {
            CursorManager.Get().ShowCursor(blockPlayer, false);
            player = Player.Get();

            if (blockPlayer)
            {
                player.BlockMoves();
                player.BlockRotation();
                player.BlockInspection();
            }
            else
            {
                player.UnblockMoves();
                player.UnblockRotation();
                player.UnblockInspection();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!showUI)
                {
                    InitData();
                    EnableCursor(true);
                }
                showUI = !showUI;
                if (!showUI)
                {
                    EnableCursor(false);
                }
            }
        }

        private void OnGUI()
        {
            if (showUI)
            {
                InitData();
                InitSkinUI();
                InitWindow();
            }
        }

        private void InitWindow()
        {
            int wid = GetHashCode();
            ModHunterScreen = GUILayout.Window(wid, ModHunterScreen, InitModHunterScreen, $"{ModName}", GUI.skin.window);
        }

        private void InitData()
        {
            hUDManager = HUDManager.Get();
            itemsManager = ItemsManager.Get();
            player = Player.Get();
        }

        private void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        private void InitModHunterScreen(int windowID)
        {
            using (var verticalScope = new GUILayout.VerticalScope($"{ModName}box"))
            {
                if (GUI.Button(new Rect(430f, 0f, 20f, 20f), "X", GUI.skin.button))
                {
                    CloseWindow();
                }

                using (var horizontalScope = new GUILayout.HorizontalScope("huntBox"))
                {
                    GUILayout.Label("Weapon-, armor- and trap blueprints", GUI.skin.label);
                    if (GUILayout.Button("Unlock hunter", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickUnlockHunterButton();
                        CloseWindow();
                    }
                }

                using (var horizontalScope = new GUILayout.HorizontalScope("tribalBox"))
                {
                    GUILayout.Label("Metal axe, bow, spear and obsidian bone blade", GUI.skin.label);
                    if (GUILayout.Button("Get tribal weapons", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickGetTribalWeaponsButton();
                        CloseWindow();
                    }
                }

                using (var horizontalScope = new GUILayout.HorizontalScope("arrowsBox"))
                {
                    GUILayout.Label("Add 5 tribal arrows to the backpack", GUI.skin.label);
                    if (GUILayout.Button("5 x tribal arrows", GUI.skin.button, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f)))
                    {
                        OnClickGetTribalArrowsButton();
                        CloseWindow();
                    }
                }

                CreateAIOption();
            }

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void CloseWindow()
        {
            showUI = false;
            EnableCursor(false);
        }

        private void CreateAIOption()
        {
            if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
            {
                using (var horizontalScope = new GUILayout.HorizontalScope("actionBox"))
                {
                    GUILayout.Label("AI can swim?", GUI.skin.label);
                    UseOptionAI = GUILayout.Toggle(UseOptionAI, "");
                }
            }
            else
            {
                using (var verticalScope = new GUILayout.VerticalScope("infoBox"))
                {
                    GUILayout.Label("AI can swim option", GUI.skin.label);
                    GUILayout.Label("is only for single player or when host", GUI.skin.label);
                    GUILayout.Label("Host can activate using ModManager.", GUI.skin.label);
                }
            }
        }

        private void CreateF8Option()
        {
            if (IsModActiveForSingleplayer || IsModActiveForMultiplayer)
            {
                GUI.Label(new Rect(30f, 250f, 200f, 20f), "Use F8 to instantly finish", GUI.skin.label);
                UseOptionF8 = GUI.Toggle(new Rect(280f, 250f, 20f, 20f), UseOptionF8, "");
            }
            else
            {
                GUI.Label(new Rect(30f, 250f, 330f, 20f), "Use F8 to instantly to finish any constructions", GUI.skin.label);
                GUI.Label(new Rect(30f, 270f, 330f, 20f), "is only for single player or when host", GUI.skin.label);
                GUI.Label(new Rect(30f, 290f, 330f, 20f), "Host can activate using ModManager.", GUI.skin.label);
            }
        }

        private void OnClickUnlockHunterButton()
        {
            try
            {
                UnlockHunter();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickUnlockHunterButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetTribalWeaponsButton()
        {
            try
            {
                GetTribalMeleeWeapons();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetTribalWeaponsButton)}] throws exception: {exc.Message}");
            }
        }

        private void OnClickGetTribalArrowsButton()
        {
            try
            {
                GetMaxFiveTribalArrows(5);
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetTribalArrowsButton)}] throws exception: {exc.Message}");
            }
        }

        public void UnlockHunter()
        {
            try
            {
                if (!HasUnlockedHunter)
                {
                    m_UnlockedHunterItemInfos = itemsManager.GetAllInfos().Values.Where(info => info.IsWeapon() || info.IsArmor() || ItemInfo.IsTrap(info.m_ID)).ToList();

                    if (!m_UnlockedHunterItemInfos.Contains(itemsManager.GetInfo(ItemID.Frog_Stretcher)))
                    {
                        m_UnlockedHunterItemInfos.Add(itemsManager.GetInfo(ItemID.Frog_Stretcher));
                    }

                    foreach (ItemInfo hunterItemInfo in m_UnlockedHunterItemInfos)
                    {
                        itemsManager.UnlockItemInNotepad(hunterItemInfo.m_ID);
                        itemsManager.UnlockItemInfo(hunterItemInfo.m_ID.ToString());
                        ShowHUDInfoLog(hunterItemInfo.m_ID.ToString(), "HUD_InfoLog_NewEntry");
                    }
                    HasUnlockedHunter = true;
                }
                else
                {
                    ShowHUDBigInfo("All hunter items were already unlocked!", $"{ModName}  Info", HUDInfoLogTextureType.Count.ToString());
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(UnlockHunter)}] throws exception: {exc.Message}");
            }
        }

        public void GetTribalMeleeWeapons()
        {
            try
            {
                foreach (ItemID tribalWeaponItemID in m_TribalWeaponItemIDList)
                {
                    ItemInfo tribalWeaponItemInfo = itemsManager.GetInfo(tribalWeaponItemID);
                    itemsManager.UnlockItemInfo(tribalWeaponItemInfo.m_ID.ToString());
                    player.AddItemToInventory(tribalWeaponItemInfo.m_ID.ToString());
                    ShowHUDBigInfo($"Added 1 x {tribalWeaponItemInfo.GetNameToDisplayLocalized()} to inventory", $"{ModName} Info", HUDInfoLogTextureType.Count.ToString());
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(GetTribalMeleeWeapons)}] throws exception: {exc.Message}");
            }
        }

        public void GetMaxFiveTribalArrows(int count = 1)
        {
            try
            {
                if (count > 5)
                {
                    count = 5;
                }
                itemsManager.UnlockItemInfo(ItemID.Tribe_Arrow.ToString());
                ItemInfo tribalArrowItemInfo = itemsManager.GetInfo(ItemID.Tribe_Arrow);
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(tribalArrowItemInfo.m_ID.ToString());
                }
                ShowHUDBigInfo($"Added {count} x {tribalArrowItemInfo.GetNameToDisplayLocalized()} to inventory", $"{ModName} Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(GetMaxFiveTribalArrows)}] throws exception: {exc.Message}");
            }
        }
    }
}
