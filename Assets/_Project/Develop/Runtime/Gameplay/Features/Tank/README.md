# Tank — расположение систем

Сначала выбираем систему, затем нужный слой внутри неё. Не распределять все системы танка между общими `Tank/Domain`, `Tank/Application` и `Tank/Presentation`.

| Где искать | Содержание |
|---|---|
| `Movement` | Движение корпуса, команды движения, состояние и физический адаптер |
| `Turret` | Наведение башни, её состояние, Unity rig и камера наводчика |
| `Weapon` | Заряжание, состояние каморы, выстрел и Unity-представление оружия |
| `Stations` | Занятость мест экипажа, вход/выход, адаптеры ролей и экраны |
| `Exterior` | Детали внешнего корпуса, ячейки, совместимость и предпросмотр |
| `Fuel` | Топливо, конфигурация и отображение |
| `Interior` | Infrastructure содержит проход через люк и общую цель с действиями крышки; ITankInteriorOccupant находится в Interior/Abstraction |
| `Ammunition` | Unity-описание снаряда и переносимый предмет снаряда |
| `Modules/GPS` | Карта, маршрут, команды GPS и экран |
| `Modules/Radar` | Контакты, сканирование, команды радара и экран |
| `Modules/Lights` | Состояние, команды и отображение освещения |
| `Modules/NightVision` | Состояние, команды и отображение ночного видения |
| `Core` | Общий контракт подключения систем `ITankRuntimeModule` и lifecycle `TankRoot` |
| `Prefabs` | Префабы танка, предметов и представлений |

## Слои внутри системы

- `Abstractions` — контракты и порты конкретной системы.
- `Domain` — состояния и предметные правила без Unity-компонентов и DI.
- `Application` — команды, сценарии и координация работы системы.
- `Infrastructure` — Unity Physics, authoring, interactable-адаптеры, installers и runtime-регистрация. Unity ScriptableObject-конфиги находятся здесь же.
- `Presentation` — View, presenter, камеры и отображение.

Создавать только используемые слои. Не создавать пять пустых папок под каждую систему или папку под отдельную задачу. Небольшая однослойная система `Ammunition` пока остаётся плоской.

Например, `Exterior` сейчас содержит:

```text
Exterior/
├── Domain/
│   ├── ExteriorAttachmentState.cs
│   └── ExteriorMountSlotState.cs
├── Infrastructure/
│   ├── ExteriorAttachmentAuthoring.cs
│   ├── ExteriorAttachmentConfig.cs
│   └── ExteriorMountSlotAuthoring.cs
└── Presentation/
    ├── ExteriorPlacementPreviewPresenter.cs
    └── ExteriorPlacementPreviewView.cs
```

`Exterior/Application` содержит команды установки, снятия и сварки. `Exterior/Abstractions` содержит порт данных preview для обычного крепления и общей цели люка.

Все interactable-компоненты используют единую систему целей и действий: [Interaction/README.md](../Interaction/README.md). Самостоятельного ввода и UI-меню внутри Tank нет.

Общий код танка сгруппирован отдельно:

```text
Core/
├── Abstractions/
│   └── ITankRuntimeModule.cs
└── Infrastructure/
    └── TankRoot.cs
```

В Core помещать только код общей сборки и lifecycle танка. Правила движения, оружия, станций и внешних деталей остаются в соответствующих системах. Старый TankDebugInputSource пользователь удалил как ненужный.

## Перенос 2026-10-19

При первом переносе по разрешению пользователя перемещены 46 из 110 существующих скриптов средствами Unity AssetDatabase. Затем по дополнительному решению пользователя общий контракт и TankRoot собраны в Core. Отладочный TankDebugInputSource пользователь удалил как ненужный; восстанавливать его не требуется. Содержимое C#-файлов, имена типов, namespace и GUID перемещённых скриптов сохранены; префабы, сцены и asset-конфиги не перемещались и не пересохранялись. Существующие namespace могут отличаться от физических путей — их переименование не входит в этот перенос.

### Актуализация namespace 2026-10-19

После переноса пользователь отдельно разрешил привести namespace к текущим путям. Объявления namespace и связанные using обновлены; GUID и игровая логика сохранены. Namespace повторяет путь папки относительно Assets.

После дополнительного переноса контракт IStationOccupant находится в Stations/Abstractions, а ITankInteriorOccupant — в Interior/Abstraction. Их namespace и обращения из зависимых скриптов обновлены согласно фактическим путям.

Обе части partial-класса `TurretAimRuntime` теперь находятся в `Turret/Application`: `TurretAimRuntime.cs` и `TurretPresentation.cs`. Они используют общий namespace `Tank.Turret.Application`; перенос второй части выполнен с сохранением `.meta` и GUID. Сам класс на новые типы не разделялся.

Перемещать существующие Unity-ассеты вместе с их `.meta`, предпочтительно через Project window или AssetDatabase. Путь к файлу не должен использоваться вместо сохранённого GUID для связи компонента с префабом или сценой.
