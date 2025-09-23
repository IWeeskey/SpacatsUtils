### Overview
Below is a concise, engineer-friendly explanation of each small utility and why they’re useful and reusable across Unity projects. All classes are taken from the provided Spacats Utils codebase (Unity 2022.3.39f1).

---

### TimeTracker
- What it is: A simple static micro-profiler that measures elapsed time between `Start(tag)` and `Finish(tag)` calls using `System.Diagnostics.Stopwatch`.
- How it works:
  - Keeps a dictionary `tag -> Stopwatch`.
  - `Start(tag)`: starts or restarts the stopwatch.
  - `Finish(tag, showLogs)`: stops the stopwatch, computes milliseconds and potential FPS (`1000/ms`), optionally logs `[TimeTracker] 'tag': X ms Potential FPS: Y`, returns `(ms, message)` and removes the timer from the dictionary.
  - Resets automatically on editor load/runtime init.
- Why useful in other projects:
  - Zero-setup spot measurements for any block of code (procedural generation, loading, complex update loops) without external profilers.
  - Lightweight, allocation-free during active timing; logs are optional.
- Example:
  ```csharp
  TimeTracker.Start("GenerateLevel");
  GenerateLevel();
  var (ms, msg) = TimeTracker.Finish("GenerateLevel");
  ```

---

### GUIButtons
- What it is: A minimal on-screen button strip helper for quick prototyping and sample scenes.
- How it works:
  - Uses IMGUI `OnGUI()` to render N buttons in a row at the bottom of the screen.
  - Configurable: button count, height percent, spacing, font scale; toggle `PerformLogic`.
  - Meant to be subclassed: override `GetButtonLabel(int)` and `OnButtonClick(int)`.
- Why useful in other projects:
  - Rapidly add interactive controls to test features in play mode (e.g., trigger scene loads, toggle pause, spawn objects) without building full UI.
  - Great for demo scenes, QA toggles, internal tools.
- Example:
  ```csharp
  public class MyDebugButtons : GUIButtons {
      protected override string GetButtonLabel(int i) => i switch {
          0 => "Pause",
          1 => "Spawn",
          _ => base.GetButtonLabel(i)
      };
      protected override void OnButtonClick(int i) { if (i==0) PauseController.Instance.SwitchPause(); }
  }
  ```

---

### GUIFps
- What it is: A simple on-screen FPS readout with short and medium rolling stats.
- How it works:
  - Updates every frame using `Time.unscaledDeltaTime`.
  - Tracks:
    - Avg over ~1s (`_avg1Result`)
    - Avg over ~10s (`_avg10Result`)
    - Minimum over ~10s (`_min10FPS`)
  - Draws labels via IMGUI with configurable position, size, color; optional extra stats.
- Why useful in other projects:
  - Always-on performance indicator in dev builds/tests without opening the Unity Profiler.
  - Helps detect frame spikes (via 10s min) vs sustained performance (1s/10s avg).

---

### GUILogViewer
- What it is: An in-game console log overlay you can open/close, with filtering by log type via color.
- How it works:
  - Singleton-like `Instance` registered via controller lifecycle.
  - Subscribes to `Application.logMessageReceived` (when `LoggingEnabled`) and stores a list of `{ Message, LogType }`.
  - Draws a semi-transparent scrollable window (IMGUI) between configurable top/bottom screen percentages; font size scales with screen width.
  - API: `OpenLog()`, `CloseLog()`, `ClearLog()`, `IsOpened`, `LoggingEnabled`.
- Why useful in other projects:
  - View logs directly on device (mobile/console) without attaching a debugger.
  - Handy for QA builds and field testing; quick toggling from `GUIButtons`.

---

### GUIPermanentMessage
- What it is: A tiny always-on text overlay to show a single, persistent message (e.g., current scene, build tag, environment).
- How it works:
  - Singleton-like `Instance` set during controller registration.
  - Exposes `Message` string; draws it with configurable position, size, color via IMGUI.
- Why useful in other projects:
  - Display current state: scene name, player ID, server region, debug flags, etc.
  - Non-invasive and resolution-independent; great for test builds.
- Example:
  ```csharp
  GUIPermanentMessage.Instance.Message = $"Scene: {SceneManager.GetActiveScene().name}";
  ```

---

### PlaneGridObjectCreator
- What it is: A quick utility to spawn a 2D grid of prefab instances under a parent transform, with position gaps and random euler rotations.
- How it works:
  - Two modes:
    - `Start()` coroutine for play mode: iterates X/Z and optionally yields per row (`IEnumeratorCreation`).
    - `GenerateImmediate()` editor/runtime immediate generation with `TimeTracker` measurement.
  - Computes centered local positions using `Gap` and grid size; randomizes eulers within `[MinEulers, MaxEulers]`.
  - `Clear()` removes all children from `Parent` via `DestroyImmediate`.
  - Virtual hook `OnObjectInstantiated(go, x, z)` to customize per-instance behavior.
