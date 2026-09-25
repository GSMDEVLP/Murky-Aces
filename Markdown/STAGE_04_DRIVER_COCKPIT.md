# Murky Aces — Stage 04.1: Driver Cockpit

> План завершения водительского места перед созданием остальных Stations.
>
> Общие архитектурные правила определяет `CODEX_Murky_Aces.md`.
> Фундамент движения Tank, occupancy и enter/exit определён
> `STAGE_03_TANK_CORE.md`.
>
> Документ содержит план и текущий прогресс реализации. Изменения выполняются
> без модификации геометрии и иерархии исходной модели танка.

---

# 1. Статус

```text
In Progress

Current task:
Stage 4.1.5 / 4.1.6 — Driving Interact input and safe input-map transition
```

## 1.1. Прогресс реализации

- [x] `Stage 4.1.1` — выполнен baseline и prefab audit.
- [x] Выделены общие `CrewStationOccupancy` и `CrewStationController`.
- [x] Driver-specific управление вынесено в `DriverStationAdapter`.
- [x] Создан `StationPanelRoot`; Renderer основного экрана и material slot
      назначаются явно через Inspector.
- [x] Main Camera постоянно остаётся дочерней `Player.CameraPivot` и больше не
      переключается на `DriverCameraAnchor`.
- [x] Добавлен cockpit look с отдельными yaw/pitch limits без вращения Player
      Rigidbody.
- [x] В `Driving` Action Map добавлен `Look`; mouse и gamepad look проходят
      через общий input pipeline.
- [x] Добавлен общий `IInteractionScope` без зависимости `PlayerInteraction`
      от Unity `Transform`.
- [x] `StationPanelRoot` реализует cockpit scope, который передаётся при входе
      и очищается при успешном выходе.
- [x] Interaction остаётся активным в водительском кресле и ограничивает
      найденные цели текущим cockpit scope.
- [ ] **Текущая задача:** добавить `Interact` в `Driving` Action Map, развести
      bindings `Interact` и `ExitStation`, подавить ввод до отпускания кнопки
      после переключения Action Map.
- [ ] Добавить interaction proxy-colliders и реальные Press/Hold targets под
      `StationPanelRoot`; проверить HUD и потерю цели.
- [ ] Реализовать `DriverFrontCamera`, индивидуальный runtime `RenderTexture`
      и instance-safe вывод на Renderer экрана.
- [ ] Связать front camera/display lifecycle с occupancy DriverStation.
- [ ] Завершить failure recovery для disable/destroy Tank и занятой станции.
- [ ] Выполнить automated tests и полный manual acceptance checklist.

## 1.2. Статус этапов

| Этап | Статус | Примечание |
|---|---|---|
| `4.1.1` Baseline и prefab audit | Выполнено | Исходные references и ограничения проверены |
| `4.1.2` Reusable Station foundation | Выполнено | Occupancy, lifecycle, role adapter и panel root внедрены |
| `4.1.3` Main Camera на Player | Выполнено | Camera rig постоянно следует за `Player.CameraPivot` |
| `4.1.4` Cockpit look | Выполнено | Ограниченные yaw/pitch без вращения body |
| `4.1.5` Driving look и interaction input | В работе | `Look` готов; `Interact` является текущей задачей |
| `4.1.6` Interaction capability и scope | В работе | Scope подключён; panel targets и полный input flow ещё не проверены |
| `4.1.7` DriverFrontCamera и RenderTexture | Не начато | Следует после panel interaction |
| `4.1.8` Lifecycle и failure recovery | Не начато | Требуется camera/display cleanup |
| `4.1.9` Tests и acceptance scene | Не начато | Финальный этап Stage 4.1 |

Stage 3 — Tank Core считается завершённым базовым слоем.

Первый срез Stage 4 должен закончить DriverStation как полноценное рабочее
место внутри танка. Только после этого следует обобщать решение для
GunnerStation и CommanderStation.

---

# 2. Цель

