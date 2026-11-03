using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIDemo
{
    /// <summary>
    /// Scena 17: ListView (wirtualizacja – tylko widoczne wiersze istnieją w drzewie) i wiązanie danych.
    ///  • lista 1000 przedmiotów: makeItem tworzy wiersz, bindItem wypełnia go danymi,
    ///  • karta postaci: Label, ProgressBar i TextField powiązane z obiektem PlayerStats (DataBinding),
    ///  • Spacja – postać otrzymuje obrażenia i złoto (UI odświeża się samo dzięki wiązaniom),
    ///  • Shift – dodaj 100 przedmiotów (RefreshItems).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class ToolkitListBindingDemo : DemoActionBase
    {
        public int startItems = 1000;

        class ItemData
        {
            public string name;
            public int count;
            public Color color;
        }

        readonly List<ItemData> m_Items = new List<ItemData>();
        readonly PlayerStats m_Stats = new PlayerStats();
        ListView m_List;
        Label m_Header;
        Label m_Selected;

        static readonly string[] Names = { "Miecz", "Tarcza", "Mikstura", "Łuk", "Strzała", "Klucz", "Mapa", "Hełm", "Pierścień", "Zwój" };
        static readonly string[] Adjectives = { "zwykły", "solidny", "rzadki", "magiczny", "legendarny" };
        static readonly Color[] Rarity =
        {
            new Color(0.75f, 0.75f, 0.75f), new Color(0.4f, 0.85f, 0.4f), new Color(0.35f, 0.6f, 1f),
            new Color(0.7f, 0.45f, 1f), new Color(1f, 0.6f, 0.15f),
        };

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            m_List = root.Q<ListView>("items");
            m_Header = root.Q<Label>("list-header");
            m_Selected = root.Q<Label>("selected");
            AddItems(startItems);

            // --- ListView
            m_List.itemsSource = m_Items;
            m_List.fixedItemHeight = 44;
            m_List.selectionType = SelectionType.Single;
            m_List.makeItem = MakeRow;
            m_List.bindItem = BindRow;
            m_List.selectionChanged += OnSelection;
            m_List.RefreshItems();

            // --- wiązanie danych: dataSource ustawione na korzeniu jest dziedziczone przez elementy potomne
            root.dataSource = m_Stats;
            root.Q<Label>("player-name").SetBinding("text", new DataBinding
                { dataSourcePath = new PropertyPath(nameof(PlayerStats.Name)), bindingMode = BindingMode.ToTarget });
            var health = root.Q<ProgressBar>("health");
            health.SetBinding("value", new DataBinding
                { dataSourcePath = new PropertyPath(nameof(PlayerStats.Health)), bindingMode = BindingMode.ToTarget });
            health.SetBinding("title", new DataBinding
                { dataSourcePath = new PropertyPath(nameof(PlayerStats.HealthText)), bindingMode = BindingMode.ToTarget });
            root.Q<Label>("gold").SetBinding("text", new DataBinding
                { dataSourcePath = new PropertyPath(nameof(PlayerStats.GoldText)), bindingMode = BindingMode.ToTarget });
            root.Q<TextField>("name-field").SetBinding("value", new DataBinding
                { dataSourcePath = new PropertyPath(nameof(PlayerStats.Name)), bindingMode = BindingMode.TwoWay });
            UpdateHeader();
        }

        void AddItems(int n)
        {
            int start = m_Items.Count;
            for (int i = 0; i < n; i++)
            {
                int k = start + i;
                int rarity = (k * 37 % 100) < 55 ? 0 : (k * 37 % 100) < 80 ? 1 : (k * 37 % 100) < 92 ? 2 : (k * 37 % 100) < 98 ? 3 : 4;
                m_Items.Add(new ItemData
                {
                    name = $"{Names[k % Names.Length]} {Adjectives[rarity]} #{k + 1}",
                    count = 1 + k * 13 % 20,
                    color = Rarity[rarity],
                });
            }
        }

        static VisualElement MakeRow()
        {
            // wywoływane rzadko – tylko dla widocznych wierszy
            var row = new VisualElement();
            row.AddToClassList("row");
            var icon = new VisualElement { name = "icon" };
            icon.AddToClassList("row-icon");
            row.Add(icon);
            var name = new Label { name = "name" };
            name.AddToClassList("row-name");
            row.Add(name);
            var count = new Label { name = "count" };
            count.AddToClassList("row-count");
            row.Add(count);
            return row;
        }

        void BindRow(VisualElement row, int index)
        {
            // wywoływane przy każdym przewinięciu – element jest ponownie używany dla innych danych
            var item = m_Items[index];
            row.Q<VisualElement>("icon").style.backgroundColor = item.color;
            row.Q<Label>("name").text = item.name;
            row.Q<Label>("count").text = "x" + item.count;
        }

        void OnSelection(IEnumerable<object> selected)
        {
            var item = selected.FirstOrDefault() as ItemData;
            if (item == null) return;
            m_Selected.text = $"Wybrano: <b>{item.name}</b>, ilość: {item.count}";
            DemoHud.Log("ListView.selectionChanged: " + item.name);
        }

        void UpdateHeader() => m_Header.text = $"Ekwipunek ({m_Items.Count} przedmiotów)";

        public override void OnMainAction()
        {
            m_Stats.Health = m_Stats.Health <= 10f ? 100f : m_Stats.Health - 10f;
            m_Stats.Gold += 25;
            DemoHud.Log($"PlayerStats: Health = {m_Stats.Health:0}, Gold = {m_Stats.Gold}");
        }

        public override void OnSecondaryAction()
        {
            AddItems(100);
            m_List.RefreshItems();
            UpdateHeader();
            DemoHud.Log("dodano 100 przedmiotów – RefreshItems()");
        }

        void Update()
        {
            if (m_List == null) return;
            int rows = m_List.Query<VisualElement>(className: "row").ToList().Count;
            DemoHud.SetStatus("list",
                $"<b>ListView:</b> {m_Items.Count} przedmiotów, wierszy w drzewie UI: <b>{rows}</b>\n" +
                "(wirtualizacja – wiersze są używane ponownie przy przewijaniu)\n" +
                $"<b>PlayerStats</b> (dataSource): Name = \"{m_Stats.Name}\", Health = {m_Stats.Health:0}, Gold = {m_Stats.Gold}\n" +
                "Wiązania: Label.text, ProgressBar.value/title ← dane (ToTarget)\nTextField.value ↔ Name (TwoWay)");
        }
    }
}
