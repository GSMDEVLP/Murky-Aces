# STAGE_03_TANK_CORE.md

# Murky Aces — Stage 03: Tank Core

> Итоговая рабочая спецификация этапа `Tank Core`.
>
> Общие архитектурные правила определяет `CODEX_Murky_Aces.md`.
> Этот документ уточняет только Stage 3. При конфликте приоритет имеет
> `CODEX_Murky_Aces.md`.

---

# 1. Цель этапа

Получить первый архитектурно правильный управляемый танк.

Минимальный игровой сценарий:

```text
Player подходит к DriverStation
        ↓
Interaction System выполняет вход
        ↓
DriverStation занимается игроком
        ↓
Player переходит в station mode
        ↓
Walking input и locomotion отключаются
        ↓
Camera переходит на DriverCameraAnchor
        ↓
Включается Driving action map
        ↓
Player управляет танком
        ↓
Tank движется по поверхности и сталкивается с окружением
        ↓
Player запрашивает выход
        ↓
Проверяется ExitAnchor
        ↓
Player возвращается в walking mode
```

Stage 3 должен заложить фундамент, который позволит добавить другие станции,
модули и повреждения без переписывания движения танка.

---

# 2. Фактический фундамент проекта

## 2.1. Уже существует

- Unity `6000.5.4f1`;
- один центральный `GameLoop`;
- Zenject и Player subcontainer;
- New Input System;
- `InputActionAsset + PlayerInput`;
- `InputService`;
- `PlayerInputSystem`;
- `PlayerIntentBuffer`;
- Player Movement через `Rigidbody + CapsuleCollider`;
- Player Look;
- Interaction lifecycle `Begin / Complete / Cancel`;
- `InteractionContext` с идентификатором игрока;
- Interaction HUD;
- `PlayerRegistry`;
- одна основная Camera, которая присоединяется к `Player.CameraPivot`.

## 2.2. Пока отсутствует

- общий способ регистрировать в `GameLoop` gameplay-системы кроме Player;
- `Driving` action map;
- driving input reader и driving intent buffer;
- API перехода Player в station mode;
- отключение Player Look и Interaction во время управления;
- переключение основной Camera между anchors;
- occupancy реальной станции;
- Tank prefab;
- Tank installer;
- Tank movement;
- collision resolver для kinematic Tank;
- ground probe.

## 2.3. Важные уточнения

Не вводить `IPlayerInput` для Stage 3.

Текущий input flow сохраняется:

```text
PlayerInput
    ↓
InputService
    ↓
PlayerInputSystem
    ↓
PlayerIntentBuffer / DrivingIntentBuffer
    ↓
Gameplay
```

Player не использует `CharacterController`. Все переходы station mode должны
работать с существующими `Rigidbody`, `CapsuleCollider`, `PlayerMovement` и
`PlayerLook`.

---

# 3. Зафиксированные решения Stage 3

## 3.1. Tank body

Использовать:

```text
Kinematic Rigidbody
```

Обязательное состояние:

```csharp
rigidbody.isKinematic = true;
```

Tank Core не использует:

- `AddForce`;
- `AddTorque`;
- `WheelCollider`;
- физическую симуляцию гусениц;
- прямое чтение устройств ввода;
- `transform.position` как основной collision-safe movement pipeline.

`Rigidbody.MovePosition` и `Rigidbody.MoveRotation` применяют уже разрешённое
движение. Проверка препятствий выполняется отдельным collision resolver внутри
Unity motion adapter.

## 3.2. Movement

Movement является контролируемым аркадным 3D movement.

Он должен поддерживать:

- движение вперёд и назад;
- acceleration и deceleration;
- brake;
- steering;
- задержку смены направления;
- поверхность под корпусом;
- изменение высоты;
- умеренные уклоны;
- ориентацию по normal поверхности;
- столкновения со стенами и крупными препятствиями.

Сложная suspension simulation в Stage 3 не входит.

## 3.3. Player в станции

Во время управления Player:

- находится в `DriverSeatAnchor`;
- не обрабатывает walking locomotion;
- не вращает собственный Rigidbody через `PlayerLook`;
- не выполняет обычные world interactions;
- не отображает обычный Interaction HUD;
- использует `Driving` action map;
- использует ту же основную Camera через другой anchor.

Station mode должен кэшировать и корректно восстанавливать состояние Player.

## 3.4. Camera

Stage 3 не создаёт вторую активную Camera.

Используется существующая основная Camera:

```text
Walking
→ Player.CameraPivot

Driving
→ DriverCameraAnchor
```

