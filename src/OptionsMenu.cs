using System;
using BepInEx.Configuration;
using UnityEngine;

namespace Cjayride.SmartShield
{
    internal static class OptionsMenu
    {
        private static readonly string[] ModeLabels = { "Best in inventory", "First in inventory", "Preferred shield" };
        private static readonly int WindowId = 0x53485344;

        // SeneaLHudLayout palette (stats/xp cards): dark plate, gold type, gold rim.
        private static readonly Color Plate = new Color(0.07f, 0.05f, 0.03f, 0.94f);
        private static readonly Color PlateInner = new Color(0.12f, 0.09f, 0.05f, 0.96f);
        private static readonly Color GoldLine = new Color(0.93f, 0.82f, 0.52f, 1f);
        private static readonly Color TitleGold = new Color(0.9f, 0.78f, 0.48f, 1f);
        private static readonly Color BodyGold = new Color(0.86f, 0.75f, 0.48f, 0.95f);
        private static readonly Color DimGold = new Color(0.75f, 0.68f, 0.55f, 1f);
        private static readonly Color FillGold = new Color(1f, 0.86f, 0.25f, 0.95f);
        private static readonly Color MuteGold = new Color(0.42f, 0.34f, 0.18f, 1f);
        private static readonly Color SelectRow = new Color(0.26f, 0.20f, 0.10f, 1f);
        private static readonly Color DropPanel = new Color(0.04f, 0.03f, 0.02f, 0.98f);
        private static readonly Color Overlay = new Color(0f, 0f, 0f, 0.45f);

        private static bool _open;
        private static bool _dropdownOpen;
        private static bool _centered;
        private static Rect _window = new Rect(0f, 0f, 640f, 720f);
        private static Vector2 _scroll;
        private static string _preferredDraft = "";
        private static string _allowedDraft = "";
        private static string _blacklistDraft = "";
        private static bool _draftsLoaded;

        private static Texture2D _white;
        private static GUIStyle _windowStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _sectionStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _dropStyle;
        private static GUIStyle _fieldStyle;
        private static GUIStyle _hintStyle;
        private static GUIStyle _closeStyle;
        private static GUIStyle _dropItemStyle;
        private static GUIStyle _dropItemSelectedStyle;
        private static GUIStyle _dropBoxStyle;
        private static GUIStyle _titleLeftStyle;

        internal static bool IsOpen => _open;

        internal static void Tick()
        {
            KeyboardShortcut hotkey = ModConfig.MenuHotkey != null
                ? ModConfig.MenuHotkey.Value
                : new KeyboardShortcut(KeyCode.S, KeyCode.LeftAlt);

            if (hotkey.MainKey != KeyCode.None && !TextInputBlocked() && hotkey.IsDown())
            {
                SetOpen(!_open);
                return;
            }

            if (_open && Input.GetKeyDown(KeyCode.Escape))
            {
                SetOpen(false);
            }
        }

        internal static void Draw()
        {
            if (!_open)
            {
                return;
            }

            EnsureStyles();

            if (!_centered)
            {
                _window.width = Mathf.Clamp(Screen.width * 0.42f, 560f, 760f);
                _window.height = Mathf.Clamp(Screen.height * 0.78f, 560f, 900f);
                _window.x = (Screen.width - _window.width) * 0.5f;
                _window.y = (Screen.height - _window.height) * 0.12f;
                _centered = true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                Color old = GUI.color;
                GUI.color = Overlay;
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _white);
                GUI.color = old;
            }

            _window = GUI.Window(WindowId, _window, DrawWindow, "", _windowStyle);
        }

        private static void SetOpen(bool open)
        {
            _open = open;
            _dropdownOpen = false;
            if (open)
            {
                LoadDrafts();
                _centered = false;
                ZCursor.LockState = CursorLockMode.None;
                ZCursor.Show();
            }
            else
            {
                CommitDrafts();
                _draftsLoaded = false;
            }
        }

