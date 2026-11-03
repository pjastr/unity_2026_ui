using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UIDemo.EditorTools
{
    /// <summary>
    /// Okno "UI Demo > Panel scen": lista scen z przyciskami Otwórz / Play / Generuj ponownie.
    /// </summary>
    public class SceneBrowserWindow : EditorWindow
    {
        Vector2 m_Scroll;

        public static void ShowWindow()
        {
            var w = GetWindow<SceneBrowserWindow>("UI Demo – sceny");
            w.minSize = new Vector2(440, 420);
        }

        void OnGUI()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Sceny demonstracyjne: UI i UX w Unity (uGUI, UI Toolkit)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Otwórz – otwiera scenę w edytorze.  ▶ – otwiera i uruchamia Play.\n" +
                                    "↻ – generuje scenę od nowa (ręczne zmiany w tej scenie zostaną nadpisane).", MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                // akcje zmieniające sceny wykonujemy poza OnGUI (delayCall), żeby nie psuć układu okna
                if (GUILayout.Button("Generuj wszystkie sceny", GUILayout.Height(26)))
                    EditorApplication.delayCall += SceneBuilder.GenerateAllMenu;
                if (GUILayout.Button("Zbuduj Windows x64", GUILayout.Height(26)))
                    EditorApplication.delayCall += SceneBuilder.BuildWindowsMenu;
            }

            EditorGUILayout.Space(6);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            foreach (var def in SceneBuilder.Scenes)
            {
                string path = SceneBuilder.ScenePath(def.file);
                bool exists = File.Exists(path);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = true;
                    EditorGUILayout.LabelField(def.file, exists ? EditorStyles.label : EditorStyles.miniLabel, GUILayout.MinWidth(220));
                    GUI.enabled = exists && !EditorApplication.isPlaying;
                    var d = def;
                    if (GUILayout.Button("Otwórz", GUILayout.Width(70)))
                        EditorApplication.delayCall += () =>
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                EditorSceneManager.OpenScene(path);
                        };
                    if (GUILayout.Button("▶", GUILayout.Width(30)))
                        EditorApplication.delayCall += () =>
                        {
                            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                            EditorSceneManager.OpenScene(path);
                            EditorApplication.isPlaying = true;
                        };
                    GUI.enabled = !EditorApplication.isPlaying;
                    if (GUILayout.Button("↻", GUILayout.Width(30)))
                        EditorApplication.delayCall += () =>
                        {
                            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                            SceneBuilder.GenerateScene(d);
                            SceneBuilder.UpdateBuildSettings();
                            AssetDatabase.SaveAssets();
                            EditorSceneManager.OpenScene(path);
                        };
                    GUI.enabled = true;
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
