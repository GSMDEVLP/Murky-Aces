# Murky Aces — Stage 04.2: Gunner Station and Tank Weapon

> Подробный план реализации рабочего места стрелка-наводчика и первого
> вертикального среза танкового орудия.
>
> Общие архитектурные правила определяет `CODEX_Murky_Aces.md`.
> Текущий reusable foundation станций описан в
> `STAGE_04_DRIVER_COCKPIT.md`.
>
> Документ является планом. Он не означает, что перечисленные системы уже
> реализованы.

---

# 1. Статус

```text
In Progress

Current checkpoint:
Stage E — GunnerCamera follows the turret; station display and GunnerStation
runtime are not connected yet

Checkpoint date:
2026-10-01

Next session:
1. Give each StationDisplayFeed its own runtime RenderTexture and bind it
   to the station screen; check Driver regression
2. Complete GunnerStation prefab references and register its controller
3. Check Gunner enter → aim → display → exit in Play Mode
```

Manual acceptance Stage 04.1 завершён. Автоматические EditMode/PlayMode tests
для этого acceptance flow в документе пока не зафиксированы.
Пользователь ранее подтвердил прохождение тестов управления; после последних
изменений GunnerCamera отдельная Play Mode проверка здесь не зафиксирована.

Текущий прогресс Stage 04.2:

- Завершено в коде: `Gunner` Action Map, typed readers и snapshots,
  `GunnerIntentBuffer`, `PlayerGunnerInputMode`, переключение input context;
- завершено в коде: `TurretAimState`, `TurretMechanism`, reference modes,
  `TurretAimConfig`, `UnityTurretRig` и регистрация gameplay/presentation
  поворота башни в `TankRoot`;
- завершено в коде и prefab: отдельная `GunnerCamera` под `Sensors`, её
  позиция берётся из `Crew_Gunner_Eye`, направление — из `MuzzlePose`;
- подготовлено в prefab: `GunnerStation`, seat/exit/interaction anchors и
  `GunnerPanelRoot`; `GunnerStationAdapter` существует, но не подключён;
- текущая точка остановки: `StationDisplayFeed` ещё требует назначенный
  asset `RenderTexture` и не создаёт отдельную runtime texture; у
  `GunnerStation` не назначен `CameraAnchor`, а `TankInstaller` создаёт
  только Driver controller. Поэтому занять GunnerStation и увидеть её
  камеру на `Display_Gunner` пока нельзя;
- `FireRequested` и `ZoomHeld` доходят до Gunner intent, но выстрел, зарядка
  и двукратный zoom ещё не подключены.

---

# 2. Цель

Создать рабочее место стрелка-наводчика, в котором Player:

- входит внутрь башни через верхний люк;
- свободно перемещается внутри башни и взаимодействует с её предметами;
- может выбрать одно из доступных мест экипажа — Gunner или CommanderLoader;
- занимает GunnerStation через существующую Interaction feature;
- остаётся физически закреплённым в кресле;
- управляет горизонтальным поворотом башни;
- управляет углом возвышения орудия;
- переключает систему отсчёта направления башни;
- наблюдает внешний мир через экран камеры прицела;
- включает двукратное увеличение камеры;
- производит выстрел только из заряженного и готового орудия;
- при отсутствии командира-заряжающего может подобрать снаряд и зарядить
  орудие, не покидая своего места;
- взаимодействует с пультом, казёнником и дополнительными модулями через
  общую Interaction feature;
- корректно покидает GunnerStation, оставаясь внутри башни;
- выходит из Tank наружу только через отдельное взаимодействие с верхним люком.

Целевой полный поток:

```text
Player снаружи подходит к верхнему люку Tank
        ↓
удерживает Interact и входит внутрь башни
        ↓
Player переходит в свободный walking mode внутри Tank
        ↓
взаимодействует с GunnerStation или CommanderLoaderStation
        ↓
удерживает Interact и занимает место
        ↓
Player прикрепляется к GunnerSeatAnchor
        ↓
включаются Gunner input context и cockpit look
        ↓
GunnerCamera выводится на Display_Gunner
        ↓
Player наводит башню и орудие
        ↓
при свободной роли CommanderLoader подбирает ShellItem
        ↓
наводится на BreechLoadPoint и удерживает Interact
        ↓
GunChamber переходит в Loaded
        ↓
Player нажимает Fire
        ↓
снаряд покидает Muzzle, GunChamber становится Empty
        ↓
Player выходит из станции
        ↓
восстанавливаются walking input, look, locomotion и interaction внутри башни
        ↓
Player взаимодействует с верхним люком изнутри
        ↓
Player выходит из Tank наружу
```

---

# 3. Пользовательское управление

Целевые клавиши для Keyboard & Mouse:

| Ввод | Действие |
|---|---|
| `W` | Поднять ствол |
| `S` | Опустить ствол |
| `A` | Повернуть башню влево |
| `D` | Повернуть башню вправо |
| `Space` | Переключить режим отсчёта направления башни |
| `ЛКМ` | Запросить выстрел |
| `ПКМ` | Включить двукратное увеличение камеры прицела |
| `E` | Press/Hold-взаимодействие с объектом под прицелом, включая верхний люк |
| `F` | Встать с GunnerStation и остаться внутри башни |
| `TBD` | Выбросить предмет из рук в Gunner context |

Mouse Look продолжает отвечать за осмотр интерьера Player, а не за движение
башни. Наведение башни поступает только через отдельное действие `Aim`.

Gamepad bindings должны быть добавлены в том же Action Map, но точная раскладка
остаётся `TBD` до отдельного решения по control scheme.

---

# 4. Текущее состояние проекта

## 4.1. Уже существующий foundation

В проекте уже существуют:

- `GameLoopRegistry` и разделение Input / Gameplay / FixedGameplay /
  Presentation;
- `PlayerInputSystem` и intent buffers;
- `PlayerInteraction`, Press и Hold interaction sessions;
- `CrewStationOccupancy`;
- `CrewStationController`;
- `ICrewStationRoleAdapter`;
- `StationPanelRoot` и `IInteractionScope`;
- `StationDisplayFeed`;
- attach/detach Player к Station anchors;
- cockpit look с ограничениями yaw/pitch;
- водительский `DriverStationAdapter`;
- `TankInstaller` и `TankRoot` как composition root танка.

## 4.2. Существующая геометрия Tank

В `TankHull.prefab` уже присутствуют подходящие узлы:

```text
Turret_Yaw
├── Gun_Pitch
│   ├── Muzzle
│   └── Breech_Load_Point
├── Station_Gunner
├── Crew_Gunner_Seat
├── Crew_Gunner_Eye
├── Display_Gunner
└── Console_Gunner
```

Также в модели присутствуют:

- `Hatch_Exit_Gunner`;
- `Gunner_Handwheel`;
- элементы gunner console;
- визуальные снаряды и ammo rack;
- казённая часть и recoil rails.