Переключение выполняет Player-side camera adapter. `DriverStation` хранит только
anchor и не управляет Unity Camera напрямую.

Zoom, свободный осмотр, ограничения углов, ПНВ и внешние экраны в Stage 3 не
реализуются.

## 3.5. Input contexts

Добавить отдельную Action Map:

```text
Driving
├── Throttle      Axis  -1..1
├── Steering      Axis  -1..1
├── Brake         Button
└── ExitStation   Button
```

`Player` и `Driving` являются взаимоисключающими gameplay-контекстами.

После входа `ExitStation` не должен сработать от той же физической кнопки,
которой был выполнен вход. Exit разрешается только после отпускания кнопки или
после явного подавления первого input edge.

## 3.6. Prefabs

Использовать:

```text
Tank Root Prefab
+
Hull Prefab
```

Hull visual и основной physical collider не должны быть одним неразделимым
prefab.

Полная module system и абстрактные module mounts в Stage 3 не создаются. Реальные
anchors добавляются только тогда, когда они нужны существующей модели.

## 3.7. Networking

Networking не реализуется.

Сохраняется направление данных:

```text
Input
    ↓
Intent
    ↓
Station authorization
    ↓
TankDrivingInput
    ↓
Tank state
    ↓
Unity presentation
```

Tank movement не должен быть связан с конкретным локальным устройством ввода.

---

# 4. Не входит в Stage 3

Не реализовывать:

- GunnerStation;
- CommanderStation;
- Loader workflow;
- turret;
- cannon;
- ammo;
- fuel;
- engine module;
- track damage;
- radar;
- radio;
- GPS;
- night vision;
- damage model;
- repair;
- armor health;
- detach;
- module replacement;
- inventory;
- cooperative carrying;
- AI;
- missions;
- networking.

Не создавать общий `TankModule` до Stage 5.

---

# 5. Общая архитектура

## 5.1. Dependency direction

```text
Unity adapters / Presentation
            ↓
Application coordination
            ↓
Gameplay state and rules
```

Gameplay не зависит от:

- `PlayerInput`;
- `InputAction`;
- `Keyboard`;
- `Mouse`;
- конкретной Camera;
- конкретного `Rigidbody`;
- scene lookup;
- `Container.Resolve<T>()`.

## 5.2. Driving flow

```text
Driving Action Map
        ↓
InputService
        ↓
PlayerInputSystem
        ↓
DrivingIntentBuffer              Player scope
        ↓
PlayerStationController
        ↓
DriverStationController
        ↓
TankDrivingInput                 immutable snapshot
        ↓
TankMovement
        ↓
ITankGroundProbe
        ↓
ITankMotionBody
        ↓
KinematicTankMotionBody          Unity adapter
        ↓
Rigidbody.MovePosition / MoveRotation
```

## 5.3. Enter flow

```text
PlayerInteraction.Complete
        ↓
DriverStationController.TryEnter
        ↓
Validate Station and Player
        ↓
DriverStation: Free → Entering
        ↓
Reserve OccupantId
        ↓
Cancel active Player interaction
        ↓
Disable locomotion, look and world interaction
        ↓
Attach Player to DriverSeatAnchor
        ↓
Move main Camera to DriverCameraAnchor
        ↓
Clear driving intent
        ↓
Switch PlayerInput to Driving
        ↓
DriverStation: Entering → Occupied
```

Если любой обязательный шаг не выполнен, transition откатывается и станция
возвращается в `Free`.

## 5.4. Exit flow

```text
ExitStation request
        ↓
DriverStationController.TryExit
        ↓
Validate current occupant
        ↓
Validate free space at DriverExitAnchor
        ↓
DriverStation: Occupied → Exiting
        ↓
Clear driving intent
        ↓
Switch PlayerInput to Player
        ↓
Move Player to DriverExitAnchor
        ↓
Restore Player Rigidbody and Collider state
        ↓
Enable locomotion, look and world interaction
        ↓
Move main Camera to Player.CameraPivot
        ↓
Release OccupantId
        ↓
DriverStation: Exiting → Free
```

Если exit point заблокирован:

```text
Player remains seated
DriverStation remains Occupied
Driving input remains active
```

---

# 6. GameLoop integration

## 6.1. Единственная Unity entry point

Существующий `GameLoop` остаётся единственным владельцем:

- `Update`;
- `FixedUpdate`;
- будущего `LateUpdate`, если он понадобится presentation-системам.

Не создавать второй runner и не передавать gameplay scheduling скрытому
Zenject `TickableManager`.

## 6.2. Общий фазовый реестр

