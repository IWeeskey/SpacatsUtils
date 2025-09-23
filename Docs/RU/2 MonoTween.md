### Что такое MonoTween
`MonoTween` — это лёгкая система твинов без корутин и без внешних библиотек. Она состоит из:
- `MonoTweenUnit` — описывает один твин (задержка, длительность, прогресс-колбэки, повторы, пошаговый режим, локальная пауза и т. п.).
- `MonoTweenController` — управляющий контроллер, который обновляет запущенные твины каждый кадр, поддерживает цепочки, паузы и измерение производительности.
- Базовая инфраструктура контроллеров: `Controller` и `ControllersHub` — обеспечивают жизненный цикл и вызовы `CSharedUpdate()` в редакторе и в Play Mode.

Ниже — как всё устроено и как этим пользоваться.

---

### Архитектура и жизненный цикл
- `MonoTweenController` наследуется от `Controller` и регистрируется в `ControllersHub`.
- `ControllersHub` каждый кадр вызывает у всех контроллеров `CSharedUpdate()` (см. `ControllersHub.SSharedUpdate()` → `controller.CSharedUpdate()`), поэтому твин-система работает и в редакторе (если включено) и в рантайме.
- `MonoTweenController.CSharedUpdate()` вызывает внутренний `UpdateLogic()`:
  - Читает глобальную паузу из `PauseController.IsPaused` и сохраняет в `IsPaused`.
  - Если включены измерения, оборачивает обновление твинов в `TimeTracker.Start/Finish` и пишет метрики в `UpdateTimeMS`, `UpdateTimeString`.
  - Обходит активные твины с конца к началу и вызывает `tween.Update(Time.deltaTime, IsPaused)`.
  - Готовые твины удаляются из активной части списка O(1) заменой на последний элемент (`_tweens[i] = _tweens[lastIndex]`, затем `_activeCount--`).
  - Если твин завершился не «сломанным», вызываются его `OnEnd` и затем `OnChain` (для перехода к следующему звену цепочки).
- При выгрузке сцены (`COnSceneUnloading`) и при `OnDisable` у контроллера вызывается `BreakAll()` — все твины очищаются.

---

### MonoTweenUnit — один твин
Ключевые поля и свойства:
- Время и задержка:
  - `Delay` — задержка старта (сек).
  - `Duration` — длительность твина (сек) — клэмпится минимум к `0.0001f`.
- Колбэки:
  - `OnStart` — вызывается один раз при фактическом старте (после `Delay`).
  - `OnLerp(float t)` — вызывается каждый апдейт с прогрессом `t` от `0..1` (линейный), или по шагам (см. «Пошаговый режим»).
  - `OnEnd` — вызывается после полного завершения твина (в контроллере).
  - `OnChain` — вызывается контроллером следом за `OnEnd`, если твин часть цепочки.
- Повторы и шаги:
  - `RepeatCount` — сколько раз повторить после первого прохождения (т. е. общее число проигрываний = `RepeatCount + 1`).
  - `StepsCount` — если > 0, включает пошаговый режим (см. ниже).
- Паузы:
  - `ApplyGlobalPause` — учитывать ли глобальную паузу (`PauseController.IsPaused`).
  - Локальная пауза: `SelfPauseON()` / `SelfPauseOFF()`.
- Служебное:
  - `IsComplete` — завершён ли твин.
  - `IsBroken` — «сломанный» флаг (устанавливается через `Break()` — см. ниже).
  - `ChainIndex` — индекс прохода цепочки (увеличивается контроллером при `StartChain`).
  - `UnitID` — произвольная строка-метка.

Конструктор:
```
new MonoTweenUnit(
    float delay,
    float duration,
    Action onStart,
    Action<float> onLerp,
    Action onEnd,
    bool applyGlobalPause = true,
    int repeatCount = 0,
    int stepsCount = 0)
```
- Значения клэмпятся: задержка ≥ 0, длительность ≥ 0.0001, повторы ≥ 0, шаги ≥ 0.

Методы:
- `Start()` — вызывает `Reset()` и регистрирует юнит в контроллере: `MonoTweenController.Instance.StartSingle(this)`.
- `Reset()` — сбрасывает внутреннее состояние (таймеры, флаги, счётчики шагов/повторов).
- `Break()` — помечает твин как сломанный. Контроллер удалит его из активного списка, не вызывая `OnEnd` и `OnChain`.
- `Update(float deltaTime, bool isGlobalPaused)` — основной апдейт, вызывается контроллером.