Исходный `TankHull.prefab` используется как геометрический donor. Runtime-код
не должен искать эти узлы по имени. Все необходимые ссылки назначаются явно
через Inspector в wrapper-prefab `TankRoot_Prototype.prefab`.

## 4.3. Архитектурные ограничения текущего состояния

Общий `CrewStationView`, выбор input mode и Gunner input pipeline уже
реализованы. Открытые зависимости перед включением GunnerStation:

- `TankInstaller` создаёт только Driver controller и occupancy;
- `GunnerStation` ещё не имеет `CameraAnchor` и собственного
  `StationDisplayFeed`;
- `StationDisplayFeed` не создаёт отдельный runtime RenderTexture на
  экземпляр станции и Tank;
- работа с предметами в руках и переход через верхний люк относятся к
  следующим срезам и ещё не закрыты.

---

# 5. Зафиксированные архитектурные правила

## 5.1. Feature-oriented структура

Новые классы группируются по механике, а не по техническому типу:

```text
Gameplay/Features/Tank/
├── Stations/
├── Turret/
├── Weapon/
├── Ammunition/
└── Modules/
```

Не создавать общие папки `Controllers`, `Managers`, `Models` или `Views` для
несвязанных механик.

## 5.2. Направление зависимостей

```text
Unity / Infrastructure / Presentation
                ↓
Application
                ↓
Domain
```

Domain-состояние башни, орудия и каморы не зависит от:

- `MonoBehaviour`;
- `Transform`;
- `Rigidbody`;
- `Camera`;
- `AudioSource`;
- Input System;
- Zenject;
- RenderTexture.

## 5.3. Центральный GameLoop

Обычные gameplay-классы не получают собственные `Update`, `FixedUpdate` и
`LateUpdate`.

Допустимая схема:

```text
Input phase
    → захват GunnerIntent

Gameplay phase
    → station lifecycle
    → turret target/current state
    → fire requests
    → loading state

FixedGameplay phase
    → физический projectile, если выбран Rigidbody-вариант

Presentation phase
    → применение позы к Transform
    → экран, звук, эффекты, анимация пульта
```

Unity lifecycle разрешён техническим View/adapter-компонентам только там, где
он действительно требуется для Unity API и cleanup.

## 5.4. Composition over inheritance

Не создавать:

```text
BaseStation
├── DriverStation
├── GunnerStation
└── CommanderStation
```

Использовать:

```text
CrewStationController
├── CrewStationOccupancy
├── CrewStationView
├── StationCapabilityProfile
├── StationDisplayFeed[]
└── ICrewStationRoleAdapter
```

Role adapter содержит только команды конкретной роли. Occupancy, enter/exit,
look, interaction scope, rollback и display lifecycle остаются общими.

## 5.5. Отсутствие прямого input polling

Запрещено читать `Keyboard.current`, `Mouse.current` или `Gamepad.current` из:

- `GunnerStationAdapter`;
- `TurretMechanism`;
- `TankGun`;
- `BreechLoadInteractable`;
- camera/module logic.

Все действия поступают через Input Action Map и intent buffers.

## 5.6. Подготовка к multiplayer без выбора networking stack

Networking stack и authority rules остаются `TBD`.

При этом domain-команды и reservations используют `actorId`, чтобы позднее их
можно было валидировать на authoritative side. Не добавлять NetworkBehaviour,
RPC или зависимости от конкретного networking package в этом этапе.

---

# 6. Решения, требующие подтверждения

До реализации соответствующего шага необходимо подтвердить:

## 6.1. Поведение Space

Рекомендуемая трактовка:

```text
HullRelative
    текущий yaw хранится относительно корпуса;
    поворот корпуса переносит направление башни вместе с Tank.

WorldStabilized
    сохраняется мировое направление башни;
    поворот корпуса компенсируется приводом башни.
```

В обоих режимах A/D продолжает изменять целевое направление.

Альтернатива — Space полностью блокирует привод. Она хуже соответствует фразе
«танк может вращаться как хочет, но башня остаётся неподвижной относительно
своего направления», поэтому не рекомендуется.

## 6.2. Поведение ПКМ

Рекомендуется `Hold`:

- ПКМ удерживается — 2x zoom включён;
- ПКМ отпущена — базовый FOV восстановлен.

Вариант `Toggle` остаётся возможным, но должен быть выбран явно.

## 6.3. Роль командира-заряжающего

Рабочее предположение плана:

```text
CommanderLoader — одна совмещённая роль и одна Station occupancy.
```

Если Commander и Loader будут отдельными ролями, permission policy зарядки
должна проверять обе occupancy.

## 6.4. Баллистическая модель

В `CODEX_Murky_Aces.md` ballistic model помечена `TBD`.

До решения разрешено определить только контракт `IGunLauncher`. Конкретная
реализация выбирается отдельно:

- физический Rigidbody projectile;
- custom ballistic simulation;
- hitscan;
- гибридная модель.

## 6.5. Боевые значения

Остаются `TBD`:

- turret traverse speed;
- gun elevation speed;
- pitch limits;
- базовый и zoom FOV;
- fire cooldown;
- reload duration;
- shell mass и muzzle velocity;
- recoil amplitude;
- damage и penetration.

Код должен получать эти значения из ScriptableObject-конфигов, а не содержать
magic numbers.

## 6.6. Вход в Tank и выход со Station

Решение подтверждено:

```text
OutsideTank
    → Interact с верхним люком
    → InsideTank, свободное перемещение внутри башни
    → Interact с GunnerStation или CommanderLoaderStation
    → OccupiedStation
    → ExitStation
    → InsideTank
    → Interact с верхним люком
    → OutsideTank
```

`ExitStation` отвечает только за освобождение занятого места. Он не выводит
Player наружу и не использует внешний hatch anchor.

Вход и выход из Tank через верхний люк являются отдельной interaction-механикой
и не входят в ответственность `CrewStationController`. Для них используются
отдельные validated anchors внутри и снаружи Tank.

Состояние «Player находится внутри Tank» независимо от station occupancy.
Player внутри башни может не занимать ни одного места и продолжать
взаимодействовать с сидениями, снарядами, казёнником и другими предметами.

Свободно перемещающийся внутри Player должен оставаться в системе координат
движущегося Tank. Конкретная реализация moving-frame поведения выбирается при
реализации Tank interior и не должна смешиваться с seat attach/detach.

---

# 7. Целевая схема систем

Вход, свободное перемещение внутри Tank и занятие места разделены:

```text
UpperHatch interaction
        ↓
Tank interior enter/exit lifecycle
        ↓
InsideTank walking + world interaction
        ↓
CrewStationInteractable
        ↓
CrewStationController
```

`CrewStationController` не управляет люком и не определяет, находится ли
Player внутри Tank. Верхний люк не освобождает занятую Station неявно: сначала
Player должен встать с места.

