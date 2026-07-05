# 🔧 Installation
- Inside your Unity project go to Window -> Package Manager
- At the bottom left corner choose "+"
- Install package from git URL
- Paste `https://github.com/Hllib/com.hlmlabs.scenehub.git`  
- Click "Install"

---

# About Scene Hub

Scene Hub is an editor window for browsing, opening, and running scenes in your project.

## Opening Scene Hub

**Menu:** `HLM Labs > Scene Hub`

## Scene lists

Scenes are grouped into sections:

- **Favorites** — scenes you marked as favorites
- **Recent** — scenes you opened recently (up to 5 shown)
- **Build Settings** — scenes enabled in Build Settings (with build index)
- **Other Project Scenes** — all other scenes in the project

Use the search bar at the top to filter scenes by name or path. Press `Esc` to clear the search. Press `F5` to refresh the lists.

Collapse or expand a section with the foldout arrow next to its title.

## Opening and playing scenes

| Action | How |
|--------|-----|
| Open scene | Click the scene icon on the row, or double-click the scene name |
| Play scene | Click the play icon on the row — after Play Mode ends, the editor returns to the scene that was open before |
| Open as additive | Row menu (`⋮`) → **Open as Additive** |

If the current scene has unsaved changes, you will be prompted to save before opening or playing another scene.

## Row menu (`⋮`)

Right-click actions are available from the **⋮** button on each row:

- **Set as Default Scene** / **Default Scene (click to unset)** — only for scenes in Build Settings
- **Add to Favorites** / **Remove from Favorites**
- **Add to Build Settings** / **Remove from Build Settings**
- **Open as Additive**
- **Locate in Project** — pings the scene in the Project window
- **Duplicate Scene**
- **Delete Scene** — cannot delete the currently open scene

## Default scene

Set a default scene from the row menu of any scene in Build Settings. When you press the editor **Play** button, Scene Hub switches to the default scene first (even if another scene is open). When Play Mode ends, the editor returns to the scene that was open before.

The default scene is shown in the bar at the top of the window.

## Toolbar

| Button | Action |
|--------|--------|
| Search field | Filter all scene lists |
| `×` | Clear search |
| `⊟` | Toggle compact row layout |
| Refresh | Reload scene lists (`F5`) |
| Create | Create a new scene |

## Visual indicators

- **►** — currently open scene
- **●** — currently open scene with unsaved changes (name also shows `*`)
- **`[0]`, `[1]`, …** — build index in Build Settings
- **★** in the top bar — default scene is set