- Why useful in other projects:
  - Rapid prototyping of arenas, stress tests (many objects), layout previews.
  - Editor-time generation to bake sample scenes; consistent positioning with easy clearing.

---

### PauseController and PauseDependent components
- PauseController (global pause state)
  - What it is: A central, scene-level pause flag with events.
  - How it works:
    - Static `IsPaused` boolean and event `OnPauseSwitched(bool)`.
    - Methods: `SwitchPause()`, `PauseON()`, `PauseOFF()`; no-op in editor if not playing.
    - Other systems (e.g., MonoTween) or components read this flag or subscribe to the event.
  - Why useful in other projects:
    - Provides a single authoritative pause source; easy to wire gameplay, UI, audio, etc.

- AnimatorPause (pause listener for Animator)
  - Subscribes to `PauseController.OnPauseSwitched`.
  - On pause: saves `Animator.speed` and sets it to 0; on resume: restores prior speed.
  - Useful whenever character or UI animation must freeze/resume cleanly.

- ParticlePause (pause listener for ParticleSystem)
  - Subscribes to the same event.
  - On pause: `ParticleSystem.Pause(true)` if playing; on resume: `.Play(true)` if paused.
  - Ensures particle effects stop advancing while paused (no desync).

- TrailPause (pause-aware TrailRenderer)
  - Subscribes to the event and tracks `_pauseTime`/`_resumeTime`.
  - On pause: sets `TrailRenderer.time = Infinity` so existing trails persist during pause.
  - On resume: extends trail time by the pause duration, then smoothly returns to the original `_trailTime` using `TimeRecoverySpeed` in `Update()`.
  - Helpers: `FinishTrail()`/`ResumeFromeFinish()` to force and restore trail length.
  - Useful to avoid trails instantly shrinking/clearing during a pause; keeps visual continuity.

- General reuse value:
  - Adopt the event-driven pattern for other systems: audio, custom updaters, physics toggles, AI loops, DOTS bridges, etc.

---

### SceneController and SceneLoaderHelper
- SceneController (high-level loading orchestration)
  - What it is: A controller singleton that standardizes immediate and async scene loading with callbacks.
  - How it works:
    - Fields: `_isLoading`, `_loadingSceneName`, `_sceneDelayTween`.
    - Events: `OnLoadStarted(sceneName)`, `OnLoading(sceneName, progress)`, `OnLoadFinished(sceneName)`.
    - `LoadSceneImmediate(sceneName, delay=0)`: loads with optional delay via a small `MonoTweenUnit`. Fires events accordingly.
    - `LoadSceneAsync(sceneName)`: guards against editor misuse, sets `_isLoading`, calls `SceneLoaderHelper.LoadSceneAsync`, and logs progress.
    - Overrides `COnSceneLoaded` to reset loading state and fire `OnLoadFinished`.
  - Why useful in other projects:
    - Centralizes scene transitions; easy to hook loading screens, analytics, audio fades.
    - Eliminates scattered `SceneManager` usage and duplicate state flags.

- SceneLoaderHelper (low-level cross-cutting helpers)
  - What it is: Static utilities around `SceneManager` for both editor and runtime.
  - How it works:
    - Editor helpers: `MarkActiveSceneDirty()` and `SaveActiveScene()` under `#if UNITY_EDITOR`.
    - Queries: `IsSceneLoaded(sceneName)`.
    - Loading:
      - `LoadScene(sceneName, mode)` immediate.
      - `LoadSceneAsync(runner, sceneName, onProgress, mode)`: starts a coroutine on `runner` that calls `LoadSceneAsync`; sets `allowSceneActivation=false` to report progress up to ~0.9, then emits 1.0 and activates.
  - Why useful in other projects:
    - Reusable pattern for async progress bars and delayed activation (hook loading UI at 0.9 until ready).
    - Keeps editor-only bits isolated and improves code hygiene.

---

### How these pieces fit together in practice
- Dev HUD: Combine `GUIFps`, `GUILogViewer`, and `GUIPermanentMessage` for on-device diagnostics: FPS, real-time logs, build/scene labels.
- Quick tools: Use `GUIButtons` to toggle `PauseController`, trigger `SceneController` loads, or call `PlaneGridObjectCreator.GenerateImmediate()`.
- Pause integration: Add `AnimatorPause`, `ParticlePause`, `TrailPause` components to relevant GameObjects and control all via a single `PauseController` in the scene.
- Loading flows: Drive UI with `SceneController` events; use `SceneLoaderHelper.LoadSceneAsync` for progress, and `OnLoadFinished` to hide loading screens.
- Performance checks: Wrap heavy operations with `TimeTracker` to spot regressions and optimize hotspots.