```text
InputSystem_Actions / Gunner map
                ↓
GunnerActionsReader
                ↓
PlayerInputSystem
                ↓
GunnerIntentBuffer
                ↓
PlayerInteractionActor : IGunnerIntentSource
                ↓
GunnerStationAdapter
        ┌───────┴────────┐
        ↓                ↓
TurretMechanism       TankGun
        ↓                ↓
ITurretRig          GunChamber
        ↓                ↓
UnityTurretRig      IGunLauncher
        ↓                ↓
Turret_Yaw /        Muzzle / projectile
Gun_Pitch
```

Ручная зарядка работает отдельно от WASD-команд:

```text
PlayerInteraction
        ↓
ShellPickupInteractable
        ↓
HeldItemSlot
        ↓
BreechLoadInteractable (Hold)
        ↓
GunChamber.Load(shell)
```

---

# 8. Этап A — завершение Stage 04.1

**Статус: manual acceptance завершён.** Проверены три последовательных цикла,
управление, cockpit look, экран, выход/восстановление и чистая Console.
Автоматические EditMode/PlayMode tests в рамках этой проверки не зафиксированы.

## 8.1. Задачи

- завершить `Interact` в Driving Action Map;
- развести `Interact` и `ExitStation`;
- подавлять вход до отпускания клавиши после смены map;
- добавить реальные cockpit interaction targets;
- завершить runtime RenderTexture ownership;
- связать display lifecycle с station occupancy;
- исправить failure recovery при disable/destroy;
- добавить EditMode/PlayMode tests;
- пройти manual acceptance DriverStation.

## 8.2. Критерий перехода

```text
DriverStation проходит три последовательных цикла
Enter → Drive → Interact → Exit

и disable/destroy Tank не оставляет Player
без walking input, Main Camera или interaction.
```

До этого критерия новые Gunner components не добавляются в рабочий prefab.

---

# 9. Этап B — обобщение Station foundation

**Статус: выполняется.** Общий lifecycle и registry уже внедрены для Driver,
но второй независимо созданный station runtime ещё не добавлен.

## 9.1. CrewStationView

**Статус: частично выполнено.** Driver-specific тип и имена устранены,
добавлены `RoleId` и `CapabilityProfile`. Массив `DisplayFeeds[]` пока не
реализован: текущий controller использует один `StationDisplayFeed`.

`DriverStationView` преобразовать в общий `CrewStationView`.

Предполагаемые данные:

```text
SeatAnchor
ExitAnchor
ViewAnchor
InteractionPoint
InteractionScope
DisplayFeeds[]
CapabilityProfile
RoleId
```

Поля и методы не содержат слова `Driver`.

## 9.2. CrewStationInteractable

**Статус: выполнено.** Общий interactable находит соответствующий controller
через `CrewStationRegistry` и `CrewRoleId`; binding поддерживает несколько
компонентов в иерархии Tank.

`DriverStationInteractable` заменить общим компонентом:

```text
CrewStationInteractable
    → GetInteractionInfo
    → Begin enter
    → Complete enter
    → Cancel enter
```

Разница между Driver/Gunner не должна находиться в interactable.

## 9.3. Station capability profile

**Статус: выполнено для текущего Driver flow.** `PlayerStationController`
выбирает input mode по `StationControlContext`, а параметры look, locomotion и
interaction получает из `StationCapabilityProfile`.

Профиль станции задаёт:

- Player control context;
- look mode и пределы;
- разрешённый interaction scope;
- необходимость locomotion block;
- список display feeds;
- exit action.

`PlayerStationController` получает описание режима станции, а не всегда
активирует `PlayerDrivingInputMode`.

## 9.4. Несколько станций одного Tank

**Статус: частично выполнено.** Глобальные singleton bindings station view,
occupancy, adapter, controller и display feed удалены. `TankInstaller` создаёт
station runtime явно через общий `CreateStation`, однако в registry пока
передаётся только Driver controller.

Текущие singleton bindings одного `CrewStationController` и одной occupancy
должны быть заменены station-scoped созданием.

Допустимые варианты:

- factory, создающая controller по сериализованной station definition;
- Zenject identifiers для конкретных station instances;
- локальный station subcontainer.

Предпочтительный вариант выбирается при реализации после проверки текущего
GameObjectContext. Runtime gameplay не должен использовать
`Container.Resolve<T>()`.

## 9.5. CrewStationRegistry

**Статус: выполнено.** Registry проверяет пустой список, `null` и повторяющиеся
роли, предоставляет lookup по `CrewRoleId`, состояние occupancy и список
controller'ов для lifecycle `TankRoot`.

Добавить registry конкретного Tank:

```text
CrewRoleId → CrewStationController / CrewStationOccupancy
```

Registry нужен для:

- определения занятости ролей;
- проверки fallback-возможностей;
- lifecycle cleanup всех станций;
- последующей интеграции multiplayer authority.

## 9.6. Критерий завершения

**Статус: не завершено.** Driver regression пройден, но критерий двух
независимых station runtime будет закрыт после добавления GunnerStation.

- DriverStation работает на новом общем foundation;
- Tank содержит минимум два независимо создаваемых station runtime;
- GunnerStation может быть добавлена без копирования enter/exit;
- общая часть не проверяет `if driver` или `if gunner`;
- Zenject bindings однозначны при нескольких станциях.

---

# 10. Этап C — Gunner input pipeline

## 10.1. Новый Action Map

**Статус: выполнено на уровне input asset.** Action Map `Gunner` и
перечисленные actions добавлены в `.inputactions`. `Aim` оформлен как один
`2D Vector Composite` с частями W / S / A / D.

Generated wrapper `InputSystem_Actions.cs` runtime-кодом не используется.
`Generate C# Class` остаётся выключенным: readers получают action asset через
`PlayerInput.actions` и явно кешируют только actions своей карты.

Добавить Action Map `Gunner`:

```text
Aim                 Vector2
Fire                Button
Zoom                Button
ToggleReferenceMode Button
Look                Vector2
Interact            Button
ExitStation         Button
Drop                Button
```

Keyboard bindings:

```text
Aim.y positive      W
Aim.y negative      S
Aim.x negative      A
Aim.x positive      D
Fire                Mouse Left
Zoom                Mouse Right
ToggleReferenceMode Space
Interact            E
ExitStation         F
Drop                TBD
Look                 Pointer Delta
```

## 10.2. Intent data

**Статус: реализовано в коде; сквозная проверка после подключения
GunnerStation ещё не выполнена.**

Завершено:

- `InputMapId` с Player / Driving / Gunner;
- `CommonInputSnapshot`;
- `PlayerActionsSnapshot` и `DrivingActionsSnapshot`;
- `PlayerActionsReader` и `DrivingActionsReader`;
- `ICommonActionsReader`;
- `ActiveCommonActionsReader` с проверкой пустого списка, `null` и
  повторяющихся map id;
- перевод `PlayerInputSystem` и `InputInstaller` на readers;
- удаление больше не используемого `InputService`.

Реализованная цепочка:

```text
GunnerActionsSnapshot
        ↓
GunnerActionsReader : ICommonActionsReader
        ↓
регистрация reader в InputInstaller
        ↓
GunnerIntentSnapshot / GunnerIntentBuffer
        ↓
IGunnerIntentSource / PlayerGunnerInputMode
```

После подключения GunnerStation проверить в Play Mode отсутствие regression:
walking, Player Look, Interact, Drop, вход в DriverStation, Driving Look,
Throttle / Steering / Brake, выход по F и восстановление Player map.

Поля реализованного `GunnerIntentSnapshot`:

```text
GunnerIntentSnapshot
├── Traverse
├── Elevation
├── FireRequested
├── ZoomHeld
└── ToggleReferenceModeRequested
```

Связанные реализованные типы:

```text
GunnerIntentBuffer
IGunnerIntentSource
PlayerGunnerInputMode
```

Однократные запросы `Fire` и `ToggleReferenceMode` должны потребляться ровно
один раз. Непрерывные оси и `ZoomHeld` читаются как состояние текущего кадра.

## 10.3. Безопасный переход между maps

При входе в GunnerStation:

- очистить накопленные intents;
- переключить Action Map;
- подавить Interact/Exit/Fire до отпускания соответствующих кнопок;
- не допустить выстрела от ЛКМ, которой Player мог закрыть UI или другое
  состояние;
- сохранить cockpit look и Interaction.

При выходе:

- очистить Gunner intents;
- вернуть Player Action Map;
- восстановить walking capabilities;
- не передать held input в Player actions.

## 10.4. Критерий завершения

**Статус: ожидает Play Mode проверку после подключения GunnerStation.**

- WASD в Gunner context не двигает Player и Tank;
- Mouse Look осматривает интерьер;
- gunner intents доходят до role adapter;
- нажатие входа не вызывает немедленное действие станции;
- выход возвращает Player context без залипших кнопок.

---

# 11. Этап D — Turret и Gun Elevation

**Статус: основные domain и Unity-компоненты собраны и зарегистрированы.**
Наведение через занятую GunnerStation и критерии раздела 11.6 ещё не
проверены, поскольку её controller пока не создан в `TankInstaller`.

## 11.1. Domain state

Добавить `TurretAimState`:

```text
CurrentYaw
TargetYaw
CurrentPitch
TargetPitch
ReferenceMode
WorldDirection
```

Состояние не хранит `Transform`.

## 11.2. Конфигурация

`TurretAimConfig : ScriptableObject`:

- traverse degrees per second;
- elevation degrees per second;
- minimum pitch;
- maximum pitch;
- yaw axis/sign;
- pitch axis/sign;
- default reference mode;
- angular epsilon;
- optional acceleration/deceleration привода.

Оси должны конфигурироваться, потому что donor-модель ориентирована не по
обязательному Unity `forward = +Z`.

## 11.3. TurretMechanism

Ответственность:

- принять нормализованный traverse/elevation input;
- изменить target angles;
- ограничить pitch;
- обновить current angles с заданной скоростью;
- переключить reference mode;
- в WorldStabilized компенсировать изменение hull yaw;
- очистить input при потере оператора;
- не знать о клавиатуре, Player и Station.

## 11.4. UnityTurretRig

Infrastructure adapter получает ссылки:

```text
Tank/Hull transform
Turret_Yaw transform
Gun_Pitch transform
Muzzle transform
Breech_Load_Point transform
```

Adapter:

- читает ориентацию корпуса;
- применяет вычисленные yaw/pitch;
- предоставляет muzzle pose;
- не содержит gameplay-правил;
- не ищет объекты через `Transform.Find` по имени.

## 11.5. Режимы Space

### HullRelative

- target yaw хранится в локальном пространстве корпуса;
- без A/D локальный угол не меняется;
- вместе с корпусом меняется мировое направление башни.

### WorldStabilized

- при переключении запоминается мировое направление;
- поворот корпуса пересчитывается в локальный yaw;
- A/D изменяет мировое target direction;
- pitch остаётся ограниченным относительно механики орудия;
- при превышении скорости привода допускается временная ошибка стабилизации.

## 11.6. Критерий завершения

- W/S плавно изменяет pitch в допустимых пределах;
- A/D плавно изменяет yaw;
- отпускание осей прекращает движение;
- выход из станции очищает input;
- HullRelative проверен на вращающемся Tank;
- WorldStabilized сохраняет направление при вращении корпуса;
- повторные переключения Space не создают скачка угла.

---

# 12. Этап E — GunnerStation и камера прицела

**Статус: камера следует за направлением орудия; станция и вывод на экран
не завершены.** `GunnerCameraPresenter` зарегистрирован после применения
углов башни. Остались runtime RenderTexture, feed на `Display_Gunner`,
`CameraAnchor`, регистрация Gunner controller и проверка enter/exit.

## 12.1. Anchors

Создать wrapper-объекты или назначить ссылки:

```text
GunnerStation
├── InteractionPoint
├── GunnerSeatAnchor
├── GunnerExitAnchor (внутри башни, рядом с сидением)
├── GunnerViewAnchor
├── GunnerPanelRoot
└── GunnerDisplayFeed
```

Donor references:

```text
GunnerSeatAnchor ← Crew_Gunner_Seat
GunnerViewAnchor ← Crew_Gunner_Eye
GunnerExitAnchor ← отдельный validated anchor внутри башни
GunnerPanelRoot  ← wrapper над Console_Gunner / Display_Gunner
```

`GunnerExitAnchor` используется только для вставания с места и никогда не
выводит Player наружу. `Hatch_Exit_Gunner` относится к отдельному lifecycle
верхнего люка. Для входа и выхода из Tank нужны отдельные безопасные точки:

```text
TankInteriorEntryAnchor
TankExteriorExitAnchor
```

Обе точки проверяются с учётом capsule Player. Внешний anchor не передаётся в
`CrewStationController`.

## 12.2. Main Camera

Main Camera остаётся связанной с `Player.CameraPivot`, как в DriverStation.

Вход в GunnerStation:

- прикрепляет тело Player к seat anchor;
- включает station cockpit look;
- не переподключает Main Camera к технической камере;
- ограничивает interaction scope рабочей зоной наводчика.

## 12.3. GunnerCamera

Техническая камера:

- следует за направлением прицела/орудия;
- выводит изображение в отдельный runtime RenderTexture;
- не имеет `MainCamera` tag;
- не имеет `AudioListener`;
- исключает interior, Player и собственный экран из culling mask;
- включается только при занятой GunnerStation;
- освобождает runtime resources при exit/disable/destroy.

## 12.4. Двукратный zoom

Zoom изменяет FOV технической GunnerCamera.

Для геометрически корректного увеличения:

```text
zoomFov = 2 * atan(tan(baseFov / 2) / zoomFactor)
zoomFactor = 2
```

Не использовать безусловное `baseFov / 2`, если требуется точное 2x оптическое
увеличение.

