using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UIDemo.EditorTools
{
    /// <summary>
    /// Przy pierwszym otwarciu projektu (brak wygenerowanych scen) proponuje wygenerowanie wszystkich scen.
    /// Pytanie pojawia się raz na sesję edytora; zawsze można też użyć menu UI Demo > Generuj wszystkie sceny.
    /// </summary>
    [InitializeOnLoad]
    static class FirstRunSetup
    {
        const string SessionKey = "UIDemo.FirstRunAsked";

        static FirstRunSetup()
        {
            EditorApplication.delayCall += Check;
        }

        static void Check()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (SessionState.GetBool(SessionKey, false))
                return;
            SessionState.SetBool(SessionKey, true);
            if (File.Exists(SceneBuilder.ScenePath(SceneBuilder.Scenes[0].file)))
                return;
            if (!EditorUtility.DisplayDialog("UI Demo – pierwsze uruchomienie",
                    "Sceny demonstracyjne nie są jeszcze wygenerowane.\n\nWygenerować teraz 18 scen (Assets/UIDemo/Scenes)?\nPrzy pierwszym generowaniu importowane są też TMP Essential Resources.\n" +
                    "Trwa to kilkanaście sekund. Można to zrobić później z menu UI Demo.",
                    "Generuj", "Później"))
                return;
            SceneBuilder.GenerateAllMenu();
        }
    }
}
