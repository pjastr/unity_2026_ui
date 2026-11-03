using UnityEngine;

namespace UIDemo
{
    /// <summary>Obraca obiekt (np. Canvas World Space z paskiem zdrowia) tak, by był zwrócony do kamery.</summary>
    public class Billboard : MonoBehaviour
    {
        public Camera cam;
        [Tooltip("true – tylko obrót wokół osi Y (pasek nie przechyla się w pionie)")]
        public bool yawOnly;

        void LateUpdate()
        {
            var c = cam != null ? cam : Camera.main;
            if (c == null) return;
            if (yawOnly)
            {
                Vector3 fwd = c.transform.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 1e-4f)
                    transform.rotation = Quaternion.LookRotation(fwd);
            }
            else
            {
                // ta sama orientacja co kamera – UI równoległe do ekranu
                transform.rotation = c.transform.rotation;
            }
        }
    }
}
