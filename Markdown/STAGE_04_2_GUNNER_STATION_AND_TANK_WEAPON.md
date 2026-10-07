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
Gunner station, shell pickup, Press breech loading and Rigidbody firing
are implemented and wired in code. Full firing acceptance is not recorded.
Current task: verify tank stability during turret rotation with the ammo rack
enabled after linking Physics/Turret to Turret_Yaw.

Checkpoint date:
2026-10-07

Next session:
1. Verify the turret/ammo-rack collision fix in Play Mode
2. Verify empty gun -> pickup -> Press load -> occupy Gunner -> LMB shot
3. Verify chamber consumption, repeated dry fire, reload and projectile TTL
4. Record remaining aiming, zoom, lifecycle and station regression checks
5. Architecture refactoring, crew fallback, sounds and extra FX are deferred
```

Уточнение пользователя от 2026-10-06 определяет первый обязательный поток:
взять снаряд → вставить его в орудие → сесть на место наводчика → выстрелить.
Орудие изначально пустое; тестовый preload не используется. Минимальные звуки
и дополнительный визуальный отклик выстрела/зарядки пока исключены.
Зарядка с занятого места и fallback роли CommanderLoader остаются последующим
расширением. В рамках этого шага пользователь вносит код самостоятельно;
архитектурный рефакторинг отложен.

Manual acceptance Stage 04.1 завершён. Автоматические EditMode/PlayMode tests
для этого acceptance flow в документе пока не зафиксированы.
Пользователь сообщил о входе через люк, нахождении внутри башни,
посадке на место стрелка, изображении внешнего мира на экране и управлении
башней. Детальные проверки критериев разделов 11.6 и 12.5 не зафиксированы.

Текущий прогресс Stage 04.2:

- Завершено в коде: `Gunner` Action Map, typed readers и snapshots,
  `GunnerIntentBuffer`, `PlayerGunnerInputMode`, переключение input context;
- завершено в коде: `TurretAimState`, `TurretMechanism`, reference modes,
  `TurretAimConfig`, `UnityTurretRig` и регистрация gameplay/presentation
  поворота башни в `TankRoot`;
- завершено в коде и prefab: отдельная `GunnerCamera` под `Sensors`, её
  позиция берётся из `Crew_Gunner_Eye`, направление — из `MuzzlePose`;
- подключено в prefab и коде: `GunnerStation`, её anchors, `GunnerPanelRoot`,
  `GunnerStationAdapter`, отдельный controller в `TankStationsInstaller` и
  `StationDisplayFeed` с собственной runtime `RenderTexture`;
- подтверждено пользователем: Player входит через люк, находится внутри
  башни, садится на место стрелка, видит внешний мир на `Display_Gunner`
  и управляет башней; позднее пользователь подтвердил работу zoom по ПКМ;
- `ZoomHeld` подключён к FOV `GunnerCamera`;
- реализованы `IHoldableItem`, освобождение предмета из `HeldItemSlot`,
  `ShellItem`, `ShellDefinition`, изначально пустой `GunChamber`,
  `GunLoadingService` и `BreechLoadInteractable` с Press-взаимодействием;
- реализованы `GunShotRequest`, `IGunLauncher`, `RigidbodyGunLauncher`,
  `GunConfig`, `TankGun` и `TankWeaponModule`; `TankWeaponInstaller` создаёт
  одну общую камору для зарядки и стрельбы;
- `GunnerStationAdapter` передаёт `FireRequested` в `TankGun.TryFire`,
  используя `MuzzlePose` и наследуемую скорость корпуса;
- созданы отдельные `Shell_Pickup` и `TankProjectile_Prototype`;
  летящий prefab очищен от компонентов подбора;
- `ShellDefinition` настроен на projectile prefab: скорость 50 м/с,
  масса 1 кг, время жизни 10 с. Это тестовые параметры прототипа;
- полный цикл стрельбы и устранение опрокидывания ещё требуют подтверждения
  в Play Mode. Наличие реализации не означает завершение acceptance.

Отметка текущего среза (по сообщению пользователя и проверке кода/prefab):

- [x] Вход в башню через верхний люк.
- [x] Player находится внутри башни и может занять GunnerStation.
- [x] Изображение внешнего мира выводится на экран стрелка.
- [x] Стрелок управляет башней с занятого места.
- [x] Выход наводчика через F оставляет Player внутри башни; выход наружу
      через люк работает по сообщению пользователя.
- [x] Водитель входит и выходит через водительский люк; F для него не используется.
- [ ] Проверены pitch, limits и оба режима отсчёта направления башни.
- [x] Zoom по ПКМ работает по сообщению пользователя; точность 2x и сброс
      при выходе требуют отдельной проверки.
- [x] Для люков используется Hold, для сидений, pickup и казённика — Press.
- [x] Реализован общий контракт предмета в руках и освобождение рук при зарядке.
- [x] Реализованы пустая камора, проверка совместимости и запрет повторной загрузки.
- [x] Реализован Rigidbody launcher с дочерним BoxCollider, начальной скоростью,
      исключением столкновений со своим танком и удалением по времени жизни.
- [x] Реализованы расход заряда после успешного запуска и cooldown.
- [x] При отказе launcher заряд остаётся в каморе.
- [x] Орудие зарегистрировано через TankWeaponModule в Gameplay-фазе.
- [x] ЛКМ наводчика подключена к TankGun через существующий intent.
- [x] SeatInteraction перенесён под constrained SeatAnchor и наследует его движение.
- [x] В DemoScene добавлен ParentConstraint на Physics/Turret с источником Turret_Yaw.
- [ ] Подтверждён в Play Mode полный цикл pickup → Press loading → посадка → выстрел.
- [ ] Подтверждено отсутствие опрокидывания при повороте башни со включённой укладкой.

Зафиксированная схема взаимодействия:

```text
E на любом люке → Hold → переход к назначенной цели
  люк водителя → сразу DriverStation
  верхний люк → свободное перемещение внутри башни

