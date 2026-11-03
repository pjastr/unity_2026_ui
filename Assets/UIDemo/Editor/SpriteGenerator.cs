using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIDemo.EditorTools
{
    /// <summary>
    /// Generuje proste grafiki interfejsu (PNG) z funkcji odległości (SDF) z wygładzaniem krawędzi:
    /// zaokrąglony prostokąt 9-slice, ramka, koło, wzór do kafelkowania, winieta i ikony.
    /// Dzięki temu projekt nie potrzebuje zewnętrznych plików graficznych.
    /// </summary>
    public static class SpriteGenerator
    {
        /// <summary>Tworzy (lub wczytuje) sprite z pliku PNG. border – obramowanie 9-slice w pikselach (L, B, R, T).</summary>
        public static Sprite Get(string dir, string name, int width, int height, Func<float, float, float> coverage,
            Vector4 border = default, bool repeat = false, bool force = false)
        {
            string path = $"{dir}/{name}.png";
            if (force || !File.Exists(path))
            {
                var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                var px = new Color32[width * height];
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float a = Mathf.Clamp01(coverage(x + 0.5f, y + 0.5f));
                    px[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
                tex.SetPixels32(px);
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.spritePixelsPerUnit = 100f;
                imp.spriteBorder = border;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = false;
                imp.filterMode = FilterMode.Bilinear;
                imp.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning("[UI Demo] Nie udało się wczytać sprite'a " + path);
            return sprite;
        }

        // ------------------------------------------------------------ funkcje pokrycia

        /// <summary>Wygładzona krawędź: odległość ze znakiem (ujemna wewnątrz) → pokrycie 0–1.</summary>
        static float Edge(float d) => Mathf.Clamp01(0.5f - d);

        public static float RoundedRect(float x, float y, float w, float h, float r)
        {
            float qx = Mathf.Abs(x - w * 0.5f) - (w * 0.5f - r);
            float qy = Mathf.Abs(y - h * 0.5f) - (h * 0.5f - r);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
            return outside + inside - r;
        }

        public static Func<float, float, float> RoundedRectFill(int size, float radius) =>
            (x, y) => Edge(RoundedRect(x, y, size, size, radius));

        public static Func<float, float, float> RoundedRectOutline(int size, float radius, float thickness) =>
            (x, y) =>
            {
                float d = RoundedRect(x, y, size, size, radius);
                return Edge(d) * Edge(-(d + thickness));
            };

        public static Func<float, float, float> Circle(int size) =>
            (x, y) => Edge(new Vector2(x - size * 0.5f, y - size * 0.5f).magnitude - (size * 0.5f - 1.5f));

        /// <summary>Wzór do kafelkowania: romby na przezroczystym tle (półprzezroczyste tło dla kontrastu).</summary>
        public static Func<float, float, float> TilePattern(int size) =>
            (x, y) =>
            {
                float cx = x - size * 0.5f, cy = y - size * 0.5f;
                float diamond = (Mathf.Abs(cx) + Mathf.Abs(cy)) / Mathf.Sqrt(2f) - size * 0.2f;
                return Mathf.Max(Edge(diamond), 0.25f);
            };

        /// <summary>Winieta: przezroczysty środek, nieprzezroczyste krawędzie.</summary>
        public static Func<float, float, float> Vignette(int size) =>
            (x, y) =>
            {
                float dx = (x / size - 0.5f) * 2f, dy = (y / size - 0.5f) * 2f;
                float r = Mathf.Sqrt(dx * dx + dy * dy) / Mathf.Sqrt(2f);
                return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, r));
            };

        // ------------------------------------------------------------ ikony (128×128)

        static float DistSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 pa = p - a, ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude;
        }

        /// <summary>Odległość ze znakiem od wielokąta (ujemna wewnątrz).</summary>
        static float DistPolygon(Vector2 p, Vector2[] v)
        {
            float d = float.MaxValue;
            bool inside = false;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i++)
            {
                d = Mathf.Min(d, DistSegment(p, v[j], v[i]));
                if ((v[i].y > p.y) != (v[j].y > p.y) &&
                    p.x < (v[j].x - v[i].x) * (p.y - v[i].y) / (v[j].y - v[i].y) + v[i].x)
                    inside = !inside;
            }
            return inside ? -d : d;
        }

        static Vector2[] Scale(Vector2[] pts, float size)
        {
            var r = new Vector2[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                r[i] = pts[i] * size;
            return r;
        }

        public static Func<float, float, float> Polygon(int size, Vector2[] normalized)
        {
            var pts = Scale(normalized, size);
            return (x, y) => Edge(DistPolygon(new Vector2(x, y), pts));
        }

        public static Func<float, float, float> Star(int size)
        {
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float ang = Mathf.PI / 2f + i * Mathf.PI / 5f;
                float r = i % 2 == 0 ? 0.47f : 0.2f;
                pts[i] = new Vector2(0.5f + Mathf.Cos(ang) * r, 0.47f + Mathf.Sin(ang) * r);
            }
            return Polygon(size, pts);
        }

        public static Func<float, float, float> Heart(int size)
        {
            float s = size;
            var c1 = new Vector2(0.33f, 0.62f) * s;
            var c2 = new Vector2(0.67f, 0.62f) * s;
            float r = 0.2f * s;
            var tri = Scale(new[] { new Vector2(0.14f, 0.56f), new Vector2(0.86f, 0.56f), new Vector2(0.5f, 0.1f) }, s);
            return (x, y) =>
            {
                var p = new Vector2(x, y);
                float d = Mathf.Min((p - c1).magnitude - r, (p - c2).magnitude - r);
                d = Mathf.Min(d, DistPolygon(p, tri));
                return Edge(d);
            };
        }

        public static Func<float, float, float> Stroke(int size, Vector2[] normalized, float thickness)
        {
            var pts = Scale(normalized, size);
            float t = thickness * size;
            return (x, y) =>
            {
                var p = new Vector2(x, y);
                float d = float.MaxValue;
                for (int i = 0; i < pts.Length - 1; i++)
                    d = Mathf.Min(d, DistSegment(p, pts[i], pts[i + 1]));
                return Edge(d - t);
            };
        }

        public static Func<float, float, float> Cross(int size, float thickness)
        {
            float t = thickness * size;
            var a1 = new Vector2(0.22f, 0.22f) * size; var b1 = new Vector2(0.78f, 0.78f) * size;
            var a2 = new Vector2(0.22f, 0.78f) * size; var b2 = new Vector2(0.78f, 0.22f) * size;
            return (x, y) =>
            {
                var p = new Vector2(x, y);
                return Edge(Mathf.Min(DistSegment(p, a1, b1), DistSegment(p, a2, b2)) - t);
            };
        }
    }
}
