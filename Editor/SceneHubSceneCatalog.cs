using System.Collections.Generic;
using UnityEditor;

namespace HLMLabs.SceneHub.Editor
{
    internal sealed class SceneHubSceneCatalog
    {
        private readonly List<string> buildScenePaths = new();
        private readonly List<string> otherScenePaths = new();
        private readonly List<string> allKnownPaths = new();
        private readonly Dictionary<string, int> buildIndexByPath = new();

        public IReadOnlyList<string> BuildScenePaths => buildScenePaths;
        public IReadOnlyList<string> OtherScenePaths => otherScenePaths;
        public IReadOnlyList<string> AllKnownPaths => allKnownPaths;

        public void Refresh()
        {
            buildScenePaths.Clear();
            buildIndexByPath.Clear();

            var buildIndex = 0;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;

                buildScenePaths.Add(scene.path);
                buildIndexByPath[scene.path] = buildIndex++;
            }

            otherScenePaths.Clear();

            foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Packages/") && !buildIndexByPath.ContainsKey(path))
                    otherScenePaths.Add(path);
            }

            allKnownPaths.Clear();
            allKnownPaths.AddRange(buildScenePaths);
            allKnownPaths.AddRange(otherScenePaths);
        }

        public int GetBuildIndex(string scenePath)
        {
            return buildIndexByPath.TryGetValue(scenePath, out var index) ? index : -1;
        }

        public bool IsInBuildSettings(string scenePath) => buildIndexByPath.ContainsKey(scenePath);
    }
}