Получить водительский cockpit, в котором Player:

- остаётся закреплённым в DriverStation;
- видит интерьер от своей камеры;
- может осматриваться внутри кабины;
- видит передний обзор Tank на экране рабочей панели;
- взаимодействует с физическими органами управления панели;
- продолжает управлять движением Tank;
- корректно выходит из DriverStation и возвращается в walking mode.

Целевой игровой поток:

```text
Player подходит к DriverStation
        ↓
Interaction выполняет вход
        ↓
Player закрепляется в DriverSeatAnchor
        ↓
Main Camera остаётся на Player.CameraPivot
        ↓
Включается cockpit look
        ↓
На экране панели отображается DriverFrontCamera
        ↓
Player управляет Tank и взаимодействует с панелью
        ↓
Player запрашивает выход
        ↓
Проверяется DriverExitAnchor
        ↓
Восстанавливаются walking look, locomotion и world interaction
```

---

# 3. Исходное состояние до Stage 4.1

## 3.1. Что существовало на baseline

- `TankRoot`;
- `TankMovement`;
- `DriverStation` и occupancy state;
- attach Player к `DriverSeatAnchor`;
- validated exit через `DriverExitAnchor`;
- переключение на `Driving` Action Map;
- переключение Main Camera на `DriverCameraAnchor`;
- отключение Player locomotion, Player Look и Interaction в station mode;
- одна основная gameplay Camera;
- модель водительской панели внутри Tank Hull.

## 3.2. Что требуется изменить концептуально

Текущая Main Camera присоединяется к `DriverCameraAnchor` и остаётся статичной.
Это не позволяет Player полноценно осматривать рабочее место и взаимодействовать
с физической панелью.

Новая модель:

```text
Main Camera
    следует за Player.CameraPivot
    показывает интерьер и панель

DriverFrontCamera
    является дочерним объектом TankRoot
    смотрит вперёд
    выводит изображение только в RenderTexture панели
```

`DriverFrontCamera` не считается второй gameplay Camera. Она не управляет
взглядом Player, не имеет `AudioListener`, не имеет тега `MainCamera` и не
выводит изображение непосредственно на экран игры.

---

# 4. Зафиксированные архитектурные решения

## 4.1. Main Camera остаётся на Player

При входе в DriverStation:

- Player body присоединяется к `DriverSeatAnchor` существующим station flow;
- Main Camera не переподключается к статичному camera anchor станции;
- Main Camera продолжает следовать за `Player.CameraPivot`;
- движение Tank автоматически переносит Player и его camera pivot вместе с
  креслом;
- положение камеры определяется Player и посадкой в кресле, а не отдельной
  статичной gameplay-камерой Tank.

Существующий `DriverCameraAnchor` не удаляется до завершения миграции и проверки
prefab-ссылок. После миграции он может использоваться как reference начального
направления взгляда либо быть удалён отдельным безопасным изменением.

## 4.2. Cockpit look отделяется от walking look

Текущий Player Look одновременно:

- изменяет pitch `Player.CameraPivot`;
- изменяет yaw физического тела Player.

Такое поведение нельзя напрямую включать в DriverStation, потому что Player body
зафиксирован в кресле и не должен вращать Rigidbody относительно Tank.

Должны существовать два режима одной системы взгляда:

```text
Walking Look
    yaw вращает Player body
    pitch вращает Player.CameraPivot

Cockpit Look
    Player body остаётся зафиксированным
    yaw и pitch вращают только camera pivot / cockpit view pivot
```

Cockpit look должен иметь настраиваемые ограничения:

- минимальный и максимальный yaw;
- минимальный и максимальный pitch;
- начальное направление при входе;
- сброс или восстановление ориентации при выходе.

Режим определяется текущими station capabilities, а не проверками
`if (_isDriving)` внутри Player Look.

## 4.3. Отдельная передняя камера Tank

Создаётся техническая camera:

```text
DriverFrontCamera
```

Она должна:

- быть дочерней `TankRoot` или отдельного логического `Sensors` root;
- иметь явно настроенные position и rotation переднего обзора;
- выводить изображение в `RenderTexture`;
- не иметь `AudioListener`;
- не иметь тега `MainCamera`;
- не участвовать в переключении `GameplayCameraRig`;
- не изменять gameplay-state;
- не рендерить Player, интерьер и собственный экран панели;
- корректно включаться и выключаться вместе с водительским cockpit lifecycle.

Первый вариант поведения:

```text
DriverStation Free       → DriverFrontCamera disabled
DriverStation Occupied   → DriverFrontCamera enabled
Tank disabled/destroyed  → DriverFrontCamera disabled and cleaned up
```

Если RenderTexture создаётся в runtime, каждый экземпляр Tank должен владеть
своим экземпляром текстуры. Несколько Tank не должны записывать изображение в
одну общую runtime-текстуру.

## 4.4. Экран панели

Главный экран водительской панели отображает RenderTexture передней камеры.

Так как экран является частью 3D-модели Tank, предпочтительный presentation
слой:

```text
MeshRenderer / material property
        ↓
RenderTexture from DriverFrontCamera
```

World Space Canvas или `RawImage` не вводятся без отдельной необходимости.

Назначение RenderTexture не должно изменять общий material asset сразу для всех
экземпляров Tank. Нужна instance-safe привязка материала или его свойства.

Первая реализация использует только один основной передний видеоканал. Radar,
GPS, Night Vision и другие экраны относятся к следующим этапам.

## 4.5. DriverPanelRoot

Для панели создаётся отдельный логический объект:

```text
DriverPanelRoot
```

Он не требует изменения или переноса существующей геометрии модели.

`DriverPanelRoot` должен предоставлять явные Inspector-ссылки на:

- Renderer экрана передней камеры;
- интерактивные кнопки;
- рычаги;
- переключатели;
- interaction colliders;
- дополнительные anchors панели, если они потребуются.

Runtime-поиск через имена объектов, `Transform.Find` или обход всей модели Tank
не использовать.

## 4.6. Interaction остаётся доступным в кресле

В текущем station mode Interaction отключается вместе с locomotion и Player
Look. Для рабочего cockpit это поведение меняется.

Capability profile водителя:

```text
Locomotion             Disabled
Player body rotation   Disabled
Cockpit look           Enabled
Panel interaction      Enabled
Driving input          Enabled
Station exit           Enabled
```

Player должен использовать существующую Interaction feature:

- `InteractionContext`;
- `IInteractionTargetFinder`;
- `IInteractable`;
- Press / Hold lifecycle;
- существующий Interaction HUD.

Отдельную систему взаимодействия для панели создавать нельзя.

Во время нахождения в DriverStation поиск целей должен быть ограничен
допустимыми cockpit-interactions. Player не должен случайно активировать объект
снаружи Tank через стенку или окно.

Допустимое направление:

```text
DriverStation capability / interaction scope
        ↓
разрешён DriverPanelRoot и явно указанные cockpit targets
```

## 4.7. Driving input context расширяется

Во время управления должны одновременно обрабатываться:

```text
Throttle
Steering
Brake
Look
Interact
ExitStation
```

`Driving` context не должен блокировать look и interaction.

Все действия продолжают проходить через существующее направление данных:

```text
InputService
        ↓
PlayerInputSystem
        ↓
Intent buffers
        ↓
Player / DriverStation gameplay
```

Прямой polling `Keyboard.current`, `Mouse.current` или `Gamepad.current` в
cockpit-коде запрещён.

Нажатие, которым Player завершил вход, не должно превращаться в немедленный
выход или активацию элемента панели.

## 4.8. Driver Cockpit — первый экземпляр общей Station-системы

Реализация Driver Cockpit не должна становиться отдельным набором компонентов,
который позднее копируется для GunnerStation и CommanderStation.

Общий reusable foundation должен быть выделен сразу и впервые использоваться
DriverStation:

