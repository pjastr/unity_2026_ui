using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UIDemo
{
    /// <summary>
    /// Dostęp do akcji z domyślnego, projektowego pliku akcji Input System
    /// (Assets/InputSystem_Actions.inputactions, ustawionego jako "Project-wide Actions").
    /// Nie tworzymy własnych akcji – korzystamy wyłącznie z map "Player" i "UI".
    ///
    /// Player/Jump      – Spacja        → akcja główna sceny
    /// Player/Sprint    – lewy Shift    → akcja dodatkowa sceny
    /// Player/Crouch    – C             → opis sceny
    /// Player/Interact  – E (przytrzymanie) → reset sceny
    /// Player/Previous  – 1, Player/Next – 2 → poprzednia / następna scena
    /// Player/Look      – ruch myszy
    /// UI/Navigate, UI/Submit, UI/Cancel, UI/Point, UI/Click, UI/RightClick, UI/ScrollWheel – obsługa interfejsu
    /// </summary>
    public static class DemoInput
    {
        static InputActionAsset s_Asset;
        static readonly Dictionary<string, InputAction> s_Cache = new Dictionary<string, InputAction>();
        static bool s_Warned;

        static bool Ensure()
        {
            if (s_Asset != null)
                return true;

            s_Cache.Clear();
            s_Asset = InputSystem.actions;
            if (s_Asset == null)
            {
                if (!s_Warned)
                {
                    s_Warned = true;
                    Debug.LogError("[UI Demo] Brak 'Project-wide Actions'. Ustaw Assets/InputSystem_Actions.inputactions " +
                                   "w Edit > Project Settings > Input System Package (albo użyj menu UI Demo > Napraw ustawienia projektu).");
                }
                return false;
            }

            if (!s_Asset.enabled)
                s_Asset.Enable();
            return true;
        }

        static InputAction Get(string path)
        {
            if (!Ensure())
                return null;
            if (s_Cache.TryGetValue(path, out var a) && a != null)
                return a;
            a = s_Asset.FindAction(path, false);
            if (a == null)
                Debug.LogWarning("[UI Demo] Nie znaleziono akcji: " + path);
            else if (!a.enabled)
                a.Enable();
            s_Cache[path] = a;
            return a;
        }

        public static InputAction Jump => Get("Player/Jump");
        public static InputAction Sprint => Get("Player/Sprint");
        public static InputAction Crouch => Get("Player/Crouch");
        public static InputAction Interact => Get("Player/Interact");
        public static InputAction Previous => Get("Player/Previous");
        public static InputAction Next => Get("Player/Next");
        public static InputAction Look => Get("Player/Look");
        public static InputAction Navigate => Get("UI/Navigate");
        public static InputAction Submit => Get("UI/Submit");
        public static InputAction Cancel => Get("UI/Cancel");
        public static InputAction Point => Get("UI/Point");
        public static InputAction Click => Get("UI/Click");
        public static InputAction RightClick => Get("UI/RightClick");
        public static InputAction ScrollWheel => Get("UI/ScrollWheel");

        public static bool PressedThisFrame(InputAction a) => a != null && a.WasPressedThisFrame();
        public static bool ReleasedThisFrame(InputAction a) => a != null && a.WasReleasedThisFrame();
        public static bool PerformedThisFrame(InputAction a) => a != null && a.WasPerformedThisFrame();
        public static bool IsPressed(InputAction a) => a != null && a.IsPressed();
        public static Vector2 ReadVector2(InputAction a) => a != null ? a.ReadValue<Vector2>() : Vector2.zero;

        public static Vector2 PointerPosition
        {
            get
            {
                var p = Point;
                if (p != null)
                    return p.ReadValue<Vector2>();
                return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            }
        }

        /// <summary>
        /// Czy gracz pisze w polu tekstowym? Wtedy skróty klawiszowe sceny (C, 1, 2, Spacja…) są wyłączone,
        /// żeby wpisanie znaku nie przełączało sceny.
        /// </summary>
        public static bool IsTyping
        {
            get
            {
                var es = EventSystem.current;
                if (es == null || es.currentSelectedGameObject == null)
                    return false;
                var input = es.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>();
                return input != null && input.isFocused;
            }
        }

        /// <summary>Nazwa urządzenia, które ostatnio było używane (do wyświetlania właściwych podpowiedzi).</summary>
        public static string LastDeviceName
        {
            get
            {
                InputDevice best = null;
                foreach (var d in InputSystem.devices)
                    if (best == null || d.lastUpdateTime > best.lastUpdateTime)
                        best = d;
                if (best is Gamepad) return "pad";
                if (best is Keyboard) return "klawiatura";
                if (best is Mouse || best is Pointer) return "mysz";
                return best != null ? best.displayName : "–";
            }
        }
    }
}
