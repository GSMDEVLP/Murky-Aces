# CODEX.md — Murky Aces

> Главный контекст и архитектурные правила проекта для Codex.
>
> Этот файл описывает текущую зафиксированную архитектуру проекта, границы модулей, правила зависимостей и рекомендуемый порядок реализации.
>
> Если какое-либо решение здесь помечено как `TBD`, Codex не должен придумывать его самостоятельно. Нужно сохранить текущую архитектуру и дождаться отдельного решения.

---

# 1. Проект

**Название:** Murky Aces

**Жанр:** кооперативный PvE FPS / социально-кооперативный экшен от первого лица с элементами:
- управления общей боевой машиной;
- выживания;
- триллера;
- хоррора;
- шутера;
- совместного обслуживания техники.

**Основная платформа:** Windows PC

**Площадка релиза:** Steam

**Количество игроков:** 1–4

**AI-боты для замены отсутствующих игроков:** не используются.

Игра должна оставаться запускаемой и играбельной:
- в одиночку;
- вдвоём;
- втроём;
- вчетвером.

Отсутствующие роли экипажа не заменяются ботами. Один игрок при необходимости может выполнять несколько функций вручную.

---

# 2. Версия Unity и технологический стек

## 2.1. Unity

Использовать:

```text
Unity 6000.5.4f1
```

## 2.2. Основной стек

Используются:

- Unity 6;
- C#;
- Zenject;
- UniTask;
- R3;
- Addressables;
- PrimeTween;
- UniText;
- New Input System;
- Odin Inspector;
- URP;
- NuGetForUnity;
- ScriptableObject для статических конфигураций.

## 2.3. Не используется

Не использовать:

- Entitas;
- ECS;
- Jenny;
- архитектуру вокруг generated ECS-кода;
- старый `UnityEngine.Input`;
- прямой gameplay-polling через `Keyboard.current`, `Mouse.current` и т.п.;
- `CharacterController` для движения Player без отдельного пересмотра принятого решения;
- обязательную физическую Rigidbody-модель движения танка.

---

# 3. Архитектурный стиль

Проект строится вокруг:

```text
Feature-Oriented OOP
+
Composition over Inheritance
+
Explicit GameLoop
+
Zenject DI
```

Главный принцип:

```text
Feature first
```

а не:

```text
Controllers/
Services/
Managers/
Models/
Views/
```

Не создавать одну огромную папку с техническими типами, куда смешиваются классы из разных механик.

Правильнее:

```text
Gameplay/
└── Features/
    ├── Player/
    ├── Input/
    ├── Interaction/
    ├── Tank/
    ├── Items/
    ├── Inventory/
    ├── Combat/
    ├── Missions/
    ├── AI/
    └── Extraction/
```

---

# 4. Общая схема зависимостей

Базовое направление зависимостей:

```text
Unity / Presentation
        ↓
Application
        ↓
Domain
```

Infrastructure предоставляет технические реализации:

```text
Domain / Application
        ↑
Infrastructure
```

Domain не должен зависеть от:

- MonoBehaviour;
- Transform;
- Rigidbody;
- Animator;
- PlayerInput;
- Addressables API;
- SceneManager;
- UI;
- конкретных Unity-компонентов.

Unity-специфичный код должен находиться ближе к Presentation / Infrastructure.

---

# 5. MonoBehaviour

MonoBehaviour использовать только там, где действительно нужен Unity lifecycle или Unity API.

Допустимые примеры:

```text
PlayerView
TankView
DoorView
GameLoopRunner
CameraController
AnimationView
AudioView
```

Не делать обычные игровые классы MonoBehaviour без необходимости.

Плохо:

```text
MissionController : MonoBehaviour
FuelController : MonoBehaviour
DamageCalculator : MonoBehaviour
InventoryService : MonoBehaviour
```

если этим классам не нужен Unity lifecycle.

Предпочтительно:

```text
plain C# class
```

---

# 6. Центральный GameLoop

В проекте используется один центральный GameLoop.

Обычные gameplay-классы не должны иметь собственные:

```csharp
Update()
FixedUpdate()
LateUpdate()
```

если это не действительно Unity-specific View-класс.

## 6.1. Точка входа

Примерная схема:

```text
GameLoopRunner : MonoBehaviour
        │
        ├── Update()
        │      ↓
        │   GameLoop.Tick()
        │
        ├── FixedUpdate()
        │      ↓
        │   GameLoop.FixedTick()
        │
        └── LateUpdate()
               ↓
            GameLoop.LateTick()
```

## 6.2. Фазы

Один центральный `GameLoop` организует обработку в следующем логическом порядке:

```text
Ввод пользователя
    ↓
Принятие решений ИИ
    ↓
Геймплей
    ↓
Презентация
    ↓
Очистка
```

