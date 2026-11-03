using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace UIDemo
{
    /// <summary>
    /// Dane postaci do wiązania danych UI Toolkit (runtime data binding).
    /// [CreateProperty] udostępnia właściwość systemowi wiązań; INotifyBindablePropertyChanged
    /// informuje UI, że wartość się zmieniła (odświeżane są tylko zmienione wiązania).
    /// </summary>
    public class PlayerStats : INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        string m_Name = "Bohater";
        float m_Health = 100f;
        int m_Gold = 50;

        [CreateProperty]
        public string Name
        {
            get => m_Name;
            set { if (m_Name == value) return; m_Name = value; Notify(nameof(Name)); }
        }

        [CreateProperty]
        public float Health
        {
            get => m_Health;
            set
            {
                if (Math.Abs(m_Health - value) < 1e-4f) return;
                m_Health = value;
                Notify(nameof(Health));
                Notify(nameof(HealthText));
            }
        }

        [CreateProperty]
        public int Gold
        {
            get => m_Gold;
            set { if (m_Gold == value) return; m_Gold = value; Notify(nameof(Gold)); Notify(nameof(GoldText)); }
        }

        [CreateProperty] public string HealthText => $"Zdrowie: {m_Health:0} / 100";
        [CreateProperty] public string GoldText => $"Złoto: {m_Gold}";

        void Notify(string property) =>
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
