# Tag Reference

This reference catalogues all built-in tags shipped with `FLFloppa Text Animator`, including supported parameters, default behaviours, and authoring tips. Keep it handy while designing markup for writers, UI designers, and engineers.

> **Notation:** Tags use curly braces, e.g. `{wave amplitude=wave_amp}`. Parameter names refer to `ParameterDefinitionAsset` identifiers unless otherwise noted.

---

## Quick Index

* `{scale}`
* `{rotate}`
* `{positionOffset}`
* `{sway}`
* `{shake}`
* `{wave}`
* `{fadeIn}`
* `{rainbow}`
* `{wait}`
* `{charByChar}`
* `{wordByWord}`
* `{action}`

Each section outlines:

* Effect summary
* Parameter table (type, default, description)
* Example markup
* Inspector tips and extension hooks

---

## `{scale}`

* __Purpose__ – Animate per-axis scale over time using AnimationCurve assets.
* __Runtime handler__ – `ScaleTagHandler`
* __Asset__ – `ScaleTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 1.0 | Duration in seconds. |
| `loop` | Bool parameter | false | If true, wraps curve samples after duration. |
| `scaleX` | AnimationCurve (asset field) | Constant 1 | X-axis scale curve. |
| `scaleY` | AnimationCurve (asset field) | Constant 1 | Y-axis scale curve. |
| `scaleZ` | AnimationCurve (asset field) | Constant 1 | Z-axis scale curve. |

```text
{scale duration=scale_short}{/scale}
```

* __Inspector tips__ – Curve summary displays first/last values; use quick preset buttons to normalize to 1.
* __Extensions__ – Derive a variant to skew characters or apply axis masks.

---

## `{rotate}`

* __Purpose__ – Rotates characters based on an AnimationCurve measured in degrees.
* __Runtime handler__ – `RotateTagHandler`
* __Asset__ – `RotateTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 1.0 | Total rotation duration. |
| `loop` | Bool parameter | false | Whether to loop the rotation curve. |
| `rotationCurve` | AnimationCurve | Linear 0 | Degrees over time; positive values rotate clockwise. |

```text
{rotate duration=rotate_half}{/rotate}
```

* __Inspector tips__ – Summary lists curve extrema; aim for ±360 for full spins.
* __Extensions__ – Add axis selection or easing presets in a derived handler.

---

## `{positionOffset}`

* __Purpose__ – Applies additive offsets to character positions using three curves.
* __Runtime handler__ – `PositionOffsetTagHandler`
* __Asset__ – `PositionOffsetTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 1.0 | Effect length. |
| `loop` | Bool parameter | false | Loop curve samples after duration. |
| `override` | Bool parameter | false | If true, overrides any existing offsets. |
| `positionX` | AnimationCurve | Constant 0 | Offset along local X axis. |
| `positionY` | AnimationCurve | Constant 0 | Offset along local Y axis. |
| `positionZ` | AnimationCurve | Constant 0 | Offset along local Z axis. |

```text
{positionOffset duration=offset_loop override=true}{/positionOffset}
```

* __Inspector tips__ – Toggle `override` when stacking multiple movement tags.
* __Extensions__ – Pair with a custom applicator to convert offsets into screen-space or world-space translations.

---

## `{sway}`

* __Purpose__ – Moves characters along a sine wave with optional group pivoting.
* __Runtime handler__ – `SwayTagHandler`
* __Asset__ – `SwayTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `frequency` | Float parameter | 1.0 | Oscillations per second. |
| `amplitude` | Float parameter | 0.5 | Displacement magnitude. |
| `phaseOffset` | Float parameter | 0.0 | Additional phase in radians. |
| `grouped` | Bool parameter | false | When true, characters sway around a shared pivot. |

```text
{sway amplitude=sway_large grouped=true}Grouped sway{/sway}
```

* __Inspector tips__ – Inspector summaries clarify grouped behaviour; combine with `{wave}` for layered motion.
* __Extensions__ – Add orientation parameters (horizontal/vertical) in derived handlers.

---

## `{shake}`

* __Purpose__ – Applies pseudo-random jitter.
* __Runtime handler__ – `ShakeTagHandler`
* __Asset__ – `ShakeTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `amplitude` | Float parameter | 0.25 | Maximum offset magnitude. |
| `frequency` | Float parameter | 20.0 | Updates per second. |
| `synchronize` | Bool parameter | false | Use shared noise across characters. |

```text
{shake amplitude=shake_big synchronize=true}Earthquake{/shake}
```

* __Inspector tips__ – High amplitude + low frequency produces choppy motion; adjust curves accordingly.
* __Extensions__ – Introduce rotational or alpha shake via custom modifiers.

---

## `{wave}`

* __Purpose__ – Produces sinusoidal displacement with amplitude/frequency controls.
* __Runtime handler__ – `WaveTagHandler`
* __Asset__ – `WaveTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `amplitude` | Float parameter | 0.5 | Offset magnitude. |
| `frequency` | Float parameter | 2.0 | Cycles per second. |
| `phase` | Float parameter | 0.0 | Phase offset in radians. |

