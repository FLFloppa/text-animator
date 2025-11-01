# Editor Setup Guide

This guide covers the authoring workflow for FLFloppa Text Animator assets inside Unity. Follow these steps when preparing a project for production use.

---

## 1. Prerequisites

* Install **FLFloppa Text Animator** (`Packages/FLFloppa Text Animator/`).
* Install **FLFloppa Editor Helpers** (required for UI Toolkit inspectors).
* Import **TextMesh Pro Essential Resources** (`Window → TextMeshPro → Import TMP Essential Resources`).

---

## 2. Create parameter definition assets

Parameters parse markup attributes into strongly typed values.

1. Project window → `Create → FLFloppa → Text Animator → Parameters`.
2. Choose the asset type:
   * `Float Parameter` – durations, amplitudes, frequencies.
   * `Int Parameter` – counters or indices.
   * `Int Randomizable Parameter` – reveal batches, random ranges.
   * `Bool Parameter` – toggles like looping/synchronisation.
   * `String Parameter` – identifiers or tokens.
3. Configure the inspector:
   * Add **Identifiers** (attribute names used in markup, e.g., `duration`, `wave_amp`).
   * Set default values (and ranges for randomisable parameters).
   * Verify the live summary card reflects identifier counts and defaults.

Reuse parameters across multiple handlers to keep authoring consistent.

---

## 3. Create tag handler assets

Tag handlers link markup tags to runtime pipeline modifiers.

1. Project window → `Create → FLFloppa → Text Animator → Handlers`.
2. Select a handler (e.g., `Wave`, `Fade In`, `Shake`).
3. In the inspector:
   * List aliases in **Tag Identifiers** (e.g., `wave`, `wavy`).
   * Assign parameter assets in the **Parameters** card.
   * Adjust AnimationCurves/Gradients for visual aspects.

Inspector summaries surface alias counts and bound parameters automatically.

---

## 4. Create action assets (optional)

Actions execute custom logic alongside reveals (SFX, analytics, gameplay triggers).

1. Project window → `Create → FLFloppa → Text Animator → Actions`.
2. Use `Action Composite` to chain child actions, or implement bespoke `ActionAsset` derivatives in code.
3. Assign action assets to handler fields such as `onCharacterAction` or `onWordAction`.

---

## 5. Assemble tag handler registries

Registries determine which handlers the parser can instantiate.

1. Project window → `Create → FLFloppa → Text Animator → Handlers → Registry`.
2. Add handler assets to the list.
3. Summary labels display assigned slots and missing references.

Use multiple registries for different UI contexts (e.g., dialogue vs. combat).

---

## 6. Configure subsystems and applicators

Subsystem assets describe how modifier data flows into outputs.

1. Project window → `Create → FLFloppa → Text Animator → Subsystems`.
2. Inspect Transform, Color, and Material subsystem assets and verify assigned applicators.
3. For reuse, create a `TextAnimatorSubsystemBundle` and populate it with subsystem assets.

Custom pipelines:

* Derive from `TextAnimatorSubsystemAsset` to add new state channels.
* Implement `TextOutputApplicatorAsset<TOutput, TState>` to target alternative renderers or properties.

---

## 7. Configure the `TextAnimator` component

1. Add the component to a `TMP_Text` object (`Add Component → TextAnimator`).
2. Assign fields:
   * **Target** – `TMP_Text` reference (auto-assigned on `OnValidate`).
   * **Handler Registry** – the registry asset created earlier.
   * **Parser Asset** – typically `CurlyBraceTagParserAsset`.
   * **Subsystem Bundle** – optional; drag the bundle asset here.
   * **Subsystems** – assign individual subsystem assets if not using a bundle.
3. Use **Initial Markup** and **Play On Enable** for quick previews.

Runtime usage:

```csharp
textAnimator.Play("{wave amplitude=wave_amp}{rainbow gradient=title_colors}Hello!{/rainbow}{/wave}");
```

---

## 8. Validate & iterate

* Enter Play Mode and tweak parameter assets; inspector summaries update live.
* Import the `Samples~/BasicSetup/` sample for a reference scene.
* Prefer adding new assets or applicators over modifying core runtime types to maintain forward compatibility.

---

## Related documents

* [`Docs/tag_reference.md`](tag_reference.md) – Parameters and effect matrix.
* [`README.md`](../README.md) – Installation, highlights, and roadmap.
* Runtime sources under `Runtime/` for handler and subsystem implementations.