1. **Ввод пользователя.** В начале кадра считывает действия локальных игроков и преобразует их в намерения или команды, которые затем обрабатывает геймплей. Эта фаза не меняет игровое состояние напрямую.
2. **Принятие решений ИИ.** ИИ выбирает действия для управляемых им сущностей и преобразует их в такие же намерения или команды для геймплея. Эта фаза не означает наличие ботов для замены отсутствующих игроков: таких ботов в проекте нет. Детали устройства ИИ остаются `TBD`.
3. **Геймплей.** Выполняет основную симуляцию и меняет игровое состояние: движение, бой, смерть, таймеры, способности и другие игровые механики. Здесь обрабатываются намерения от игрока и ИИ.
4. **Презентация.** Читает итоговое игровое состояние и отражает его в Unity: обновляет `Animator` и UI, запускает визуальные эффекты и звуки. Обычно она не меняет геймплейные данные.
5. **Очистка.** В конце прохода удаляет или сбрасывает временные данные, накопившиеся за кадр, если такая очистка нужна.

`FixedUpdate` используется для физически зависимой части **Геймплея**, а `LateUpdate` — для поздней части **Презентации** (например, камеры). Это точки вызова Unity, а не дополнительные логические фазы. `FixedUpdate` может выполняться несколько раз за один визуальный кадр или не выполняться вовсе, поэтому порядок выше не следует понимать как буквальную последовательность вызовов `Update → FixedUpdate → LateUpdate` в каждом кадре.

Внутренняя детализация фаз может расширяться позже, но их ответственность и порядок задаются архитектурой, а не случайным порядком `MonoBehaviour`.

---

# 7. Zenject

Zenject — основной DI-контейнер проекта.

Zenject отвечает за:

- создание зависимостей;
- lifetime;
- composition root;
- factories;
- scene-level bindings;
- feature installers.

## 7.1. Запрещён Service Locator

Не использовать runtime-код вида:

```csharp
Container.Resolve<T>();
```

посреди gameplay-логики.

Зависимости должны быть явно видимы через constructor injection или `[Inject]`.

## 7.2. ProjectContext

ProjectContext должен оставаться маленьким.

Туда попадают только глобальные сервисы, живущие всю игру.

Пример:

```text
GameLoop
SceneLoader
AssetLoader
Save infrastructure
ConfigProvider
Audio infrastructure
```

Не размещать там конкретные runtime-объекты:

```text
Player
Tank
Enemy
Mission
Weapon
```

## 7.3. Scene / Feature installers

Предполагаемая схема:

```text
ProjectInstaller
        │
        ├── GameLoop
        ├── Assets
        ├── Scene services
        └── Global services

GameplayInstaller
        │
        ├── PlayerInstaller
        ├── InputInstaller
        ├── InteractionInstaller
        ├── TankInstaller
        ├── ItemsInstaller
        ├── CombatInstaller
        ├── MissionInstaller
        └── AIInstaller
```

---

# 8. Сцены

На текущем этапе зафиксирован базовый поток:

```text
Bootstrap
    ↓
MainMenu
    ↓
Lobby
    ↓
Gameplay
```

Расширение сцен и устройство отдельных карт пока `TBD`.

Codex не должен самовольно вводить новую сложную scene-архитектуру без необходимости.

---

# 9. Game Flow

Базовый flow:

```text
Application Start
        ↓
Bootstrap
        ↓
Main Menu
        ↓
Lobby
        ↓
Gameplay
```

В дальнейшем предполагается отдельный GameFlow-контроллер или state machine.

Внутри Gameplay ожидается собственный flow миссии.

Пример:

```text
MissionStart
    ↓
Scavenge
    ↓
Objective
    ↓
Extraction
    ↓
Completed / Failed
```

---

# 10. Player

Player movement уже реализован.

Для движения используется:

```text
Rigidbody
+
CapsuleCollider
```

Движение и физический поворот Player выполняются через существующий центральный `GameLoop` в `FixedTick`.

Не заменять `Rigidbody` на `CharacterController` без отдельного пересмотра этого решения.

Player должен строиться через композицию.

Пример:

```text
Player
├── Movement
├── Camera
├── Interaction
├── Inventory
├── Health
├── Equipment
├── Hands
└── StationController
```

Не создавать монолитный:

```text
PlayerController
```

на тысячи строк.

---

# 11. Input

Используется только:

```text
New Input System
```

Базовая схема:

```text
InputActionAsset
        ↓
PlayerInput
        ↓
InputService
        ↓
PlayerInputSystem
        ↓
PlayerIntentBuffer
        ↓
Gameplay
```

`InputService` является Unity-specific reader для `PlayerInput`. `PlayerInputSystem` преобразует считанные значения в намерения игрока, а gameplay зависит только от `PlayerIntentBuffer`.

Отдельный `IPlayerInput` не вводится, пока существует только один реальный источник локального ввода. Если позже понадобятся replay, сетевые команды или другие источники намерений, абстракцию следует вводить на уровне источника намерений (`IPlayerIntentSource`), а не маскировать их под локальный ввод.

Gameplay-код не должен напрямую читать:

```csharp
Keyboard.current
Mouse.current
Gamepad.current
Input.GetKey(...)
Input.GetAxis(...)
```

## 11.1. Контексты ввода

Игра имеет разные режимы:

```text
Walking
Driving
Gunner
Commander
UI
Inventory
Terminal
```

Не размазывать по коду проверки вида:

```csharp
if (_isDriving)
if (_isGunner)
if (_isInMenu)
```

Предпочтительно использовать Action Maps / Input Context.

---

# 12. Interaction

Interaction — отдельная фундаментальная feature.

Через неё должны проходить:

- двери;
- люки;
- рычаги;
- кнопки;
- посадка в места экипажа;
- подбор предметов;
- загрузка снарядов;
- ремонт;
- заправка;
- сварка;
- установка модулей;
- эвакуация;
- совместное перемещение тяжёлых предметов.

Базовые категории взаимодействий:

```text
Press
Hold
Continuous
Cooperative
```

Не создавать отдельную систему взаимодействия под каждый объект.

## 12.1. Базовый поток

```text
InputService
        ↓
PlayerInputSystem
        ↓
PlayerIntentBuffer
        ↓
PlayerInteraction
        ├── IInteractionTargetFinder
        ├── IInteractable
        └── read-only interaction state
                    ↓
        InteractionHudPresenter
                    ↓
        InteractionHudView
```

- `InputService` сообщает сырые состояния кнопки: pressed, held и released;
- тип взаимодействия и длительность Hold задаются interactable, а не `InputAction`;
- `PlayerInteraction` владеет текущей целью, прогрессом и жизненным циклом Begin / Complete / Cancel;
- Unity Physics используется только реализацией `IInteractionTargetFinder`;
- gameplay не изменяет Canvas напрямую: `InteractionHudPresenter` читает состояние `PlayerInteraction`, а `InteractionHudView` отображает его через uGUI;
- состояние Interaction создаётся отдельно для каждого Player в его Zenject subcontainer.

Для HUD используется MVP: `PlayerInteraction` выступает Model, `InteractionHudPresenter` является обычным C#-классом, а `InteractionHudView` — единственным `MonoBehaviour`, работающим с Canvas, TMP и `Image`. Отдельный интерфейс View пока не вводится, так как существует только одна реализация.

Hold прерывается при отпускании кнопки, потере цели, выходе из дистанции, появлении препятствия, недоступности цели или смене gameplay-контекста. Прогресс не переносится между целями.

## 12.2. Контракты и границы ответственности

`Interact` в `InputActionAsset` должен быть обычным `Button` без встроенного `Hold interaction`. `InputService` считывает три независимых состояния:

```text
PressedThisFrame
Held
ReleasedThisFrame
```

`PlayerInputSystem` переносит эти состояния в `PlayerIntentBuffer`. Таймер Hold не хранится ни в `InputService`, ни в `PlayerIntentBuffer`.

`IInteractable` предоставляет текущее предложение взаимодействия и поддерживает жизненный цикл:

```text
GetInteractionInfo
Begin
Complete
Cancel
```

`InteractionInfo` должен содержать как минимум:

```text
доступность
режим Press / Hold
длительность Hold
данные для UI prompt
```

Контекст взаимодействия должен идентифицировать игрока, выполняющего действие. Это необходимо для будущей проверки занятости станций и cooperative interaction, но не должно зависеть от выбранной network-библиотеки.

`PlayerInteraction` — обычный C# application-класс без собственного `Update`. Он обновляется существующим `GameplayPhase` после обработки `PlayerLook` и отвечает за:

- текущую цель;
- начало взаимодействия;
- накопление Hold-прогресса;
- повторную проверку доступности цели;
- завершение или отмену;
- предоставление read-only состояния фокуса и времени активного Hold для Presenter.

`IInteractionTargetFinder` только ищет цель и не выполняет взаимодействие. Unity-реализация использует луч из точки взгляда, настраиваемую дистанцию и маску слоёв. Препятствие должно блокировать цель: нельзя искать interactable отдельной маской так, чтобы луч проходил сквозь стены. Для коллайдеров на дочерних объектах используется явная ссылка на корневой interactable, например `InteractionTargetLink`.

`InteractionHudPresenter` читает `PlayerInteraction` в presentation-фазе и передаёт View готовые команды `Hide`, `ShowPress` и `ShowHold`. Gameplay не должен напрямую изменять Canvas или конкретные UI-компоненты.

Логическая структура feature:

```text
Interaction/
├── Abstraction/
│   ├── IInteractable
│   ├── IInteractionTargetFinder
│   └── IPickupReceiver
├── Domain/
│   ├── InteractionInfo
│   ├── InteractionContext
│   ├── InteractionMode
│   └── InteractionCancelReason
├── Application/
│   ├── PlayerInteraction
│   └── HoldInteractionSession
├── Infrastructure/
│   ├── InteractionInstaller
│   ├── PhysicsInteractionTargetFinder
│   └── InteractionTargetLink
├── Presentation/
│   ├── InteractionHudPresenter
│   └── InteractionHudView
└── Sandbox/
    ├── ToggleInteractable
    ├── TimedInteractable
    └── PickupInteractable
```