```text
{wave amplitude=wave_amp frequency=wave_fast}Wavy text{/wave}
```

* __Inspector tips__ – Summary shows linked parameters; combine with `{rainbow}` for colourful waves.
* __Extensions__ – Replace sine with custom wave functions in a derived handler.

---

## `{fadeIn}`

* __Purpose__ – Reveals characters by animating alpha.
* __Runtime handler__ – `FadeInTagHandler`
* __Asset__ – `FadeInTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 0.5 | Fade length. |
| `alphaCurve` | AnimationCurve | Linear 0→1 | Custom alpha curve; supports overshoot or easing. |

```text
{fadeIn duration=fade_slow}Subtle fade{/fadeIn}
```

* __Inspector tips__ – Clamp alpha between 0 and 1 using quick actions; preview curve stats in summaries.
* __Extensions__ – Pair with a material applicator to fade outlines or glow channels separately.

---

## `{rainbow}`

* __Purpose__ – Applies a gradient across characters with optional time-based cycling.
* __Runtime handler__ – `RainbowTagHandler`
* __Asset__ – `RainbowTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 2.0 | Loop length in seconds. |
| `loop` | Bool parameter | true | If false, gradient plays once. |
| `colorShift` | Float parameter | 0.0 | Offset into the gradient over time. |
| `gradient` | Gradient | Rainbow preset | Colour gradient evaluated across characters. |

```text
{rainbow duration=rainbow_cycle gradient=title_colors}Title{/rainbow}
```

* __Inspector tips__ – Gradient previews show key counts; use copy/paste to reuse palettes.
* __Extensions__ – Create multi-gradient variants for contextual colour schemes.

---

## `{wait}`

* __Purpose__ – Inserts a pause in the timeline without altering visuals.
* __Runtime handler__ – `WaitTagHandler`
* __Asset__ – `WaitTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `duration` | Float parameter | 0.5 | Pause length in seconds. |

```text
{wait duration=pause_short}
```

* __Inspector tips__ – Summary highlights the parameter asset; ideal for dialogue timing adjustments.
* __Extensions__ – Implement random ranges or conditional waits in derived handlers.

---

## `{charByChar}`

* __Purpose__ – Reveals characters in batches with optional per-character actions.
* __Runtime handler__ – `CharByCharTagHandler`
* __Asset__ – `CharByCharTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `perCharacterDuration` | Float parameter | 0.05 | Delay between characters. |
| `revealCount` | IntRandomizable parameter | 1 | Characters revealed per step. |
| `onCharacterAction` | ActionAsset | null | Optional action invoked per character. |

```text
{charByChar perCharacterDuration=reveal_fast revealCount=reveal_pair onCharacterAction=character_sfx}
```

* __Inspector tips__ – Inspector warnings highlight missing actions; randomizable parameters show min/max values.
* __Extensions__ – Hook into `ActionAsset` derivatives for SFX, VFX, or analytics.

---

## `{wordByWord}`

* __Purpose__ – Reveals text per word with optional callbacks.
* __Runtime handler__ – `WordByWordTagHandler`
* __Asset__ – `WordByWordTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `perWordDuration` | Float parameter | 0.2 | Delay between word batches. |
| `wordsPerBatch` | IntRandomizable parameter | 1 | Words revealed per step. |
| `onWordAction` | ActionAsset | null | Optional action triggered per word. |

```text
{wordByWord perWordDuration=reveal_medium wordsPerBatch=reveal_two onWordAction=word_sfx}
```

* __Inspector tips__ – Summary shows action assignment and batch range.
* __Extensions__ – Trigger dialogue audio or timeline events per word.

---

## `{action}`

* __Purpose__ – Invokes a scripted `ActionAsset` without applying visual changes.
* __Runtime handler__ – `ActionTagHandler`
* __Asset__ – `ActionTagHandlerAsset`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `action` | ActionAsset | Required | ScriptableObject implementing runtime logic. |

```text
{action action=shake_camera}
```

* __Inspector tips__ – Summary warns when the action reference is missing.
* __Extensions__ – Build composite actions with `ActionCompositeAsset` for complex behaviours.

---

## Extending the library

* __New handlers__ – Derive from `TagHandlerAsset`, expose parameters via serialized fields, and implement an `ITagHandler` to manipulate modifier chains.
* __New parameters__ – Extend `ParameterDefinitionAsset<T>` for custom parsing (hex colours, enums, vectors).
* __New subsystems__ – Implement `TextAnimatorSubsystemAsset` and accompanying applicators to target non-TextMeshPro outputs or additional properties.
* __Reusable actions__ – Chain behaviours with `ActionCompositeAsset` or implement bespoke `ActionAsset` derivatives for your gameplay hooks.

For a guided editor workflow, see the [Editor Setup Guide](editor_setup.md). For installation and samples, refer to the top-level `README.md`.
