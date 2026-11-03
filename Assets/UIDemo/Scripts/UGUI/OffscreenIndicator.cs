using TMPro;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Znacznik celu na Canvasie ekranowym (Overlay). Gdy cel jest widoczny – znacznik nad celem
    /// (Camera.WorldToScreenPoint). Gdy cel jest poza ekranem lub za kamerą – znacznik przy krawędzi ekranu
    /// i strzałka wskazująca kierunek.
    /// </summary>
    public class OffscreenIndicator : MonoBehaviour
    {
        public Transform target;
        public Camera cam;
        public RectTransform marker;
        public RectTransform arrow;
        public TMP_Text distanceText;
        public Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
        [Tooltip("Margines od krawędzi ekranu w pikselach")]
        public float margin = 70f;

        public bool OnScreen { get; private set; }

        void LateUpdate()
        {
            if (target == null || cam == null) return;
            Vector3 sp = cam.WorldToScreenPoint(target.position + worldOffset);
            bool behind = sp.z < 0f;
            if (behind)
                sp = new Vector3(Screen.width - sp.x, Screen.height - sp.y, -sp.z); // odbicie punktu za kamerą

            var screenRect = new Rect(margin, margin, Screen.width - 2f * margin, Screen.height - 2f * margin);
            OnScreen = !behind && screenRect.Contains(new Vector2(sp.x, sp.y));

            Vector2 pos = sp;
            if (!OnScreen)
            {
                // rzut na krawędź prostokąta ekranu wzdłuż kierunku od środka
                Vector2 center = new Vector2(Screen.width, Screen.height) * 0.5f;
                Vector2 dir = pos - center;
                if (dir.sqrMagnitude < 1f) dir = Vector2.down;
                float sx = (screenRect.width * 0.5f) / Mathf.Max(1e-3f, Mathf.Abs(dir.x));
                float sy = (screenRect.height * 0.5f) / Mathf.Max(1e-3f, Mathf.Abs(dir.y));
                pos = center + dir * Mathf.Min(sx, sy);
                arrow.gameObject.SetActive(true);
                arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            }
            else
            {
                arrow.gameObject.SetActive(false);
            }
            marker.position = pos; // Canvas Overlay: pozycja w pikselach ekranu
            float dist = Vector3.Distance(cam.transform.position, target.position);
            if (distanceText != null)
                distanceText.text = $"Cel  {dist:0} m";
        }
    }
}
