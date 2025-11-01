# Editor Setup Guide

This guide walks through creating the core ScriptableObject assets and configuring the `TextAnimator` component inside the Unity Editor. Follow the sections in order when introducing FLFloppa Text Animator into a new project.

---

## 1. Prerequisites

* Install the package (`Packages/FLFloppa Text Animator/`).
* Install **FLFloppa Editor Helpers** – required for UI Toolkit inspectors.
* Ensure TextMesh Pro is imported into the project (`Window → TextMeshPro → Import TMP Essential Resources`).

---

## 2. Create parameter definition assets

Parameters provide strongly typed access to markup attributes. Create them via the Project window context menu:

1. Right-click in the desired folder → `Create → FLFloppa → Text Animator → Parameters`.
2. Choose the appropriate asset type:
   * `Float Parameter` – numeric values such as duration, amplitude, frequency.
   * `Int Parameter` or `Int Randomizable Parameter` – counts, batch sizes, reveal ranges.
   * `Bool Parameter` – toggles such as loop/synchronize flags.
   * `String Parameter` – textual tokens.
3. In the inspector:
   * Populate the **Identifiers** list with attribute names used in markup (e.g., `duration`, `wave_amp`).
   * Configure default values (and random ranges where applicable).
   * Use the summary card to verify identifier counts and defaults.

> Parameters are reusable across multiple handlers—create a single `wave_amp` parameter and assign it anywhere the wave amplitude is required.

---

## 3. Create tag handler assets

Tag handlers map markup tags to runtime pipeline modifiers.

1. Right-click → `Create → FLFloppa → Text Animator → Handlers`.
2. Pick a handler type (e.g., `Wave`, `Shake`, `Fade In`).
3. In the inspector:
   * Use the **Tag Identifiers** section to list aliases (e.g., `wave`, `wavy`).
   * Assign the required parameter assets in the **Parameters** card.
   * Adjust AnimationCurve or Gradient fields directly for visual effects.

The inspector summary updates automatically, showing the number of identifiers and the parameters/actions bound to the asset.

---

## 4. Create action assets (optional)

Actions execute custom logic during timeline playback (sound cues, particles, game events).

1. Right-click → `Create → FLFloppa → Text Animator → Actions`.
2. Choose `Action Asset` implementations such as `Composite` to chain multiple actions.
3. Assign these assets to handler fields like `onCharacterAction` or `onWordAction`.

Implement bespoke behaviour by deriving from `ActionAsset` in code and exposing serialized fields as needed.

---

## 5. Build a tag handler registry

The registry determines which handlers are available to the parser.

1. Right-click → `Create → FLFloppa → Text Animator → Handlers → Registry`.
2. In the inspector, add handler assets to the list.
3. The summary label shows how many handlers are assigned and highlights missing references.

> Multiple registries can exist—swap them at runtime for different gameplay contexts (e.g., battle UI vs. dialogue UI).

---

## 6. Configure subsystems and applicators

Subsystem assets define how modifier data flows into output renderers (Transform, Color, Material).

1. Create subsystem assets via `Create → FLFloppa → Text Animator → Subsystems`.
2. Inspect each asset to verify assigned applicators (e.g., `TextMeshProTransformApplicator`).
3. For reusable bundles, create a `TextAnimatorSubsystemBundle` and add subsystem assets to the list.

Custom render pipelines:

* Derive from `TextAnimatorSubsystemAsset` to introduce new data channels.
* Implement applicators by extending `TextOutputApplicatorAsset<TOutput, TState>`.

---

## 7. Configure the TextAnimator component

1. Add the `TextAnimator` MonoBehaviour to a `TMP_Text` object (`Add Component → TextAnimator`).
2. Assign references in the inspector:
   * **Target** – the TextMesh Pro component (auto-assigned on `OnValidate`).
   * **Handler Registry** – the `TagHandlerRegistryAsset` created earlier.
   * **Parser Asset** – typically `CurlyBraceTagParserAsset` unless you provide a custom parser.
   * **Subsystem Bundle** – optional; drag the bundle asset here for a preconfigured set.
   * **Subsystems** – assign specific subsystem assets if not using a bundle.
3. Optionally populate `Initial Markup` and toggle `Play On Enable` for automatic preview.

At runtime, call `TextAnimator.Play("{wave amplitude=wave_amp}Hello{/wave}")` to animate text via script.

---

## 8. Validate and iterate

* Enter Play Mode—the inspector summaries update live while you tweak parameters, curves, and gradients.
* Use the `Samples~/BasicSetup/` scene as a reference implementation.
* When expanding the system, prefer creating new assets over modifying core runtime code to keep editor tooling intact.

---

## Additional resources

* [`Docs/tag_reference.md`](tag_reference.md) – Detailed list of built-in tags and parameters.
* [`README.md`](../README.md) – Overview, installation instructions, and roadmap.
* Runtime source under `Runtime/` for reference implementations of handlers, actions, and subsystems.
