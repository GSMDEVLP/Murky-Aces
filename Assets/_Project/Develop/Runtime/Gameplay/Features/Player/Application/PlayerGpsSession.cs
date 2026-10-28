using System;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Abstraction;
using _Project.Develop.Runtime.Gameplay.Features.Player.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerGpsSession : IPlayerGpsSession
    {
        private readonly IPlayerInputContext _inputContext;
        private readonly PlayerIntentBuffer _playerIntent;
        private readonly DrivingIntentBuffer _drivingIntent;
        private readonly CommanderIntentBuffer _commanderIntent;
        private readonly PlayerGpsInputHandler _gpsInput;
        private readonly IPlayerGpsCamera  _camera;
        private readonly ICrewLocationReader _location;
        private readonly InteractionControl _interactionControl;

        private IGpsScreen  _screen;
        private IInteractionActor _actor;

        private InputMapId _returnMap;
        private long _locationRevision;
        private bool _interactionWasEnabled;
        private bool _isClosing;

        public bool IsActive { get; private set; }

        public bool IsReady =>
            IsActive &&
            !_isClosing &&
            _camera != null &&
            _camera.IsAvailable &&
            _camera.IsFocused;

        public PlayerGpsSession(
            IPlayerInputContext inputContext,
            PlayerIntentBuffer playerIntent,
            DrivingIntentBuffer drivingIntent,
            CommanderIntentBuffer commanderIntent,
            IPlayerGpsCamera camera,
            ICrewLocationReader location,
            InteractionControl interactionControl,
            PlayerGpsInputHandler gpsInput)
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

            _interactionControl = interactionControl ??
                throw new ArgumentNullException(nameof(interactionControl));

            _gpsInput = gpsInput ??
                throw new ArgumentNullException(nameof(gpsInput));
        }

        public bool CanBegin(
            IGpsScreen screen,
            IInteractionActor actor)
        {
            if (IsActive ||
                screen == null ||
                !screen.CanUse(actor) ||
                _camera == null ||
                !_camera.IsAvailable ||
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

        public bool TryBegin(IGpsScreen screen, IInteractionActor actor)
        {
            if (!CanBegin(screen, actor))
                return false;

            if (!_interactionControl.IsGameplayEnabled)
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
            _interactionWasEnabled = _interactionControl.IsGameplayEnabled;
            _isClosing = false;
            IsActive = true;

            screen.AttachSession(this);
            _gpsInput.ResetCursor();
            screen.HideCursor();
            _interactionControl.SetGameplayEnabled(false);
            ResetButtons();

            return true;
        }

        public void Tick(float deltaTime)
        {
            GpsIntentSnapshot input = _gpsInput.ConsumeInput();

            if (!IsActive)
                return;

            if (_location.Revision != _locationRevision ||
                _screen == null ||
                !_screen.CanUse(_actor) ||
                _camera == null ||
                !_camera.IsAvailable ||
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

            _gpsInput.HandleInput(_screen, _actor, input);
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

                _interactionControl.SetGameplayEnabled(
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
            _gpsInput.ClearInput();
            _playerIntent.ClearLook();
            _playerIntent.SuppressInteractionUntilReleased();

            _drivingIntent.Clear();
            _drivingIntent.SuppressExitUntilReleased();

            _commanderIntent.Clear();
            _commanderIntent.SuppressExitUntilReleased();
        }
    }
}
