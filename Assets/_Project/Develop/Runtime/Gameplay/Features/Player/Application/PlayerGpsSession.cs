using System;
using Zenject;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.GPS;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerGpsSession
    {
        private readonly IPlayerInputContext _inputContext;
        private readonly PlayerIntentBuffer _playerIntent;
        private readonly DrivingIntentBuffer _drivingIntent;
        private readonly CommanderIntentBuffer _commanderIntent;
        private readonly GpsIntentBuffer _gpsIntent;
        private readonly PlayerGpsCameraView _camera;
        private readonly ICrewLocationReader _location;
        private readonly LazyInject<PlayerInteraction> _interaction;

        private GpsScreenInteractable _screen;
        private IInteractionActor _actor;

        private InputMapId _returnMap;
        private long _locationRevision;
        private bool _interactionWasEnabled;
        private bool _isClosing;
        private Vector2 _cursorUv = new Vector2(0.5f, 0.5f);

        public bool IsActive { get; private set; }

        public bool IsReady =>
            IsActive &&
            !_isClosing &&
            _camera != null &&
            _camera.IsFocused;

        public PlayerGpsSession(
            IPlayerInputContext inputContext,
            PlayerIntentBuffer playerIntent,
            DrivingIntentBuffer drivingIntent,
            CommanderIntentBuffer commanderIntent,
            PlayerGpsCameraView camera,
            ICrewLocationReader location,
            LazyInject<PlayerInteraction> interaction,
            GpsIntentBuffer gpsIntent)
        {
            _inputContext = inputContext ??
                throw new ArgumentNullException(nameof(inputContext));

            _playerIntent = playerIntent ??
                throw new ArgumentNullException(nameof(playerIntent));

            _drivingIntent = drivingIntent ??
                throw new ArgumentNullException(nameof(drivingIntent));

            _commanderIntent = commanderIntent ??
                throw new ArgumentNullException(nameof(commanderIntent));

            _camera = camera ??
                throw new ArgumentNullException(nameof(camera));

            _location = location ??
                throw new ArgumentNullException(nameof(location));

            _interaction = interaction ??
                throw new ArgumentNullException(nameof(interaction));

            _gpsIntent = gpsIntent ??
                throw new ArgumentNullException(nameof(gpsIntent));
        }

        public bool CanBegin(
            GpsScreenInteractable screen,
            IInteractionActor actor)
        {
            if (IsActive ||
                screen == null ||
                !screen.CanUse(actor) ||
                _camera == null ||
                !_camera.isActiveAndEnabled ||
                _camera.IsActive ||
                _location.Current.Kind != CrewLocationKind.Station ||
                _location.Current.StationRole != screen.Role)
            {
                return false;
            }

            InputMapId expectedMap = screen.Role == CrewRoleId.Driver
                ? InputMapId.Driving
                : InputMapId.Commander;

            return _inputContext.CurrentMap == expectedMap;
        }

        public bool TryBegin(GpsScreenInteractable screen, IInteractionActor actor)
        {
            if (!CanBegin(screen, actor))
                return false;

            PlayerInteraction interaction = _interaction.Value;

            if (!interaction.IsGameplayEnabled)
                return false;

            InputMapId returnMap = _inputContext.CurrentMap;

            if (!_camera.TryFocus(screen.ViewAnchor))
                return false;

            if (!_inputContext.TryUseMap(InputMapId.GPS))
            {
                _camera.ForceRestore();
                return false;
            }

            _screen = screen;
            _actor = actor;
            _returnMap = returnMap;
            _locationRevision = _location.Revision;
            _interactionWasEnabled = interaction.IsGameplayEnabled;
            _isClosing = false;
            IsActive = true;

            screen.AttachSession(this);
            _cursorUv = new Vector2(0.5f, 0.5f);
            screen.HideCursor();
            interaction.SetGameplayEnabled(false);
            ResetButtons();

            return true;
        }

        public void Tick(float deltaTime)
        {
            bool selectionRequested = _gpsIntent.ConsumeSelectionRequest();
            float zoom = _gpsIntent.ConsumeZoom();
            Vector2 cursorDelta = _gpsIntent.ConsumeCursorDelta();

            if (!IsActive)
                return;

            if (_location.Revision != _locationRevision ||
                _screen == null ||
                !_screen.CanUse(_actor) ||
                _camera == null ||
                !_camera.isActiveAndEnabled ||
                _inputContext.CurrentMap != InputMapId.GPS)
            {
                TryForceClose();
                return;
            }

            if (_isClosing)
            {
                if (!_camera.IsActive)
                    TryFinish();

                return;
            }

            if (!_camera.IsActive)
            {
                TryForceClose();
                return;
            }

            if (_playerIntent.InteractPressedThisFrame &&
                _camera.TryReturn())
            {
                _screen.HideCursor();
                _isClosing = true;
                ResetButtons();
                return;
            }

            if (!IsReady)
                return;

            if (zoom != 0f)
                _screen.TryZoom(_actor, zoom);

            if (_screen.Role == CrewRoleId.CommanderLoader)
            {
                float sensitivity = _screen.CursorSensitivity;
                float aspect = _screen.ScreenAspect;

                _cursorUv = new Vector2(
                    Mathf.Clamp01(
                        _cursorUv.x +
                        cursorDelta.x * sensitivity / aspect),
                    Mathf.Clamp01(
                        _cursorUv.y +
                        cursorDelta.y * sensitivity));

                _screen.TrySetCursor(_actor, _cursorUv);
                if (selectionRequested)
                    _screen.TrySelectDestination(_actor, _cursorUv);
            }
        }
        public bool TryForceClose()
        {
            if (!IsActive)
                return true;

            if (_camera != null)
                _camera.ForceRestore();

            return TryFinish();
        }

        private bool TryFinish()
        {
            if (_screen != null)
                _screen.HideCursor();
            bool sameLocation =
                _location.Revision == _locationRevision;

            if (sameLocation)
            {
                if (!_inputContext.TryUseMap(_returnMap))
                    return false;

                ResetButtons();

                _interaction.Value.SetGameplayEnabled(
                    _interactionWasEnabled);
            }

            if (_screen != null)
                _screen.DetachSession(this);

            _screen = null;
            _actor = null;
            _isClosing = false;
            IsActive = false;

            return true;
        }

        private void ResetButtons()
        {
            _gpsIntent.Clear();
            _playerIntent.ClearLook();
            _playerIntent.SuppressInteractionUntilReleased();

            _drivingIntent.Clear();
            _drivingIntent.SuppressExitUntilReleased();

            _commanderIntent.Clear();
            _commanderIntent.SuppressExitUntilReleased();
        }
    }
}