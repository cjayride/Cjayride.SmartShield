using UnityEngine;

namespace Cjayride.SmartShield
{
    internal static class MenuIcons
    {
        internal static Texture2D Shield;
        internal static Texture2D SwordShield;
        internal static Texture2D Axe;
        internal static Texture2D ButcherKnife;
        internal static Texture2D Bow;
        internal static Texture2D TwoHand;
        internal static Texture2D Sheath;
        internal static Texture2D Hotbar;
        internal static Texture2D CheckOff;
        internal static Texture2D CheckOn;

        private static bool _ready;
        private static GUIStyle _box;

        internal static GUIStyle BoxStyle
        {
            get
            {
                Ensure();
                return _box;
            }
        }

        internal static void Ensure()
        {
            if (_ready)
            {
                return;
            }

            Shield = PaintShield();
            SwordShield = PaintSwordShield();
            Axe = PaintAxe();
            ButcherKnife = PaintButcherKnife();
            Bow = PaintBow();
            TwoHand = PaintTwoHand();
            Sheath = PaintSheath();
            Hotbar = PaintHotbar();
            CheckOff = PaintCheck(false);
            CheckOn = PaintCheck(true);

            _box = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 8, 2, 2),
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = false,
                stretchHeight = false
            };
            _box.normal.background = null;
            _ready = true;
        }

        private static Texture2D PaintShield()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color rim = new Color(0.78f, 0.66f, 0.38f, 1f);
            Color fill = new Color(0.28f, 0.22f, 0.12f, 1f);
            Color boss = new Color(0.9f, 0.78f, 0.48f, 1f);

            for (int y = 3; y <= 28; y++)
            {
                float t = (y - 3) / 25f;
                float half = y < 18 ? 10f : 10f * (1f - (t - 0.6f) / 0.55f);
                if (half < 2f)
                {
                    half = 2f;
                }

                int cx = 16;
                int x0 = cx - (int)half;
                int x1 = cx + (int)half;
                for (int x = x0; x <= x1; x++)
                {
                    bool edge = x == x0 || x == x1 || y == 3 || y == 28;
                    Put(px, s, x, y, edge ? rim : fill);
                }
            }

            FillCircle(px, s, 16, 13, 3, boss);
            return Bake(s, px);
        }

        private static Texture2D PaintSwordShield()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color steel = new Color(0.72f, 0.72f, 0.78f, 1f);
            Color hilt = new Color(0.55f, 0.38f, 0.18f, 1f);
            Color rim = new Color(0.78f, 0.66f, 0.38f, 1f);
            Color fill = new Color(0.28f, 0.22f, 0.12f, 1f);

            for (int y = 8; y <= 28; y++)
            {
                float half = y < 18 ? 7f : 7f * (1f - (y - 18) / 14f);
                int x0 = 20 - (int)half;
                int x1 = 20 + (int)half;
                for (int x = Mathf.Max(14, x0); x <= x1; x++)
                {
                    bool edge = x == x0 || x == x1 || y == 8;
                    Put(px, s, x, y, edge ? rim : fill);
                }
            }

            for (int y = 3; y <= 24; y++)
            {
                Put(px, s, 9, y, steel);
                Put(px, s, 10, y, steel);
            }

            for (int x = 6; x <= 13; x++)
            {
                Put(px, s, x, 22, hilt);
            }

            Put(px, s, 9, 25, hilt);
            Put(px, s, 10, 25, hilt);
            Put(px, s, 9, 26, hilt);
            Put(px, s, 10, 26, hilt);
            return Bake(s, px);
        }

        private static Texture2D PaintAxe()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color wood = new Color(0.5f, 0.32f, 0.14f, 1f);
            Color blade = new Color(0.7f, 0.72f, 0.78f, 1f);
            for (int y = 4; y <= 28; y++)
            {
                Put(px, s, 11, y, wood);
                Put(px, s, 12, y, wood);
            }

            for (int y = 5; y <= 14; y++)
            {
                int span = 4 + (14 - y) / 2;
                for (int x = 13; x <= 13 + span; x++)
                {
                    Put(px, s, x, y, blade);
                }
            }

            return Bake(s, px);
        }

        private static Texture2D PaintButcherKnife()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color steel = new Color(0.72f, 0.74f, 0.8f, 1f);
            Color handle = new Color(0.45f, 0.3f, 0.14f, 1f);
            for (int y = 6; y <= 24; y++)
            {
                Put(px, s, 12, y, steel);
                Put(px, s, 13, y, steel);
            }

            for (int y = 4; y <= 11; y++)
            {
                Put(px, s, 14 + (y - 4) / 2, y, steel);
            }

            for (int y = 22; y <= 28; y++)
            {
                Put(px, s, 11, y, handle);
                Put(px, s, 12, y, handle);
                Put(px, s, 13, y, handle);
            }

            return Bake(s, px);
        }

        private static Texture2D PaintBow()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color wood = new Color(0.55f, 0.38f, 0.16f, 1f);
            Color stringC = new Color(0.85f, 0.82f, 0.7f, 1f);
            for (int y = 3; y <= 28; y++)
            {
                float n = (y - 15.5f) / 13f;
                int x = 10 + (int)(n * n * 10f);
                Put(px, s, x, y, wood);
                Put(px, s, x + 1, y, wood);
                Put(px, s, 22, y, stringC);
            }

            for (int x = 12; x <= 21; x++)
            {
                Put(px, s, x, 15, new Color(0.72f, 0.72f, 0.76f, 1f));
            }

            return Bake(s, px);
        }

        private static Texture2D PaintTwoHand()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color steel = new Color(0.7f, 0.72f, 0.78f, 1f);
            Color hilt = new Color(0.45f, 0.3f, 0.14f, 1f);
            for (int y = 2; y <= 22; y++)
            {
                Put(px, s, 15, y, steel);
                Put(px, s, 16, y, steel);
                if (y < 10)
                {
                    Put(px, s, 14, y, steel);
                    Put(px, s, 17, y, steel);
                }
            }

            for (int x = 10; x <= 21; x++)
            {
                Put(px, s, x, 21, hilt);
            }

            for (int y = 22; y <= 28; y++)
            {
                Put(px, s, 15, y, hilt);
                Put(px, s, 16, y, hilt);
            }

            return Bake(s, px);
        }

        private static Texture2D PaintSheath()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color body = new Color(0.22f, 0.18f, 0.12f, 1f);
            Color rim = new Color(0.78f, 0.66f, 0.38f, 1f);
            Color strap = new Color(0.45f, 0.32f, 0.16f, 1f);

            for (int y = 6; y <= 26; y++)
            {
                for (int x = 10; x <= 22; x++)
                {
                    bool edge = x == 10 || x == 22 || y == 6 || y == 26;
                    Put(px, s, x, y, edge ? rim : body);
                }
            }

            for (int y = 4; y <= 28; y++)
            {
                Put(px, s, 8, y, strap);
                Put(px, s, 9, y, strap);
            }

            // small shield on the back
            FillCircle(px, s, 16, 14, 4, rim);
            FillCircle(px, s, 16, 14, 2, new Color(0.3f, 0.24f, 0.12f, 1f));
            return Bake(s, px);
        }

        private static Texture2D PaintHotbar()
        {
            int s = 32;
            Color[] px = Clear(s);
            Color slot = new Color(0.18f, 0.14f, 0.08f, 1f);
            Color rim = new Color(0.78f, 0.66f, 0.38f, 1f);
            Color empty = new Color(0.1f, 0.08f, 0.05f, 1f);
            int[] xs = { 3, 12, 21 };
            for (int i = 0; i < xs.Length; i++)
            {
                int x0 = xs[i];
                for (int y = 10; y <= 22; y++)
                {
                    for (int x = x0; x <= x0 + 8; x++)
                    {
                        bool edge = x == x0 || x == x0 + 8 || y == 10 || y == 22;
                        Put(px, s, x, y, edge ? rim : (i == 1 ? slot : empty));
                    }
                }
            }

            return Bake(s, px);
        }

        private static Texture2D PaintCheck(bool on)
        {
            int s = 32;
            Color[] px = Clear(s);
            Color rim = new Color(0.78f, 0.66f, 0.38f, 1f);
            Color fill = new Color(0.10f, 0.08f, 0.04f, 1f);
            Color mark = new Color(0.86f, 0.75f, 0.48f, 1f);
            for (int y = 2; y <= 29; y++)
            {
                for (int x = 2; x <= 29; x++)
                {
                    bool edge = x <= 4 || x >= 27 || y <= 4 || y >= 27;
                    Put(px, s, x, y, edge ? rim : fill);
                }
            }

            if (on)
            {
                for (int i = 0; i < 9; i++)
                {
                    Put(px, s, 8 + i, 16 + i / 2, mark);
                    Put(px, s, 9 + i, 16 + i / 2, mark);
                }

                for (int i = 0; i < 14; i++)
                {
                    Put(px, s, 16 + i, 20 - i, mark);
                    Put(px, s, 17 + i, 20 - i, mark);
                }
            }

            return Bake(s, px);
        }

        private static Color[] Clear(int s)
        {
            Color[] px = new Color[s * s];
            for (int i = 0; i < px.Length; i++)
            {
                px[i] = Color.clear;
            }

            return px;
        }

        private static void Put(Color[] px, int s, int x, int y, Color c)
        {
            if ((uint)x >= (uint)s || (uint)y >= (uint)s)
            {
                return;
            }

            px[(s - 1 - y) * s + x] = c;
        }

        private static void FillCircle(Color[] px, int s, int cx, int cy, int r, Color c)
        {
            int r2 = r * r;
            for (int y = cy - r; y <= cy + r; y++)
            {
                for (int x = cx - r; x <= cx + r; x++)
                {
                    int dx = x - cx;
                    int dy = y - cy;
                    if (dx * dx + dy * dy <= r2)
                    {
                        Put(px, s, x, y, c);
                    }
                }
            }
        }

        private static Texture2D Bake(int s, Color[] px)
        {
            Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