        private static void LoadDrafts()
        {
            _preferredDraft = ModConfig.PreferredShield != null ? ModConfig.PreferredShield.Value ?? "" : "";
            _allowedDraft = ModConfig.AllowedWeaponTypes != null ? ModConfig.AllowedWeaponTypes.Value ?? "" : "";
            _blacklistDraft = ModConfig.BlacklistShields != null ? ModConfig.BlacklistShields.Value ?? "" : "";
            _draftsLoaded = true;
        }

        private static void CommitDrafts()
        {
            if (!_draftsLoaded)
            {
                return;
            }

            SetString(ModConfig.PreferredShield, _preferredDraft);
            SetString(ModConfig.AllowedWeaponTypes, _allowedDraft);
            SetString(ModConfig.BlacklistShields, _blacklistDraft);
        }

        private static void DrawWindow(int id)
        {
            if (Event.current.type == EventType.Repaint)
            {
                DrawBorder(new Rect(0f, 0f, _window.width, _window.height), GoldLine, 2f);
            }

            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(MenuIcons.Shield, MenuIcons.BoxStyle, GUILayout.Width(36f), GUILayout.Height(36f));
            GUILayout.Space(10f);
            GUILayout.Label("SMART SHIELD", _titleLeftStyle, GUILayout.Height(36f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("Alt+S to toggle  ·  Esc to close  ·  drag the title to move", _hintStyle);
            GUILayout.Space(8f);

            Rect inner = new Rect(18f, 88f, _window.width - 36f, _window.height - 160f);
            Rect view = new Rect(0f, 0f, inner.width - 22f, _dropdownOpen ? 1080f : 860f);
            _scroll = GUI.BeginScrollView(inner, _scroll, view);

            GUILayout.BeginArea(view);

            GUILayout.Label("GENERAL", _sectionStyle);
            Toggle(ModConfig.Enabled, "Enable SmartShield");

            GUILayout.Space(14f);
            GUILayout.Label("SHIELD SELECTION", _sectionStyle);
            DrawModeDropdown();

            GUILayout.Space(16f);
            GUILayout.Label("Preferred shield prefab (used when mode is Preferred)", _labelStyle);
            _preferredDraft = GUILayout.TextField(_preferredDraft ?? "", _fieldStyle, GUILayout.Height(32f));
            GUILayout.Label("Example: ShieldWood  ·  leave empty to fall back to Best", _hintStyle);

            GUILayout.Space(14f);
            GUILayout.Label("BEHAVIOR", _sectionStyle);
            Toggle(ModConfig.AutoEquipOnOneHanded, "Auto-equip shield with a one-handed weapon", MenuIcons.SwordShield);
            Toggle(ModConfig.EquipShieldWithAxe, "Equip shield with a 1H axe (uncheck for tree chopping)", MenuIcons.Axe);
            Toggle(ModConfig.EquipShieldWithButcherKnife, "Equip shield with a butcher knife (uncheck for skinning)", MenuIcons.ButcherKnife);
            Toggle(ModConfig.EquipShieldWithTorch, "Equip shield with a torch", MenuIcons.Torch);
            Toggle(ModConfig.EquipAfterRanged, "Also equip after switching from bow / crossbow", MenuIcons.Bow);
            Toggle(ModConfig.UnequipOnTwoHanded, "Unequip shield when using two-handed or ranged", MenuIcons.TwoHand);
            Toggle(ModConfig.UnequipOnSheath, "Stow shield on R-sheath (weapon only on your back)", MenuIcons.Sheath);
            Toggle(ModConfig.UnequipOnHotbarDeselect, "Unequip shield when you deselect a 1H weapon on the hotbar", MenuIcons.Hotbar);

            GUILayout.Space(14f);
            GUILayout.Label("FILTERS", _sectionStyle);
            GUILayout.Label("Allowed weapon types / skills / prefabs (empty = all 1H)", _labelStyle);
            _allowedDraft = GUILayout.TextField(_allowedDraft ?? "", _fieldStyle, GUILayout.Height(32f));
            GUILayout.Label("Comma-separated. Example: Swords, Knives, SpearBronze", _hintStyle);

            GUILayout.Space(8f);
            GUILayout.Label("Blacklisted shield prefabs (never auto-equipped)", _labelStyle);
            _blacklistDraft = GUILayout.TextField(_blacklistDraft ?? "", _fieldStyle, GUILayout.Height(32f));
            GUILayout.Label("Comma-separated. Example: ShieldWood, ShieldBone", _hintStyle);

            GUILayout.EndArea();
            GUI.EndScrollView();

            GUILayout.BeginArea(new Rect(18f, _window.height - 58f, _window.width - 36f, 46f));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", _buttonStyle, GUILayout.Height(40f)))
            {
                CommitDrafts();
                ShieldService.ReapplyFromMenu();
            }

            GUILayout.Space(12f);
            if (GUILayout.Button("Close", _closeStyle, GUILayout.Height(40f)))
            {
                SetOpen(false);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUI.DragWindow(new Rect(0f, 0f, _window.width, 56f));
        }

        private static void DrawModeDropdown()
        {
            GUILayout.Label("How to pick a shield", _labelStyle);
            int idx = ModeIndex();
            string current = ModeLabels[idx] + (_dropdownOpen ? "   ▲" : "   ▼");
            if (GUILayout.Button(current, _dropStyle, GUILayout.Height(36f)))
            {
                _dropdownOpen = !_dropdownOpen;
            }

            if (!_dropdownOpen)
            {
                return;
            }

            GUILayout.Space(6f);
            GUILayout.BeginVertical(_dropBoxStyle);
            GUILayout.Space(8f);
            for (int i = 0; i < ModeLabels.Length; i++)
            {
                GUIStyle row = i == idx ? _dropItemSelectedStyle : _dropItemStyle;
                if (GUILayout.Button(ModeLabels[i], row, GUILayout.Height(32f)))
                {
                    string value = i == 2 ? "Preferred" : (i == 1 ? "First" : "Best");
                    if (ModConfig.SelectionMode.Value != value)
                    {
                        ModConfig.SelectionMode.Value = value;
                        ShieldService.ReapplyFromMenu();
                    }

                    _dropdownOpen = false;
                }

                if (i < ModeLabels.Length - 1)
                {
                    GUILayout.Space(6f);
                }
            }

            GUILayout.Space(8f);
            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
            {
                DrawBorder(GUILayoutUtility.GetLastRect(), GoldLine, 2f);
            }
        }

        private static int ModeIndex()
        {
            string mode = ModConfig.SelectionModeNormalized;
            if (mode.Equals("First", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (mode.Equals("Preferred", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return 0;
        }

        private static void Toggle(ConfigEntry<bool> entry, string label, Texture2D icon = null)
        {
            if (entry == null)
            {
                return;
            }

            GUILayout.BeginHorizontal(GUILayout.Height(40f));
            if (icon != null)
            {
                GUILayout.Box(icon, MenuIcons.BoxStyle, GUILayout.Width(32f), GUILayout.Height(32f));
            }

            Rect check = GUILayoutUtility.GetRect(28f, 28f, GUILayout.Width(28f), GUILayout.Height(28f));
            GUILayout.Space(10f);
            GUILayout.Label(label, _toggleStyle, GUILayout.Height(32f));
            GUILayout.EndHorizontal();
            Rect row = GUILayoutUtility.GetLastRect();
            GUILayout.Space(6f);

            Event ev = Event.current;
            if (ev.type == EventType.Repaint)
            {
                GUI.DrawTexture(check, entry.Value ? MenuIcons.CheckOn : MenuIcons.CheckOff);
            }
            else if (ev.type == EventType.MouseDown && ev.button == 0 && row.Contains(ev.mousePosition))
            {
                entry.Value = !entry.Value;
                ev.Use();
                ShieldService.ReapplyFromMenu();
            }
        }

        private static void SetString(ConfigEntry<string> entry, string value)
        {
            if (entry == null)
            {
                return;
            }

            string next = value ?? "";
            if (entry.Value != next)
            {
                entry.Value = next;
            }
        }

        private static void DrawBorder(Rect rect, Color color, float t)
        {
            if (_white == null)
            {
                return;
            }

            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, t), _white);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - t, rect.width, t), _white);
            GUI.DrawTexture(new Rect(rect.x, rect.y, t, rect.height), _white);
            GUI.DrawTexture(new Rect(rect.xMax - t, rect.y, t, rect.height), _white);
            GUI.color = old;
        }

        private static void EnsureStyles()
        {
            if (_windowStyle != null && _white != null)
            {
                return;
            }

            _white = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
            _white.hideFlags = HideFlags.HideAndDontSave;

            MenuIcons.Ensure();

            Texture2D plate = Solid(Plate);
            Texture2D inner = Solid(PlateInner);
            Texture2D mute = Solid(MuteGold);
            Texture2D goldDim = Solid(new Color(GoldLine.r, GoldLine.g, GoldLine.b, 0.28f));
            Texture2D dropBg = Solid(DropPanel);
            Texture2D rowBg = Solid(new Color(0.10f, 0.08f, 0.05f, 1f));
            Texture2D rowSel = Solid(SelectRow);

            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(0, 0, 0, 0)
            };
            _windowStyle.normal.background = plate;
            _windowStyle.onNormal.background = plate;
            _windowStyle.normal.textColor = Color.clear;
            _windowStyle.onNormal.textColor = Color.clear;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = TitleGold;

            _titleLeftStyle = new GUIStyle(_titleStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 26
            };

            _sectionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            _sectionStyle.normal.textColor = TitleGold;

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            _labelStyle.normal.textColor = BodyGold;

            _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
            _hintStyle.normal.textColor = DimGold;
            _hintStyle.alignment = TextAnchor.MiddleCenter;

            _toggleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true,
                padding = new RectOffset(4, 4, 6, 4)
            };
            _toggleStyle.normal.textColor = BodyGold;
            _toggleStyle.onNormal.textColor = TitleGold;
            _toggleStyle.hover.textColor = GoldLine;
            _toggleStyle.onHover.textColor = TitleGold;
            _toggleStyle.focused.textColor = BodyGold;
            _toggleStyle.onFocused.textColor = TitleGold;

            _buttonStyle = Button(mute, TitleGold, 20);
            _buttonStyle.hover.textColor = BodyGold;
            _closeStyle = Button(goldDim, TitleGold, 20);
            _dropStyle = Button(inner, BodyGold, 18);
            _dropStyle.alignment = TextAnchor.MiddleLeft;
            _dropStyle.padding = new RectOffset(12, 12, 6, 6);

            _dropBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 4, 4),
                margin = new RectOffset(0, 0, 0, 0)
            };
            _dropBoxStyle.normal.background = dropBg;