Переход FOV может быть мгновенным в первом vertical slice. Плавная анимация
относится к polish.

## 12.5. Критерий завершения

- Player занимает и покидает GunnerStation;
- после выхода со Station Player находится внутри башни рядом с сидением;
- выйти наружу можно только через взаимодействие с верхним люком;
- экран показывает направление прицела;
- изображение не содержит recursive feedback;
- ПКМ включает ровно 2x zoom;
- Main Camera и AudioListener остаются единственными основными компонентами;
- exit/disable выключает feed и освобождает instance resources.

---

# 13. Этап F — состояние орудия и первый выстрел

## 13.1. GunChamber

Минимальные состояния:

```text
Empty
Loading
Loaded
Firing
Cooldown
```

Допустимо упростить первый slice до `Empty / Loaded / Cooldown`, если Loading
полностью контролируется interaction session. Финальная схема фиксируется при
реализации без нарушения внешнего контракта.

`GunChamber` хранит ссылку/идентификатор `ShellDefinition`, а не GameObject
снаряда в руках.

## 13.2. TankGun

Ответственность:

- проверить наличие shell;
- проверить cooldown;
- принять однократный fire request;
- сформировать `GunShotRequest`;
- передать запрос в `IGunLauncher`;
- очистить chamber после подтверждённого запуска;
- начать cooldown;
- предоставить состояние для presentation;
- отклонить dry fire без изменения chamber.

## 13.3. IGunLauncher

Контракт получает:

```text
Muzzle pose
ShellDefinition
Inherited Tank velocity
Actor / Tank identity
```

Реализация баллистики остаётся заменяемой.

## 13.4. Первый vertical slice

До ручной зарядки разрешён тестовый preloaded shell:

```text
GunnerStation occupied
GunChamber preloaded
        ↓
Player наводится
        ↓
Fire request
        ↓
один launch event из Muzzle
        ↓
GunChamber Empty
        ↓
повторный Fire отклонён
```

Preload существует только для изоляции проверки наведения и firing pipeline.
Он удаляется или выключается до финального acceptance.

## 13.5. Presentation

Первый slice должен предусмотреть события для:

- muzzle flash;
- shot audio;
- recoil animation;
- camera impulse;
- smoke;
- dry fire feedback.

Конкретные assets могут быть временными. Gameplay-состояние не должно зависеть
от успешного воспроизведения эффекта.

## 13.6. Критерий завершения

- ЛКМ создаёт один запрос на одно нажатие;
- заряженное орудие производит один выстрел;
- пустое орудие не создаёт projectile;
- projectile/launch pose совпадает с `Muzzle`;
- выстрел очищает chamber;
- выход из Station не оставляет fire request;
- firing не зависит от частоты кадров.

---

# 14. Этап G — предметы, снаряды и руки

## 14.1. Общий holdable item contract

Убрать зависимость `HeldItemSlot` от sandbox `PickupInteractable`.

Целевые контракты:

```text
IHoldableItem
├── ItemId
├── IsHeld
├── CanBeHeldBy(actor)
├── EnterHeldState(anchor)
└── LeaveHeldState(dropPose)

IItemCarrier
├── HeldItem
├── IsOccupied
├── TryReceive(item)
├── TryRelease(item)
└── TryDrop()
```

Точные имена могут быть скорректированы, но Interaction и Station не должны
зависеть от конкретного класса снаряда.

## 14.2. ShellDefinition

ScriptableObject-конфиг типа снаряда:

- stable shell type id;
- display name;
- projectile/launcher data reference;
- совместимый weapon id/caliber;
- визуальный prefab;
- масса и баллистические параметры после решения ballistic model;
- будущие damage/penetration данные.

## 14.3. ShellItem

World item:

- реализует общий holdable contract;
- содержит `ShellDefinition`;
- имеет Rigidbody и world colliders;
- отключает world physics в руках;
- корректно восстанавливает physics при drop;
- не может одновременно принадлежать двум carriers;
- поддерживает reservation по actor id.

## 14.4. Изменение PlayerInteraction

Удалить глобальное правило:

```text
если руки заняты → не искать interaction target
```

Вместо него:

- поиск цели выполняется всегда при активной capability;
- каждый target сам определяет доступность через `InteractionContext`;
- новый pickup недоступен при занятых руках;
- BreechLoadInteractable доступен только при совместимом ShellItem в руках;
- panel toggles могут оставаться доступными с предметом в руках, если это
  разрешено дизайном.

## 14.5. Критерий завершения

- Player подбирает ShellItem через E;
- новый предмет нельзя подобрать при занятых руках;
- удерживаемый снаряд не блокирует поиск казённика;
- drop корректно восстанавливает физику;
- item reservation снимается при drop, destroy или failed pickup.

---

# 15. Этап H — ручная зарядка

## 15.1. BreechLoadInteractable

Компонент связывает Interaction feature и application service загрузки.

`GetInteractionInfo` возвращает Hold только если:

- actor занимает подходящую Station либо имеет loader capability;
- permission policy разрешает fallback-loading;
- actor держит совместимый ShellItem;
- chamber пуста;
- gun не firing и не cooldown в запрещающем состоянии;
- другой actor не зарезервировал breech.

## 15.2. Begin

- повторно проверить условия;
- зарезервировать breech за actor id;
- зарезервировать ShellItem;
- включить loading audio;
- при необходимости запустить presentation state.

ShellItem остаётся в руках до успешного Complete.

## 15.3. Cancel

Причины:

- InputReleased;
- TargetLost;
- TargetUnavailable;
- ContextChanged;
- Station exit;
- Tank disable/destroy.

Действия:

- остановить loading audio;
- снять reservations;
- оставить ShellItem в руках;
- вернуть chamber в Empty;
- очистить presentation state.

## 15.4. Complete

- повторно подтвердить actor и reservations;
- извлечь ShellItem из carrier без world drop;
- поместить его `ShellDefinition` в GunChamber;
- уничтожить или перевести world representation в pooled state;
- снять reservations;
- остановить loading audio;
- сообщить presentation о Loaded state.

## 15.5. Interaction scope

Текущий `StationPanelRoot`, разрешающий только дочерние targets, недостаточен
для снарядов и казённика.

Нужен составной scope:

```text
GunnerInteractionScope
├── GunnerPanelRoot
├── BreechInteractionRoot
├── AllowedAmmoRoots / AmmoReachVolume
└── explicit allowed targets
```

Scope должен:

- не позволять взаимодействовать через корпус с внешними объектами;
- разрешать только снаряды в зоне видимости и допустимой зоне досягаемости;
- разрешать BreechLoadInteractable;
- поддерживать обычный Physics raycast из Player view;
- не телепортировать предмет автоматически без успешного Interact.

## 15.6. Критерий завершения

- Gunner подбирает видимый снаряд, не покидая места;
- наведение на breech показывает корректный prompt;
- удержание E воспроизводит звук и progress;
- отпускание E отменяет загрузку без потери снаряда;
- потеря цели отменяет загрузку;
- Complete помещает shell в chamber;
- после загрузки можно немедленно произвести выстрел;
- два actors не могут одновременно загрузить один breech или shell.