Логика `Update` кратко:
- Если `IsComplete` или `IsBroken` — помечается завершённым и выходит.
- Если `ApplyGlobalPause && isGlobalPaused` — выходим (стоит игра на «паузе»).
- Если локальная пауза — выходим.
- Если ещё не стартовал: накапливаем `Delay`. Когда истёк — отмечаем старт, подготавливаем пошаговый режим и вызываем `OnStart`.
- Увеличиваем `_time`, считаем `t = clamp01(_time / Duration)`.
- Если `StepsCount > 0` — пошаговый режим: выстреливаем `OnLerp(stepProgress)` каждый раз, когда `_time` пересекает очередную границу шага. `stepProgress = stepIndex / StepsCount`.
- Иначе обычный режим: `OnLerp(t)` каждый кадр.
- Когда `t` достиг 1:
  - Если `_currentRepeat < RepeatCount` — увеличиваем повтор, обнуляем таймеры/индексы и продолжаем.
  - Иначе — `IsComplete = true`.

---

### MonoTweenController — как управляет твиными
Главные моменты (см. `MonoTweenController.cs`):
- Хранит список `_tweens` и счётчик активных `_activeCount`. Список может содержать «хвост» неактивных слотов (для O(1) удаления).
- `Add(MonoTweenUnit)` — добавляет твин в активную часть, если контроллер `enabled`.
- Ежекатровое обновление в `UpdateLogic()`:
  - Берёт глобальную паузу: `IsPaused = PauseController.IsPaused`.
  - Цикл по активным твиным с конца к началу: `tween.Update(Time.deltaTime, IsPaused)`.
  - По завершении твина:
    - Удаление O(1): текущий слот затирается последним активным, последний — в `null`, `activeCount--`.
    - Если твин не сломан: вызывает `tween.OnEnd?.Invoke()` и `tween.OnChain?.Invoke()`.
- Служебные методы:
  - `BreakAll()` — очищает все твины и обнуляет активные. Вызывается при выгрузке сцены и при отключении контроллера.
  - `StartSingle(MonoTweenUnit)` — зарегистрировать один твин.
  - `BreakSingle(MonoTweenUnit)` — вызвать `unit.Break()`.
- Цепочки:
  - `StartChain(int repeatCount, params MonoTweenUnit[] tweens)` — запускает последовательность твинов один за другим.
    - Внутри каждому элементу назначается `OnChain`: у промежуточных — пуск следующего `tweens[i+1].Start()`, у последнего — инкремент `tweens[0].ChainIndex` и перезапуск первого, если `repeatCount < 0` или `ChainIndex < repeatCount`.
    - В итоге `StartChain(-1, ...)` — бесконечный цикл.
  - `StopChain(params MonoTweenUnit[])` — вызывает `Break()` для каждого юнита цепочки (их `OnEnd/OnChain` не вызовутся).
  - `PauseChain(bool pause, params MonoTweenUnit[])` — локально ставит/снимает паузу `SelfPauseON/OFF` у каждого юнита.

Полезные свойства/метрики:
- `ActiveTweensCount` — сколько твинов сейчас активно.
- `TweensListCount` — размер внутреннего списка (может быть больше активных из-за O(1) удаления).
- `PerformMeasurements`, `UpdateTimeMS`, `UpdateTimeString` — измерение времени апдейта.

---

### Пошаговый режим (Steps)
Если задать `StepsCount > 0`, твин перестаёт слать «плавный» `t` каждый кадр. Вместо этого он шлёт дискретные значения прогресса `step/StepsCount` в моменты времени, когда пересекаются границы шагов:
- Длительность шага: `_stepDuration = Duration / StepsCount`.
- Пока `_time` перешагивает очередной `_nextStepTime`, увеличивается индекс шага и вызывается `OnLerp(stepProgress)`.
- Это удобно для «поэтапных» анимаций, прогресс-баров, счётчиков и т. п.

Пример из `TweenStepExample.cs`:
```
_stepTween = new MonoTweenUnit(
    delay: 0f,
    duration: TweenDuration,
    onStart: () => { ResetPosition(); },
    onLerp: (float lerp) => { LerpAnimatonTarget(LocalPositions[0], LocalPositions[1], lerp); },
    onEnd: () => { StartLoop(); },
    applyGlobalPause: true,
    repeatCount: 0,
    stepsCount: Steps
);
```
Перед каждым стартом можно менять `StepsCount` и `Duration`, затем `Reset()` и запускать снова.

---