`PlayerInteraction`, его finder и runtime state создаются отдельно для каждого Player внутри его Zenject subcontainer. Scene-level interactable не должен зависеть от конкретного локального `PlayerInput`.

## 12.3. Проверка без готового танка

Фундамент проверяется в sandbox на простых Unity-примитивах:

- [x] Toggle — Press-переключатель для двери или рычага;
- [x] Timed — Hold-взаимодействие для люка;
- [x] Pickup — подбор предмета, присоединение к `HeldItemSlot`, блокировка других взаимодействий и выброс;
- [ ] Occupancy — отложен до появления реального `Tank` / `Station`.

Проверено вручную:

- Press на `ToggleInteractable` срабатывает;
- Hold на `TimedInteractable` завершается корректно;
- фактическое время Hold при настройке `2 секунды` составило `1,994 секунды`, что находится в пределах кадровой погрешности;
- `PickupInteractable` подбирается и присоединяется к точке удержания игрока.

`Occupancy` намеренно не проверяется на искусственном кресле. Механика занятости будет реализована вместе с реальными `DriverStation` и другими местами экипажа на этапах `Tank Core` / `Stations`.

На этом этапе не реализуются полноценные `TankRoot`, `DriverStation`, Inventory или предметная система. Будущие игровые объекты должны подключаться как новые реализации `IInteractable`, не меняя общий процесс взаимодействия.

## 12.4. Порядок реализации

1. [x] Убрать встроенный `Hold interaction` у action `Interact`, сохранив привязки клавиатуры и gamepad.
2. [x] Добавить сырые состояния Interact в `InputService`, `PlayerInputSystem` и `PlayerIntentBuffer`.
3. [x] Создать базовые контракты Interaction без Unity-зависимостей в их данных.
4. [x] Реализовать `PhysicsInteractionTargetFinder` и явное разрешение дочернего коллайдера в корневой interactable.
5. [x] Реализовать state machine `PlayerInteraction` для Press, Hold, Complete и Cancel.
6. [x] Подключить `PlayerInteraction` к существующему `GameplayPhase` после `PlayerLook`.
7. [x] Подключить базовый UI prompt через MVP и presentation-фазу.
   - [x] Созданы `InteractionHudPresenter`, `InteractionHudView` и `PresentationPhase`.
   - [x] В `Player.prefab` подготовлены `PromptText` и `HoldProgress` с `Background` и `Fill`.
   - [x] Завершены Zenject-binding и вызов `PresentationPhase` из `GameLoop`.
   - [ ] Провести финальный ручной UI-тест.
8. [ ] Завершить sandbox-проверки.
   - [x] Press.
   - [x] Hold.
   - [x] Pickup и присоединение к `HeldItemSlot`.
   - [ ] Полный цикл Drop: `Q`, восстановление физики и повторный подбор.
   - [ ] Потеря цели и выход из дистанции.
   - [ ] Препятствие между игроком и целью.
   - [ ] Отпускание кнопки до завершения Hold.
   - [ ] Недоступная или занятая цель.
   - [ ] Occupancy — отложен до этапов `Tank Core` / `Stations` и не блокирует завершение текущего среза.

На этапе 2 не реализовывать `Continuous`, полноценный `Cooperative`, систему станций, Inventory или игровые предметы. Для них сохраняются точки расширения, но конкретная логика добавляется на соответствующих следующих этапах.

## 12.5. Pickup и предмет в руках

Подбор предмета использует общий режим `Press` и не создаёт отдельную систему взаимодействия.

```text
PickupInteractable
        ↓
InteractionContext.PickupReceiver
        ↓
HeldItemSlot
        ↓
RightHandAnchor
```

- `IPickupReceiver` определяет занятость слота, приём и выброс предмета;
- `HeldItemSlot` принадлежит конкретному Player и передаётся через его `InteractionInstaller`;
- `PlayerFacade` продолжает хранить только игровые фазы и не предоставляет слот предмета;
- во время удержания `PickupInteractable` отключает `Rigidbody` и world-коллайдеры;
- занятый слот блокирует поиск и запуск других взаимодействий;
- action `Drop` проходит через `InputService`, `PlayerInputSystem` и `PlayerIntentBuffer`;
- выброс по `Q` освобождает слот, восстанавливает физику и добавляет импульс вперёд;
- `RightHandAnchor` не должен наследовать вертикальный pitch камеры, если предмет должен оставаться вертикальным.

Текущая обработка `Drop` остаётся в `PlayerInteraction`. Выделение отдельного `PlayerHeldItemController` отложено до момента, когда управление предметом в руках станет самостоятельной развивающейся механикой.

---

# 13. Роли экипажа

Не создавать разные классы персонажей:

```text
Driver
Gunner
Loader
Commander
```