---

# 16. Этап I — fallback роли командира-заряжающего

## 16.1. Permission policy

Добавить отдельную policy/application service:

```text
IGunLoadingPermission
```

Она отвечает на вопрос, может ли конкретный actor выполнять загрузку из своей
текущей роли.

Правило для Gunner:

```text
GunnerStation занята actor
AND CommanderLoaderStation свободна
AND actor имеет доступ к gunner loading scope
→ loading allowed
```

Если CommanderLoaderStation занята другим Player:

```text
Gunner fallback loading denied
```

Наведение, zoom и firing остаются доступными.

## 16.2. Отсутствующая Station

До создания CommanderStation registry должен различать:

- role отсутствует в конфигурации Tank;
- role существует и свободна;
- role занята;
- role временно входит/выходит.

Рабочее правило для прототипа:

```text
роль не сконфигурирована → считать fallback доступным
```

Это правило должно быть подтверждено перед реализацией.

## 16.3. Критерий завершения

- свободная роль позволяет Gunner загрузить shell;
- занятая роль блокирует начало нового loading interaction;
- начатая загрузка отменяется, если permission потерян до Complete;
- освобождение роли снова открывает fallback;
- policy тестируется без Unity scene.

---

# 17. Этап J — прожекторы, ПНВ и пульт

Этот этап выполняется после полного цикла `pickup → load → aim → fire`.

## 17.1. Прожекторы

Создать module-level контракт:

```text
ITankSearchlightModule
├── IsAvailable
├── IsEnabled
└── TryToggle(actorId)
```

Панельный interactable передаёт запрос модулю. Прямая ссылка UI/кнопки на
конкретные `Light` нежелательна.

## 17.2. ПНВ

Создать optional `INightVisionModule`:

- существует не на каждом Tank;
- влияет только на выбранный display/camera feed;
- включается физическим тумблером через Interaction;
- не является обязательным условием работы GunnerCamera;
- presentation может использовать URP renderer feature, Volume или material
  pipeline после отдельного технического решения.

Точная реализация эффекта ПНВ остаётся `TBD`.

## 17.3. Декоративный пульт

Реакция на WASD является presentation-задачей:

- рычаг/джойстик отражает traverse/elevation input;
- animation читает итоговый intent/state;
- animation не управляет TurretMechanism;
- отсутствие animator не ломает gameplay;
- возврат в neutral происходит при отпускании input и exit Station.

## 17.4. Критерий завершения

- searchlights управляются через module contract;
- отсутствие модуля даёт unavailable prompt, а не исключение;
- ПНВ изменяет только gunner display feed;
- декоративные controls соответствуют WASD и не влияют на domain state.

---

# 18. Prefab composition

Предполагаемая итоговая структура wrapper-prefab:

```text
TankRoot_Prototype
├── HullAnchor
│   └── TankHull
│       ├── Turret_Yaw
│       │   ├── Gun_Pitch
│       │   │   ├── Muzzle
│       │   │   └── Breech_Load_Point
│       │   ├── Station_Gunner
│       │   ├── Crew_Gunner_Seat
│       │   ├── Crew_Gunner_Eye
│       │   ├── Display_Gunner
│       │   └── Console_Gunner
│       └── existing geometry
│
├── Stations
│   ├── DriverStation
│   │   └── existing common station configuration
│   └── GunnerStation
│       ├── InteractionPoint
│       ├── GunnerExitAnchor
│       ├── GunnerPanelRoot
│       └── interaction proxy colliders
│
├── TankInterior
│   ├── UpperHatchInteractable
│   ├── TankInteriorEntryAnchor
│   ├── TankExteriorExitAnchor
│   ├── CommanderLoaderStation
│   └── interior interaction objects
│
├── TurretRuntime
│   ├── UnityTurretRig
│   ├── GunnerCamera
│   └── GunnerDisplayFeed
│
├── WeaponRuntime
│   ├── GunView
│   ├── BreechLoadInteractable
│   ├── MuzzleEffects
│   └── Audio
│
├── Modules
│   ├── Searchlights
│   └── NightVision optional
│
└── Installer
    └── TankInstaller
```

Правила prefab wiring:

- не редактировать fileID/GUID вручную;
- scene/prefab изменения выполнять через подключённый Unity Editor;
- не менять геометрию donor prefab без отдельной необходимости;
- не искать runtime references по строковым именам;
- все обязательные ссылки валидировать в installer/view;
- optional modules не должны делать весь Tank prefab невалидным;
- camera target textures принадлежат экземпляру Tank.

---

# 19. Zenject composition

## 19.1. Предполагаемые bindings

Tank scope:

```text
CrewStationRegistry
TurretAimConfig
TurretAimState
ITurretRig → UnityTurretRig instance
TurretMechanism
GunConfig
GunChamber
TankGun
IGunLauncher → selected infrastructure implementation
IGunLoadingPermission
```

Station scope:

```text
CrewStationView instance
CrewStationOccupancy
ICrewStationRoleAdapter
CrewStationController
CrewStationInteractable instance
StationDisplayFeed[]
```

Gunner role adapter:

```text
GunnerStationAdapter
    ← TurretMechanism
    ← TankGun
    ← IGunnerIntentSource from current actor on activation
```

## 19.2. Registration in GameLoop

`TankRoot` должен регистрировать:

- все `CrewStationController` в Gameplay;
- `TurretMechanism` в Gameplay либо FixedGameplay согласно выбранной схеме;
- `TankGun` в Gameplay;
- projectile simulation в FixedGameplay, если она принадлежит Tank runtime;
- presentation adapters в Presentation.

При disable/destroy регистрация снимается в обратном порядке, inputs и active
sessions очищаются.

---

# 20. Failure recovery и lifecycle

Обязательные сценарии:

## 20.1. Station enter

- занятая станция отклоняет вход;
- failed attach отменяет occupancy transition;
- failed input-map switch откатывает Player capabilities;
- failed adapter activation отсоединяет Player;
- display feed включается только после полного успешного enter.

## 20.2. Station exit

- blocked exit оставляет Player в станции;
- blocked exit сохраняет Gunner context;
- успешный exit очищает gunner intents;
- успешный exit перемещает Player к внутреннему anchor рядом с сидением;
- station exit не переносит Player наружу;
- активный loading Hold отменяется;
- held shell остаётся у Player либо обрабатывается отдельным подтверждённым
  правилом;
- camera feed отключается после завершения exit.

## 20.3. Tank interior enter/exit

- вход через верхний люк переводит Player к безопасному внутреннему anchor;
- выход через люк доступен только Player, который находится внутри Tank и не
  занимает Station;
- заблокированный внешний anchor отменяет выход и оставляет Player внутри;
- station exit и hatch exit не используют один и тот же anchor;
- повторное взаимодействие с люком не создаёт двойной переход;
- Player внутри корректно следует за системой координат движущегося Tank.

