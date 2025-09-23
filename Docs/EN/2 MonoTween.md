### MonoTween: Overview
MonoTween is a lightweight, code-first tweening utility designed to run inside your Unity scene via a single manager, `MonoTweenController`. You create tweens as plain C# objects (`MonoTweenUnit`) and the controller updates them every frame, handling delays, durations, steps, repeats, pausing, chaining, and completion callbacks.

Key ideas:
- `MonoTweenUnit` holds the logic and callbacks for a single tween.
- `MonoTweenController` is a scene component that updates all active `MonoTweenUnit` instances.
- You can run a single tween, a chain of tweens, pause/resume them (globally or individually), stop them, and measure controller update time.
- The controller integrates with the project’s `ControllersHub` to unify lifecycle and scene events across controllers.

---

### How it works (internals in plain English)
A `MonoTweenUnit` encapsulates a timeline with:
- Delay: initial wait before the tween starts.
- Duration: how long the tween runs from 0 to 1 progress.
- Steps (optional): discrete step-based progression instead of smooth progression.
- Repeat count: number of times to replay after it reaches 1. 0 means no repeats.
- Callbacks:
  - `OnStart`: fired once, when the tween begins after the delay.
  - `OnLerp(float t)`: fired during the tween with normalized progress `t` in [0..1]. For step-based mode, it’s invoked per step with `t` equal to stepProgress.
  - `OnEnd`: fired once, after final completion (unless the tween is broken).
  - `OnChain`: used by the controller for chaining (to launch the next tween in a chain).

Lifecycle and update flow:
1. You construct a `MonoTweenUnit` with delay, duration, callbacks, and options.
2. You call `Start()` on the unit. Internally it resets its state and registers itself to `MonoTweenController.Instance` via `StartSingle`.
3. Each frame, `MonoTweenController` calls `Update(deltaTime, isGlobalPaused)` on active tweens:
   - Honors global pause if `ApplyGlobalPause` is true and `PauseController.IsPaused` is active.
   - Honors per-tween self-pause.
   - Waits out the configured `Delay`.
   - Fires `OnStart` once when the tween first begins.
   - In smooth mode (no steps): computes `t = timeElapsed / Duration` clamped to [0..1], invokes `OnLerp(t)` each frame.
   - In step mode (`StepsCount > 0`): splits the duration into equal step slots; invokes `OnLerp(stepProgress)` whenever the timeline crosses a step boundary. This means `OnLerp` is called exactly `StepsCount` times per run, with `stepProgress = stepIndex / StepsCount`.
   - When `t` reaches 1.0:
     - If `RepeatCount > 0` and not yet exhausted, the unit restarts timing (including steps); otherwise it marks itself complete.
4. When a unit completes, the controller removes it from the active list, invokes `OnEnd`, and then `OnChain` (unless the tween was forcibly broken).

Breaking and pausing:
- `Break()`: marks the unit as broken; controller drops it and skips `OnEnd`/`OnChain`.
- `SelfPauseON()` / `SelfPauseOFF()`: per-tween pause without affecting others.
- `ApplyGlobalPause` (constructor param): if true (default), the tween respects the global `PauseController.IsPaused` state.

Performance notes:
- The controller reuses a list segment for active tweens and compacts from the end when units complete.
- You can enable execution-time measurements on the controller (`PerformMeasurements`) to get `UpdateTimeMS` and a formatted string in the inspector/editor.

---

### Setup: getting started
1. Add `ControllersHub` to your scene.
   - This hub is the backbone that registers and updates all controllers in a unified way across scenes and editor runtime. It also forwards scene lifecycle events and can reparent controllers under a common hub object for organization.
2. Add `MonoTweenController` to the scene.
   - It will auto-register with `ControllersHub` on enable and become the singleton `MonoTweenController.Instance` (required by `MonoTweenUnit.Start()`).
   - Execution order is set to `DefaultExecutionOrder(-10)` so tween updates occur early.
3. (Optional) Add a `PauseController` if you want global pause support.
   - `MonoTweenController` checks `PauseController.IsPaused` each update to compute the `isGlobalPaused` argument passed into units.

Important: `MonoTweenUnit.Start()` uses `MonoTweenController.Instance` internally. If the controller is not in the scene yet, you’ll get an error: “MonoTweenController is not initialized yet!”. Always ensure the controller exists before starting tweens.

---

### Quick usage examples

#### Smooth tween (no steps)
```csharp
using Spacats.Utils;
using UnityEngine;

public class MoveExample : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        var startPos = target.position;
        var endPos = startPos + new Vector3(3f, 0f, 0f);

        var tween = new MonoTweenUnit(
            delay: 0.2f,
            duration: 1.5f,
            onStart: () => Debug.Log("Move start"),
            onLerp: t =>
            {
                // Linear move; plug in your own easing if needed
                target.position = Vector3.Lerp(startPos, endPos, t);
            },
            onEnd: () => Debug.Log("Move end"),
            applyGlobalPause: true,
            repeatCount: 0,
            stepsCount: 0
        );

        tween.Start();
    }
}
```

