using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using GameLoopFixedTickable = _Project.Develop.Runtime.Core.GameLoop.Abstractions.IFixedTickable;
using GameLoopTickable = _Project.Develop.Runtime.Core.GameLoop.Abstractions.ITickable;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement
{
    public sealed class PlayerLook :
        MonoBehaviour,
        GameLoopTickable,
        GameLoopFixedTickable
    {
        [Header("References")]
        [SerializeField] private Rigidbody _body;
        [SerializeField] private Transform _cameraPivot;

        [Header("Walking Look")]
        [SerializeField] private float _maxWalkingPitch = 85f;

        private float _minCockpitYaw;
        private float _maxCockpitYaw;
        private float _minCockpitPitch;
        private float _maxCockpitPitch;

        private PlayerIntentBuffer _intent;

        private LookMode _mode = LookMode.Walking;

        private float _walkingYaw;
        private float _walkingPitch;

        private float _cockpitYaw;
        private float _cockpitPitch;

        private bool _walkingInitialized;
        private bool _gameplayEnabled = true;

        public bool IsGameplayEnabled =>
            _gameplayEnabled;

        public bool IsCockpitMode =>
            _mode == LookMode.Cockpit;

        public Quaternion Heading
        {
            get
            {
                if (IsCockpitMode)
                    return _body.rotation;

                EnsureWalkingInitialized();

                return Quaternion.Euler(
                    0f,
                    _walkingYaw,
                    0f);
            }
        }

        [Inject]
        public void Construct(
            PlayerIntentBuffer intent)
        {
            _intent = intent;
        }

        public void Tick(float deltaTime)
        {
            if (_gameplayEnabled == false ||
                _intent == null)
            {
                return;
            }

            Vector2 look = _intent.LookDelta;

            if (IsCockpitMode)
            {
                TickCockpitLook(look);
                return;
            }

            TickWalkingLook(look);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            if (_gameplayEnabled == false ||
                IsCockpitMode)
            {
                return;
            }

            _body.MoveRotation(Heading);
        }

        public void EnterCockpitMode(Vector2 yawLimits, Vector2 pitchLimits, Vector2 initialLookAngles)
        {
            _minCockpitYaw = yawLimits.x;
            _maxCockpitYaw = yawLimits.y;

            _minCockpitPitch = pitchLimits.x;
            _maxCockpitPitch = pitchLimits.y;

            _mode = LookMode.Cockpit;

            _cockpitYaw = Mathf.Clamp(initialLookAngles.x, _minCockpitYaw, _maxCockpitYaw);
            _cockpitPitch = Mathf.Clamp(initialLookAngles.y, _minCockpitPitch, _maxCockpitPitch);

            ApplyCockpitRotation();
            _intent?.ClearLook();
        }

        public void EnterWalkingMode()
        {
            _mode = LookMode.Walking;

            _cameraPivot.localRotation =
                Quaternion.identity;

            _walkingInitialized = false;
            _intent?.ClearLook();
        }

        public void SetGameplayEnabled(
            bool isEnabled)
        {
            if (_gameplayEnabled == isEnabled)
                return;

            _gameplayEnabled = isEnabled;
            _intent?.ClearLook();

            if (isEnabled &&
                IsCockpitMode == false)
            {
                _walkingInitialized = false;
            }
        }

        private void TickWalkingLook(
            Vector2 look)
        {
            EnsureWalkingInitialized();

            _walkingYaw += look.x;

            _walkingPitch = Mathf.Clamp(
                _walkingPitch - look.y,
                -_maxWalkingPitch,
                _maxWalkingPitch);

            _cameraPivot.localRotation =
                Quaternion.Euler(
                    _walkingPitch,
                    0f,
                    0f);
        }

        private void TickCockpitLook(
            Vector2 look)
        {
            _cockpitYaw = Mathf.Clamp(
                _cockpitYaw + look.x,
                _minCockpitYaw,
                _maxCockpitYaw);

            _cockpitPitch = Mathf.Clamp(
                _cockpitPitch - look.y,
                _minCockpitPitch,
                _maxCockpitPitch);

            ApplyCockpitRotation();
        }

        private void ApplyCockpitRotation()
        {
            _cameraPivot.localRotation =
                Quaternion.Euler(
                    _cockpitPitch,
                    _cockpitYaw,
                    0f);
        }

        private void EnsureWalkingInitialized()
        {
            if (_walkingInitialized)
                return;

            _walkingYaw =
                _body.rotation.eulerAngles.y;

            _walkingPitch =
                Mathf.DeltaAngle(
                    0f,
                    _cameraPivot
                        .localEulerAngles.x);

            _walkingInitialized = true;
        }

        private enum LookMode
        {
            Walking,
            Cockpit
        }
    }
}