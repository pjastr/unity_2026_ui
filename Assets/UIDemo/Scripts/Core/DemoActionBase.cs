using System.Collections.Generic;
using UnityEngine;

namespace UIDemo
{
    /// <summary>
    /// Bazowa klasa dla komponentów reagujących na akcje sceny:
    ///  • akcja główna – Spacja (Player/Jump) → OnMainAction(),
    ///  • akcja dodatkowa – lewy Shift (Player/Sprint) → OnSecondaryAction().
    /// Komponenty rejestrują się same; DemoHud wywołuje metody na wszystkich aktywnych.
    /// (Lewy przycisk myszy i Enter zostają dla interfejsu – klikanie i zatwierdzanie.)
    /// </summary>
    public abstract class DemoActionBase : MonoBehaviour
    {
        static readonly List<DemoActionBase> s_All = new List<DemoActionBase>();

        protected virtual void OnEnable()
        {
            if (!s_All.Contains(this))
                s_All.Add(this);
        }

        protected virtual void OnDisable()
        {
            s_All.Remove(this);
        }

        /// <summary>Spacja (akcja Player/Jump).</summary>
        public virtual void OnMainAction() { }

        /// <summary>Lewy Shift (akcja Player/Sprint).</summary>
        public virtual void OnSecondaryAction() { }

        public static void InvokeMain()
        {
            foreach (var a in s_All.ToArray())
                if (a != null && a.isActiveAndEnabled)
                    a.OnMainAction();
        }

        public static void InvokeSecondary()
        {
            foreach (var a in s_All.ToArray())
                if (a != null && a.isActiveAndEnabled)
                    a.OnSecondaryAction();
        }
    }
}