Текущий `GameLoop` обновляет только `PlayerRegistry`. Перед подключением Tank его
нужно минимально расширить общим фазовым реестром.

Логические группы:

```text
Input tickables
Gameplay tickables
Fixed gameplay tickables
Presentation tickables
```

Требования:

- все input tickables выполняются до gameplay tickables;
- Tank movement выполняется в fixed gameplay;
- регистрация и удаление симметричны;
- уничтоженный объект не остаётся в реестре;
- `PlayerRegistry` может остаться registry игроков, но не должен быть единственным
  способом scheduling;
- существующие Player phases должны продолжить выполняться в прежнем порядке.

Предпочтительные контракты:

```text
IInputTickable
IGameplayTickable
IFixedGameplayTickable
IPresentationTickable
```

Точные имена можно согласовать с существующими `ITickable` и `IFixedTickable`, но
принадлежность к фазе должна быть явной.

## 6.3. Порядок Stage 3

```text
Update
├── Player input snapshot
├── Player station coordination
├── Player gameplay
└── Presentation

FixedUpdate
├── Player locomotion, если она активна
└── TankMovement
    ├── read latest TankDrivingInput
    ├── update TankMotionState
    ├── query ground
    ├── resolve collision
    └── apply kinematic motion
```

---

# 7. Структура каталогов

Пока существующие feature находятся в `Assets/Scripts`, Tank также размещается в
этом корне.

```text
Assets/Scripts/
│
├── Tank/
│   ├── TankRoot.cs
│   │
│   ├── Movement/
│   │   ├── TankDrivingInput.cs
│   │   ├── TankMotionState.cs
│   │   ├── TankGroundInfo.cs
│   │   ├── TankMovement.cs
│   │   ├── ITankMotionBody.cs
│   │   ├── ITankGroundProbe.cs
│   │   └── TankMovementConfig.cs
│   │
│   ├── Stations/
│   │   ├── DriverStation.cs
│   │   ├── DriverStationState.cs
│   │   ├── DriverStationController.cs
│   │   ├── DriverStationInteractable.cs
│   │   └── DriverStationView.cs
│   │
│   └── Infrastructure/
│       ├── KinematicTankMotionBody.cs
│       ├── TankGroundProbe.cs
│       └── TankInstaller.cs
│
├── Player/
│   ├── Domain/
│   │   └── DrivingIntentBuffer.cs
│   ├── Infrastructure/
│   │   ├── PlayerInteractionActor.cs
│   │   └── PlayerInputContext.cs
│   ├── Presentation/
│   │   └── PlayerCameraController.cs
│   └── Stations/
│       ├── IStationOccupant.cs
│       └── PlayerStationController.cs
│
└── Interaction/
    └── Abstraction/
        └── IInteractionActor.cs
```

Не создавать параллельную структуру только для Tank в
`Assets/Scripts/GamePlay/Feature/Tank`.

Переезд всех features в
`Assets/_Project/Develop/Runtime/Gameplay/Features` является отдельной задачей и
не входит в Stage 3.

Не создавать пустые папки и классы заранее.

---

# 8. Границы классов

## 8.1. TankRoot

`TankRoot` представляет runtime-экземпляр танка.

Отвечает за:

- identity конкретного Tank;
- композицию созданных Tank-систем;
- lifecycle регистрации в `GameLoop`;
- явный доступ к действительно ключевым частям.

Не отвечает за:

- чтение Input;
- расчёт движения;
- Camera;
- Player transitions;
- future modules;
- damage.

Не создавать `TankReferences`, пока не появится доказанная необходимость.
`TankRoot` не должен становиться Service Locator.

## 8.2. TankDrivingInput

Immutable value object одного driving snapshot.

Минимальная форма:

```csharp
public readonly struct TankDrivingInput
{
    public float Throttle { get; }
    public float Steering { get; }
    public bool IsBraking { get; }
}
```

Диапазоны:

```text
Throttle: -1..1
Steering: -1..1
```

Значения нормализуются до передачи в `TankMovement`.

## 8.3. TankMotionState

Runtime-state конкретного танка.

Может содержать:

- current forward speed;
- current turn speed;
- current direction;
- braking state;
- direction switch state;
- remaining direction switch delay.

Не хранить state в `ScriptableObject`.

## 8.4. TankMovementConfig

`ScriptableObject` только со статическими tuning values.

Минимальные параметры:

```text
ForwardMaxSpeed
ReverseMaxSpeed
ForwardAcceleration
ReverseAcceleration
Deceleration
BrakeDeceleration
TurnSpeed
DirectionSwitchDelay
GroundProbeDistance
GroundOffset
GroundAlignmentSpeed
MaximumSlopeAngle
CollisionSkin
```

