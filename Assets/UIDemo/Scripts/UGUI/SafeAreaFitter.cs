using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Dopasowuje panel do bezpiecznego obszaru ekranu (wycięcia aparatu, zaokrąglone rogi telefonów).
    /// Na komputerze safeArea zwykle obejmuje cały ekran.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        Rect m_Last;
        Vector2Int m_LastScreen;

        void Update()
        {
            Rect safe = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (safe == m_Last && screen == m_LastScreen)
                return;
            m_Last = safe;
            m_LastScreen = screen;
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rt.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
