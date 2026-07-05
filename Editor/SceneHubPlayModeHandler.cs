using UnityEditor;
using UnityEditor.SceneManagement;

namespace HLMLabs.SceneHub.Editor
{
    [InitializeOnLoad]
    internal static class SceneHubPlayModeHandler
    {
        private static bool isPlayingFromHub;
        private static bool isChangingToDefaultScene;
        private static string lastSceneBeforePlayFromHub;
        private static bool restoreSceneAfterPlayFromHub;

        static SceneHubPlayModeHandler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void BeginPlayFromHub(string currentScenePath)
        {
            lastSceneBeforePlayFromHub = currentScenePath;
            restoreSceneAfterPlayFromHub = true;
            isPlayingFromHub = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                TryRestoreSceneAfterPlayFromHub();
                TryRestoreSceneAfterDefaultPlay();
                return;
            }

            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            if (isPlayingFromHub || isChangingToDefaultScene)
            {
                isPlayingFromHub = false;
                return;
            }

            var defaultScene = EditorUserSettings.GetConfigValue(SceneHubConstants.DefaultSceneKey) ?? string.Empty;
            if (string.IsNullOrEmpty(defaultScene))
                return;

            var currentScene = EditorSceneManager.GetActiveScene().path;
            if (currentScene == defaultScene)
                return;

            EditorApplication.isPlaying = false;
            isChangingToDefaultScene = true;

            EditorUserSettings.SetConfigValue(SceneHubConstants.LastSceneBeforePlayKey, currentScene);

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(defaultScene);
                EditorApplication.delayCall += () =>
                {
                    isChangingToDefaultScene = false;
                    EditorApplication.isPlaying = true;
                };
            }
            else
            {
                isChangingToDefaultScene = false;
                EditorUserSettings.SetConfigValue(SceneHubConstants.LastSceneBeforePlayKey, string.Empty);
            }
        }

        private static void TryRestoreSceneAfterPlayFromHub()
        {
            if (!restoreSceneAfterPlayFromHub || string.IsNullOrEmpty(lastSceneBeforePlayFromHub))
                return;

            EditorSceneManager.OpenScene(lastSceneBeforePlayFromHub);
            restoreSceneAfterPlayFromHub = false;
            lastSceneBeforePlayFromHub = null;
        }

        private static void TryRestoreSceneAfterDefaultPlay()
        {
            var lastScene = EditorUserSettings.GetConfigValue(SceneHubConstants.LastSceneBeforePlayKey) ?? string.Empty;
            if (string.IsNullOrEmpty(lastScene))
                return;

            if (EditorSceneManager.GetActiveScene().path != lastScene)
                EditorSceneManager.OpenScene(lastScene);

            EditorUserSettings.SetConfigValue(SceneHubConstants.LastSceneBeforePlayKey, string.Empty);
        }
    }
}