E на кресле внутри башни → Press → занять Station
E на прочем доступном объекте → Press → выполнить его действие
F на месте внутри башни → встать и остаться внутри Tank
Выход водителя → Hold E на водительском люке
```

Общая Interaction feature уже отвечает за поиск цели, нажатие/удержание,
progress и отмену. Люк задаёт цель перехода; Station отвечает за занятие места.
Базовый flow входа/выхода подтверждён пользователем; повторные regression
и lifecycle-проверки остаются в разделе 22.

## 1.1. Текущая задача — устойчивость Tank при повороте башни

Симптом: при повороте башни вправо корпус опрокидывается влево.
При диагностике обнаружены отдельные кинематические Rigidbody снарядов
в TurretRack с включёнными физическими столкновениями. Укладка следовала
за Turret_Yaw, а стенки Physics/Turret оставались относительно корпуса.
Расчёт пересечений без изменения сцены показал контакт с TurretColliderLEFT
при повороте на 30–60°, до примерно 8 см при 45°.
Это подтверждённый геометрический конфликт; причинная связь с опрокидыванием
и успешное устранение симптома требуют проверки в Play Mode.

В сохранённой DemoScene ParentConstraint на Physics/Turret уже добавлен.
TurretRack сохраняет существующий constraint на тот же Turret_Yaw.
В root-prefab этот новый constraint пока отсутствует: изменение находится
в scene override. При закреплении настройки нужно учесть это различие.

- [ ] Проверить, что Physics/Turret и TurretRack следуют за одной башней.
- [ ] При необходимости сравнить поворот с временно отключённым TurretRack,
      затем включить его обратно.
- [ ] Со включёнными четырьмя снарядами проверить поворот в обе стороны
      без толчков, крена и опрокидывания корпуса.
- [ ] Повторить проверку после подбора снаряда и после вставания через F.
- [ ] После устойчивого поворота выполнить полный acceptance стрельбы.

Следующий шаг после этой задачи — приёмка первого ручного цикла стрельбы,
а не реализация нового оружейного механизма. Урон, пробитие, удаление projectile
по попаданию, pooling, CommanderLoader fallback, звук и дополнительные FX
в этом прототипе ещё не реализованы; архитектурный рефакторинг отложен.

---

# 2. Цель

Создать рабочее место стрелка-наводчика, в котором Player:

- входит внутрь башни через верхний люк;
- свободно перемещается внутри башни и взаимодействует с её предметами;
- может выбрать одно из доступных мест экипажа — Gunner или CommanderLoader;
- до посадки подбирает ShellItem и вручную заряжает орудие из walking mode;
- занимает GunnerStation через существующую Interaction feature;
- остаётся физически закреплённым в кресле;
- управляет горизонтальным поворотом башни;
- управляет углом возвышения орудия;
- переключает систему отсчёта направления башни;
- наблюдает внешний мир через экран камеры прицела;
- включает двукратное увеличение камеры;
- производит выстрел только из заряженного и готового орудия;
- в последующем расширении при отсутствии командира-заряжающего может подобрать
  снаряд и зарядить орудие, не покидая своего места;
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
подбирает ShellItem через Interact
        ↓
наводится на BreechLoadPoint и нажимает Interact (Press)
        ↓
ShellItem извлекается из рук, GunChamber переходит в Loaded
        ↓
взаимодействует с GunnerStation или CommanderLoaderStation
        ↓
нажимает Interact и занимает место
        ↓
Player прикрепляется к GunnerSeatAnchor
        ↓
включаются Gunner input context и cockpit look
        ↓
GunnerCamera выводится на Display_Gunner
        ↓
Player наводит башню и орудие
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
| `E` | `Hold` для люков, `Press` для остальных объектов под прицелом |
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
реализованы. После подключения GunnerStation остаются открытыми:

- проверка точности 2x zoom и сброса FOV при выходе;
- детальная Play Mode проверка выхода со станции, выхода через люк,
  reference modes и освобождения ресурсов display;
- приёмка уже реализованных предметов в руках, Press-зарядки и Rigidbody-выстрела;
- устойчивость корпуса при повороте башни с физическими снарядами в укладке.

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

Пользователь выбрал Rigidbody projectile для первого прототипа.
Реализован `RigidbodyGunLauncher` через контракт `IGunLauncher`.
Выбор не фиксирует финальную боевую баллистику; альтернативы для дальнейшей работы:

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

В первом обязательном потоке Player выполняет её в walking mode внутри башни,
до посадки. Доступность зарядки не требует занятой GunnerStation.

```text
PlayerInteraction
        ↓