Runtime-значения в config не записываются.

## 8.5. TankMovement

Plain C# gameplay-класс и fixed gameplay participant.

Отвечает за:

- интерпретацию `TankDrivingInput`;
- acceleration и deceleration;
- brake;
- forward/reverse;
- direction switch delay;
- steering;
- обновление `TankMotionState`;
- вычисление desired displacement и rotation;
- запрос ground data;
- передачу движения в `ITankMotionBody`.

Не отвечает за:

- New Input System;
- Physics queries;
- конкретный Rigidbody;
- Camera;
- DriverStation occupancy;
- Player state;
- audio и VFX.

## 8.6. ITankMotionBody

Граница между gameplay movement и Unity Physics.

Предоставляет только данные и операции, необходимые движению:

```text
Position
Rotation
Resolve and apply requested motion
```

Интерфейс не раскрывает gameplay-слою конкретный `Rigidbody` или `Collider`.

## 8.7. KinematicTankMotionBody

Unity adapter, `MonoBehaviour`.

Отвечает за:

- kinematic Rigidbody;
- основной Hull collider;
- collision casts;
- allowed displacement;
- безопасное применение position и rotation;
- физические настройки Tank body.

Не рассчитывает throttle, acceleration, brake или gear switching.

Первая реализация может использовать `cast-and-stop`. Переход к
`cast-and-slide` допускается после ручной проверки, не меняя `TankMovement`.

## 8.8. ITankGroundProbe

Возвращает `TankGroundInfo`:

```text
HasGround
GroundPoint
GroundNormal
Distance
```

Gameplay не знает, использует реализация raycast, shape cast или несколько
probe points.

## 8.9. TankGroundProbe

Unity implementation `ITankGroundProbe`.

Отвечает за:

- Physics queries к поверхности;
- ground point;
- ground normal;
- ground distance.

Не принимает решений о throttle, brake и switching direction.

## 8.10. DriverStation

Plain runtime-state станции.

Хранит:

```text
State
OccupantId
```

States:

```text
Free
Entering
Occupied
Exiting
```

Переходы выполняются только через явные методы `Try...` и проверяют текущего
occupant.

`DriverStation` не управляет Input System, Player Rigidbody, Camera или Tank
movement.

## 8.11. DriverStationView

Unity component с scene references:

```text
DriverSeatAnchor
DriverExitAnchor
DriverCameraAnchor
Interaction point
Exit validation settings
```

Stage 3 использует одну exit point. Структура не должна запрещать добавить
несколько anchors позже.

## 8.12. DriverStationController

Application coordinator.

Связывает:

- существующую Interaction feature;
- `DriverStation`;
- `PlayerStationController`;
- driving input;
- Camera adapter;
- Tank input endpoint.

Отвечает за enter, exit, rollback и cleanup. Не рассчитывает движение Tank.

## 8.13. DriverStationInteractable

Тонкий Unity adapter, `MonoBehaviour`, который реализует существующий
`IInteractable` и делегирует enter lifecycle в `DriverStationController`.

Он нужен потому, что текущий `InteractionTargetLink` хранит ссылку на
`MonoBehaviour`.

Не хранит отдельную occupancy и не переключает Player, Camera или Input.

## 8.14. PlayerStationController

Player-side coordinator station mode.

Отвечает за:

- запрет повторного входа в другую станцию;
- отключение и восстановление locomotion;
- отключение и восстановление Player Look;
- отмену активного Interaction;
- отключение и восстановление world interaction/HUD;
- кэширование Rigidbody/Collider state;
- присоединение Player к seat anchor;
- восстановление Player в exit pose;
- переключение camera anchor;
- переключение input context;
- передачу driving intent текущей станции.

Player capability switches должны находиться здесь, а не быть размазаны по
проверкам `if (isDriving)` во всех gameplay-классах.

## 8.15. PlayerInputContext

Unity-specific adapter над существующим `PlayerInput`.

Отвечает только за явное переключение Action Maps:

```text
Player ↔ Driving
```

Не читает gameplay actions и не управляет Tank.

## 8.16. PlayerCameraController

Player-side presentation adapter для существующей основной Camera.

Отвечает за переключение Camera между `Player.CameraPivot` и station camera
anchor, сохраняя одну активную gameplay Camera.

Не хранит occupancy и не принимает решение о входе или выходе.

## 8.17. Interaction actor

`InteractionContext` должен идентифицировать не только число, но и участника
взаимодействия через минимальный `IInteractionActor`.

