# Tag Reference

This document lists every tag shipped with `FLFloppa Text Animator`, their supported parameters, recommended usage patterns, and extensibility hooks. Use it alongside the custom inspectors in the Unity Editor for in-context summaries and validation hints.

> **Notation:** Tags use curly braces, e.g. `{wave amplitude=wave_amp}`. Parameters reference `ParameterDefinitionAsset` instances unless otherwise specified.

---

## Quick Index

* [`{scale}`](#scale)
* [`{rotate}`](#rotate)
* [`{positionoffset}`](#positionoffset)
* [`{sway}`](#sway)
* [`{shake}`](#shake)
* [`{wave}`](#wave)
* [`{fadeIn}`](#fadeIn)
* [`{rainbow}`](#rainbow)
* [`{wait}`](#wait)
* [`{charbychar}`](#charbychar)
* [`{wordbyword}`](#wordbyword)
* [`{action}`](#action)

Each section describes:

* A short effect summary
* Parameter table (type, default, notes)
* Example markup
* Inspector tips & extension hooks

---

## `{scale}`

* __Purpose__ – Animate per-axis scale over time using AnimationCurve assets.
* __Runtime handler__ – `ScaleTagHandler`
* __ScriptableObject__ – `ScaleTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 1.0 | Total effect duration in seconds. |
| `loop` | Bool parameter | false | Whether to loop the curve values when timeline extends past duration. |
| `scaleX` | AnimationCurve (asset field) | linear (1) | Curve sampled for X scale. |
| `scaleY` | AnimationCurve (asset field) | linear (1) | Curve sampled for Y scale. |
| `scaleZ` | AnimationCurve (asset field) | linear (1) | Curve sampled for Z scale. |

```text
{scale duration=scale_short}{/scale}
```

* __Inspector tips__ – Curves preview stats (min/max) in the inspector summary; use the "Curve Presets" quick actions to align start/end values.
* __Extensions__ – Add additional axis curves by subclassing `ScaleTagHandler` or create a variant that swaps X/Y for italic-like skew effects.

---

## `{rotate}`

* __Purpose__ – Rotates characters based on an AnimationCurve of degrees.
* __Runtime handler__ – `RotateTagHandler`
* __ScriptableObject__ – `RotateTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 1.0 | Total effect duration. |
| `loop` | Bool parameter | false | Loop behaviour for the rotation curve. |
| `rotationCurve` | AnimationCurve (asset field) | linear (0) | Degrees over time; positive values rotate clockwise. |

```text
{rotate duration=rotate_short}{/rotate}
```

* __Inspector tips__ – The inspector summary shows curve extrema; ensure the curve hits 360° if you want a full spin.
* __Extensions__ – Implement an easing preset button by extending the editor inspector or add axis-specific rotations by extending the runtime handler.

---

## `{positionoffset}`

* __Purpose__ – Applies additive position offsets using three AnimationCurves.
* __Runtime handler__ – `PositionOffsetTagHandler`
* __ScriptableObject__ – `PositionOffsetTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 1.0 | Effect duration. |
| `loop` | Bool parameter | false | Whether to wrap the curves. |
| `override` | Bool parameter | false | If true, ignores existing offsets produced by other modifiers. |
| `positionX/Y/Z` | AnimationCurve (asset field) | zero curve | Offset per axis. |

```text
{positionoffset duration=pos_loop}{/positionoffset}
```

* __Inspector tips__ – Toggle `override` to reset offsets when stacking multiple movement tags.
* __Extensions__ – For screen-space movement, implement a custom applicator that writes offsets in canvas space.

---

## `{sway}`

* __Purpose__ – Moves characters along a sine wave with optional grouped pivot.
* __Runtime handler__ – `SwayTagHandler`
* __ScriptableObject__ – `SwayTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `frequency` | Float parameter | 1.0 | Oscillations per second. |
| `amplitude` | Float parameter | 0.5 | Movement distance. |
| `phaseOffset` | Float parameter | 0.0 | Additional phase (radians). |
| `grouped` | Bool parameter | false | Calculate sway from group pivot rather than individual characters. |

```text
{sway amplitude=sway_wide grouped=true}Grouped sway{/sway}
```

* __Inspector tips__ – When `grouped` is enabled, the inspector displays the computed pivot rules.
* __Extensions__ – Add vertical-only or horizontal-only toggles by extending `SwayTagHandler`.

---

## `{shake}`

* __Purpose__ – Applies pseudo-random jitter to characters.
* __Runtime handler__ – `ShakeTagHandler`
* __ScriptableObject__ – `ShakeTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `amplitude` | Float parameter | 0.25 | Maximum positional offset. |
| `frequency` | Float parameter | 20.0 | Updates per second. |
| `synchronize` | Bool parameter | false | If true, all characters share the same random offset per frame. |

```text
{shake amplitude=shake_large synchronize=true}Earthquake{/shake}
```

* __Inspector tips__ – Use the summary to monitor amplitude/frequency pairings; high values may reveal aliasing in low frame rate contexts.
* __Extensions__ – For rotation-based shake, subclass and append angular noise or animate material properties (e.g., glitch shaders).

---

## `{wave}`

* __Purpose__ – Produces sinusoidal displacement with amplitude/frequency controls.
* __Runtime handler__ – `WaveTagHandler`
* __ScriptableObject__ – `WaveTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `amplitude` | Float parameter | 0.5 | Maximum offset magnitude. |
| `frequency` | Float parameter | 2.0 | Wave cycles per second. |
| `phase` | Float parameter | 0.0 | Phase offset in radians. |

```text
{wave amplitude=wave_amp frequency=wave_fast}Wavy text{/wave}
```

* __Inspector tips__ – Combine with `{rainbow}` for animated gradients; inspector summaries show the bound parameter assets for quick auditing.
* __Extensions__ – Implement easing or custom wave shapes by replacing the sine call in `WaveTagHandler`.

---

## `{fadeIn}`

* __Purpose__ – Gradually reveals characters by manipulating alpha.
* __Runtime handler__ – `FadeInTagHandler`
* __ScriptableObject__ – `FadeInTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 0.5 | Fade duration in seconds. |
| `alphaCurve` | AnimationCurve (asset field) | linear (0→1) | Custom alpha curve, allowing overshoot or eased fades. |

```text
{fadeIn duration=fade_slow}Subtle fade{/fadeIn}
```

* __Inspector tips__ – The inspector summary lists curve bounds; use built-in shortcuts to clamp alpha between 0 and 1.
* __Extensions__ – Pair with custom applicators to fade in outlines or glow separately from vertex colors.

---

## `{rainbow}`

* __Purpose__ – Applies a gradient over characters and time.
* __Runtime handler__ – `RainbowTagHandler`
* __ScriptableObject__ – `RainbowTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 2.0 | Color cycle length. |
| `loop` | Bool parameter | true | Whether to loop gradient sampling. |
| `colorShift` | Float parameter | 0.0 | Offset into the gradient over time. |
| `gradient` | Gradient (asset field) | Rainbow preset | Gradient evaluated across characters. |

```text
{rainbow duration=rainbow_cycle gradient=title_colors}Title text{/rainbow}
```

* __Inspector tips__ – Gradient previews show key count; use the copy/paste buttons to reuse gradients across assets.
* __Extensions__ – Implement palette swapping by extending the handler to sample multiple gradients based on context.

---

## `{wait}`

* __Purpose__ – Inserts a timeline pause without rendering changes.
* __Runtime handler__ – `WaitTagHandler`
* __ScriptableObject__ – `WaitTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 0.5 | Pause length in seconds. |

```text
{wait duration=pause_short}
```

* __Inspector tips__ – Summary displays assigned duration parameter; helpful when orchestrating dialogue pacing.
* __Extensions__ – Extend to support random ranges or conditional waits by subclassing `WaitTagHandler`.

---

## `{charByChar}`

* __Purpose__ – Triggers character reveals in batches with optional per-character action callbacks.
* __Runtime handler__ – `CharByCharTagHandler`
* __ScriptableObject__ – `CharByCharTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `perCharacterDuration` | Float parameter | 0.05 | Delay between characters. |
| `revealCount` | IntRandomizable parameter | 1 | Characters revealed per step (supports random range). |
| `onCharacterAction` | ActionAsset | null | Optional action invoked per revealed character. |

```text
{charByChar perCharacterDuration=reveal_fast revealCount=reveal_pair onCharacterAction=character_sfx}
```

* __Inspector tips__ – Summary strings highlight assigned actions; the inspector warns when the randomizable parameter lacks range values.
* __Extensions__ – Combine with custom `ActionAsset` derivatives to play sounds or spawn particles per character.

---

## `{wordByWord}`

* __Purpose__ – Reveals text in word batches with optional word-level action callbacks.
* __Runtime handler__ – `WordByWordTagHandler`
* __ScriptableObject__ – `WordByWordTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `perWordDuration` | Float parameter | 0.2 | Delay between words. |
| `wordsPerBatch` | IntRandomizable parameter | 1 | Words revealed per step. |
| `onWordAction` | ActionAsset | null | Optional action triggered per revealed word. |

```text
{wordbByWord perWordDuration=reveal_medium wordsPerBatch=reveal_two onWordAction=word_sfx}
```

* __Inspector tips__ – Inspector summarises action assignment and batch sizes; randomizable parameters show min/max directly.
* __Extensions__ – Pair with timeline events to synchronize voice-over cues or subtitle callouts.

---

## `{action}`

* __Purpose__ – Invokes a scripted `ActionAsset` without modifying visual state.
* __Runtime handler__ – `ActionTagHandler`
* __ScriptableObject__ – `ActionTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `action` | ActionAsset | Required | ScriptableObject implementing the custom action. |

```text
{action action=shake_camera}
```

* __Inspector tips__ – Summary warns when the action asset is missing.
* __Extensions__ – Build branching dialogue triggers by implementing custom `ActionAsset` classes that publish to your gameplay systems.

---

## Extending the tag library

* __Add new handlers__ – Derive from `TagHandlerAsset`, expose serialized fields for parameters, and implement a runtime handler (`ITagHandler`) that manipulates the relevant modifier chains.
* __Create new parameters__ – Inherit from `ParameterDefinitionAsset<T>` to parse custom attribute strings (e.g., color hex codes, enum-like tokens).
* __Custom subsystems__ – Build a new `TextAnimatorSubsystemAsset` to control additional state (e.g., blur, outline width). Pair it with a `TextOutputApplicatorAsset<TOutput, TState>` that applies the state to your renderer.
* __Composable actions__ – Chain reusable behaviours with `ActionCompositeAsset`, or craft specialized actions for audio cues, timelines, or analytics.

Refer to the README and existing asset inspectors for concrete authoring workflows. Keep parameter identifiers consistent and document them for narrative/design teams to ensure markup stays maintainable.
