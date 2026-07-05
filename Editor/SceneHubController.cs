using System;
using System.Collections.Generic;
using UnityEngine;

namespace HLMLabs.SceneHub.Editor
{
    internal sealed class SceneHubController
    {
        private readonly SectionConfig[] sections =
        {
            new(
                SceneHubConstants.SectionFavorites,
                "Favorites",
                () => SceneHubResources.FavoriteIcon,
                controller => controller.Preferences.Favorites,
                hideWhenEmpty: true,
                hideWhenSearching: false,
                resolveBuildIndex: true,
                emptyMessage: null),

            new(
                SceneHubConstants.SectionRecent,
                "Recent",
                () => SceneHubResources.RecentIcon,
                controller =>
                {
                    var recent = controller.Preferences.RecentScenes;
                    if (recent.Count == 0)
                        return Array.Empty<string>();

                    var count = Math.Min(recent.Count, SceneHubConstants.MaxRecentDisplayed);
                    return recent.GetRange(0, count);
                },
                hideWhenEmpty: true,
                hideWhenSearching: true,
                resolveBuildIndex: true,
                emptyMessage: null),

            new(
                SceneHubConstants.SectionBuild,
                "Build Settings",
                () => SceneHubResources.BuildIcon,
                controller => controller.Catalog.BuildScenePaths,
                hideWhenEmpty: false,
                hideWhenSearching: false,
                resolveBuildIndex: true,
                emptyMessage: "No scenes in Build Settings."),

            new(
                SceneHubConstants.SectionOther,
                "Other Project Scenes",
                () => SceneHubResources.FolderIcon,
                controller => controller.Catalog.OtherScenePaths,
                hideWhenEmpty: false,
                hideWhenSearching: false,
                resolveBuildIndex: false,
                emptyMessage: "No other scenes found.")
        };

        public SceneHubPreferences Preferences { get; } = new();
        public SceneHubSceneCatalog Catalog { get; } = new();
        public SceneHubSceneOperations Operations { get; }

        public SceneHubController()
        {
            Preferences.Load();
            Operations = new SceneHubSceneOperations(Preferences, RefreshCatalog);
        }

        public void RefreshCatalog()
        {
            Catalog.Refresh();
            Preferences.PruneMissingScenes(Catalog.AllKnownPaths);
        }

        public SceneHubViewModel BuildViewModel(SceneHubViewState viewState, string currentScenePath, bool isDirty)
        {
            var defaultScene = Preferences.DefaultScene;
            var sectionViewModels = new List<SceneHubSectionViewModel>();

            foreach (var section in sections)
            {
                if (section.HideWhenSearching && !string.IsNullOrEmpty(viewState.SearchFilter))
                    continue;

                var sourcePaths = section.GetSourcePaths(this);
                var filteredPaths = SceneHubUtil.FilterScenes(sourcePaths, viewState.SearchFilter);

                if (section.HideWhenEmpty && filteredPaths.Count == 0)
                    continue;

                var rows = BuildRows(
                    filteredPaths,
                    section.ResolveBuildIndex,
                    currentScenePath,
                    isDirty,
                    defaultScene);

                sectionViewModels.Add(new SceneHubSectionViewModel(
                    section.Id,
                    section.Title,
                    section.GetIcon(),
                    rows,
                    section.EmptyMessage));
            }

            return new SceneHubViewModel(defaultScene, sectionViewModels);
        }

        private List<SceneHubSceneRow> BuildRows(
            IReadOnlyList<string> paths,
            bool resolveBuildIndex,
            string currentScenePath,
            bool isDirty,
            string defaultScene)
        {
            var rows = new List<SceneHubSceneRow>(paths.Count);

            foreach (var path in paths)
            {
                var isCurrent = path == currentScenePath;
                var buildIndex = resolveBuildIndex ? Catalog.GetBuildIndex(path) : -1;
                var isInBuildSettings = resolveBuildIndex && Catalog.IsInBuildSettings(path);

                rows.Add(new SceneHubSceneRow(
                    path,
                    SceneHubUtil.GetDisplayName(path, isCurrent && isDirty),
                    buildIndex,
                    isInBuildSettings,
                    isCurrent,
                    path == defaultScene,
                    Preferences.IsFavorite(path),
                    isCurrent && isDirty));
            }

            return rows;
        }

        private sealed class SectionConfig
        {
            public string Id { get; }
            public string Title { get; }
            public Func<Texture2D> GetIcon { get; }
            public Func<SceneHubController, IReadOnlyList<string>> GetSourcePaths { get; }
            public bool HideWhenEmpty { get; }
            public bool HideWhenSearching { get; }
            public bool ResolveBuildIndex { get; }
            public string EmptyMessage { get; }

            public SectionConfig(
                string id,
                string title,
                Func<Texture2D> getIcon,
                Func<SceneHubController, IReadOnlyList<string>> getSourcePaths,
                bool hideWhenEmpty,
                bool hideWhenSearching,
                bool resolveBuildIndex,
                string emptyMessage)
            {
                Id = id;
                Title = title;
                GetIcon = getIcon;
                GetSourcePaths = getSourcePaths;
                HideWhenEmpty = hideWhenEmpty;
                HideWhenSearching = hideWhenSearching;
                ResolveBuildIndex = resolveBuildIndex;
                EmptyMessage = emptyMessage;
            }
        }
    }
}