```text
IInteractionActor
└── Id
```

Конкретный Player actor может дополнительно реализовать capabilities:

```text
IPickupReceiver
IStationOccupant
```

Interactable проверяет требуемую capability явным приведением интерфейса. Не
добавлять в `InteractionContext` отдельное поле под каждую будущую механику и не
использовать generic service locator.

Существующий Pickup должен продолжить работать через `IPickupReceiver`.

## 8.18. PlayerInteractionActor

Per-player application adapter и конкретная реализация `IInteractionActor`.

Он:

- предоставляет стабильный Player id;
- делегирует `IPickupReceiver` существующему `HeldItemSlot`;
- делегирует `IStationOccupant` в `PlayerStationController`;
- создаётся в Player subcontainer.

Он не предоставляет generic `Get<T>()` и не становится Service Locator.

---

# 9. Prefab architecture

Рекомендуемая иерархия:

```text
TankRoot                         Rigidbody (Kinematic)
│                                TankRoot component
│                                KinematicTankMotionBody
│
├── Collision
│   └── HullCollider
│
├── HullAnchor
│   └── Hull Prefab Instance
│
├── Stations
│   └── DriverStation
│       ├── InteractionPoint
│       ├── DriverSeatAnchor
│       ├── DriverCameraAnchor
│       └── DriverExitAnchor
│
└── GroundProbes
    └── Probe anchors required by chosen implementation
```

`Rigidbody` находится на движущемся Tank root, чтобы Hull, Station anchors и
Player внутри станции перемещались как единое целое.

Hull Prefab содержит визуальную геометрию:

- MeshRenderer или SkinnedMeshRenderer;
- materials;
- LOD, если уже нужен модели;
- visual children.

Центральный collision collider остаётся на Tank Root prefab и не зависит от
замены Hull visual.

---

# 10. Collision и grounding

## 10.1. Collision pipeline

```text
TankMovement
        ↓
Desired displacement and rotation
        ↓
KinematicTankMotionBody
        ↓
Collision query
        ↓
Allowed movement
        ↓
Rigidbody.MovePosition / MoveRotation
```

Минимальные требования:

- Tank не проходит через обычную стену;
- Tank не проходит через крупный obstacle;
- translation учитывает skin distance;
- rotation рядом с геометрией не вызывает очевидного проникновения;
- collision logic отсутствует в `TankMovement`;
- игровые скорости ограничены значениями config.

## 10.2. Ground pipeline

```text
TankGroundProbe
        ↓
TankGroundInfo
        ↓
TankMovement
        ↓
Target height and surface-aligned rotation
        ↓
ITankMotionBody
```

Минимальные требования:

- определяется наличие ground;
- Tank удерживает настраиваемый ground offset;
- Tank следует умеренному изменению высоты;
- Tank ориентируется по ground normal со сглаживанием;
- слишком крутой slope блокирует подъём;
- корпус не уходит под поверхность.

Поведение при полном отсутствии ground, падение с высоты и сложная suspension
остаются `TBD` и не входят в acceptance-сцену Stage 3.

---

# 11. Zenject boundaries

`TankInstaller` устанавливается на Tank Root prefab или его GameObjectContext.

Он связывает:

```text
TankRoot
TankMovement
TankMotionState
DriverStation
DriverStationController
TankMovementConfig
ITankMotionBody       → KinematicTankMotionBody
ITankGroundProbe      → TankGroundProbe
```

Static config передаётся через serialized reference installer-а или принятый в
проекте config provider.

Не использовать:

```csharp
Container.Resolve<T>()
```

в runtime gameplay-коде.

Player-specific `DrivingIntentBuffer` остаётся в Player subcontainer. Tank state
не должен становиться глобальным singleton.

---

# 12. Этапы реализации

Каждый этап выполняется отдельно. После каждого этапа проект должен
компилироваться, а соответствующая проверка — проходить.

## Stage 3.0 — Foundation audit

Статус:

```text
Completed by document audit
```

Зафиксировано:

- Player использует Rigidbody, не CharacterController;
- `IPlayerInput` отсутствует и не нужен;
- GameLoop сейчас scheduling-ит только Player;
- Driving map отсутствует;
- station/camera API отсутствуют;
- Interaction feature переиспользуется;
- Tank code ещё не создан.

Результат этапа — этот документ.

---

## Stage 3.1 — Generalized GameLoop registration

Сделать:

- ввести явные phase contracts;
- добавить общий registry фаз;
- перевести существующие Player phases на registry без изменения поведения;
- обеспечить register/unregister;
- сохранить единственный `GameLoop` MonoBehaviour.