```text
CrewStation
├── StationOccupancy
├── StationLifecycle
├── StationView / anchors
├── StationCapabilityProfile
├── StationLook
├── StationInteractionScope
├── StationPanelRoot
└── StationDisplayFeed
```

Общими для всех управляемых мест являются:

- правила occupancy;
- enter, exit, cancel и rollback;
- attach/detach Player;
- переключение Player capabilities;
- station look и ограничения углов;
- общие действия `Look`, `Interact` и `ExitStation`;
- ограничение interaction targets рабочей областью станции;
- подключение технической camera к RenderTexture;
- вывод RenderTexture на экран панели;
- включение, выключение и cleanup camera resources;
- восстановление Player после disable/destroy станции или Tank.

Ролевыми должны оставаться только команды и адаптеры конкретного рабочего
места:

```text
DriverStationAdapter
    → Throttle / Steering / Brake
    → TankMovement

GunnerStationAdapter
    → Aim / Fire requests
    → будущие Turret / Gun systems

CommanderStationAdapter
    → observation / module requests
    → будущие Tank Modules
```

Передняя камера водителя является конфигурацией общего `StationDisplayFeed`, а
не уникальной системой вывода изображения. Позднее GunnerStation и
CommanderStation могут передать этому же механизму другой camera source,
RenderTexture и screen Renderer.

`DriverPanelRoot` является конкретным экземпляром общей ответственности
`StationPanelRoot`. Driver-specific название допустимо на prefab-объекте, но
работа с panel references, interaction scope и display feeds не должна быть
зашита в отдельный driver-only алгоритм.

Reusable foundation строится композицией. Не создавать иерархию наследования с
копиями поведения для `DriverStation`, `GunnerStation` и `CommanderStation`.
Общие классы должны сразу использоваться DriverStation, а не оставаться пустыми
abstractions на будущее.

Обязательный архитектурный критерий:

```text
Добавление GunnerStation или CommanderStation
не требует копирования station lifecycle,
occupancy, station look, interaction scope
или display / RenderTexture pipeline.

Для новой станции добавляются только:
- prefab references и anchors;
- capability profile;
- input bindings конкретной роли;
- role adapter к соответствующей системе Tank;
- конфигурация camera source и экранов.
```

---

# 5. Предлагаемая иерархия prefab

```text
TankRoot
│
├── Collision
│   └── HullCollider
│
├── HullAnchor
│   └── Existing Tank Hull Prefab
│       └── Existing Driver Panel Geometry
│
├── Stations
│   └── DriverStation
│       ├── InteractionPoint
│       ├── DriverSeatAnchor
│       ├── DriverExitAnchor
│       └── DriverPanelRoot
│
└── Sensors
    └── DriverFrontCamera
```

`DriverPanelRoot` является логической точкой сборки ссылок. Существующая panel
geometry может оставаться внутри Hull Prefab.

---

# 6. Границы ответственности

## GameplayCameraRig

Отвечает только за основную камеру Player:

- walking view;
- cockpit view;
- сохранение единственной Main Camera.

Не отвечает за RenderTexture панели.

## Player station mode

Отвечает за:

- attach/detach Player;
- выбор capability profile;
- включение cockpit look;
- восстановление walking capabilities;
- rollback при неуспешном переходе.

Не управляет материалом экрана и технической front camera напрямую.

## DriverStation

Отвечает за:

- occupancy;
- вход и выход;
- передачу driving intent в TankMovement;
- активацию водительского cockpit lifecycle.

Не реализует camera rendering и UI-материалы самостоятельно.

## Driver cockpit presentation

Отвечает за:

- техническую front camera;
- RenderTexture;
- привязку изображения к экрану панели;
- визуальное включение и выключение экрана.

Не изменяет Tank movement или Player state.

## DriverPanelRoot

Отвечает за:

- явные ссылки на элементы панели;
- область допустимых interaction targets;
- presentation references панели.

