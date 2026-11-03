# UI i UX w Unity – sceny demonstracyjne

Projekt Unity **6.3 LTS (6000.3.24f1)** do wykładu 5 „UI i UX w Unity”. Pokazuje system **uGUI** (Canvas, RectTransform, Canvas Scaler, Image, TextMeshPro, kontrolki, nawigacja, układ automatyczny, UI w przestrzeni świata), zagadnienia **UX** (informacja zwrotna, menu i pauza, ustawienia, drag & drop, prawa Fittsa i Hicka, dostępność) oraz **UI Toolkit** (UXML, USS, ListView, wiązanie danych, panele w przestrzeni świata).

Każde zagadnienie ma **osobną scenę** (`01_…` – `18_…`). W każdej scenie jest panel z opisem po polsku (lewy górny róg), panel stanu z wartościami na żywo i dziennikiem zdarzeń (prawy górny róg) oraz pasek ze sterowaniem (dół ekranu).

- Render pipeline: **URP** (ustawienia z szablonu „Universal 3D”)
- Pakiety: **uGUI 2.0** (z TextMeshPro), **Input System**, moduły UI Toolkit (`uielements`) i Accessibility – bez dodatkowych pakietów
- Wejście: **Input System** – wyłącznie akcje z domyślnego pliku `Assets/InputSystem_Actions.inputactions` (Project-wide Actions); Event System używa modułu **Input System UI Input Module** z mapą akcji **UI**
- Grafiki (sprite'y) są **generowane kodem** do `Assets/UIDemo/Sprites` – projekt nie zawiera zewnętrznych plików graficznych ani dźwiękowych (dźwięki interfejsu są syntetyzowane w `UiSound.cs`)

---

## 1. Szybki start

1. Otwórz projekt w Unity Hub (wersja **6000.3.24f1**) albo uruchom `Narzedzia/Otworz_projekt_w_Unity.bat`. Przy pierwszym otwarciu Unity pobierze pakiety – może to potrwać kilka minut.
2. Sceny są **generowane kodem**. Przy pierwszym otwarciu pojawi się pytanie „Wygenerować teraz 18 scen?” – wybierz **Generuj**. Przy pierwszym generowaniu projekt importuje też **TMP Essential Resources** (domyślna czcionka TextMeshPro). Można to zrobić także później: menu **UI Demo → Generuj wszystkie sceny**.
3. **UI Demo → Otwórz pierwszą scenę i uruchom Play** (albo otwórz dowolną scenę z `Assets/UIDemo/Scenes` i naciśnij Play).
4. Klawiszami **1 / 2** przechodzisz do poprzedniej / następnej sceny (działa także w buildzie).

> Jeżeli pojawi się okno „TMP Importer” – kliknij **Import TMP Essentials**, a potem ponownie **UI Demo → Generuj wszystkie sceny**.
> Jeżeli klawisze nie działają – **UI Demo → Napraw ustawienia projektu** i uruchom ponownie edytor.
> W oknie **Game** ustaw proporcje 16:9 (np. *Full HD 1920×1080*); w scenie 03 zmieniaj je celowo.

### Build Windows (opcjonalnie)
`Narzedzia/Generuj_sceny_i_zbuduj_Windows.bat` (edytor z tym projektem musi być zamknięty) – generuje sceny i buduje `Builds/Windows/UIDemo.exe`. Log: `Logs/batch_build.log`. Niektóre funkcje działają tylko w buildzie: zmiana rozdzielczości (scena 12) i czytnik ekranu Windows Narrator (scena 15).

---

## 2. Menu „UI Demo” w edytorze

| Polecenie | Działanie |
|---|---|
| **Generuj wszystkie sceny** | Tworzy od nowa 18 scen, sprite'y, `UiSkin` i Panel Settings oraz ustawia Build Settings |
| **Panel scen** | Okno z listą scen: *Otwórz*, ▶ (otwórz i Play), ↻ (wygeneruj tylko tę scenę) |
| **Otwórz pierwszą scenę i uruchom Play** | Skrót do pokazu |
| **Zbuduj wersję Windows (x64)** | Build do `Builds/Windows/UIDemo.exe` |
| **Napraw ustawienia projektu** | Active Input Handling = Input System, Project-wide Actions, TMP Essential Resources, Build Settings |
| **Otwórz instrukcję (README.md)** | Otwiera ten plik |

**Uwaga:** ponowne generowanie nadpisuje ręczne zmiany w scenach. Eksperymentuj, zmieniając wartości w Inspectorze w trakcie Play, albo zrób kopię sceny.

---

## 3. Sterowanie (domyślne akcje Input System)

| Klawisz / mysz | Akcja z `InputSystem_Actions` | Działanie |
|---|---|---|
| **Spacja** | Player/Jump | akcja główna sceny |
| **lewy Shift** | Player/Sprint | akcja dodatkowa sceny |
| **mysz, LPM** | UI/Point, UI/Click | obsługa interfejsu |
| **strzałki / WASD / lewa gałka pada** | UI/Navigate | zmiana zaznaczenia (nawigacja) |
| **Enter / przycisk A** | UI/Submit | zatwierdzenie |
| **Esc / przycisk B** | UI/Cancel | anulowanie, cofnięcie (scena 11: pauza) |
| **kółko myszy** | UI/ScrollWheel | przewijanie list |
| **C** | Player/Crouch | pokaż / ukryj panel opisu |
| **przytrzymaj E** | Player/Interact (Hold) | reset sceny |
| **1 / 2** | Player/Previous / Player/Next | poprzednia / następna scena |

Skróty sceny są wyłączone, gdy piszesz w polu tekstowym (InputField).

---

## 4. Opis scen

### Część I – uGUI

| Scena | Co pokazuje | Spacja | Shift |
|---|---|---|---|
| **01_Canvas_TrybyRenderowania** | Render Mode: Screen Space – Overlay, Screen Space – Camera (Plane Distance, obiekt 3D przecina UI), World Space | kolejny tryb | kamera krąży |
| **02_RectTransform_Kotwice** | kotwice w punkcie i rozciągnięte, pivot, marginesy; ramka-rodzic zmienia rozmiar; romby = anchorMin/anchorMax | kolejne proporcje rodzica | płynna zmiana rozmiaru |
| **03_CanvasScaler_Rozdzielczosc** | Constant Pixel Size, Scale With Screen Size (Match 0 / 0,5 / 1), Constant Physical Size; wzór na scaleFactor; Screen.safeArea | kolejny tryb | bezpieczny obszar |
| **04_Image_Sprite_9Slice** | Image Type: Simple, Sliced (9-slice), Tiled, Filled; Rect Mask 2D i Mask | animacja rozmiaru | metoda wypełnienia |
| **05_TextMeshPro_Tekst** | znaczniki rich text, obrys SDF, Overflow / Ellipsis / Truncate / Auto Size, długość tekstów PL / EN / DE | kolejny język | tryb przepełnienia |
| **06_Kontrolki_Zdarzenia** | Button (onClick w kodzie i w Inspectorze), Toggle, ToggleGroup, Slider, TMP_Dropdown, TMP_InputField; dziennik zdarzeń | Transition: Color Tint / Sprite Swap / None | interactable |
| **07_Nawigacja_Klawiatura_Pad** | zaznaczenie EventSystem, Navigation: Automatic / Horizontal / Vertical / Explicit / None, ramka zaznaczenia | tryb Navigation | usuń zaznaczenie / ponowne zaznaczanie |
| **08_Layout_ScrollView** | Vertical / Horizontal / Grid Layout Group, Layout Element, Content Size Fitter, ScrollRect | dodaj przedmiot | Constraint siatki |
| **09_HUD_Feedback** | pasek zdrowia z opóźnionym śladem, HUD adaptacyjny, liczby obrażeń, winieta, cooldown (Filled Radial 360), powiadomienia | obrażenia | leczenie (odnowienie 4 s) |
| **10_WorldSpace_Diegeza** | UI przestrzenne (paski nad przeciwnikami, Billboard), diegetyczne (ekran terminala), niediegetyczne (znacznik celu, strzałka poza ekranem) | trafienie przeciwnika | kamera krąży |

### Część II – UX

| Scena | Co pokazuje | Spacja | Shift |
|---|---|---|---|
| **11_Menu_Pauza_Przejscia** | stos ekranów, Esc cofa, powrót zaznaczenia, `Time.timeScale = 0` vs `unscaledDeltaTime`, CanvasGroup, okno potwierdzenia | pauza (w rozgrywce) | długość przejść |
| **12_Ustawienia_PlayerPrefs** | głośność, pełny ekran, rozdzielczość z potwierdzeniem (10 s), jakość, skala UI, rozmiar tekstu, napisy; zapis w PlayerPrefs | ustawienia domyślne | wczytaj z PlayerPrefs |
| **13_DragDrop_Tooltip** | ekwipunek: IBeginDragHandler / IDragHandler / IEndDragHandler / IDropHandler, zamiana, kosz; podpowiedź z opóźnieniem | sortowanie | opóźnienie podpowiedzi |
| **14_Prawa_UX_Fitts_Hick** | pomiar czasu trafienia w cel (ID = log2(D/W + 1)) i czasu wyboru z n opcji; dopasowanie prostej T = a + b·x | Fitts ↔ Hick | wyczyść wyniki |
| **15_Dostepnosc** | skala tekstu 50–200%, wysoki kontrast, kolor + kształt, napisy, symulacja daltonizmu, AccessibilitySettings, AccessibilityHierarchy (czytnik ekranu) | symulacja daltonizmu | hierarchia czytnika ekranu |

### Część III – UI Toolkit

| Scena | Co pokazuje | Spacja | Shift |
|---|---|---|---|
| **16_UIToolkit_UXML_USS** | UIDocument + Panel Settings, `MainMenu.uxml`, `MainMenu.uss`: zmienne, `:hover`, `:focus`, przejścia; obsługa w C# (`Q<Button>`, `clicked`, `RegisterValueChangedCallback`) | motyw jasny / ciemny | klasa `.compact` |
| **17_UIToolkit_ListView_Binding** | ListView z 1000 elementów (wirtualizacja), runtime data binding: `dataSource`, `DataBinding`, ToTarget / TwoWay, `[CreateProperty]` | zmiana danych | +100 przedmiotów |
| **18_UIToolkit_WorldSpace** | Panel Settings z Render Mode = World Space, UIDocument: World Space Size Mode = Fixed | nowe wartości | Billboard |

---

## 5. Struktura projektu

```
Assets/
  InputSystem_Actions.inputactions   domyślne akcje (Project-wide Actions)
  Settings/                          ustawienia URP
  UIDemo/
    Scripts/Core/      DemoHud, DemoInput, DemoActionBase, UiFactory (tworzenie elementów uGUI), UiSkin, UiSound, UiFeedback
    Scripts/UGUI/      sceny 01–10 (CanvasModesDemo, AnchorsDemo, ScalerDemo, … OffscreenIndicator, WorldHealthBar, Billboard)
    Scripts/UX/        sceny 11–15 (MenuDemo, SettingsDemo, InventoryItem/Slot, Tooltip, UxLawsDemo, AccessibilityDemo)
    Scripts/Toolkit/   sceny 16–18 (ToolkitMenuDemo, ToolkitListBindingDemo, PlayerStats, ToolkitWorldDemo)
    UI/                pliki UXML i USS (MainMenu, MainMenuLight, Inventory, WorldPanel) + motyw TSS (tworzony przy generowaniu)
    Editor/            generator scen (SceneBuilder, SceneDefinitions_*, SpriteGenerator), panel scen, pierwsze uruchomienie
    Scenes/ Sprites/ Data/ Materials/   (tworzone przez generator)
  TextMesh Pro/        TMP Essential Resources (importowane przez generator)
Narzedzia/             skrypty .bat: otwarcie projektu, generowanie scen, build Windows
```

Pliki UXML i USS można otworzyć w **UI Builder** (dwuklik). Drzewo elementów i style w trakcie gry: **Window → UI Toolkit → Debugger**.