Роль определяется текущим Station / доступными возможностями.

Схема:

```text
Player
  ↓
Station
  ↓
Capabilities
```

Пример:

```text
Player enters DriverStation
        ↓
Walking input disabled
Driving input enabled
        ↓
Tank driving available
```

Игрок может покинуть место и выполнять другую функцию.

---

# 14. Tank

Танк — один из центральных объектов проекта.

Не создавать монолитный:

```text
TankController
TankManager
TankSystem
```

который отвечает за всё.

Танк должен собираться композицией.

Пример:

```text
Tank
├── Movement
├── Fuel
├── Damage
├── Engine
├── Tracks
│   ├── LeftTrack
│   └── RightTrack
├── Turret
│   ├── Rotation
│   ├── Gun
│   ├── AmmoLoader
│   └── AmmoStorage
├── Stations
│   ├── DriverStation
│   ├── GunnerStation
│   └── CommanderStation
└── Modules
    ├── Radar
    ├── Radio
    ├── GPS
    ├── Lights
    └── NightVision
```

---

# 15. Tank Movement

Movement танка:

```text
контролируемое аркадное
```

Полная Rigidbody-физика корпуса не обязательна.

Цель:

- предсказуемое управление;
- понятное поведение;
- отсутствие зависимости core movement от сложной симуляции гусениц;
- возможность позже адаптировать под multiplayer.

---

# 16. Разрушаемые модули и физика

Бронелисты и отдельные модули могут отрываться после тяжёлых попаданий.

Базовая схема:

```text
Attached
    ↓
Heavy hit
    ↓
Detach
    ↓
Enable Rigidbody
    ↓
Apply impulse
    ↓
Unity Physics
```

Пока модуль закреплён на танке, он не обязан независимо симулироваться PhysX.

После отсоединения может использовать:

- Rigidbody;
- Collider;
- impulse;
- gravity;
- collision.

Это относится только к отсоединённому объекту и не означает, что сам танк должен управляться через физическую Rigidbody-модель.

---

# 17. Config vs Runtime State

Статическая конфигурация:

```text
ScriptableObject
```

Runtime-state:

```text
обычный runtime-объект
```

Пример:

```text
TankEngineConfig
├── MaxFuel
├── FuelConsumption
├── Durability
└── RepairDuration
```

и:

```text
TankEngine
├── CurrentFuel
├── CurrentDurability
└── IsBroken
```

ScriptableObject не должен хранить изменяемое состояние конкретной игровой сессии.

То же правило применять к:

- предметам;
- оружию;
- модулям;
- противникам;
- миссиям;
- loot-конфигам;
- балансным данным.

---

# 18. R3

R3 не является фундаментом gameplay-логики.

Предпочтительно использовать для:

- observable state;
- UI;
- presentation;
- реакции интерфейса на изменение состояния.

Gameplay преимущественно строить через:

```text
GameLoop
+
обычные method calls
+
явные зависимости
```

Не превращать проект в набор бесконтрольных reactive chains.

---

# 19. События

Не вводить глобальный EventBus по умолчанию.

Если один объект уже имеет явную зависимость от другого — использовать прямой вызов.

Пример:

```text
RepairController
        ↓
TankDamage
```

а не:

```text
GlobalEventBus.Publish(...)
```

Локальные события или R3 допустимы для presentation и наблюдения за состоянием.

---

# 20. Gameplay и Meta

Gameplay и Meta — разные домены.

## Gameplay

```text
Player
Tank
Enemies
Items
Combat
Mission
Extraction
```

## Meta

```text
Lobby
Currency
Shop
Equipment preparation
Progression
Perks
Unlocks
```

Не смешивать meta-логику внутрь gameplay feature.

---

# 21. Предлагаемая структура проекта

```text
Assets/_Project/
│
├── Develop/
│   │
│   ├── Editor/
│   │
│   └── Runtime/
│       │
│       ├── EntryPoint/
│       │   ├── GameEntryPoint.cs
│       │   ├── ProjectInstaller.cs
│       │   └── GameLoop/
│       │
│       ├── Core/
│       │   ├── GameLoop/
│       │   ├── StateMachine/
│       │   ├── Time/
│       │   └── Factories/
│       │
│       ├── Gameplay/
│       │   ├── Infrastructure/
│       │   │   ├── GameplayInstaller.cs
│       │   │   ├── GameplayBootstrap.cs
│       │   │   └── GameplayFlow.cs
│       │   │
│       │   └── Features/
│       │       ├── Player/
│       │       ├── Input/
│       │       ├── Interaction/
│       │       ├── Inventory/
│       │       ├── Items/
│       │       ├── Tank/
│       │       ├── Combat/
│       │       ├── AI/
│       │       ├── Missions/
│       │       └── Extraction/
│       │
│       ├── Meta/
│       │   ├── Infrastructure/
│       │   └── Features/
│       │       ├── Currency/
│       │       ├── Equipment/
│       │       ├── Shop/
│       │       ├── Progression/
│       │       └── Lobby/
│       │
│       ├── UI/
│       │
│       └── Infrastructure/
│           ├── Assets/
│           ├── Scenes/
│           ├── Save/
│           ├── Audio/
│           └── Steam/
│
├── AddressablesResources/
├── Resources/
├── Scenes/
├── Art/
├── Audio/
└── Configs/
```

