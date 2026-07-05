using System.Collections.Generic;
using UnityEditor;

namespace HLMLabs.SceneHub.Editor
{
    internal sealed class SceneHubPreferences
    {
        private readonly List<string> favoriteScenes = new();
        private readonly HashSet<string> favoriteLookup = new();

        public IReadOnlyList<string> Favorites => favoriteScenes;
        public List<string> RecentScenes { get; } = new();
        public HashSet<string> CollapsedSections { get; } = new();
        public bool IsCompactView { get; set; }

        public string DefaultScene
        {
            get => EditorUserSettings.GetConfigValue(SceneHubConstants.DefaultSceneKey) ?? string.Empty;
            set => EditorUserSettings.SetConfigValue(SceneHubConstants.DefaultSceneKey, value ?? string.Empty);
        }

        public void Load()
        {
            favoriteScenes.Clear();
            favoriteLookup.Clear();
            favoriteScenes.AddRange(SceneHubUtil.ParsePipeSeparatedList(
                EditorUserSettings.GetConfigValue(SceneHubConstants.FavoriteScenesKey)));

            foreach (var path in favoriteScenes)
                favoriteLookup.Add(path);

            RecentScenes.Clear();
            RecentScenes.AddRange(SceneHubUtil.ParsePipeSeparatedList(
                EditorUserSettings.GetConfigValue(SceneHubConstants.RecentScenesKey)));

            IsCompactView = EditorUserSettings.GetConfigValue(SceneHubConstants.CompactViewKey) == "True";

            CollapsedSections.Clear();
            foreach (var section in SceneHubUtil.ParsePipeSeparatedList(
                         EditorUserSettings.GetConfigValue(SceneHubConstants.CollapsedSectionsKey)))
            {
                CollapsedSections.Add(section);
            }
        }

        public bool IsFavorite(string scenePath) => favoriteLookup.Contains(scenePath);

        public void SaveFavorites()
        {
            EditorUserSettings.SetConfigValue(
                SceneHubConstants.FavoriteScenesKey,
                SceneHubUtil.SerializePipeSeparatedList(favoriteScenes));
        }

        public void SaveRecentScenes()
        {
            EditorUserSettings.SetConfigValue(
                SceneHubConstants.RecentScenesKey,
                SceneHubUtil.SerializePipeSeparatedList(RecentScenes));
        }

        public void SaveCollapsedSections()
        {
            EditorUserSettings.SetConfigValue(
                SceneHubConstants.CollapsedSectionsKey,
                SceneHubUtil.SerializePipeSeparatedList(CollapsedSections));
        }

        public void SaveCompactView()
        {
            EditorUserSettings.SetConfigValue(SceneHubConstants.CompactViewKey, IsCompactView.ToString());
        }

        public void ToggleFavorite(string scenePath)
        {
            if (favoriteLookup.Contains(scenePath))
            {
                favoriteLookup.Remove(scenePath);
                favoriteScenes.Remove(scenePath);
            }
            else
            {
                favoriteLookup.Add(scenePath);
                favoriteScenes.Add(scenePath);
            }

            SaveFavorites();
        }

        public void AddToRecent(string scenePath)
        {
            RecentScenes.Remove(scenePath);
            RecentScenes.Insert(0, scenePath);

            if (RecentScenes.Count > SceneHubConstants.MaxRecentScenes)
                RecentScenes.RemoveAt(RecentScenes.Count - 1);

            SaveRecentScenes();
        }

        public void RemoveSceneReferences(string scenePath)
        {
            favoriteLookup.Remove(scenePath);
            favoriteScenes.Remove(scenePath);
            RecentScenes.Remove(scenePath);
            SaveFavorites();
            SaveRecentScenes();

            if (DefaultScene == scenePath)
                DefaultScene = string.Empty;
        }

        public void PruneMissingScenes(IReadOnlyList<string> knownPaths)
        {
            var known = new HashSet<string>(knownPaths);

            var favoritesChanged = favoriteScenes.RemoveAll(path => !known.Contains(path)) > 0;
            favoriteLookup.Clear();
            foreach (var path in favoriteScenes)
                favoriteLookup.Add(path);

            var recentChanged = RecentScenes.RemoveAll(path => !known.Contains(path)) > 0;

            if (!string.IsNullOrEmpty(DefaultScene) && !known.Contains(DefaultScene))
                DefaultScene = string.Empty;

            if (favoritesChanged)
                SaveFavorites();

            if (recentChanged)
                SaveRecentScenes();
        }

        public void ToggleSectionCollapsed(string sectionId, bool collapsed)
        {
            if (collapsed)
                CollapsedSections.Add(sectionId);
            else
                CollapsedSections.Remove(sectionId);

            SaveCollapsedSections();
        }

        public bool IsSectionCollapsed(string sectionId) => CollapsedSections.Contains(sectionId);
    }
}
