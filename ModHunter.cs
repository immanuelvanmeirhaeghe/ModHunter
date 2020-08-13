using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModHunter
{
    class ModHunter : MonoBehaviour
    {
        private static ModHunter s_Instance;

        private bool showUI = false;

        private static ItemsManager itemsManager;

        private static Player player;

        private static HUDManager hUDManager;

        public bool IsModHunterActive = false;

        public static bool HasUnlockedHunter = false;

        public static bool HasUnlockedTraps = false;

        public bool IsOptionInstantFinishConstructionsActive;

        /// <summary>
        /// ModAPI required security check to enable this mod feature.
        /// </summary>
        /// <returns></returns>
        public bool IsLocalOrHost => ReplTools.AmIMaster() || !ReplTools.IsCoopEnabled();

        public ModHunter()
        {
            IsModHunterActive = true;
            s_Instance = this;
        }

        public static ModHunter Get()
        {
            return s_Instance;
        }

        private void Update()
        {
            if (IsLocalOrHost && Input.GetKeyDown(KeyCode.Pause))
            {
                if (!showUI)
                {
                    hUDManager = HUDManager.Get();

                    itemsManager = ItemsManager.Get();

                    player = Player.Get();

                    EnableCursor(true);
                }
                // toggle menu
                showUI = !showUI;
                if (!showUI)
                {
                    EnableCursor(false);
                }
            }

            if (!IsOptionInstantFinishConstructionsActive && IsLocalOrHost && IsModHunterActive && Input.GetKeyDown(KeyCode.F8))
            {
                ShowHUDBigInfo("Feature disabled in multiplayer!", "Mod Hunter Info", HUDInfoLogTextureType.Count.ToString());
            }
        }

        private void OnGUI()
        {
            if (showUI)
            {
                InitData();
                InitModUI();
            }
        }

        private static void InitData()
        {
            hUDManager = HUDManager.Get();

            itemsManager = ItemsManager.Get();

            player = Player.Get();

            InitSkinUI();
        }

        private static void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        private void InitModUI()
        {
            GUI.Box(new Rect(10f, 170f, 450f, 150f), "ModHunter UI - Press PAUSE to open/close", GUI.skin.window);

            GUI.Label(new Rect(30f, 190f, 200f, 20f), "Click to unlock all weapons, armor and traps", GUI.skin.label);
            if (GUI.Button(new Rect(280f, 190f, 150f, 20f), "Unlock hunter", GUI.skin.button))
            {
                OnClickUnlockHunterButton();
                showUI = false;
                EnableCursor(false);
            }

            GUI.Label(new Rect(30f, 210f, 200f, 20f), "Click to get tribal weapons", GUI.skin.label);
            if (GUI.Button(new Rect(280f, 210f, 150f, 20f), "Get tribal", GUI.skin.button))
            {
                OnClickGetTribalButton();
                showUI = false;
                EnableCursor(false);
            }

            GUI.Label(new Rect(30f, 240f, 200f, 20f), "Use F8 to instantly finish constructions", GUI.skin.label);
            IsOptionInstantFinishConstructionsActive = GUI.Toggle(new Rect(280f, 240f, 20f, 20f), IsOptionInstantFinishConstructionsActive, "");

        }

        public bool UseOptionF8
        {
            get
            {
                return IsOptionInstantFinishConstructionsActive;
            }
        }

        public static void OnClickUnlockHunterButton()
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

        public static void OnClickGetTribalButton()
        {
            try
            {
                GetTribalMeleeWeapons();
                GetTribalArrows(5);
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(OnClickUnlockHunterButton)}] throws exception: {exc.Message}");
            }
        }

        public static void UnlockHunter()
        {
            try
            {
                if (!HasUnlockedHunter)
                {
                    List<ItemInfo> hunterItemInfoList = itemsManager.GetAllInfos().Values.Where(info => info.IsWeapon() || info.IsArmor() || ItemInfo.IsTrap(info.m_ID)).ToList();

                    if (!hunterItemInfoList.Contains(itemsManager.GetInfo(ItemID.Frog_Stretcher)))
                    {
                        hunterItemInfoList.Add(itemsManager.GetInfo(ItemID.Frog_Stretcher));
                    }

                    foreach (ItemInfo hunterItemInfo in hunterItemInfoList)
                    {
                        itemsManager.UnlockItemInNotepad(hunterItemInfo.m_ID);
                        itemsManager.UnlockItemInfo(hunterItemInfo.m_ID.ToString());
                        ShowHUDInfoLog(hunterItemInfo.m_ID.ToString(), "HUD_InfoLog_NewEntry");
                    }
                    HasUnlockedHunter = true;
                }
                else
                {
                    ShowHUDBigInfo("All hunter items were already unlocked", "Mod Hunter Info", HUDInfoLogTextureType.Count.ToString());
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(UnlockHunter)}] throws exception: {exc.Message}");
            }
        }

        public static void GetTribalMeleeWeapons(int count = 1)
        {
            try
            {
                List<ItemID> tribalWeaponItemIDList = new List<ItemID>
                    {
                        ItemID.Obsidian_Bone_Blade,
                        ItemID.Stick_Blade,
                        ItemID.Tribe_Axe,
                        ItemID.Tribe_Bow,
                        ItemID.Tribe_Spear
                    };
                foreach (ItemID tribalWeaponItemID in tribalWeaponItemIDList)
                {
                    ItemInfo tribalWeaponItemInfo = itemsManager.GetInfo(tribalWeaponItemID);
                    itemsManager.UnlockItemInfo(tribalWeaponItemInfo.m_ID.ToString());
                    player.AddItemToInventory(tribalWeaponItemInfo.m_ID.ToString());
                    ShowHUDBigInfo($"Added {count} x {itemsManager.GetInfo(tribalWeaponItemID).GetNameToDisplayLocalized()} to inventory", "Mod Hunter Info", HUDInfoLogTextureType.Count.ToString());
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(GetTribalMeleeWeapons)}] throws exception: {exc.Message}");
            }
        }

        public static void GetTribalArrows(int count = 1)
        {
            try
            {
                itemsManager.UnlockItemInfo(ItemID.Tribe_Arrow.ToString());
                ItemInfo tribalArrowItemInfo = itemsManager.GetInfo(ItemID.Tribe_Arrow);
                for (int i = 0; i < count; i++)
                {
                    player.AddItemToInventory(tribalArrowItemInfo.m_ID.ToString());
                }
                ShowHUDBigInfo($"Added {count} x {itemsManager.GetInfo(ItemID.Tribe_Arrow).GetNameToDisplayLocalized()} to inventory", "Mod Hunter Info", HUDInfoLogTextureType.Count.ToString());
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(GetTribalArrows)}] throws exception: {exc.Message}");
            }
        }

        public static void UnlockAllConstructions()
        {
            try
            {
                if (!HasUnlockedTraps)
                {
                    List<ItemInfo> list = itemsManager.GetAllInfos().Values.Where(info => info.IsConstruction()).ToList();
                    foreach (ItemInfo constructionItemInfo in list)
                    {
                        itemsManager.UnlockItemInNotepad(constructionItemInfo.m_ID);
                        itemsManager.UnlockItemInfo(constructionItemInfo.m_ID.ToString());
                        ShowHUDInfoLog(constructionItemInfo.m_ID.ToString(), "HUD_InfoLog_NewEntry");
                    }
                    HasUnlockedTraps = true;
                }
                else
                {
                    ShowHUDBigInfo("All traps unlocked", "Mod Hunter Info", HUDInfoLogTextureType.Count.ToString());
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{nameof(ModHunter)}.{nameof(ModHunter)}:{nameof(UnlockAllConstructions)}] throws exception: {exc.Message}");
            }
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

        private static void EnableCursor(bool enabled = false)
        {
            CursorManager.Get().ShowCursor(enabled, false);
            player = Player.Get();

            if (enabled)
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

    }
}