ShellPickupInteractable
        ↓
HeldItemSlot
        ↓
BreechLoadInteractable (Press)
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

**Статус на 2026-10-07: базовое обобщение реализовано.** Driver и Gunner
имеют отдельные controllers в общем CrewStationRegistry и используют общий
lifecycle. CommanderLoaderStation и дополнительные display feeds остаются
расширениями; regression/lifecycle acceptance ещё не завершён.

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

**Статус: реализовано для Driver и Gunner.** `TankStationsInstaller` создаёт
отдельные controller и occupancy для обеих станций и передаёт их в
`CrewStationRegistry`. Driver regression после подключения Gunner требует
отдельной проверки.

Ниже сохранены архитектурные варианты для дальнейшего расширения списка
станций.

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

**Статус на 2026-10-07: реализовано и подключено.** Пользователь подтвердил
управление башней и zoom через GunnerStation; FireRequested теперь связан
с TankGun. Полная проверка suppression и стрельбы остаётся в acceptance.

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
Пользователь подтвердил управление башней с места стрелка. Точные проверки
pitch, limits и двух reference modes из раздела 11.6 ещё не зафиксированы.

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

**Статус на 2026-10-07: основной flow реализован и подтверждён пользователем;
детальный acceptance ещё открыт.**
Основной поток станции и экрана работает по сообщению пользователя.
`GunnerCameraPresenter` зарегистрирован после применения
углов башни. `GunnerStation` имеет `CameraAnchor`, собственный display feed
и controller; `StationDisplayFeed` создаёт runtime `RenderTexture`.
Пользователь подтвердил посадку, изображение внешнего мира, управление
башней, работу zoom, выход через F внутрь башни и отдельный выход через люк.
Остались проверка точности 2x zoom, повторных циклов и lifecycle feed.

