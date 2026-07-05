using UnityEditor;
using UnityEngine;

namespace HLMLabs.SceneHub.Editor
{
    internal static class SceneHubResources
    {
        public static readonly Color CurrentRowBackground = new(0.24f, 0.37f, 0.58f, 0.4f);
        public static readonly Color EvenRowBackground = new(0.22f, 0.22f, 0.22f, 0.3f);
        public static readonly Color OddRowBackground = new(0.18f, 0.18f, 0.18f, 0.3f);
        public static readonly Color SectionSeparator = new(0.5f, 0.5f, 0.5f, 0.5f);
        public static readonly Color DefaultBarEmpty = new(0.3f, 0.3f, 0.3f, 0.3f);
        public static readonly Color DefaultBarSet = new(0.2f, 0.3f, 0.2f, 0.3f);
        public static readonly Color DefaultStar = new(1f, 0.85f, 0.3f);
        public static readonly Color DirtyIndicator = new(1f, 0.6f, 0.3f);
        public static readonly Color CurrentIndicator = new(0.5f, 0.8f, 1f);
        public static readonly Color BuildIndexText = new(0.6f, 0.6f, 0.6f);

        private static bool iconsLoaded;
        private static bool stylesInitialized;

        public static Texture2D SceneIcon { get; private set; }
        public static Texture2D PlayIcon { get; private set; }
        public static Texture2D RefreshIcon { get; private set; }
        public static Texture2D BuildIcon { get; private set; }
        public static Texture2D FolderIcon { get; private set; }
        public static Texture2D FavoriteIcon { get; private set; }
        public static Texture2D RecentIcon { get; private set; }
        public static Texture2D CreateIcon { get; private set; }

        public static GUIStyle CurrentIndicatorLabel { get; private set; }
        public static GUIStyle DirtyIndicatorLabel { get; private set; }
        public static GUIStyle BuildIndexLabel { get; private set; }
        public static GUIStyle SceneNameNormal { get; private set; }
        public static GUIStyle SceneNameBold { get; private set; }
        public static GUIStyle SceneNameCompactNormal { get; private set; }
        public static GUIStyle SceneNameCompactBold { get; private set; }
        public static GUIStyle DefaultStarLabel { get; private set; }

        public static void EnsureIconsLoaded()
        {
            if (iconsLoaded)
                return;

            SceneIcon = LoadIcon("d_SceneAsset Icon");
            PlayIcon = LoadIcon("d_PlayButton");
            RefreshIcon = LoadIcon("d_Refresh");
            BuildIcon = LoadIcon("d_BuildSettings.SelectedIcon");
            FolderIcon = LoadIcon("d_Folder Icon");
            FavoriteIcon = LoadIcon("d_Favorite Icon");
            RecentIcon = LoadIcon("d_UnityEditor.AnimationWindow");
            CreateIcon = LoadIcon("d_CreateAddNew");
            iconsLoaded = true;
        }

        public static void EnsureStylesInitialized()
        {
            if (stylesInitialized)
                return;

            if (!AreEditorStylesReady())
                return;

            CurrentIndicatorLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = CurrentIndicator },
                fontStyle = FontStyle.Bold
            };

            DirtyIndicatorLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = DirtyIndicator },
                fontStyle = FontStyle.Bold
            };

            BuildIndexLabel = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = BuildIndexText }
            };

            SceneNameNormal = new GUIStyle(EditorStyles.label) { fontSize = 12 };
            SceneNameBold = new GUIStyle(EditorStyles.label) { fontSize = 12, fontStyle = FontStyle.Bold };
            SceneNameCompactNormal = new GUIStyle(EditorStyles.label) { fontSize = 11 };
            SceneNameCompactBold = new GUIStyle(EditorStyles.label) { fontSize = 11, fontStyle = FontStyle.Bold };

            DefaultStarLabel = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = DefaultStar },
                fontSize = 12
            };

            stylesInitialized = true;
        }

        public static GUIStyle GetSceneNameStyle(bool isCurrent, bool isCompact)
        {
            EnsureStylesInitialized();

            if (isCompact)
                return isCurrent ? SceneNameCompactBold ?? EditorStyles.label : SceneNameCompactNormal ?? EditorStyles.label;

            return isCurrent ? SceneNameBold ?? EditorStyles.label : SceneNameNormal ?? EditorStyles.label;
        }

        private static bool AreEditorStylesReady()
        {
            try
            {
                return EditorStyles.label != null;
            }
            catch
            {
                return false;
            }
        }

        private static Texture2D LoadIcon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName).image as Texture2D;
        }
    }
}
