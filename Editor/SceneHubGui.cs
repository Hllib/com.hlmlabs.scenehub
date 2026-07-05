using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HLMLabs.SceneHub.Editor
{
    internal static class SceneHubGui
    {
        public static void HandleKeyboardShortcuts(Action refreshCatalog, SceneHubViewState viewState)
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown)
                return;

            if (e.keyCode == KeyCode.F5)
            {
                refreshCatalog();
                e.Use();
            }
            else if (e.keyCode == KeyCode.Escape)
            {
                viewState.SearchFilter = string.Empty;
                GUI.FocusControl(null);
                e.Use();
            }
        }

        public static void DrawToolbar(SceneHubController controller, SceneHubViewState viewState, Action refreshCatalog)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            viewState.SearchFilter = EditorGUILayout.TextField(
                viewState.SearchFilter,
                EditorStyles.toolbarSearchField,
                GUILayout.MinWidth(100));

            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(18)))
            {
                viewState.SearchFilter = string.Empty;
                GUI.FocusControl(null);
            }

            GUILayout.FlexibleSpace();

            var newCompactView = GUILayout.Toggle(
                controller.Preferences.IsCompactView,
                new GUIContent("⊟", "Compact View"),
                EditorStyles.toolbarButton,
                GUILayout.Width(24));

            if (newCompactView != controller.Preferences.IsCompactView)
            {
                controller.Preferences.IsCompactView = newCompactView;
                controller.Preferences.SaveCompactView();
            }

            if (GUILayout.Button(new GUIContent(SceneHubResources.RefreshIcon, "Refresh (F5)"), EditorStyles.toolbarButton, GUILayout.Width(28)))
                refreshCatalog();

            if (GUILayout.Button(new GUIContent(SceneHubResources.CreateIcon, "Create New Scene"), EditorStyles.toolbarButton, GUILayout.Width(28)))
                controller.Operations.CreateNewScene();

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawDefaultSceneBar(string defaultScene)
        {
            SceneHubResources.EnsureStylesInitialized();

            var bgColor = string.IsNullOrEmpty(defaultScene)
                ? SceneHubResources.DefaultBarEmpty
                : SceneHubResources.DefaultBarSet;

            var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            EditorGUI.DrawRect(rect, bgColor);

            GUILayout.Space(8);

            if (string.IsNullOrEmpty(defaultScene))
            {
                EditorGUILayout.LabelField("No default scene set", EditorStyles.miniLabel);
            }
            else
            {
                GUILayout.Label("★", SceneHubResources.DefaultStarLabel ?? EditorStyles.label, GUILayout.Width(14));
                EditorGUILayout.LabelField(
                    "Default: " + SceneHubUtil.GetDisplayName(defaultScene, isDirty: false),
                    EditorStyles.miniLabel);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawSections(SceneHubController controller, SceneHubViewModel viewModel)
        {
            for (var i = 0; i < viewModel.Sections.Count; i++)
            {
                var section = viewModel.Sections[i];

                if (DrawSectionHeader(
                        controller.Preferences,
                        section.Title,
                        section.Icon,
                        section.Rows.Count,
                        section.Id))
                {
                    if (section.Rows.Count == 0 && !string.IsNullOrEmpty(section.EmptyMessage))
                        EditorGUILayout.HelpBox(section.EmptyMessage, MessageType.Info);
                    else
                        DrawSceneRows(controller, section.Rows);
                }

                if (i < viewModel.Sections.Count - 1)
                    EditorGUILayout.Space(8);
            }
        }

        private static bool DrawSectionHeader(
            SceneHubPreferences preferences,
            string title,
            Texture2D icon,
            int count,
            string sectionId)
        {
            var isCollapsed = preferences.IsSectionCollapsed(sectionId);

            EditorGUILayout.BeginHorizontal();

            var newCollapsed = !EditorGUILayout.Foldout(!isCollapsed, GUIContent.none, true, EditorStyles.foldout);

            GUILayout.Space(-15);

            if (icon != null)
                GUILayout.Label(new GUIContent(icon), GUILayout.Width(18), GUILayout.Height(18));

            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"({count})", EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();

            if (newCollapsed != isCollapsed)
                preferences.ToggleSectionCollapsed(sectionId, newCollapsed);

            if (!newCollapsed)
            {
                var lineRect = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(lineRect, SceneHubResources.SectionSeparator);
            }

            return !newCollapsed;
        }

        private static void DrawSceneRows(SceneHubController controller, IReadOnlyList<SceneHubSceneRow> rows)
        {
            for (var i = 0; i < rows.Count; i++)
                DrawSceneRow(controller, rows[i], i % 2 == 0);
        }

        private static void DrawSceneRow(SceneHubController controller, SceneHubSceneRow row, bool isEvenRow)
        {
            SceneHubResources.EnsureStylesInitialized();

            var isCompactView = controller.Preferences.IsCompactView;
            var rowHeight = isCompactView ? 20 : 26;
            var rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(rowHeight));

            EditorGUI.DrawRect(rowRect, GetRowColor(row.IsCurrent, isEvenRow));

            GUILayout.Space(4);

            if (!isCompactView)
            {
                if (row.IsCurrent)
                {
                    var indicatorStyle = row.IsDirty
                        ? SceneHubResources.DirtyIndicatorLabel ?? EditorStyles.boldLabel
                        : SceneHubResources.CurrentIndicatorLabel ?? EditorStyles.boldLabel;
                    var indicator = row.IsDirty ? "●" : "►";
                    GUILayout.Label(indicator, indicatorStyle, GUILayout.Width(14));
                }
                else
                {
                    GUILayout.Space(14);
                }
            }

            if (row.IsInBuildSettings && row.BuildIndex >= 0)
            {
                GUILayout.Label($"[{row.BuildIndex}]", SceneHubResources.BuildIndexLabel ?? EditorStyles.miniLabel, GUILayout.Width(24));
            }
            else if (row.IsInBuildSettings)
            {
                GUILayout.Space(24);
            }

            var nameStyle = SceneHubResources.GetSceneNameStyle(row.IsCurrent, isCompactView);
            var labelRect = GUILayoutUtility.GetRect(new GUIContent(row.DisplayName), nameStyle, GUILayout.MinWidth(80));

            if (Event.current.type == EventType.MouseDown &&
                labelRect.Contains(Event.current.mousePosition) &&
                Event.current.clickCount == 2)
            {
                controller.Operations.OpenScene(row.Path);
                Event.current.Use();
            }

            EditorGUI.LabelField(labelRect, new GUIContent(row.DisplayName, row.Path), nameStyle);

            GUILayout.FlexibleSpace();

            var buttonSize = isCompactView ? 18 : 22;
            var buttonWidth = isCompactView ? 22 : 28;

            if (GUILayout.Button(new GUIContent(SceneHubResources.SceneIcon, "Open Scene"), GUILayout.Width(buttonWidth), GUILayout.Height(buttonSize)))
                controller.Operations.OpenScene(row.Path);

            if (GUILayout.Button(new GUIContent(SceneHubResources.PlayIcon, "Play Scene"), GUILayout.Width(buttonWidth), GUILayout.Height(buttonSize)))
                controller.Operations.RunScene(row.Path);

            if (GUILayout.Button(new GUIContent("⋮", "More Options"), GUILayout.Width(buttonWidth), GUILayout.Height(buttonSize)))
                ShowSceneContextMenu(controller, row);

            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowSceneContextMenu(SceneHubController controller, SceneHubSceneRow row)
        {
            var menu = new GenericMenu();
            var preferences = controller.Preferences;
            var operations = controller.Operations;

            if (row.IsInBuildSettings)
            {
                var defaultLabel = row.IsDefault
                    ? "★ Default Scene (click to unset)"
                    : "☆ Set as Default Scene";

                menu.AddItem(new GUIContent(defaultLabel), row.IsDefault, () =>
                {
                    preferences.DefaultScene = row.IsDefault ? string.Empty : row.Path;
                });

                menu.AddSeparator(string.Empty);
            }

            var favoriteLabel = row.IsFavorite ? "♥ Remove from Favorites" : "♡ Add to Favorites";
            menu.AddItem(new GUIContent(favoriteLabel), row.IsFavorite, () => preferences.ToggleFavorite(row.Path));

            var buildLabel = row.IsInBuildSettings ? "Remove from Build Settings" : "Add to Build Settings";
            menu.AddItem(new GUIContent(buildLabel), row.IsInBuildSettings, () => operations.ToggleBuildSettings(row.Path));

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Open as Additive"), false, () => operations.OpenSceneAdditive(row.Path));
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Locate in Project"), false, () => operations.LocateScene(row.Path));
            menu.AddItem(new GUIContent("Duplicate Scene"), false, () => operations.DuplicateScene(row.Path));
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete Scene"), false, () =>
                operations.DeleteScene(row.Path, row.IsCurrent));

            menu.ShowAsContext();
        }

        private static Color GetRowColor(bool isCurrentScene, bool isEvenRow)
        {
            if (isCurrentScene)
                return SceneHubResources.CurrentRowBackground;

            return isEvenRow ? SceneHubResources.EvenRowBackground : SceneHubResources.OddRowBackground;
        }
    }
}