Не создавать все папки заранее.

Папка появляется только тогда, когда появляется соответствующая feature.

---

# 22. Code Style

Использовать существующие правила проекта.

## 22.1. Namespace

Namespace повторяет путь от `Assets/`.

Префикс:

```text
_Project.
```

обязателен.

## 22.2. Именование

Использовать:

```text
camelCase
```

для локальных переменных и параметров.

Использовать:

```text
_camelCase
```

для private полей.

Использовать:

```text
PascalCase
```

для:

- public/protected полей;
- свойств;
- методов;
- классов;
- интерфейсов;
- структур;
- enum;
- namespace.

Использовать:

```text
UPPER_SNAKE_CASE
```

для `const`.

## 22.3. Bool

Предпочтительно:

```text
IsX
CanX
HasX
TryX
```

Не использовать размытые:

```text
CheckX
```

если метод фактически имеет конкретное значение.

## 22.4. var

Предпочитать явные типы вместо `var`.

## 22.5. Проверка false

Использовать:

```csharp
if (condition == false)
```

а не:

```csharp
if (!condition)
```

## 22.6. Magic numbers

Не использовать magic numbers.

Выносить значения в:

- const;
- config;
- serialized field;
- ScriptableObject.

## 22.7. Async

Использовать:

```text
UniTask
UniTask<T>
UniTaskVoid
```

Не использовать:

```text
Task
Task<T>
async void
```

кроме случаев, когда сигнатура event-handler жёстко требует `void`.

Все async-методы должны иметь суффикс:

```text
Async
```

`CancellationToken` — последний параметр.

---

# 23. Naming: запрещённые размытые типы

Не создавать без серьёзной причины:

```text
SomethingManager
SomethingHelper
SomethingUtils
GameManager
TankManager
PlayerManager
```

Имя класса должно отражать конкретную ответственность.

Пример:

```text
TankFuelController
TankDamage
MissionFlow
SceneLoader
Inventory
InteractionResolver
```

лучше, чем:

```text
TankManager
GameplayHelper
GameController
```

---

# 24. Основные архитектурные запреты

Codex не должен:

1. Возвращать Entitas / ECS.
2. Создавать второй параллельный GameLoop.
3. Добавлять `Update()` в обычные gameplay-классы без причины.
4. Использовать старый Input API.
5. Читать `Keyboard.current` напрямую из gameplay.
6. Делать tank movement зависимым от обязательного Rigidbody.
7. Делать Player roles отдельными наследниками Player.
8. Хранить runtime-state в ScriptableObject.
9. Делать глобальный EventBus центральным способом связи.
10. Использовать Zenject как Service Locator.
11. Создавать огромные `Manager`-классы.
12. Смешивать Meta и Gameplay.
13. Создавать новую feature вне её доменной папки без необходимости.
14. Самостоятельно выбирать networking stack — сетевой слой пока отложен.

---

# 25. Networking

Networking пока сознательно не проектируется.

Статус:

```text
TBD
```

Но архитектура должна оставаться network-friendly.

Стараться сохранять направление:

```text
Input
    ↓
Intent / Command
    ↓
Gameplay
    ↓
State
    ↓
Presentation
```

Не связывать игровую логику жёстко с конкретным локальным вводом.

Не добавлять сетевую библиотеку без отдельного решения.

---

# 26. Этапы реализации

---

## Этап 0 — Архитектурный фундамент

### Цель

Создать устойчивый каркас проекта.

### Сделать

- структуру `_Project`;
- ProjectContext;
- SceneContext;
- ProjectInstaller;
- GameplayInstaller;
- центральный GameLoop;
- базовые GameLoop interfaces;
- сценовый flow;
- input abstraction;
- ScriptableObject conventions;
- базовые services.

### Не делать

- полноценный combat;
- AI;
- networking;
- meta progression;
- сложную систему танковых модулей.

### Готово, если

- проект запускается;
- есть Bootstrap → MainMenu → Lobby → Gameplay;
- Zenject собирает зависимости;
- gameplay не размазан по случайным MonoBehaviour.Update.

---

## Этап 1 — Player + Input

**Статус: завершён.**

### Цель

Интегрировать уже готовый Player Movement в новую архитектуру.

### Текущий статус

Выполнено: локальный игрок создаётся через Zenject-фабрику, его фазы регистрируются и обновляются центральным `GameLoop`. Действия `PlayerInput` через `InputService` и `PlayerInputSystem` преобразуются в `PlayerIntentBuffer`, который используют движение и вращение камеры.