Не становится монолитным `DriverPanelController`, который выполняет все функции
Tank.

---

# 7. Не входит в этот срез

Не реализовывать:

- GunnerStation;
- CommanderStation;
- Loader workflow;
- turret rotation;
- gun elevation;
- firing;
- ammo loading;
- inventory;
- Radar;
- GPS;
- Radio;
- Night Vision;
- полноценные приборы Engine и Fuel;
- damage state панели;
- networking и authority;
- отдельные камеры для каждого будущего экрана;
- изменение геометрии или материалов исходной модели без необходимости;
- новая общая архитектура Tank Modules.

Этот срез должен создать шаблон рабочего места, но не реализовывать функции
следующих этапов раньше времени.

---

# 8. Порядок реализации

## Stage 4.1.1 — Baseline и prefab audit

Проверить и зафиксировать:

- рабочий цикл `Enter → Drive → Exit`;
- текущую привязку Main Camera к `DriverCameraAnchor`;
- положение `Player.CameraPivot` после attach к креслу;
- Renderer существующего экрана панели;
- material slots экрана;
- расположение элементов панели и их colliders;
- слои Player, Tank interior, Tank exterior и interactables;
- отсутствие второй gameplay Camera и второго `AudioListener`.

Готово, если текущий baseline воспроизводим и известны все prefab references,
которые потребуются следующему шагу.

## Stage 4.1.2 — Reusable Station foundation и DriverPanelRoot

Сначала выделить общие ответственности станции, которые DriverStation будет
использовать как первый реальный экземпляр:

- station occupancy и lifecycle;
- station capability profile;
- station look;
- station interaction scope;
- station panel references;
- station display feed lifecycle.

Затем подготовить Driver-specific logical root и явные references без изменения
модели:

- screen Renderer;
- screen material slot;
- cockpit interaction roots;
- optional view anchors;
- front camera reference.

Готово, если runtime-код не должен искать элементы панели по имени, а общие
station-компоненты реально используются DriverStation и не дублируют его
lifecycle.

## Stage 4.1.3 — Main Camera остаётся на Player

Изменить station camera flow так, чтобы:

- Main Camera продолжала использовать `Player.CameraPivot`;
- attach Player к seat продолжал перемещать камеру вместе с Tank;
- вход больше не делал Main Camera дочерней статичного
  `DriverCameraAnchor`;
- rollback входа возвращал исходное camera state;
- выход не создавал скачка или потерянной camera reference.

Готово, если Player сидит в DriverStation и видит интерьер своей основной
камерой.

## Stage 4.1.4 — Cockpit look

Добавить station-specific режим взгляда:

- yaw без вращения Player Rigidbody;
- pitch без вращения Player body;
- ограничения углов;
- начальная ориентация;
- очистка накопленного look intent при входе и выходе;
- восстановление обычного walking look после выхода.

Готово, если Player может осмотреть панель и кабину, но его body остаётся
зафиксированным в кресле.

## Stage 4.1.5 — Driving look и interaction input

Расширить driving input context:

- Look;
- Interact pressed;
- Interact held;
- Interact released;
- существующий ExitStation.

Проверить клавиатуру/мышь и gamepad bindings.

Готово, если cockpit look и Interaction получают intents через общий Input
pipeline, не нарушая driving controls.

## Stage 4.1.6 — Interaction capability и scope

Сохранить Interaction активным в водительском кресле и ограничить допустимые
цели панелью:

- Press работает;
- Hold работает;
- HUD показывает prompt и progress;
- потеря цели отменяет Hold;
- внешний объект за пределами cockpit scope недоступен;
- ExitStation не конфликтует с Interact.

Готово, если Player может использовать панель и не может взаимодействовать с
недопустимыми объектами через корпус Tank.

## Stage 4.1.7 — DriverFrontCamera и RenderTexture

Подготовить технический camera pipeline:

- camera находится спереди Tank;
- camera следует за Tank;
- target — RenderTexture;
- culling mask исключает интерьер, Player и экран панели;
- отсутствуют `AudioListener` и `MainCamera` tag;
- изображение имеет правильное соотношение сторон;
- экран панели получает изображение instance-safe способом;
- camera отключается при свободной DriverStation.

Готово, если экран панели стабильно показывает передний обзор во время движения
Tank.

## Stage 4.1.8 — Lifecycle и failure recovery

Проверить:

- failed enter;
- blocked exit;
- successful exit;
- disable DriverStation;
- disable Tank;
- destroy Tank без водителя;
- destroy Tank с водителем;
- повторные enter/exit;
- повторный cleanup;
- восстановление Main Camera, walking look и Interaction;
- выключение или освобождение front camera resources.

Главный invariant:

```text
Исчезновение или disable Tank не оставляет Player
без Main Camera, walking look или Interaction.
```

## Stage 4.1.9 — Tests и acceptance scene

Подготовить автоматические и ручные проверки из разделов 10 и 11.

Не переходить к GunnerStation и CommanderStation, пока водительский cockpit не
проходит полный acceptance flow.

---

# 9. Предполагаемые области изменений

До реализации проверить необходимость изменений в следующих областях:

```text
Player/
├── PlayerStationController
├── PlayerStationCapabilities
├── Player Look mode
├── PlayerInputContext
└── Player.CameraPivot

Input/
├── InputActionAsset
├── InputService
├── PlayerInputSystem
└── intent buffers

Tank/
├── DriverStation
├── DriverStationView
├── DriverPanelRoot / DriverPanelView
├── DriverCockpit presentation
├── DriverFrontCamera
└── TankInstaller

Interaction/
├── target scope/filter
└── existing HUD and interaction lifecycle
```

Конкретные имена новых классов фиксируются при реализации после проверки
существующих ответственностей. Не создавать пустые abstractions заранее.

---

# 10. Automated tests

## 10.1. Capability switching

Проверить:

- entering DriverStation отключает locomotion;
- entering DriverStation не отключает panel interaction;
- entering DriverStation включает cockpit look;
- Player body rotation остаётся отключённым;
- exit восстанавливает исходные capabilities;
- failed exit сохраняет cockpit capabilities;
- failed enter полностью откатывает изменения.

## 10.2. Look mode

Проверить:

- cockpit yaw ограничен;
- cockpit pitch ограничен;
- Player Rigidbody не вращается от cockpit look;
- look intent очищается при смене mode;
- walking look восстанавливается после выхода.

## 10.3. Interaction scope

Проверить:

- цель внутри DriverPanelRoot доступна;
- цель вне разрешённого cockpit scope недоступна;
- Press выполняется;
- Hold выполняется;
- Hold отменяется при потере цели;
- prompt соответствует текущей цели.

## 10.4. Camera lifecycle

Проверить:

- Main Camera остаётся связанной с Player;
- front camera не становится Main Camera;
- существует только один активный `AudioListener`;
- front camera включается при занятии DriverStation;
- front camera выключается после выхода;
- cleanup безопасен при disable/destroy.

---

# 11. Manual acceptance checklist

## 11.1. Enter

- [ ] Player входит через существующую DriverStation interaction.
- [ ] Player фиксируется в `DriverSeatAnchor`.
- [ ] Tank не получает input от кнопки входа до её отпускания.
- [ ] Main Camera остаётся на Player view hierarchy.
- [ ] Camera не переключается на статичный `DriverCameraAnchor`.

## 11.2. Cockpit view

- [ ] Player видит интерьер и водительскую панель.
- [ ] Player может смотреть влево и вправо.
- [ ] Player может смотреть вверх и вниз.
- [ ] Углы cockpit look ограничены.
- [ ] Player body и Tank не вращаются от look input.
- [ ] Camera не пересекает очевидным образом геометрию кабины.

## 11.3. Panel screen