Для люков используются два разных маршрута после
одинакового Hold-взаимодействия: люк водителя ведёт прямо в DriverStation,
верхний люк ведёт в свободное состояние внутри башни. Посадка на место
стрелка изнутри выполняется через Press. Базовые переходы работают;
оставшиеся regression-проверки перечислены в разделе 22.

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

**Статус на 2026-10-07:** реализация и подключение завершены в коде;
Play Mode acceptance раздела 13.6 / 22.4 остаётся открытым.

- [x] GunChamber хранит идентификатор заряда и изначально пуст.
- [x] TankGun проверяет заряд и cooldown, расходует заряд только после успешного launch.
- [x] GunShotRequest и IGunLauncher передают данные запуска.
- [x] RigidbodyGunLauncher создаёт отдельный projectile из MuzzlePose.
- [x] GunnerStationAdapter подключает ЛКМ; TankWeaponModule обновляет cooldown.
- [ ] Подтверждён полный цикл ручной зарядки и выстрела в Play Mode.

Первый выстрел выполняется только после ручного pickup и loading из этапов G/H.
Сначала реализуется состояние каморы, затем предметы/зарядка, затем firing.

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

В текущем прототипе Domain-камора хранит только shell id и состояния Empty/Loaded.
ShellDefinition хранится в GunLoadingService, cooldown — в TankGun.
Продолжительного Loading/Firing state и очереди запросов нет.

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

Подтверждённый первый поток использует изначально пустую камору:

```text
Player walking inside turret
GunChamber Empty
        ↓
pickup ShellItem
        ↓
manual loading at BreechLoadPoint
        ↓
GunChamber Loaded, hands free
        ↓
Player occupies GunnerStation
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

Preload не используется. После первого выстрела для повторного запуска Player
встаёт с места, берёт следующий снаряд, заряжает орудие и снова занимает место.

## 13.5. Presentation

Звуки выстрела/зарядки и дополнительный визуальный отклик отложены:
muzzle flash, recoil animation, camera impulse, smoke и dry fire feedback
не входят в текущий slice. Проверка выполняется по запуску из Muzzle,
состоянию каморы и расходованию загруженного снаряда.
Существующий Interaction HUD сохраняется; новые эффекты для него не требуются.

## 13.6. Критерий завершения

- ЛКМ создаёт один запрос на одно нажатие;
- камора изначально Empty и получает снаряд только через ручную зарядку;
- Player может зарядить орудие до посадки, затем занять GunnerStation;
- заряженное орудие производит один выстрел;
- пустое орудие не создаёт projectile;
- projectile/launch pose совпадает с `Muzzle`;
- выстрел очищает chamber;
- выход из Station не оставляет fire request;
- firing не зависит от частоты кадров.

---

# 14. Этап G — предметы, снаряды и руки

**Статус на 2026-10-07:** базовая реализация завершена в коде;
drop, failure recovery и подробный Play Mode acceptance остаются открытыми.

- [x] HeldItemSlot работает через IHoldableItem и IPickupReceiver.
- [x] PickupInteractable реализует общий holdable contract.
- [x] ShellItem хранит ShellDefinition; pickup physics остаётся в PickupInteractable.
- [x] Поиск целей доступен с предметом в руках; held item исключается из targeting.
- [x] ShellDefinition содержит projectile prefab, скорость, массу и время жизни.
- [x] Pickup prefab отделён от летящего projectile; укладка вынесена из модели.
- [ ] Подтверждены pickup/drop и recovery в Play Mode.

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

**Статус на 2026-10-07:** Press-зарядка реализована;
сквозная приёмка вместе с выстрелом ещё не зафиксирована.
Hold используется только на люках. Длительная loading session,
progress и reservations для неё в первый прототип не входят.

- [x] BreechLoadInteractable расположен вне модели и возвращает Press.
- [x] GunLoadingService проверяет interior context, held item, совместимость и пустую камору.
- [x] Успешная загрузка освобождает руки и удаляет pickup-представление снаряда.
- [x] Зарядка и стрельба используют одну камору из TankWeaponInstaller.
- [ ] Подтверждён полный повторяемый цикл зарядки и выстрела в Play Mode.

## 15.1. BreechLoadInteractable

Компонент связывает Interaction feature и application service загрузки.

`GetInteractionInfo` возвращает Press только если:

- для первого потока actor находится в Interior, свободно перемещается и не
  занимает Station; занятая GunnerStation не требуется;
- для последующего seated-loading потока actor занимает подходящую Station,
  а permission policy разрешает fallback-loading;
- actor держит совместимый ShellItem;
- chamber пуста;
- компонент казённика и TankWeaponView активны.

В текущем коде GunLoadingService проверяет interior context, но отдельного
запрета занятой Station и проверки cooldown при зарядке нет.
Seated-loading permission и межакторные reservations остаются расширениями.

## 15.2. Begin

- повторно проверить доступность Press-взаимодействия;
- использовать обычный Press lifecycle общей Interaction feature;
- не вводить Hold progress, звук или дополнительные эффекты.

ShellItem остаётся в руках до успешного Complete.

## 15.3. Cancel

В первом прототипе нет длительной загрузки по удержанию.
Cancel не изменяет камору и предмет в руках; отдельные reservations не создаются.
Если Complete не проходит повторную проверку, предмет остаётся в руках.

## 15.4. Complete

- повторно подтвердить actor, held item и совместимость;
- записать shell id в пустую GunChamber;
- освободить HeldItemSlot без world drop; при отказе откатить запись каморы;
- сохранить ShellDefinition в GunLoadingService;
- отключить и уничтожить world representation после успешной загрузки.

## 15.5. Interaction scope

В walking mode внутри Tank ограничение StationInteractionScope снято;
pickup и breech доступны через обычный поиск целей и проверки самого target.
Для первого потока дополнительный scope зарядки с занятого места не требуется.

Для последующего seated-loading потока используется отдельный
`StationInteractionScope`, а `StationPanelRoot` отвечает за экран панели.

Нужен составной scope:

```text
StationInteractionScope
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

- Player в walking mode внутри башни подбирает видимый снаряд до посадки;
- наведение на breech показывает корректный prompt;
- одно нажатие E выполняет загрузку без Hold progress, нового звука и эффектов;
- несовместимый снаряд или занятая камора не расходуют предмет;
- отказ освобождения HeldItemSlot откатывает загрузку;
- Complete помещает shell в chamber;
- после загрузки Player занимает GunnerStation и может произвести выстрел;
- два actors не могут одновременно загрузить один breech или shell.

---

# 16. Этап I — fallback роли командира-заряжающего

Последующее расширение: зарядка с занятого места. Оно не блокирует первый
поток pickup → loading в walking mode → посадка → выстрел.

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
- pending interaction очищается; длительной loading Hold session в прототипе нет;
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
- Press Begin повторно проверяет доступность;
- Cancel не меняет shell и chamber;
- Complete перемещает definition в chamber;
- отказ освобождения held item откатывает chamber;
- повторная загрузка занятой каморы отклоняется;
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