Не делать:

- отдельный Tank runner;
- Zenject `ITickable` как параллельный gameplay loop;
- unrelated refactor Player logic.

Готово, если:

- Player по-прежнему ходит и смотрит;
- Interaction по-прежнему обновляется;
- порядок Input → Gameplay → Presentation сохранён;
- тестовый fixed participant можно зарегистрировать и удалить;
- в сцене один GameLoop.

---

## Stage 3.2 — Tank movement contracts and config

Создать:

```text
TankDrivingInput
TankMotionState
TankGroundInfo
TankMovementConfig
ITankMotionBody
ITankGroundProbe
TankMovement
```

Реализовать pure movement rules:

- acceleration;
- deceleration;
- brake;
- forward/reverse limits;
- steering normalization;
- direction switch delay;
- zero-input settling.

Готово, если:

- проект компилируется;
- `TankMovement` не использует Input System и Physics API;
- runtime-state отсутствует в config;
- pure rules можно тестировать без сцены.

---

## Stage 3.3 — Flat-ground kinematic prototype

Создать:

```text
TankRoot
KinematicTankMotionBody
TankInstaller
prototype Tank prefab
```

На этом шаге допускается простое collision-free test area.

Подключить `TankMovement` к fixed gameplay phase центрального GameLoop.

Для теста input snapshot может задаваться временным inspector/debug source,
который удаляется до Stage 3.10. Не читать клавиатуру напрямую.

Готово, если:

- Tank движется вперёд и назад;
- Tank поворачивается;
- acceleration, brake и switching delay видимы;
- используется kinematic Rigidbody;
- `AddForce` и `AddTorque` отсутствуют;
- Tank обновляется только центральным GameLoop.

---

## Stage 3.4 — Grounding

Создать:

```text
TankGroundProbe
```

Реализовать:

- detection поверхности;
- ground point и normal;
- ground offset;
- изменение высоты;
- alignment корпуса;
- ограничение maximum slope.

Перед реализацией зафиксировать в коде и Inspector выбранную probe-схему. Её
можно изменить внутри `TankGroundProbe`, не меняя `TankMovement`.

Готово, если Tank проходит тестовую сцену с:

- flat ground;
- умеренным slope;
- плавным изменением высоты;
- границей слишком крутого slope.

---

## Stage 3.5 — Collision-safe motion

Добавить collision resolution в `KinematicTankMotionBody`.

Первая версия:

```text
cast-and-stop
```

`cast-and-slide` добавляется только при необходимости после ручной проверки.

Готово, если:

- Tank не проходит через стену;
- Tank не проходит через крупный obstacle;
- Tank не перескакивает obstacle на максимальной скорости;
- поворот рядом со стеной не создаёт очевидного проникновения;
- collision code отсутствует в `TankMovement`.

---

## Stage 3.6 — Final Tank prefab composition

Собрать:

- Tank Root prefab;
- отдельный Hull prefab;
- Hull collider;
- DriverStation hierarchy;
- DriverSeatAnchor;
- DriverCameraAnchor;
- DriverExitAnchor;
- InteractionPoint;
- ground probe anchors, если они нужны реализации.

Готово, если:

- Hull visual можно заменить без изменения movement;
- collider не зависит от Hull prefab;
- вся движущаяся hierarchy находится под Rigidbody root;
- prefab проходит Zenject validation.

---

## Stage 3.7 — Interaction actor and Player station capability

Сделать:

- минимальный `IInteractionActor`;
- создать per-player `PlayerInteractionActor`;
- адаптировать `InteractionContext`;
- сохранить работу Pickup через `IPickupReceiver` capability;
- создать `IStationOccupant`;
- создать `PlayerStationController` внутри Player feature;
- добавить минимальные enable/disable APIs в Player Movement, Look и Interaction;
- реализовать cache/restore Rigidbody и Collider state;
- реализовать attach/detach к тестовому seat anchor.

Не переписывать Player Movement.

Готово, если:

- `InteractionContext` получает actor с корректным id;
- Pickup и Drop из Stage 2 не сломаны;
- Player можно перевести в station mode на тестовом anchor;
- Player не ходит, не прыгает и не вращает body в station mode;
- Interaction HUD скрыт в station mode;
- исходное Player state полностью восстанавливается.

---

## Stage 3.8 — DriverStation state and Interaction

Сделать:

- создать `DriverStation`;
- создать `DriverStationState`;
- создать `DriverStationView`;
- создать `DriverStationController`;
- создать `DriverStationInteractable`;
- подключить существующий `IInteractable` lifecycle.

Готово, если:

- свободная станция предлагает Enter;
- занятая станция недоступна другому actor;
- один actor не занимает станцию дважды;
- `Begin / Complete / Cancel` не оставляют зависшую reservation;
- Player занимает `DriverSeatAnchor` реального Tank prefab;
- failed enter откатывает Player и Station state.

---

## Stage 3.9 — Camera anchor switching

Добавить `PlayerCameraController` для существующей основной Camera.

Поддержать:

```text
Player.CameraPivot ↔ DriverCameraAnchor
```

Готово, если:

- в сцене остаётся одна активная gameplay Camera;
- при входе используется DriverCameraAnchor;
- при выходе восстанавливается Player.CameraPivot;
- повторные enter/exit не ломают parent или local pose.

---

## Stage 3.10 — Driving input context

Добавить `Driving` map в существующий InputActionAsset.

Расширить существующий input pipeline:

```text
InputService
PlayerInputSystem
DrivingIntentBuffer
```

Добавить `PlayerInputContext`, который переключает `PlayerInput` между картами,
не смешивая эту ответственность с чтением actions.

`InputService` должен хранить явные references на action maps/actions. Его cache
не должен зависеть только от смены `InputActionAsset`.

`PlayerStationController` передаёт snapshot только текущей станции.

Готово, если:

- `Player` map активен вне станции;
- `Driving` map активен внутри DriverStation;
- walking actions не двигают Player во время управления;
- Throttle, Steering и Brake управляют Tank;
- отпускание controls приводит input к neutral;
- входная кнопка не вызывает немедленный выход;
- `TankMovement` не знает о `PlayerInput` и `InputAction`.

После этого временный debug input source из Stage 3.3 удаляется.

---

## Stage 3.11 — Validated exit flow

Реализовать проверку свободного места в `DriverExitAnchor`.

Проверка должна учитывать объём Player CapsuleCollider, а не только одну точку.

Готово, если:

- при свободном anchor Player выходит;
- Player появляется вне Hull collision;
- walking input, look, interaction и camera восстанавливаются;
- DriverStation становится `Free`;
- при заблокированном anchor Player остаётся водителем;
- failed exit не оставляет частично восстановленное состояние.

---

## Stage 3.12 — Lifecycle and failure recovery

Обработать:

- disabling Tank;
- disabling DriverStation;
- destroying Tank без водителя;
- destroying Tank с водителем;
- unloading scene;
- despawning Player;
- exception/failed transition rollback;
- unregister из GameLoop.

Главное правило:

```text
Player не должен остаться без walking input,
если Tank или DriverStation исчезли.
```

Готово, если все cleanup paths освобождают station и восстанавливают Player или
безопасно завершают его lifecycle.

---

## Stage 3.13 — Tests and acceptance scene

Создать или подготовить сцену проверки:

```text
Player
Tank
flat surface
moderate slope
too-steep slope
height transition
wall
large obstacle
blocked exit volume
```

Добавить EditMode tests для pure C# частей, если test assembly ещё отсутствует —
создать минимальную отдельную test assembly без изменения runtime architecture.

Проверить полный цикл не менее трёх раз подряд:

```text
Enter → Drive → Stop → Exit
```

Готово, если выполнен checklist из раздела 13.

---

# 13. Acceptance checklist

## 13.1. Startup

- [ ] Scene запускается без exceptions.
- [ ] Zenject разрешает Tank dependencies.
- [ ] В сцене один центральный GameLoop.
- [ ] Tank корректно регистрируется и unregister-ится.

## 13.2. Existing Player and Interaction

- [ ] Существующий Player Movement не сломан.
- [ ] Player Look не сломан вне станции.
- [ ] Press interaction работает.
- [ ] Hold interaction работает.
- [ ] Pickup и Drop продолжают работать.
- [ ] Interaction HUD работает вне станции.

## 13.3. Enter

- [ ] DriverStation показывает корректный prompt.
- [ ] Player занимает свободную станцию.
- [ ] Занятую станцию нельзя занять повторно.
- [ ] Player находится в DriverSeatAnchor.
- [ ] Player locomotion отключён.
- [ ] Player Look не вращает body.
- [ ] World interaction и HUD отключены.
- [ ] Camera находится в DriverCameraAnchor.
- [ ] Driving map активна.
- [ ] Нет немедленного exit от входной кнопки.

## 13.4. Tank movement

- [ ] Throttle двигает вперёд и назад.
- [ ] Steering поворачивает.
- [ ] Brake замедляет и останавливает.
- [ ] Соблюдаются forward/reverse max speed.
- [ ] Работает direction switch delay.
- [ ] Zero input не оставляет бесконечный throttle.
- [ ] Используется kinematic Rigidbody.
- [ ] `AddForce` и `AddTorque` не используются.
- [ ] Movement выполняется в fixed gameplay phase.

