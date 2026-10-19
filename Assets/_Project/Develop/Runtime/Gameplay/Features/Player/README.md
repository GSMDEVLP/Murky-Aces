# Player — структура папок

Пользователь 2026-10-19 явно выбрал **пять общих архитектурных слоёв в корне Player**. Сохранять эту схему при последующих добавлениях.

```text
Player/
├── Abstractions/
├── Application/
├── Domain/
├── Infrastructure/
│   ├── Interior/
│   ├── Movement/
│   ├── Phases/
│   └── Station/
├── Presentation/
│   └── GPS/
└── Prefabs/
```

Prefabs содержит ресурсы и не является дополнительным слоем исходного кода.

| Папка | Что находится здесь |
|---|---|
| Abstractions | Восемь контрактов: контекст ввода, физическое прикрепление к станции и переходы в интерьер, режим станционного ввода, сессия GPS, камера GPS, экран GPS и владелец сессии GPS |
| Application | PlayerStationController, PlayerTankInteriorController, PlayerGpsSession и режимы ввода водителя/наводчика/командира |
| Domain | PlayerIntentBuffer, DrivingIntentBuffer, GunnerIntentBuffer, CommanderIntentBuffer, GpsIntentBuffer |
| Infrastructure | PlayerFacade, PlayerSpawner, PlayerRegistry, InputInstaller, InteractionInstaller, PlayerInteractionActor, PlayerInputContext, HeldItemSlot и Unity-адаптеры в подпапках |
| Infrastructure/Movement | PlayerMovement и PlayerLook |
| Infrastructure/Phases | InputPhase, GameplayPhase и PresentationPhase — подключение конкретных систем игрока к GameLoop |
| Infrastructure/Station | UnityPlayerStationBody, BodySnapshot, StationCapabilityProfile и PlayerStationCapabilities |
| Infrastructure/Interior | PlayerTankInteriorBody |
| Presentation/GPS | PlayerGpsCameraView |
| Prefabs | Префаб игрока |

StationCapabilityProfile — сериализуемые настройки станции, поэтому он вынесен из папки интерфейсов. PlayerRegistry хранит конкретные PlayerFacade, а PlayerStationCapabilities применяет настройки к Unity-компонентам движения и обзора; их реализации размещены в Infrastructure.

## Перенос 2026-10-19

Восемь интерфейсов перенесены из Application/Abstractions в Abstractions. StationCapabilityProfile размещён в Infrastructure/Station. Камера GPS перенесена в Presentation/GPS, PlayerRegistry — в Infrastructure, PlayerStationCapabilities — в Infrastructure/Station. Опустевшая Infrastructure/GPS удалена.

При реорганизации папок содержимое C#-файлов, имена классов, namespace и GUID были сохранены. Затем 2026-10-19 пользователь отдельно разрешил актуализировать namespace: теперь они повторяют физические пути относительно Assets, включая Player.Abstractions. Связанные using обновлены; GUID и игровая логика сохранены.

Перенос файлов не устраняет все зависимости слоёв: в частности, PlayerIntentBuffer и GpsIntentBuffer продолжают использовать UnityEngine.Vector2, а прикладные контроллеры и фазы пока ссылаются на некоторые конкретные реализации. Изменение контрактов, логики и состава данных требует отдельного шага.

Проверка после переноса: все 38 скриптов сохранили C#-содержимое и GUID; пути соответствуют выбранной карте слоёв, пустых папок нет. Содержимое всех 19 сериализованных ресурсов под Assets/_Project (`.prefab`, `.unity`, `.asset`) совпадает с состоянием до переноса. После восстановления соединения с редактором выполнены импорт и запрос компиляции: `isCompiling = false`, `scriptCompilationFailed = false`, в Console нет Error. На префабе игрока нет Missing Script, все шесть проверенных ссылок InputInstaller назначены.
