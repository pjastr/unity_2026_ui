using TMPro;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Zestaw grafik i kolorów wspólny dla wszystkich scen (asset Assets/UIDemo/Data/UiSkin.asset).
    /// Sprite'y są generowane kodem przez generator scen (menu UI Demo) – zob. Assets/UIDemo/Sprites.
    /// </summary>
    [CreateAssetMenu(menuName = "UI Demo/UI Skin", fileName = "UiSkin")]
    public class UiSkin : ScriptableObject
    {
        [Header("Tekst")]
        public TMP_FontAsset font;

        [Header("Sprite'y (generowane)")]
        [Tooltip("Zaokrąglony prostokąt z obramowaniem 9-slice (Border = 24 px)")]
        public Sprite rounded;
        [Tooltip("Sama ramka zaokrąglonego prostokąta (9-slice)")]
        public Sprite roundedOutline;
        public Sprite circle;
        [Tooltip("Wzór do trybu Tiled")]
        public Sprite tile;
        [Tooltip("Miękki gradient od krawędzi (winieta obrażeń)")]
        public Sprite vignette;
        public Sprite iconHeart;
        public Sprite iconStar;
        public Sprite iconTriangle;
        public Sprite iconSquare;
        public Sprite iconDiamond;
        public Sprite iconArrow;
        public Sprite iconCheck;
        public Sprite iconCross;

        [Header("Sprite'y wbudowane (UI/Skin)")]
        public Sprite builtinBackground;
        public Sprite builtinInputField;
        public Sprite builtinKnob;
        public Sprite builtinCheckmark;
        public Sprite builtinDropdownArrow;
        public Sprite builtinMask;

        [Header("Kolory")]
        public Color background = new Color(0.07f, 0.09f, 0.13f, 0.94f);
        public Color panel = new Color(0.14f, 0.17f, 0.23f, 0.96f);
        public Color control = new Color(0.24f, 0.29f, 0.38f, 1f);
        public Color accent = new Color(1f, 0.67f, 0.16f, 1f);
        public Color text = new Color(0.94f, 0.95f, 0.97f, 1f);
        public Color muted = new Color(0.66f, 0.71f, 0.79f, 1f);
        public Color good = new Color(0.32f, 0.8f, 0.42f, 1f);
        public Color bad = new Color(0.92f, 0.32f, 0.27f, 1f);
        public Color info = new Color(0.33f, 0.62f, 1f, 1f);
    }
}