## 13.5. Grounding

- [ ] Tank устойчив на flat ground.
- [ ] Tank следует умеренному slope.
- [ ] Tank проходит плавное изменение высоты.
- [ ] Tank не поднимается по слишком крутому slope.
- [ ] Hull не уходит под ground.
- [ ] Ground Physics API изолирован в adapter.

## 13.6. Collision

- [ ] Tank не проходит через стену.
- [ ] Tank не проходит через крупный obstacle.
- [ ] Tank не туннелирует на максимальной configured speed.
- [ ] Поворот рядом со стеной не создаёт очевидного penetration.
- [ ] Collision code отсутствует в `TankMovement`.

## 13.7. Exit

- [ ] Player выходит в свободный DriverExitAnchor.
- [ ] Player locomotion восстанавливается.
- [ ] Player Look восстанавливается.
- [ ] Interaction и HUD восстанавливаются.
- [ ] Player Camera возвращается в CameraPivot.
- [ ] DriverStation освобождается.
- [ ] Заблокированный exit оставляет Player внутри.
- [ ] Неуспешный exit не создаёт partial state.

## 13.8. Lifecycle

- [ ] Tank можно disable/destroy без dangling GameLoop entries.
- [ ] Пустая Station корректно очищается.
- [ ] Station с Player выполняет safe recovery.
- [ ] Scene unload не оставляет permanently disabled Player state.
- [ ] Повторные enter/exit работают.

## 13.9. Architecture

- [ ] Нет `TankManager`.
- [ ] Нет монолитного `TankController`.
- [ ] Нет второго Input System.
- [ ] Нет второго GameLoop.
- [ ] Нет device polling внутри Tank Core.
- [ ] Нет Physics API внутри `TankMovement`.
- [ ] Runtime-state не хранится в ScriptableObject.
- [ ] Tank не зависит от конкретного Player instance.
- [ ] Используется существующая Interaction feature.
- [ ] DI выполняется через Zenject без runtime resolve.

---

# 14. Automated tests

## 14.1. TankMovement

Проверить:

- acceleration;
- deceleration;
- brake deceleration;
- forward max speed;
- reverse max speed;
- direction switch delay;
- zero input;
- normalized throttle;
- normalized steering;
- state remains runtime-local.

## 14.2. DriverStation

Проверить:

```text
Free → Entering
Entering → Occupied
Occupied rejects another actor
Occupied → Exiting
Exiting → Free
Cancel Entering → Free
Failed Exit → Occupied
```

## 14.3. Integration

PlayMode или manual tests:

- kinematic collision;
- ground probe;
- seat attachment;
- camera anchor switching;
- Action Map switching;
- blocked exit;
- destroy Tank with occupant.

---

# 15. Открытые решения

Не блокируют создание contracts, но должны фиксироваться перед соответствующей
реализацией:

- конкретная ground probe scheme;
- точная Hull collider shape;
- tuned maximum slope angle;
- tuned ground offset;
- поведение на краю обрыва;
- поведение при полной потере ground;
- необходимость `cast-and-slide` после первого `cast-and-stop` prototype;
- driver freelook;
- driver camera angle limits;
- camera transitions/smoothing;
- hatch animation;
- Tank audio;
- networking authority.

Stage 3 не должен молча превращать эти пункты в новые глобальные архитектурные
зависимости.

---

# 16. Критерий завершения Stage 3

Stage 3 завершён, когда стабильно работает:

```text
Player walks to Tank
        ↓
Interaction prompt
        ↓
Enter DriverStation
        ↓
Player fixed inside Tank
        ↓
Driving input context
        ↓
Arcade 3D kinematic movement
        ↓
Ground handling
        ↓
Collision handling
        ↓
Exit request
        ↓
Validated exit
        ↓
Player locomotion, look, interaction and camera restored
```

После этого можно переходить к:

```text
Stage 4 — Stations / Crew Roles
```

без переписывания `TankMovement`, motion adapters и occupancy foundation.

---

# 17. Отчёт после каждого implementation stage

После каждого этапа перечислить:

```text
Что изменено
Какие файлы добавлены
Какие файлы изменены
Какие зависимости появились
Что назначить в Inspector
Как вручную проверить
Какие automated tests запущены
Какие TBD остались
```

Не переходить к следующему этапу, пока текущий не компилируется и не проходит
свою минимальную проверку.