Зафиксировано: Player использует `Rigidbody`; отдельный `IPlayerInput` не требуется, пока gameplay изолирован от Unity Input через `PlayerIntentBuffer`.

### Сделать

- [x] Rigidbody movement;
- [x] InputActionAsset;
- [x] PlayerInput;
- [x] Action Maps;
- [x] переход Player input в gameplay через `PlayerIntentBuffer`.

### Готово, если

- [x] игрок ходит;
- [x] вращает камеру;
- [x] gameplay не читает клавиатуру напрямую.

---

## Этап 2 — Interaction

**Статус: основной фундамент реализован. Остались финальные ручные проверки UI, отмены Hold и полного цикла Drop. `Occupancy` отложен до появления реального танка и станций.**

### Цель

Создать единый фундамент взаимодействий.

### Сделать

- [x] поиск interactable;
- [x] Press;
- [x] Hold;
- [x] базовый interrupt interaction через `Cancel`;
- [x] базовый UI prompt подключён через View, Presenter и presentation-фазу;
- [x] sandbox-подбор предмета через `PickupInteractable` и `HeldItemSlot`;
- [x] выброс предмета через action `Drop`;
- [x] базовую подготовку к Cooperative interaction через `InteractionContext`, идентификатор игрока и причины отмены.

### Проверено

- [x] `ToggleInteractable` изменяет состояние по Press;
- [x] `TimedInteractable` завершается по Hold;
- [x] длительность Hold соответствует настройке с кадровой погрешностью;
- [x] `PickupInteractable` присоединяется к слоту игрока;
- [ ] UI отображает Press prompt;
- [ ] UI отображает и заполняет Hold progress;
- [ ] полный цикл Drop восстанавливает физику и позволяет повторный подбор;
- [ ] взаимодействие корректно отменяется во всех предусмотренных сценариях;
- [ ] `Occupancy` для кресла — отложен до этапов `Tank Core` / `Stations`.

### Готово, если

одна архитектура обслуживает:

- дверь;
- рычаг;
- предмет;
- люк.

Кресло временно исключено из критерия готовности этапа 2. Его `Occupancy` будет проверяться на реальной станции экипажа, а не на отдельном sandbox-прототипе.

---

## Этап 3 — Tank Core

### Цель

Получить первый управляемый танк.

### Сделать

- TankRoot;
- TankMovement;
- DriverStation;
- enter/exit;
- Driving Input Context.

### Готово, если

```text
Player approaches tank
→ enters DriverStation
→ drives tank
→ exits
```

---

## Этап 4 — Stations

### Цель

Разделить обязанности экипажа.

### Сделать

- DriverStation;
- GunnerStation;
- CommanderStation;
- Loader workflow;
- station occupancy;
- capability switching.

### Готово, если

один и тот же Player может менять роль через смену станции.

---

## Этап 5 — Tank Modules

### Цель

Получить модульную архитектуру танка.

### Сделать

- TankModule abstraction;
- Engine;
- Fuel;
- Lights;
- Radar;
- Radio;
- GPS;
- Night Vision;
- module configs.

### Готово, если

новый модуль можно добавить без переписывания TankRoot.

---

## Этап 6 — Tank Damage / Detach / Repair

### Цель

Сделать повреждения танка частью core gameplay.

### Сделать

- damage model;
- independent module state;
- left/right track state;
- heavy hit;
- detach;
- Rigidbody activation;
- repair flow;
- tool requirements.

### Готово, если

бронелист или модуль:

```text
получает тяжёлое попадание
→ отсоединяется
→ получает физику
→ может быть отремонтирован / заменён по правилам игры
```

---

## Этап 7 — Items + Inventory + Hands

### Цель

Создать общую предметную систему.

### Сделать

- ItemConfig;
- ItemInstance;
- inventory;
- hands;
- pickup;
- drop;
- tools;
- consumables;
- ammo;
- heavy items.

### Готово, если

одна архитектура поддерживает разные предметы без отдельного кода под каждый.

---

## Этап 8 — Cooperative Carrying

### Цель

Реализовать перенос тяжёлых объектов несколькими игроками.

### Сделать

- cooperative interaction;
- required participant count;
- attach points;
- coordinated movement;
- release / interrupt.

### Готово, если

тяжёлый объект требует нужное число игроков и корректно переносится.

---

## Этап 9 — Tank Weapon

### Цель

Реализовать полный цикл танкового орудия.

### Сделать

- turret rotation;
- gun elevation;
- firing;
- ammo storage;
- ammo loading;
- reload interaction;
- shell types foundation.

### Готово, если

игрок может:

```text
взять снаряд
→ загрузить
→ навестись
→ выстрелить
```

---

## Этап 10 — Player Combat

### Цель

Реализовать базовую личную боевую систему.

### Сделать

- weapon usage;
- ammo;
- damage;
- health;
- death foundation.

---

## Этап 11 — Mission Framework

### Цель

Создать универсальную систему миссий.

### Поддержать базовые objective-типы