## 20.4. Disable/destroy Tank

- Player, находящийся в Station или свободно внутри Tank, принудительно
  перемещается в безопасную внешнюю точку и получает восстановление;
- active hold отменяется;
- breech и shell reservations снимаются;
- turret input очищается;
- fire request очищается;
- runtime RenderTexture освобождается;
- technical camera выключается;
- GameLoop registrations удаляются;
- не остаётся второго AudioListener или потерянной Main Camera.

## 20.5. Disable/destroy items

- уничтоженный held item освобождает HeldItemSlot;
- уничтоженный shell во время loading отменяет session;
- уничтоженный breech target снимает reservation;
- scene unload не оставляет static/shared ownership.

---

# 21. Automated tests

## 21.1. EditMode — Station foundation

Проверить:

- независимые occupancy Driver и Gunner;
- вход только одного actor;
- enter/cancel/complete transitions;
- exit/cancel/complete transitions;
- rollback при неуспешном adapter activation;
- registry lookup по role id;
- отсутствие driver-specific зависимости в common controller.

## 21.2. EditMode — Gunner intent

- оси clamp в диапазоне `[-1, 1]`;
- fire request потребляется один раз;
- toggle request потребляется один раз;
- Clear обнуляет continuous и pending intents;
- suppression не пропускает held input после map switch.

## 21.3. EditMode — TurretMechanism

- traverse изменяет target yaw с правильным знаком;
- elevation изменяет target pitch;
- pitch clamps по config;
- current angles ограничены скоростью привода;
- HullRelative следует за корпусом;
- WorldStabilized компенсирует hull yaw;
- переключение mode не создаёт angular jump;
- ClearInput останавливает движение.

## 21.4. EditMode — TankGun

- Empty chamber отклоняет Fire;
- Loaded chamber создаёт один shot request;
- успешный shot очищает chamber;
- cooldown блокирует повторный shot;
- fire request не переживает деактивацию Station;
- launcher failure не теряет shell без явно выбранного правила.

## 21.5. EditMode — loading

- несовместимый shell отклонён;
- занятый chamber отклоняет Begin;
- Begin создаёт reservations;
- Cancel снимает reservations и сохраняет shell у carrier;
- Complete перемещает definition в chamber;
- второй actor не может начать Hold;
- потеря permission отменяет session;
- свободная CommanderLoader role разрешает fallback;
- занятая role запрещает fallback.

## 21.6. PlayMode — prefab wiring

- все обязательные serialized references назначены;
- `Turret_Yaw`, `Gun_Pitch`, `Muzzle`, `Breech_Load_Point` доступны adapter;
- верхний люк переводит Player между внешней и внутренней точками;
- Player может свободно находиться внутри башни, не занимая Station;
- GunnerStation входит и выходит;
- выход с GunnerStation оставляет Player внутри Tank;
- внешний выход возможен только через люк;
- display feed включается по occupancy;
- RenderTexture не разделяется двумя Tank instances;
- Main Camera не переподключается;
- существует один активный AudioListener;
- disable/destroy Tank восстанавливает Player.

---

# 22. Manual acceptance checklist

## 22.1. Enter / Exit

- [ ] Player входит внутрь Tank через Hold interaction с верхним люком.
- [ ] После входа Player свободно перемещается внутри башни.
- [ ] Внутри доступны как минимум GunnerStation и CommanderLoaderStation.
- [ ] Player входит в GunnerStation через отдельный Hold interaction.
- [ ] Player закрепляется в GunnerSeatAnchor.
- [ ] Tank не получает случайный Fire от кнопки входа.
- [ ] Main Camera остаётся на Player.CameraPivot.
- [ ] ExitStation возвращает walking input и interaction внутри Tank.
- [ ] ExitStation перемещает Player к GunnerExitAnchor рядом с сидением.
- [ ] ExitStation не перемещает Player наружу.
- [ ] После вставания Player может взаимодействовать со вторым сидением и
      предметами внутри башни.
- [ ] Выйти наружу можно только через взаимодействие с верхним люком.
- [ ] Цикл Enter → Exit работает минимум три раза.

## 22.2. Look и экран

- [ ] Mouse Look осматривает cockpit в заданных пределах.
- [ ] Player body не вращается относительно кресла.
- [ ] Display_Gunner показывает внешний вид по направлению прицела.
- [ ] Экран не показывает сам себя.
- [ ] ПКМ даёт корректное 2x увеличение.
- [ ] После отпускания ПКМ базовый FOV восстанавливается.

## 22.3. Наведение

- [ ] W поднимает ствол.
- [ ] S опускает ствол.
- [ ] A поворачивает башню влево.
- [ ] D поворачивает башню вправо.
- [ ] Pitch не выходит за limits.
- [ ] Отпускание WASD останавливает приводы.
- [ ] Space переключает режим без скачка.
- [ ] HullRelative проверен при вращении Tank.
- [ ] WorldStabilized сохраняет направление при вращении Tank.

## 22.4. Firing

- [ ] Empty chamber не производит projectile.
- [ ] Loaded chamber производит один shot на нажатие ЛКМ.
- [ ] Projectile/shot event появляется из Muzzle.
- [ ] После выстрела chamber становится Empty.
- [ ] Cooldown запрещает слишком частый повторный выстрел.
- [ ] Выстрел имеет звук и минимальный visual feedback.

## 22.5. Pickup и loading

- [ ] Gunner видит доступный shell в зоне interaction.
- [ ] E подбирает shell в руки.
- [ ] Удерживаемый shell не блокирует targeting breech.
- [ ] На BreechLoadPoint отображается правильный prompt.
- [ ] Hold E запускает звук и progress.
- [ ] Отпускание E отменяет loading.
- [ ] Потеря цели отменяет loading.
- [ ] Успешный Hold загружает chamber.
- [ ] После загрузки можно сразу выстрелить.
- [ ] Один shell нельзя загрузить дважды.

## 22.6. Role fallback

- [ ] Свободная CommanderLoader role разрешает Gunner loading.
- [ ] Занятая CommanderLoader role запрещает Gunner loading.
- [ ] Освобождение role снова разрешает loading.
- [ ] Запрет loading не отключает aim, zoom и fire уже заряженного орудия.

## 22.7. Modules

- [ ] Searchlight toggle работает через Interaction.
- [ ] ПНВ влияет только на gunner feed.
- [ ] Отсутствующий optional module не ломает Station.
- [ ] Пульт визуально реагирует на WASD.

## 22.8. Lifecycle

- [ ] Disable GunnerStation безопасно восстанавливает Player.
- [ ] Disable Tank отменяет loading и очищает input.
- [ ] Destroy Tank освобождает camera resources.
- [ ] Scene unload не оставляет runtime RenderTexture.
- [ ] После recovery Player имеет Main Camera, walking look и interaction.

---

