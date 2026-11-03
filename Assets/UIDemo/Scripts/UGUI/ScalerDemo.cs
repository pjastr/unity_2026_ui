using UnityEngine;
using UnityEngine.UI;

namespace UIDemo
{
    /// <summary>
    /// Scena 03: Canvas Scaler – jak UI skaluje się przy różnych rozdzielczościach.
    ///  • Spacja – kolejny tryb: Constant Pixel Size → Scale With Screen Size (Match 0 / 0,5 / 1) → Constant Physical Size,
    ///  • Shift – pokaż/ukryj bezpieczny obszar ekranu (Screen.safeArea).
    /// Zmień rozmiar okna Game (np. Free Aspect, 1280×720, 2560×1080, 4K), aby zobaczyć różnicę.
    /// </summary>
    public class ScalerDemo : DemoActionBase
    {
        public CanvasScaler scaler;
        public Canvas canvas;
        public GameObject safeAreaPanel;

        int m_Mode = 2;

        struct ModeDef
        {
            public string name;
            public CanvasScaler.ScaleMode mode;
            public float match;
            public ModeDef(string n, CanvasScaler.ScaleMode m, float match) { name = n; mode = m; this.match = match; }
        }

        static readonly ModeDef[] Modes =
        {
            new ModeDef("Constant Pixel Size", CanvasScaler.ScaleMode.ConstantPixelSize, 0f),
            new ModeDef("Scale With Screen Size, Match = 0 (szerokość)", CanvasScaler.ScaleMode.ScaleWithScreenSize, 0f),
            new ModeDef("Scale With Screen Size, Match = 0,5", CanvasScaler.ScaleMode.ScaleWithScreenSize, 0.5f),
            new ModeDef("Scale With Screen Size, Match = 1 (wysokość)", CanvasScaler.ScaleMode.ScaleWithScreenSize, 1f),
            new ModeDef("Constant Physical Size", CanvasScaler.ScaleMode.ConstantPhysicalSize, 0f),
        };

        void Start() => Apply();

        public override void OnMainAction()
        {
            m_Mode = (m_Mode + 1) % Modes.Length;
            Apply();
        }

        public override void OnSecondaryAction()
        {
            if (safeAreaPanel == null) return;
            safeAreaPanel.SetActive(!safeAreaPanel.activeSelf);
            DemoHud.Log("bezpieczny obszar: " + (safeAreaPanel.activeSelf ? "widoczny" : "ukryty"));
        }

        void Apply()
        {
            var m = Modes[m_Mode];
            scaler.uiScaleMode = m.mode;
            scaler.matchWidthOrHeight = m.match;
            scaler.scaleFactor = 1f;
            scaler.physicalUnit = CanvasScaler.Unit.Millimeters;
            scaler.fallbackScreenDPI = 96f;
            scaler.defaultSpriteDPI = 96f;
            DemoHud.Log("UI Scale Mode = " + m.name);
        }

        void Update()
        {
            var m = Modes[m_Mode];
            float w = Screen.width, h = Screen.height;
            Vector2 refRes = scaler.referenceResolution;
            // ten sam wzór, którego używa CanvasScaler (średnia ważona w skali log2)
            float logW = Mathf.Log(w / refRes.x, 2f);
            float logH = Mathf.Log(h / refRes.y, 2f);
            float formula = Mathf.Pow(2f, Mathf.Lerp(logW, logH, scaler.matchWidthOrHeight));
            Vector2 canvasSize = ((RectTransform)canvas.transform).rect.size;
            Rect safe = Screen.safeArea;
            float dpi = Screen.dpi > 0f ? Screen.dpi : scaler.fallbackScreenDPI;

            string extra = m.mode == CanvasScaler.ScaleMode.ScaleWithScreenSize
                ? $"wzór: 2^((1−m)·log2({w:0}/{refRes.x:0}) + m·log2({h:0}/{refRes.y:0})) = {formula:0.000}"
                : m.mode == CanvasScaler.ScaleMode.ConstantPhysicalSize
                    ? $"DPI ekranu = {dpi:0} (gdy nieznane: Fallback Screen DPI = {scaler.fallbackScreenDPI:0})"
                    : "1 piksel UI = 1 piksel ekranu (scaleFactor = 1)";

            DemoHud.SetStatus("scaler",
                $"<b>UI Scale Mode:</b> {m.name}\n" +
                $"Ekran: {w:0} × {h:0} px   Reference Resolution: {refRes.x:0} × {refRes.y:0}\n" +
                $"<b>canvas.scaleFactor = {canvas.scaleFactor:0.000}</b>\n{extra}\n" +
                $"Rozmiar Canvasa w jednostkach UI: {canvasSize.x:0} × {canvasSize.y:0}\n" +
                $"Screen.safeArea: x={safe.x:0} y={safe.y:0} w={safe.width:0} h={safe.height:0}\n" +
                "<i>Zmieniaj rozmiar okna Game, aby porównać tryby.</i>");
        }
    }
}