- [x] Player входит внутрь Tank через Hold interaction с верхним люком.
- [x] После входа Player свободно перемещается внутри башни.
- [ ] Внутри доступны как минимум GunnerStation и CommanderLoaderStation.
- [x] Player входит в GunnerStation через отдельный Press interaction.
- [x] Player закрепляется в GunnerSeatAnchor.
- [ ] Tank не получает случайный Fire от кнопки входа.
- [ ] Main Camera остаётся на Player.CameraPivot.
- [x] ExitStation возвращает walking input и interaction внутри Tank.
- [x] ExitStation перемещает Player к GunnerExitAnchor рядом с сидением.
- [x] ExitStation не перемещает Player наружу.
- [ ] После вставания Player может взаимодействовать со вторым сидением и
      предметами внутри башни.
- [x] Экипаж башни выходит наружу через Hold-взаимодействие с верхним люком.
- [x] Водитель входит и выходит через водительский люк, без F.
- [ ] Цикл Enter → Exit работает минимум три раза.

## 22.2. Look и экран

- [ ] Mouse Look осматривает cockpit в заданных пределах.
- [ ] Player body не вращается относительно кресла.
- [x] Display_Gunner показывает внешний вид по направлению прицела.
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
- [ ] Первый выстрел выполняется после ручной зарядки; preload отсутствует.

## 22.5. Pickup и loading

- [ ] Player в walking mode внутри башни видит доступный shell.
- [ ] E подбирает shell в руки.
- [ ] Удерживаемый shell не блокирует targeting breech.
- [ ] На BreechLoadPoint отображается правильный prompt.
- [ ] Одно нажатие E (Press) загружает chamber; Hold используется только на люках.
- [ ] Несовместимый shell не расходуется и остаётся в руках.
- [ ] Попытка загрузить занятую камору не расходует shell.
- [ ] После успешной Press-зарядки chamber становится Loaded.
- [ ] После загрузки руки свободны; Player садится в GunnerStation и стреляет.
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

## 22.9. Текущая проверка физики башни

- [ ] Physics/Turret и TurretRack движутся вместе с Turret_Yaw.
- [ ] При повороте в обе стороны с четырьмя снарядами танк не получает боковой толчок.
- [ ] При подборе снаряда и вставании через F не возникает крен или опрокидывание.
- [ ] Изменение ParentConstraint из DemoScene закреплено в нужном wrapper-prefab
      после подтверждения результата.

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
└── StationInteractionScope

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
└── loading presentation/audio (отложено)

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

## Slice 5 — Items and ShellItem

```text
generic holdable contract
+ pickup/drop in walking mode
+ interaction while hands occupied
```

## Slice 6 — Empty chamber and manual loading

```text
GunChamber initially Empty
+ Breech Press before occupying Station
+ chamber load / shell consumption / hands free
+ validation / failed load recovery
```

## Slice 7 — First shot after manual loading

```text
manual loading completed
+ occupy GunnerStation
+ FireRequested / Muzzle launch event
+ Empty state / repeated dry fire rejected
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
- минимальные звуки выстрела/зарядки и дополнительный визуальный отклик
  (временно отложены по указанию пользователя);
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
Player подбирает ShellItem
        ↓
Press E загружает изначально пустой GunChamber с проверкой условий
        ↓
ShellItem извлекается из рук; руки свободны
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
- освобождение станции внутри башни через F оставляет Player внутри Tank;
- экипаж башни входит и выходит наружу через верхний люк;
- DriverStation использует водительский люк для входа и выхода, без F;
- Player может свободно находиться внутри Tank без занятой Station;
- role-specific код ограничен adapters и конкретными gameplay features;
- Gunner input проходит через Action Map и intent buffer;
- Turret/Gun domain не зависит от Unity API;
- interaction с shell/breech использует общую Interaction feature;
- camera feed владеет instance-safe resources;
- ballistic implementation заменяема через контракт;
- последующий seated-loading fallback определяется отдельной policy;
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
