using Enums;
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

        private bool showUI = false;

        public Rect ModHunterWindow = new Rect(10f, 170f, 450f, 150f);

        private static ItemsManager itemsManager;

        private static Player player;

        private static HUDManager hUDManager;

        private static readonly List<ItemID> m_TribalWeaponItemIDList = new List<ItemID>
                    {
                        ItemID.metal_axe,
                        ItemID.Obsidian_Bone_Blade,
                        /// ItemID.Tribe_Axe,
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

        public static void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            Localization localization = GreenHellGame.Instance.GetLocalization();
            ((HUDMessages)hUDManager.GetHUD(typeof(HUDMessages))).AddMessage(localization.Get(localizedTextKey) + "  " + localization.Get(itemID));
        }

        public static void ShowHUDBigInfo(string text, string header, string textureName)
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
                // toggle menu
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
            ModHunterWindow = GUI.Window(0, ModHunterWindow, InitModWindow, $"{nameof(ModHunter)}", GUI.skin.window);
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

        private void InitModWindow(int windowId)
        {
            if (GUI.Button(new Rect(440f, 170f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseWindow();
            }

            GUI.Label(new Rect(30f, 190f, 200f, 20f), "All weapon-, armor- and trap blueprints", GUI.skin.label);
            if (GUI.Button(new Rect(280f, 190f, 150f, 20f), "Unlock hunter", GUI.skin.button))
            {
                OnClickUnlockHunterButton();
                CloseWindow();
            }

            GUI.Label(new Rect(30f, 210f, 200f, 20f), "Metal axe, bow, spear and obsidian bone blade", GUI.skin.label);
            if (GUI.Button(new Rect(280f, 210f, 150f, 20f), "Get tribal weapons", GUI.skin.button))
            {
                OnClickGetTribalWeaponsButton();
                CloseWindow();
            }

            GUI.Label(new Rect(30f, 230f, 200f, 20f), "Add 5 tribal arrows to the backpack", GUI.skin.label);
            if (GUI.Button(new Rect(280f, 230f, 150f, 20f), "5 x tribal arrows", GUI.skin.button))
            {
                OnClickGetTribalArrowsButton();
                CloseWindow();
            }

            CreateAIOption();

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
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
                GUI.Label(new Rect(30f, 250f, 200f, 20f), "AI can swim?", GUI.skin.label);
                UseOptionAI = GUI.Toggle(new Rect(280f, 250f, 20f, 20f), UseOptionAI, "");
            }
            else
            {
                GUI.Label(new Rect(30f, 250f, 330f, 20f), "AI can swim option", GUI.skin.label);
                GUI.Label(new Rect(30f, 270f, 330f, 20f), "is only for single player or when host", GUI.skin.label);
                GUI.Label(new Rect(30f, 290f, 330f, 20f), "Host can activate using ModManager.", GUI.skin.label);
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
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(OnClickUnlockHunterButton)}] throws exception: {exc.Message}");
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
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(OnClickGetTribalWeaponsButton)}] throws exception: {exc.Message}");
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
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(OnClickGetTribalArrowsButton)}] throws exception: {exc.Message}");
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
                    ShowHUDBigInfo("All hunter items were already unlocked!", $"{nameof(ModHunter)}  Info", HUDInfoLogTextureType.Count.ToString());
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(UnlockHunter)}] throws exception: {exc.Message}");
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
                    ShowHUDBigInfo($"Added 1 x {tribalWeaponItemInfo.GetNameToDisplayLocalized()} to inventory", $"{nameof(ModHunter)} Info", HUDInfoLogTextureType.Count.ToString());
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(GetTribalMeleeWeapons)}] throws exception: {exc.Message}");
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
                ShowHUDBigInfo($"Added {count} x {tribalArrowItemInfo.GetNameToDisplayLocalized()} to inventory", $"{nameof(ModHunter)} Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(GetMaxFiveTribalArrows)}] throws exception: {exc.Message}");
            }
        }
    }
}
