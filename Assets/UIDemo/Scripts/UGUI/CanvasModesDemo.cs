using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Scena 01: tryby renderowania Canvas (Render Mode).
    ///  • Spacja – kolejny tryb: Screen Space – Overlay → Screen Space – Camera → World Space,
    ///  • Shift – kamera krąży wokół sceny / stoi.
    /// Sześcian przelatuje przez płaszczyznę UI: w trybie Camera częściowo zasłania panel (UI ma głębokość),
    /// w trybie Overlay zawsze jest pod UI, w trybie World Space panel jest zwykłym obiektem 3D.
    /// </summary>
    public class CanvasModesDemo : DemoActionBase
    {
        public Canvas demoCanvas;
        public Camera cam;
        [Tooltip("Położenie i obrót panelu w trybie World Space")]
        public Transform worldPose;
        public Transform movingCube;
        public float planeDistance = 6f;
        public float worldScale = 0.004f;
        public Vector2 worldSize = new Vector2(1200f, 700f);

        int m_Mode;
        bool m_Orbit;
        float m_Angle;
        Vector3 m_CamStartPos;
        Quaternion m_CamStartRot;

        static readonly RenderMode[] Modes =
            { RenderMode.ScreenSpaceOverlay, RenderMode.ScreenSpaceCamera, RenderMode.WorldSpace };

        void Start()
        {
            m_CamStartPos = cam.transform.position;
            m_CamStartRot = cam.transform.rotation;
            Apply();
        }

        public override void OnMainAction()
        {
            m_Mode = (m_Mode + 1) % Modes.Length;
            Apply();
            UiSound.Play(UiSound.Kind.Click);
        }

        public override void OnSecondaryAction()
        {
            m_Orbit = !m_Orbit;
            if (!m_Orbit)
            {
                cam.transform.SetPositionAndRotation(m_CamStartPos, m_CamStartRot);
                m_Angle = 0f;
            }
            DemoHud.Log(m_Orbit ? "kamera krąży" : "kamera w pozycji początkowej");
        }

        void Apply()
        {
            var mode = Modes[m_Mode];
            demoCanvas.renderMode = mode;
            var rt = (RectTransform)demoCanvas.transform;
            switch (mode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    break;
                case RenderMode.ScreenSpaceCamera:
                    demoCanvas.worldCamera = cam;
                    demoCanvas.planeDistance = planeDistance;
                    break;
                case RenderMode.WorldSpace:
                    demoCanvas.worldCamera = cam; // kamera zdarzeń (raycast kliknięć)
                    rt.sizeDelta = worldSize;
                    rt.SetPositionAndRotation(worldPose.position, worldPose.rotation);
                    rt.localScale = Vector3.one * worldScale;
                    break;
            }
            DemoHud.Log("Render Mode = " + mode);
        }

        void Update()
        {
            if (m_Orbit)
            {
                m_Angle += 20f * Time.deltaTime;
                Vector3 center = new Vector3(0f, 1.5f, 6f);
                Vector3 offset = Quaternion.Euler(0f, m_Angle, 0f) * (m_CamStartPos - center);
                cam.transform.position = center + offset;
                cam.transform.LookAt(center);
            }

            if (movingCube != null)
            {
                // sześcian przelatuje przez płaszczyznę UI (odległość planeDistance przed kamerą)
                float z = planeDistance + Mathf.Sin(Time.time * 0.8f) * 2.5f;
                movingCube.position = m_CamStartPos + m_CamStartRot * new Vector3(1.6f, -0.2f, z);
                movingCube.Rotate(30f * Time.deltaTime, 45f * Time.deltaTime, 0f, Space.Self);
            }

            var mode = Modes[m_Mode];
            string info;
            switch (mode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    info = "UI rysowane na wierzchu całej sceny, bez udziału kamery.\nSześcian zawsze jest POD panelem.";
                    break;
                case RenderMode.ScreenSpaceCamera:
                    info = $"UI na płaszczyźnie {demoCanvas.planeDistance:0.0} m przed kamerą (Plane Distance).\n" +
                           "Obiekty 3D bliżej kamery zasłaniają panel; działa post-processing kamery.";
                    break;
                default:
                    info = $"Canvas jest obiektem sceny: skala {worldScale}, rozmiar {worldSize.x:0}×{worldSize.y:0} px\n" +
                           $"= {worldSize.x * worldScale:0.0} × {worldSize.y * worldScale:0.0} m. Kamera zdarzeń: {cam.name}.";
                    break;
            }
            DemoHud.SetStatus("canvas",
                $"<b>Render Mode:</b> {mode}\n{info}\n" +
                $"Sort Order = {demoCanvas.sortingOrder}   scaleFactor = {demoCanvas.scaleFactor:0.00}\n" +
                $"Kamera: {(m_Orbit ? "krąży" : "stoi")}");
        }
    }
}