```text
DestroyTarget
Escort
RetrieveItem
RetrieveHeavyCargo
DestroyStructure
Evacuate
```

Не делать один огромный MissionController.

---

## Этап 12 — Mission Timing + Extraction

### Цель

Собрать первый полный gameplay-loop миссии.

### Базовый flow

```text
Landing
→ Scavenge
→ Threat phase
→ Objective
→ Extraction
→ Result
```

Тайминги должны быть конфигурируемыми через ScriptableObject.

---

## Этап 13 — AI Infrastructure

### Цель

Создать общий фундамент врагов.

### Сделать

- perception;
- target selection;
- movement;
- attack;
- state transitions;
- common AI abstractions.

После этого добавлять конкретные типы врагов.

---

## Этап 14 — POI + Loot

### Цель

Наполнить карту игровым содержимым.

### Сделать

- POI;
- loot spawn;
- containers;
- destroyed vehicles;
- bunkers;
- trenches;
- camps;
- map-specific points.

---

## Этап 15 — Lobby / Meta

### Цель

Собрать pre-mission loop.

### Сделать

- terminal;
- map selection;
- equipment preparation;
- shop;
- currency;
- value hand-in.

Остальные meta-механики добавлять позже.

---

## Этап 16 — Save / Progression

### Цель

Сохранять только уже устоявшиеся данные.

### Возможные данные

- currency;
- equipment;
- unlocks;
- tank setup;
- progression;
- statistics.

Точная структура — `TBD`.

---

## Этап 17 — UI / UX / Audio / Feedback

### Цель

Сделать системы понятными игроку.

### Добавить

- interaction prompts;
- fuel indicators;
- damage feedback;
- module state;
- mission UI;
- extraction feedback;
- audio feedback;
- station feedback.

---

## Этап 18 — Optimization / Tests / Tooling

### Цель

Подготовить проект к масштабированию.

### Сделать

- profiling;
- GC checks;
- GameLoop profiling;
- physics profiling;
- AI profiling;
- validation tools;
- editor tools;
- tests для критичных систем.

---

## Этап 19 — Networking

Статус:

```text
отложено
```

Сетевой стек будет выбран отдельно.

До этого момента не связывать core gameplay с конкретной network-библиотекой.

---

# 27. Milestones

## Milestone 1 — Foundation

```text
Этапы 0–2
```

Результат:

```text
Architecture
+
Player
+
Input
+
Interaction
```

---

## Milestone 2 — Playable Tank

```text
Этапы 3–7
```

Результат:

```text
Tank
+
Stations
+
Modules
+
Damage
+
Items
```

---

## Milestone 3 — Core Cooperative Gameplay

```text
Этапы 8–10
```

Результат:

```text
Cooperative carrying
+
Tank weapon
+
Player combat
```

---

## Milestone 4 — First Complete Mission

```text
Этапы 11–14
```

Результат:

```text
Mission
+
Extraction
+
AI
+
POI
```

После этого должен существовать первый полноценный vertical slice:

```text
Lobby
→ Landing
→ Loot / Tank preparation
→ Enemy phase
→ Objective
→ Extraction
→ Result
```

---

## Milestone 5 — Meta Loop

```text
Этапы 15–17
```

Результат:

```text
Lobby
+
Progression
+
Save
+
Full UI
```

---

## Milestone 6 — Production

```text
Этапы 18–19
```

Результат:

```text
Optimization
+
Tests
+
Networking
```

---

# 28. Правило работы Codex

Перед реализацией новой большой feature Codex должен:

1. Найти существующие связанные классы.
2. Проверить, к какому модулю относится feature.
3. Не создавать дублирующую систему.
4. Соблюсти текущие зависимости.
5. Не внедрять новую архитектуру без необходимости.
6. Не менять фундаментальные решения без явной задачи.
7. Сначала использовать существующий GameLoop.
8. Сначала использовать существующий Input abstraction.
9. Использовать Zenject для сборки зависимостей.
10. Хранить static config отдельно от runtime state.

После реализации крупной feature желательно кратко перечислить:

- какие файлы добавлены;
- какие файлы изменены;
- какие зависимости добавлены;
- что необходимо настроить в Unity Inspector;
- какие ручные проверки нужно выполнить.

---

# 29. Приоритет при конфликте правил

Если код в старом проекте противоречит этому файлу:

```text
CODEX.md имеет приоритет
```

кроме случаев, когда пользователь отдельно просит сохранить конкретную старую реализацию.

Если решение не определено:

```text
не придумывать
```

Пометить как `TBD` или запросить решение.

---

# 30. Текущие TBD

Пока не зафиксированы окончательно:

- networking stack;
- host / dedicated server architecture;
- save format;
- Steam Cloud;
- AI architecture details;
- ballistic model;
- final inventory model;
- final module damage states;
- final UI architecture;
- final audio middleware;
- final map streaming model;
- final scene organization for multiple maps;
- multiplayer authority rules.

Эти решения не должны появляться самовольно как новые архитектурные зависимости.