            _dropItemStyle = Button(rowBg, BodyGold, 17);
            _dropItemStyle.alignment = TextAnchor.MiddleLeft;
            _dropItemStyle.padding = new RectOffset(12, 12, 4, 4);
            _dropItemStyle.hover.textColor = TitleGold;

            _dropItemSelectedStyle = Button(rowSel, TitleGold, 17);
            _dropItemSelectedStyle.alignment = TextAnchor.MiddleLeft;
            _dropItemSelectedStyle.padding = new RectOffset(12, 12, 4, 4);
            _dropItemSelectedStyle.normal.textColor = TitleGold;
            _dropItemSelectedStyle.hover.textColor = BodyGold;

            _fieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 18,
                padding = new RectOffset(8, 8, 6, 6)
            };
            _fieldStyle.normal.textColor = BodyGold;
            _fieldStyle.focused.textColor = FillGold;
            _fieldStyle.normal.background = inner;
            _fieldStyle.focused.background = inner;
        }

        private static GUIStyle Button(Texture2D bg, Color text, int size)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button)
            {
                fontSize = size,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.background = bg;
            style.hover.background = bg;
            style.active.background = bg;
            style.onNormal.background = bg;
            style.normal.textColor = text;
            style.hover.textColor = TitleGold;
            style.active.textColor = TitleGold;
            return style;
        }

        private static Texture2D Solid(Color color)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static bool TextInputBlocked()
        {
            if (Console.instance != null && Console.IsVisible())
            {
                return true;
            }

            if (Chat.instance != null && Chat.instance.HasFocus())
            {
                return true;
            }

            if (TextInput.IsVisible())
            {
                return true;
            }

            return false;
        }
    }
}
