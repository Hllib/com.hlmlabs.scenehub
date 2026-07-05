using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLMLabs.SceneHub.Editor
{
    public sealed class SceneHubWindow : EditorWindow
    {
        private SceneHubController controller;
        private readonly SceneHubViewState viewState = new();

        [MenuItem(SceneHubConstants.MenuPath)]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneHubWindow>(SceneHubConstants.WindowTitle);
            window.minSize = new Vector2(320, 250);
        }

        private void OnEnable()
        {
            EnsureController();
            EditorApplication.projectChanged += OnProjectChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
        }

        private void EnsureController()
        {
            SceneHubResources.EnsureIconsLoaded();

            if (controller != null)
                return;

            controller = new SceneHubController();
            titleContent = new GUIContent(SceneHubConstants.WindowTitle, SceneHubResources.SceneIcon);
            controller.RefreshCatalog();
        }

        private void OnProjectChanged()
        {
            if (controller == null)
                return;

            controller.RefreshCatalog();
            Repaint();
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            Repaint();
        }

        private void OnGUI()
        {
            EnsureController();
            SceneHubResources.EnsureStylesInitialized();

            var activeScene = EditorSceneManager.GetActiveScene();
            var viewModel = controller.BuildViewModel(viewState, activeScene.path, activeScene.isDirty);

            SceneHubGui.HandleKeyboardShortcuts(RefreshCatalog, viewState);
            SceneHubGui.DrawDefaultSceneBar(viewModel.DefaultScene);

            EditorGUILayout.Space(2);
            SceneHubGui.DrawToolbar(controller, viewState, RefreshCatalog);
            EditorGUILayout.Space(4);

            viewState.ScrollPosition = EditorGUILayout.BeginScrollView(viewState.ScrollPosition);
            SceneHubGui.DrawSections(controller, viewModel);
            EditorGUILayout.EndScrollView();
        }

        private void RefreshCatalog()
        {
            EnsureController();
            controller.RefreshCatalog();
            Repaint();
        }
    }
}