# 23. Предполагаемые области изменений

```text
Input/
├── InputSystem_Actions.inputactions
├── PlayerInputSystem
├── InputMapId
├── CommonInputSnapshot
├── PlayerActionsSnapshot
├── DrivingActionsSnapshot
├── ICommonActionsReader
├── ActiveCommonActionsReader
├── PlayerActionsReader
├── DrivingActionsReader
├── GunnerActionsSnapshot
├── GunnerActionsReader
├── IGunnerIntentSource
└── GunnerIntentSnapshot

Player/
├── PlayerStationController
├── PlayerStationCapabilities
├── IPlayerInputContext
├── PlayerGunnerInputMode
├── GunnerIntentBuffer
├── HeldItemSlot
└── PlayerInteractionActor

Interaction/
├── PlayerInteraction
├── InteractionContext
├── IInteractionScope
└── item/carrier contracts

Tank/Stations/
├── CrewStationView
├── CrewStationInteractable
├── CrewStationController
├── CrewStationRegistry
├── StationCapabilityProfile
├── StationDisplayFeed
├── GunnerStationAdapter
└── GunnerInteractionScope

Tank/Interior/
├── upper hatch interactable
├── interior enter/exit lifecycle
├── interior/exterior anchors
└── moving-frame support for a walking Player

Tank/Turret/
├── TurretAimConfig
├── TurretAimState
├── TurretReferenceMode
├── TurretMechanism
├── ITurretRig
└── UnityTurretRig

Tank/Weapon/
├── GunConfig
├── GunChamber
├── TankGun
├── GunShotRequest
├── IGunLauncher
├── GunView
└── projectile implementation (после решения TBD)

Tank/Ammunition/
├── ShellDefinition
├── ShellItem
├── BreechLoadInteractable
├── GunLoadingPermission
└── loading presentation/audio

Tank/Modules/
├── ITankSearchlightModule
├── SearchlightModule
├── INightVisionModule
└── NightVision presentation

Tank/Infrastructure/
├── TankInstaller
├── TankRoot
├── TankRoot_Prototype.prefab
└── optional ScriptableObject configs
```

Конкретные имена новых файлов фиксируются во время реализации после проверки,
что существующий класс уже не выполняет нужную ответственность. Не создавать
пустые abstractions только ради совпадения со списком.

---

# 24. Рекомендуемый порядок коммитов / vertical slices

## Slice 1 — Driver acceptance gate

```text
Завершение Stage 04.1
без Gunner runtime изменений
```

## Slice 2 — Reusable multi-station foundation

```text
CrewStationView
+ generic input context
+ multiple station instances
+ Driver regression
```

## Slice 3 — Empty GunnerStation

```text
upper hatch enter
+ free movement inside Tank
+ GunnerStation enter / cockpit look / display / seat exit
+ upper hatch exit
без turret и gun gameplay
```

## Slice 4 — Aim and reference modes

```text
WASD
+ Turret_Yaw
+ Gun_Pitch
+ Space modes
+ zoom
```

## Slice 5 — Preloaded shot

```text
GunChamber Loaded
+ Fire
+ Muzzle launch event
+ Empty state
```

## Slice 6 — Items and ShellItem

```text
generic holdable contract
+ pickup/drop
+ interaction while hands occupied
```

## Slice 7 — Manual loading

```text
Breech Hold
+ audio/progress
+ chamber load
+ cancellation/recovery
```

## Slice 8 — Crew fallback

```text
CrewStationRegistry
+ CommanderLoader occupancy policy
```

## Slice 9 — Modules and polish

```text
searchlights
+ optional night vision
+ control animations
+ feedback
```

Каждый slice заканчивается отдельной проверяемой игровой возможностью и не
оставляет основной prefab в частично сломанном состоянии.

---

# 25. Не входит в первый обязательный slice

Не фиксировать и не реализовывать без отдельного решения:

- финальную ballistic model;
- финальный damage/penetration model;
- destructible turret/gun modules;
- networking package и RPC;
- client prediction/reconciliation;
- server authority rules;
- полноценный inventory UI;
- ammo stacking;
- все финальные типы shell;
- финальные VFX/audio assets;
- сложную gyro-stabilization по pitch и roll;
- AI gunner;
- автоматическую замену отсутствующего Player ботом;
- commander observation system;
- loader station, если она будет отдельной ролью;
- окончательную архитектуру ПНВ.

Первый обязательный результат — локальный single-player vertical slice с
архитектурными границами, пригодными для дальнейшей сетевой адаптации.

---

# 26. Definition of Done

Stage 04.2 считается завершённым, когда стабильно работает:

```text
Player входит в Tank через верхний люк
        ↓
свободно перемещается внутри башни
        ↓
Player занимает GunnerStation
        ↓
Main Camera остаётся на Player
        ↓
GunnerCamera показывает внешний вид на Display_Gunner
        ↓
WASD управляет Turret_Yaw и Gun_Pitch
        ↓
Space переключает подтверждённые reference modes
        ↓
ПКМ даёт 2x zoom
        ↓
при свободной CommanderLoader role Player подбирает ShellItem
        ↓
Hold E загружает GunChamber со звуком и cancellation
        ↓
ЛКМ производит один выстрел из Muzzle
        ↓
GunChamber становится Empty
        ↓
Player выходит
        ↓
Player остаётся внутри башни с восстановленными walking input, look,
interaction и camera
        ↓
Player взаимодействует с верхним люком и выходит наружу
```

Дополнительные обязательные архитектурные критерии:

- DriverStation и GunnerStation используют один station lifecycle;
- station exit и Tank exit являются разными lifecycle;
- освобождение Station всегда оставляет Player внутри Tank;
- внешний вход и выход выполняются только через верхний люк;
- Player может свободно находиться внутри Tank без занятой Station;
- role-specific код ограничен adapters и конкретными gameplay features;
- Gunner input проходит через Action Map и intent buffer;
- Turret/Gun domain не зависит от Unity API;
- interaction с shell/breech использует общую Interaction feature;
- camera feed владеет instance-safe resources;
- ballistic implementation заменяема через контракт;
- role fallback определяется отдельной policy;
- все failure paths снимают reservations и восстанавливают Player;
- DriverStation не получает regression.

---

# 27. Отчёт после реализации

После завершения перечислить:

```text
Какие решения из раздела 6 подтверждены
Что реализовано по каждому vertical slice
Какие файлы добавлены
Какие файлы изменены
Какие ScriptableObject configs созданы
Какие prefab objects добавлены
Какие donor transforms назначены в Inspector
Как устроены Gunner Action Map и suppression input
Как реализованы HullRelative и WorldStabilized
Как устроены GunChamber и firing pipeline
Как устроены ShellItem, HeldItemSlot и Breech loading
Как проверяется CommanderLoader occupancy
Как создаётся и освобождается runtime RenderTexture
Какие EditMode и PlayMode tests запущены
Как вручную проверить полный flow
Какие TBD остались
```