- [ ] Главный экран показывает передний обзор Tank.
- [ ] Изображение движется вместе с Tank.
- [ ] Изображение не содержит Player и интерьер.
- [ ] Экран не создаёт рекурсивный feedback самого себя.
- [ ] Соотношение сторон соответствует геометрии экрана.
- [ ] Нет второй Main Camera.
- [ ] Нет второго `AudioListener`.

## 11.4. Panel interaction

- [ ] На элементах панели появляется корректный prompt.
- [ ] Press interaction работает.
- [ ] Hold interaction работает.
- [ ] Hold progress отображается.
- [ ] Player не может взаимодействовать с внешней целью через корпус Tank.
- [ ] Interaction не блокирует Throttle, Steering и Brake.

## 11.5. Driving

- [ ] Throttle работает одновременно с cockpit look.
- [ ] Steering работает одновременно с cockpit look.
- [ ] Brake работает одновременно с cockpit look.
- [ ] Поведение `TankMovement` не изменилось.
- [ ] Collision и ground handling не получили regression.

## 11.6. Exit

- [ ] ExitStation корректно запрашивает выход.
- [ ] Blocked exit оставляет Player в cockpit mode.
- [ ] Успешный exit возвращает Player в walking mode.
- [ ] Walking locomotion восстанавливается.
- [ ] Walking look восстанавливается.
- [ ] World interaction восстанавливается.
- [ ] Main Camera продолжает следовать за Player.
- [ ] DriverFrontCamera и экран переходят в неактивное состояние.

## 11.7. Lifecycle

- [ ] Повторный цикл `Enter → Drive → Exit` работает минимум три раза.
- [ ] Disable DriverStation не оставляет Player в partial state.
- [ ] Disable Tank не оставляет Player без камеры или управления.
- [ ] Destroy Tank безопасно обрабатывает занятую DriverStation.
- [ ] Scene unload не оставляет runtime camera resources.

## 11.8. Architecture reuse

- [ ] DriverStation использует общий station occupancy и lifecycle.
- [ ] Station look не содержит driver-specific driving logic.
- [ ] Station interaction scope не зависит от `TankMovement`.
- [ ] RenderTexture pipeline не зависит от роли Driver.
- [ ] `DriverPanelRoot` использует общую ответственность Station panel.
- [ ] Common station code не содержит проверок `if (_isDriver)`.
- [ ] Для GunnerStation не потребуется копировать DriverStation controller.
- [ ] Для CommanderStation не потребуется копировать camera/display lifecycle.
- [ ] Ролевой код водителя ограничен driving intents и адаптером к
      `TankMovement`.

---

# 12. Критерий завершения

Stage 4.1 — Driver Cockpit завершён, когда стабильно работает:

```text
Player занимает DriverStation
        ↓
остаётся закреплённым в кресле
        ↓
Main Camera остаётся на Player
        ↓
Player свободно осматривает интерьер в заданных пределах
        ↓
экран панели показывает передний обзор Tank
        ↓
Player взаимодействует с органами управления панели
        ↓
Player продолжает управлять Tank
        ↓
Player выходит
        ↓
walking locomotion, look, interaction и camera полностью восстановлены
```

После выполнения этого критерия cockpit camera, panel references, interaction
scope и capability switching можно использовать как проверенный шаблон для
GunnerStation и CommanderStation.

Дополнительный обязательный критерий архитектуры:

```text
Новая управляемая Station создаётся конфигурацией общей Station-системы
и отдельным role adapter.

Копирование Driver lifecycle, StationLook, InteractionScope
или StationDisplayFeed для новой роли запрещено.
```

---

# 13. Отчёт после реализации

После завершения перечислить:

```text
Что изменено
Какие файлы добавлены
Какие файлы изменены
Какие prefab objects добавлены
Какие ссылки назначены в Inspector
Как устроены Main Camera и DriverFrontCamera
Как RenderTexture назначается экрану панели
Как ограничен cockpit interaction scope
Как вручную проверить полный flow
Какие automated tests запущены
Какие TBD остались
```