#### Stepped tween (invoke `OnLerp` a fixed number of times)
```csharp
var tween = new MonoTweenUnit(
    delay: 0f,
    duration: 1.0f,
    onStart: null,
    onLerp: stepT =>
    {
        // stepT will be 1/5, 2/5, 3/5, 4/5, 5/5 across the timeline
        DoSomethingAtDiscreteStep(stepT);
    },
    onEnd: () => Debug.Log("Stepped tween done"),
    applyGlobalPause: true,
    repeatCount: 0,
    stepsCount: 5
);

tween.Start();
```

#### Repeating tween
```csharp
var pulse = new MonoTweenUnit(
    delay: 0f,
    duration: 0.5f,
    onStart: null,
    onLerp: t => myObject.localScale = Vector3.one * Mathf.Lerp(1f, 1.2f, t),
    onEnd: () => myObject.localScale = Vector3.one,
    applyGlobalPause: true,
    repeatCount: 3,   // play 4 times total: initial + 3 repeats
    stepsCount: 0
);

pulse.Start();
```

#### Chaining tweens
You can chain multiple `MonoTweenUnit`s so that each starts after the previous finishes. You can also repeat the whole chain.

```csharp
var show = new MonoTweenUnit(0f, 0.25f, null, t => panel.alpha = t, null);
var hold = new MonoTweenUnit(0f, 1.00f, null, t => { /* do nothing, just wait */ }, null);
var hide = new MonoTweenUnit(0f, 0.25f, null, t => panel.alpha = 1f - t, null);

// Run once:
MonoTweenController.Instance.StartChain(repeatCount: 1, show, hold, hide);

// Run 3 times:
MonoTweenController.Instance.StartChain(repeatCount: 3, show, hold, hide);

// Infinite loop (pass a negative repeatCount):
MonoTweenController.Instance.StartChain(repeatCount: -1, show, hold, hide);

// Pause or stop the whole chain:
MonoTweenController.Instance.PauseChain(true, show, hold, hide); // pause
MonoTweenController.Instance.PauseChain(false, show, hold, hide); // resume
MonoTweenController.Instance.StopChain(show, hold, hide); // stop immediately
```

Notes on chaining:
- The controller wires each tween’s `OnChain` so finishing one starts the next.
- After the last tween, the controller either restarts the first (if `repeatCount < 0` for infinite or until count is met) or stops the chain.

#### Manually breaking a tween
```csharp
var spin = new MonoTweenUnit(0f, 2.0f, null, t =>
{
    target.rotation = Quaternion.Euler(0f, t * 360f, 0f);
}, () => Debug.Log("spin done"));

spin.Start();

// Later
spin.Break(); // controller removes it and will NOT call OnEnd/OnChain
```

#### Per-tween pause vs. global pause
```csharp
// Per-tween pause
tween.SelfPauseON();
// ... later
tween.SelfPauseOFF();

// Global pause (requires PauseController in the scene)
PauseController.IsPaused = true;  // all tweens with applyGlobalPause == true will freeze
```

---

### Inspector/runtime info on MonoTweenController
- `ActiveTweensCount`: number of currently updating tweens.
- `TweensListCount`: underlying list capacity currently in use.
- `PerformMeasurements`: when enabled, measures update cost via `TimeTracker`.
- `UpdateTimeMS` and `UpdateTimeString`: last frame’s measured duration.

---

### Role of ControllersHub (and why you need it)
`ControllersHub` is a singleton manager that:
- Registers all `Controller`-derived components (including `MonoTweenController`).
- Centralizes lifecycle notifications: scene loading/unloading, enable/disable, editor vs. play mode execution.
- Calls each controller’s shared update method (`CSharedUpdate`) where `MonoTweenController` performs its tween updates.
- Keeps your hierarchy clean (it can reparent controllers under a single hub object and maintain a canonical name like "[SpaCats] ControllersHub").

In other words, `ControllersHub` is the backbone; `MonoTweenController` is the tween-specific worker. Add both to the scene so `MonoTweenController.Instance` is valid and receives updates.

---

### Best practices and tips
- Always ensure `MonoTweenController` exists before calling `MonoTweenUnit.Start()`.
- Prefer caching references you touch inside `OnLerp` (e.g., `Transform`, `CanvasGroup`) for performance.
- Add your own easing by applying an easing function to `t` before using it (e.g., `t = t*t*(3f - 2f*t)` for smoothstep) in `OnLerp`.
- Use `stepsCount` for deterministic, event-like progress (particle bursts, sounds, milestones), and `stepsCount == 0` for smooth motion.
- Use `repeatCount < 0` when chaining for infinite loops; otherwise provide an explicit number.
- Use `SelfPauseON/OFF` for ad-hoc pauses (e.g., hovering a tooltip), and `PauseController.IsPaused` for global game pause.
- Call `StopChain` to cancel a chain; it `Break()`s each tween so `OnEnd`/`OnChain` aren’t called post-break.

---

### Minimal checklist to start using MonoTween
- Add `ControllersHub` to the scene.
- Add `MonoTweenController` to the scene (it will register with the hub and become the singleton instance).
- (Optional) Add `PauseController` if you want global pause behavior.
- Create `MonoTweenUnit` instances in your scripts and call `Start()` on them.
- Optionally use `StartChain`, `PauseChain`, and `StopChain` from `MonoTweenController.Instance` to orchestrate multiple tweens.

With that, you’re ready to tween transforms, UI, and any numeric values by mapping `t` in `OnLerp` to your properties.