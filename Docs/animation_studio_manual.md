# Animation Studio Manual

_Release: Now with Animation Studio!_

The Animation Studio is a set of dockable Unity Editor windows that keep markup, effect discovery, parameter editing, and preview playback in sync. This manual describes the workspace layout, provides setup instructions, and highlights best practices for team workflows.

---

## 1. Opening the workspace

1. Install **FLFloppa Text Animator** and **FLFloppa Editor Helpers** packages in your project.
2. In Unity, open `Window → FLFloppa → Text Animator → Animation Studio`.
3. Four auxiliary windows appear (they can be docked anywhere):
   * **Markup** – rich text editor for tag markup.
   * **Catalog** – grouped list of tag handler cards with drag-and-drop support.
   * **Inspector** – card stack bound to the current selection, exposing parameters and handler properties.
   * **Preview** – camera view of the preview scene with playback controls and status messages.

> 💡 _Tip:_ Save a custom editor layout after docking the windows so you can restore the workspace quickly.

---

## 2. Workspace overview

### 2.1 Markup window

* Synchronizes selection ranges with the inspector and catalog.
* Supports undo-safe tag insertion, removal, and reordering.
* Displays live validation and caret position for quick troubleshooting.

### 2.2 Catalog window

* Displays tag handler cards grouped by category.
* Search-as-you-type filters cards by display name or description.
* Drag cards into the markup to wrap the current selection.
* Double-click a card to apply its handler immediately.

### 2.3 Inspector window

* Lists the tags intersecting the current selection in markup.
* Reorder tags using drag handles; updates markup structure automatically.
* Edit parameter values inline with full undo support.
* Inspect handler asset fields (e.g., AnimationCurves, Textures) without leaving the studio.

### 2.4 Preview window

* Renders a dedicated preview scene using your handler registry and parser asset.
* Provides play/pause/stop transport buttons and progress feedback.
* Auto-refreshes during edits when enabled.

---

## 3. Authoring workflow

1. **Select target markup** in the Markup window.
2. **Choose effects** from the Catalog; reorder them in the Inspector to adjust nesting.
3. **Tune parameters** (scalars, curves, gradients) directly in the Inspector.
4. **Preview changes** with the Playback controls; toggle auto-refresh for immediate feedback.
5. **Iterate quickly** using undo/redo and the catalog search.

---

## 4. Handler registry integration

* The studio uses the active `TagHandlerRegistryAsset` configured in `Animation Studio Settings`.
* Update your registry assets to organize handlers into groups for the catalog window.
* Missing handlers are surfaced as warning cards, helping teams spot broken references.

---

## 5. Troubleshooting

* **Tags not appearing in Inspector:** Ensure the markup selection includes the tag range and the registry contains the handler.
* **Catalog cards missing:** Verify the registry references `AnimationStudioCatalogGroupAsset` instances containing your handler cards.
* **Preview not updating:** Confirm auto-refresh is enabled, or manually click **Rebuild** in the Preview window.
* **Parameter flicker:** Close extraneous inspector panes; the studio windows manage their own serialized object bindings.

---

## 6. Keyboard shortcuts

| Command | Action |
| --- | --- |
| `Ctrl/Cmd + Enter` | Apply selected handler to current markup selection |
| `Ctrl/Cmd + Shift + R` | Rebuild preview transport |
| `Ctrl/Cmd + Shift + F` | Focus search field in the catalog |

Shortcuts can be customized via Unity's shortcut manager under the **FLFloppa/Animation Studio** category.

---

## 7. Team workflows

* Store common catalog groups (Gameplay, Cinematics, UI) in version-controlled assets.
* Share preview scenes configured with representative lighting and text components.
* Use git diff tools to review markup changes produced via the studio for code reviews.

---

## 8. Release notes

This version ships with the "Now with Animation Studio!" update. Consult [`Docs/CHANGELOG.md`](CHANGELOG.md) for detailed release history.

---

## 9. Feedback

Have suggestions or find issues? Open a GitHub issue or email `flfloppa@yandex.ru`. Include screenshots and reproduction steps to help us refine the studio.
