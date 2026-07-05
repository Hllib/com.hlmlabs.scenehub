using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HLMLabs.SceneHub.Editor
{
    internal static class SceneHubUtil
    {
        public static bool MatchesFilter(string path, string searchFilter)
        {
            if (string.IsNullOrEmpty(searchFilter))
                return true;

            var filter = searchFilter.ToLowerInvariant();

            return Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains(filter) ||
                   path.ToLowerInvariant().Contains(filter);
        }

        public static List<string> FilterScenes(IEnumerable<string> scenes, string searchFilter)
        {
            if (string.IsNullOrEmpty(searchFilter))
                return scenes as List<string> ?? scenes.ToList();

            return scenes.Where(path => MatchesFilter(path, searchFilter)).ToList();
        }

        public static string GetDisplayName(string path, bool isDirty)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            return isDirty ? name + " *" : name;
        }

        public static IEnumerable<string> ParsePipeSeparatedList(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Enumerable.Empty<string>();

            return value.Split('|').Where(entry => !string.IsNullOrEmpty(entry));
        }

        public static string SerializePipeSeparatedList(IEnumerable<string> values)
        {
            return string.Join("|", values);
        }
    }
}
