using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModHunter
{
    public enum MessageType
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// ModHunter is a mod for Green Hell that allows a player to unlock all weapons, armor and traps.
	/// A player can get tribal weapons.
    /// It also gives the AI the possibility to swim if enabled.
    /// (only in single player mode - Use ModManager for multiplayer).
    /// Enable the mod UI by pressing Home.
    /// </summary>
    public class ModHunter : MonoBehaviour
    {
        private static ModHunter Instance;

        private static readonly string ModName = nameof(ModHunter);
        private static readonly float ModScreenTotalWidth = 500f;
        private static readonly float ModScreenTotalHeight = 150f;
        private static readonly float ModScreenMinWidth = 50f;
        private static readonly float ModScreenMinHeight = 30f;
        private static readonly float ModScreenMaxHeight = 180f;

        private static bool IsMinimized { get; set; } = false;
        private bool ShowUI = false;

        public static Rect ModHunterScreen = new Rect(Screen.width / 6f, Screen.height / 6f, ModScreenTotalWidth, ModScreenTotalHeight);

        private static ItemsManager LocalItemsManager;
        private static Player LocalPlayer;
        private static HUDManager LocalHUDManager;

        public static List<ItemID> TribalWeaponItemIDs = new List<ItemID>
                    {
                        ItemID.metal_axe,
                        ItemID.Obsidian_Bone_Blade,
                        ItemID.Tribe_Bow,
                        ItemID.Tribe_Spear
                    };
        public static List<ItemInfo> UnlockedHunterItemInfos = new List<ItemInfo>();
        public bool HasUnlockedHunter { get; private set; }
        public bool InstantFinishConstructionsOption { get; private set; }

        public bool IsModActiveForMultiplayer { get; private set; }
        public bool IsModActiveForSingleplayer => ReplTools.AmIMaster();

        public static string AlreadyUnlockedHunter() => $"All hunter items were already unlocked!";
        public static string AddedToInventoryMessage(int count, ItemInfo itemInfo) => $"Added {count} x {itemInfo.GetNameToDisplayLocalized()} to inventory.";
        public static string PermissionChangedMessage(string permission) => $"Permission to use mods and cheats in multiplayer was {permission}!";
        public static string HUDBigInfoMessage(string message, MessageType messageType, Color? headcolor = null)
            => $"<color=#{ (headcolor != null ? ColorUtility.ToHtmlStringRGBA(headcolor.Value) : ColorUtility.ToHtmlStringRGBA(Color.red))  }>{messageType}</color>\n{message}";

        public void Start()
        {
            ModManager.ModManager.onPermissionValueChanged += ModManager_onPermissionValueChanged;
        }

        private void ModManager_onPermissionValueChanged(bool optionValue)
        {
            IsModActiveForMultiplayer = optionValue;
            ShowHUDBigInfo(
                          (optionValue ?
                            HUDBigInfoMessage(PermissionChangedMessage($"granted"), MessageType.Info, Color.green)
                            : HUDBigInfoMessage(PermissionChangedMessage($"revoked"), MessageType.Info, Color.yellow))
                            );
        }

        public ModHunter()
        {
            useGUILayout = true;
            Instance = this;
        }

        public static ModHunter Get()
        {
            return Instance;
        }

        public void ShowHUDInfoLog(string itemID, string localizedTextKey)
        {
            Localization localization = GreenHellGame.Instance.GetLocalization();
            ((HUDMessages)LocalHUDManager.GetHUD(typeof(HUDMessages))).AddMessage(localization.Get(localizedTextKey) + "  " + localization.Get(itemID));
        }

        public void ShowHUDBigInfo(string text)
        {
            string header = $"{ModName} Info";
            string textureName = HUDInfoLogTextureType.Count.ToString();
            HUDBigInfo hudBigInfo = (HUDBigInfo)LocalHUDManager.GetHUD(typeof(HUDBigInfo));
            HUDBigInfoData.s_Duration = 2f;
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
            LocalPlayer = Player.Get();

            if (blockPlayer)
            {
                LocalPlayer.BlockMoves();
                LocalPlayer.BlockRotation();
                LocalPlayer.BlockInspection();
            }
            else
            {
                LocalPlayer.UnblockMoves();
                LocalPlayer.UnblockRotation();
                LocalPlayer.UnblockInspection();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!ShowUI)
                {
                    InitData();
                    EnableCursor(true);
                }
                ToggleShowUI();
                if (!ShowUI)
                {
                    EnableCursor(false);
                }
            }
        }

        private void ToggleShowUI()
        {
            ShowUI = !ShowUI;
        }

        private void OnGUI()
        {
            if (ShowUI)
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
            LocalHUDManager = HUDManager.Get();
            LocalItemsManager = ItemsManager.Get();
            LocalPlayer = Player.Get();
        }

        private void InitSkinUI()
        {
            GUI.skin = ModAPI.Interface.Skin;
        }

        private void InitModHunterScreen(int windowID)
        {
            using (var modContentScope = new GUILayout.VerticalScope(GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.MinHeight(ModScreenMinHeight), GUILayout.MaxHeight(ModScreenMaxHeight)))
            {
                ScreenMenuBox();
                if (!IsMinimized)
                {
                    UnlockHunterBox();
                    GetTribalWeaponsBox();
                    GetTribalArrowsBox();
                }
            }
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 10000f));
        }

        private void ScreenMenuBox()
        {
            if (GUI.Button(new Rect(ModHunterScreen.width - 40f, 0f, 20f, 20f), "-", GUI.skin.button))
            {
                CollapseWindow();
            }

            if (GUI.Button(new Rect(ModHunterScreen.width - 20f, 0f, 20f, 20f), "X", GUI.skin.button))
            {
                CloseWindow();
            }
        }

        private void CollapseWindow()
        {
            if (!IsMinimized)
            {
                ModHunterScreen.Set(ModHunterScreen.x, Screen.height - ModScreenMinHeight, ModScreenMinWidth, ModScreenMinHeight);
                IsMinimized = true;
            }
            else
            {
                ModHunterScreen.Set(ModHunterScreen.x, Screen.height / ModScreenMinHeight, ModScreenTotalWidth, ModScreenTotalHeight);
                IsMinimized = false;
            }
            InitWindow();
        }

        private void GetTribalArrowsBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Add 5 tribal arrows to the backpack", GUI.skin.label);
                if (GUILayout.Button("5 x tribal arrows", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickGetTribalArrowsButton();
                }
            }
        }

        private void GetTribalWeaponsBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Metal axe, bow, spear and obsidian bone blade", GUI.skin.label);
                if (GUILayout.Button("Get tribal weapons", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickGetTribalWeaponsButton();
                }
            }
        }

        private void UnlockHunterBox()
        {
            using (var horizontalScope = new GUILayout.HorizontalScope(GUI.skin.box))
            {
                GUILayout.Label("Weapon-, armor- and trap blueprints", GUI.skin.label);
                if (GUILayout.Button("Unlock hunter", GUI.skin.button, GUILayout.MaxWidth(200f)))
                {
                    OnClickUnlockHunterButton();
                }
            }
        }

        private void CloseWindow()
        {
            ShowUI = false;
            EnableCursor(false);
        }

        private void OnClickUnlockHunterButton()
        {
            try
            {
                UnlockHunter();
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickUnlockHunterButton)}] throws exception:\n{exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetTribalWeaponsButton)}] throws exception:\n{exc.Message}");
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
                ModAPI.Log.Write($"[{ModName}.{ModName}:{nameof(OnClickGetTribalArrowsButton)}] throws exception:\n{exc.Message}");
            }
        }

        public void UnlockHunter()
        {
            try
            {
                if (!HasUnlockedHunter)
                {
                    UnlockedHunterItemInfos = LocalItemsManager.GetAllInfos().Values.Where(info => info.IsWeapon() || info.IsArmor() || ItemInfo.IsTrap(info.m_ID)).ToList();

                    if (!UnlockedHunterItemInfos.Contains(LocalItemsManager.GetInfo(ItemID.Frog_Stretcher)))
                    {
                        UnlockedHunterItemInfos.Add(LocalItemsManager.GetInfo(ItemID.Frog_Stretcher));
                    }

                    foreach (ItemInfo hunterItemInfo in UnlockedHunterItemInfos)
                    {
                        LocalItemsManager.UnlockItemInNotepad(hunterItemInfo.m_ID);
                        LocalItemsManager.UnlockItemInfo(hunterItemInfo.m_ID.ToString());
                        ShowHUDInfoLog(hunterItemInfo.m_ID.ToString(), "HUD_InfoLog_NewEntry");
                    }
                    HasUnlockedHunter = true;
                }
                else
                {
                    ShowHUDBigInfo(HUDBigInfoMessage(AlreadyUnlockedHunter(), MessageType.Warning, Color.yellow));
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(UnlockHunter)}] throws exception:\n{exc.Message}");
            }
        }

        public void GetTribalMeleeWeapons()
        {
            try
            {
                foreach (ItemID tribalWeaponItemID in TribalWeaponItemIDs)
                {
                    ItemInfo tribalWeaponItemInfo = LocalItemsManager.GetInfo(tribalWeaponItemID);
                    LocalItemsManager.UnlockItemInfo(tribalWeaponItemInfo.m_ID.ToString());
                    LocalPlayer.AddItemToInventory(tribalWeaponItemInfo.m_ID.ToString());
                    ShowHUDBigInfo(HUDBigInfoMessage(AddedToInventoryMessage(1, tribalWeaponItemInfo), MessageType.Info, Color.green));
                }
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(GetTribalMeleeWeapons)}] throws exception:\n{exc.Message}");
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
                LocalItemsManager.UnlockItemInfo(ItemID.Tribe_Arrow.ToString());
                ItemInfo tribalArrowItemInfo = LocalItemsManager.GetInfo(ItemID.Tribe_Arrow);
                for (int i = 0; i < count; i++)
                {
                    LocalPlayer.AddItemToInventory(tribalArrowItemInfo.m_ID.ToString());
                }
                ShowHUDBigInfo(HUDBigInfoMessage(AddedToInventoryMessage(count, tribalArrowItemInfo), MessageType.Info, Color.green));
            }
            catch (Exception exc)
            {
                ModAPI.Log.Write($"[{ModName}:{nameof(GetMaxFiveTribalArrows)}] throws exception:\n{exc.Message}");
            }
        }
    }
}