### Пауза: глобальная и локальная
- Глобальная пауза: контроллер каждую рамку читает `PauseController.IsPaused`. Если у юнита `ApplyGlobalPause == true` и глобальная пауза включена — юнит не обновляется.
- Локальная пауза: можно поставить на паузу конкретные юниты, не трогая остальные:
  - Вручную через `SelfPauseON()` / `SelfPauseOFF()`.
  - Массово через `MonoTweenController.PauseChain(true/false, units...)`.
- Игнор глобальной паузы: передайте `applyGlobalPause: false` в конструктор — юнит будет обновляться даже при глобальной паузе (см. пример `TweenChainExampleIgnorePause.cs`).

---

### Цепочки твинов (StartChain)
Создаёте несколько `MonoTweenUnit` и запускаете их в цепочке:
```
var t0 = new MonoTweenUnit(0f, 1f, null, t => MoveAtoB(t), null);
var t1 = new MonoTweenUnit(0f, 1f, null, t => MoveBtoC(t), null);
var t2 = new MonoTweenUnit(0f, 1f, null, t => MoveCtoA(t), null);

// Бесконечная цепь
MonoTweenController.Instance.StartChain(-1, t0, t1, t2);

// Остановить
MonoTweenController.Instance.StopChain(t0, t1, t2);

// Поставить/снять локальную паузу на всю цепочку
MonoTweenController.Instance.PauseChain(true, t0, t1, t2);
```
Особенности:
- Контроллер сам назначит `OnChain` у каждого звена, чтобы запускать следующее.
- У последнего звена `OnChain` перезапускает первое и увеличивает `t0.ChainIndex`.
- Аргумент `repeatCount` в `StartChain(repeatCount, ...)` — это количество повторов «круга». Значение `< 0` — бесконечно; `0` — сыграть один круг и остановиться; `3` — сыграть 4 круга (индексы 0..3).

---

### Примеры использования из проекта
- `TweenChainExample.cs` — 4 твина, которые по кругу двигают объект по 4 точкам. Запуск: `StartChain(-1, ...)`, пауза всей цепочки через локальную паузу `PauseChain`.
- `TweenChainExampleIgnorePause.cs` — то же, но юниты создаются с `applyGlobalPause: false` и игнорируют глобальную паузу. Также видно обновление `CurrentTweenPlayingIndex` и `CurrentChainIndex`.
- `TweenStepExample.cs` — демонстрирует «ступенчатый» прогресс через `StepsCount` и циклический перезапуск в `onEnd`.

---

### Частые вопросы и тонкости
- Почему твин не стартует сразу? Учитывается `Delay` — сначала накапливается задержка, только затем `OnStart` и обновления прогресса.
- Чем отличается `RepeatCount` от `StartChain(repeatCount, ...)`? 
  - `RepeatCount` — повторы внутри одного юнита (прогоняет ту же анимацию много раз подряд перед завершением юнита).
  - `StartChain(repeatCount, ...)` — количество повторов всей последовательности юнитов.
- Почему не вызывается `OnEnd`? Если юнит «сломали» через `Break()`, контроллер удалит его без `OnEnd/OnChain`.
- Можно ли изменить длительность/шаги «на лету»? Да: перед следующим стартом (или перезапуском) меняете поля (`Duration`, `StepsCount`), затем `Reset()` и `Start()` или `StartSingle()`.
- Производительность: удаление из списка — O(1); есть опция замеров `PerformMeasurements` с выводом в `UpdateTimeMS`/`UpdateTimeString`.

---

### Быстрый старт (шаблон)
```
// 1) Получите контроллер (если он в сцене инициализирован ControllersHub'ом)
var tween = new MonoTweenUnit(
    delay: 0f,
    duration: 0.5f,
    onStart: () => {/* init */},
    onLerp: t => { target.position = Vector3.Lerp(a, b, t); },
    onEnd: () => {/* done */},
    applyGlobalPause: true,
    repeatCount: 0,
    stepsCount: 0
);

// 2) Запустите
MonoTweenController.Instance.StartSingle(tween);

// 3) Поставить на паузу локально
tween.SelfPauseON();

// 4) Снять паузу
tween.SelfPauseOFF();

// 5) Остановить (без OnEnd/OnChain)
MonoTweenController.Instance.BreakSingle(tween); // эквивалент tween.Break();
```

Если нужно детальнее разобрать конкретный сценарий (например, совмещение глобальной и локальной паузы, разные режимы повторов или цепей) — опишите кейс, приведу точный фрагмент кода и рекомендации.