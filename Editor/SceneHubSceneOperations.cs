using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HLMLabs.SceneHub.Editor
{
    internal sealed class SceneHubSceneOperations
    {
        private readonly SceneHubPreferences preferences;
        private readonly System.Action onCatalogChanged;

        public SceneHubSceneOperations(SceneHubPreferences preferences, System.Action onCatalogChanged)
        {
            this.preferences = preferences;
            this.onCatalogChanged = onCatalogChanged;
        }

        public bool OpenScene(string scenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            EditorSceneManager.OpenScene(scenePath);
            preferences.AddToRecent(scenePath);
            return true;
        }

        public void OpenSceneAdditive(string scenePath)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            preferences.AddToRecent(scenePath);
        }

        public bool RunScene(string scenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            SceneHubPlayModeHandler.BeginPlayFromHub(EditorSceneManager.GetActiveScene().path);
            EditorSceneManager.OpenScene(scenePath);
            preferences.AddToRecent(scenePath);
            EditorApplication.isPlaying = true;
            return true;
        }

        public void ToggleBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            var existingIndex = scenes.FindIndex(scene => scene.path == scenePath);

            if (existingIndex >= 0)
                scenes.RemoveAt(existingIndex);
            else
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
            onCatalogChanged();
        }

        public void CreateNewScene()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create New Scene",
                "NewScene",
                "unity",
                "Create a new scene");

            if (string.IsNullOrEmpty(path))
                return;

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, path);
            onCatalogChanged();
            preferences.AddToRecent(path);
        }

        public void DuplicateScene(string scenePath)
        {
            var directory = Path.GetDirectoryName(scenePath);
            var fileName = Path.GetFileNameWithoutExtension(scenePath);
            var extension = Path.GetExtension(scenePath);

            var newPath = Path.Combine(directory, $"{fileName}_Copy{extension}");
            var counter = 1;

            while (File.Exists(newPath))
            {
                newPath = Path.Combine(directory, $"{fileName}_Copy{counter}{extension}");
                counter++;
            }

            var savePath = EditorUtility.SaveFilePanelInProject(
                "Duplicate Scene",
                Path.GetFileNameWithoutExtension(newPath),
                "unity",
                "Save duplicated scene",
                directory);

            if (string.IsNullOrEmpty(savePath))
                return;

            AssetDatabase.CopyAsset(scenePath, savePath);
            AssetDatabase.Refresh();
            onCatalogChanged();

            var newSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(savePath);
            if (newSceneAsset != null)
                EditorGUIUtility.PingObject(newSceneAsset);
        }

        public void LocateScene(string scenePath)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
                return;

            EditorGUIUtility.PingObject(sceneAsset);
            Selection.activeObject = sceneAsset;
        }

        public void DeleteScene(string scenePath, bool isCurrentScene)
        {
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);

            if (isCurrentScene)
            {
                EditorUtility.DisplayDialog(
                    "Cannot Delete Scene",
                    $"Cannot delete '{sceneName}' because it is currently open.\n\nPlease open a different scene first.",
                    "OK");
                return;
            }

            var confirmed = EditorUtility.DisplayDialog(
                "Delete Scene?",
                $"Are you sure you want to delete '{sceneName}'?\n\nPath: {scenePath}\n\nThis action cannot be undone!",
                "Delete",
                "Cancel");

            if (!confirmed)
                return;

            preferences.RemoveSceneReferences(scenePath);
            AssetDatabase.DeleteAsset(scenePath);
            AssetDatabase.Refresh();
            onCatalogChanged();
        }
    }
}
