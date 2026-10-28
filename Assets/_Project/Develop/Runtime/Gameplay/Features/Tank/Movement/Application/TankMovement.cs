using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Application
{
    public sealed class TankMovement : IFixedGameplayTickable
    {
        private const float StopSpeedEpsilon = 0.05f;

        private readonly TankMovementSettings _settings;
        private readonly ITankMotionBody _motionBody;

        private TankDrivingInput _input;
        private float _currentForwardSpeed;
        private float _currentTurnSpeed;
        private bool _isBraking;
        private float _remainingDirectionSwitchDelay;
        private int _pendingDirection;

        public TankMotionState State => new TankMotionState(
            _currentForwardSpeed,
            _currentTurnSpeed,
            _isBraking,
            _pendingDirection,
            _remainingDirectionSwitchDelay);

        public TankMovement(
            TankMovementSettings settings,
            ITankMotionBody motionBody)
        {
            _settings = settings ??
                throw new ArgumentNullException(nameof(settings));

            _motionBody = motionBody ??
                throw new ArgumentNullException(nameof(motionBody));

            _input = TankDrivingInput.Neutral;
        }

        public void SetInput(in TankDrivingInput input)
        {
            _input = input;
        }

        public void ClearInput()
        {
            _input = TankDrivingInput.Neutral;
        }

        public void FixedTick(float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f)
                return;

            SynchronizeStateFromBody();
            UpdateDrive(fixedDeltaTime);
            UpdateSteering(fixedDeltaTime);
        }

        private void SynchronizeStateFromBody()
        {
            Quaternion rotation = _motionBody.Rotation;

            Vector3 forward =
                rotation * Vector3.forward;

            Vector3 up =
                rotation * Vector3.up;

            _currentForwardSpeed =
                Vector3.Dot(
                    _motionBody.LinearVelocity,
                    forward);

            _currentTurnSpeed =
                Vector3.Dot(
                    _motionBody.AngularVelocity,
                    up) *
                Mathf.Rad2Deg;
        }

        private void UpdateDrive(float fixedDeltaTime)
        {
            _isBraking = _input.IsBraking;

            if (_input.IsBraking)
            {
                CancelDirectionChange();

                ApplyLongitudinalDeceleration(
                    _settings.BrakeDeceleration,
                    fixedDeltaTime);

                return;
            }

            int requestedDirection =
                GetDirection(_input.Throttle);

            if (requestedDirection == 0)
            {
                CancelDirectionChange();

                ApplyLongitudinalDeceleration(
                    _settings.Deceleration,
                    fixedDeltaTime);

                return;
            }

            if (_pendingDirection != 0)
            {
                UpdateDirectionChange(requestedDirection, fixedDeltaTime);

                return;
            }

            int currentDirection =
                GetDirection(
                    _currentForwardSpeed,
                    StopSpeedEpsilon);

            if (currentDirection != 0 &&
                currentDirection != requestedDirection)
            {
                BeginDirectionChange(requestedDirection);

                ApplyLongitudinalDeceleration(
                    _settings.Deceleration,
                    fixedDeltaTime);

                return;
            }

            ApplyThrottle(requestedDirection);
        }

        private void UpdateDirectionChange(
            int requestedDirection,
            float fixedDeltaTime)
        {
            if (requestedDirection !=
                _pendingDirection)
            {
                int currentDirection =
                    GetDirection(
                        _currentForwardSpeed,
                        StopSpeedEpsilon);

                if (currentDirection != 0 &&
                    requestedDirection == currentDirection)
                {
                    CancelDirectionChange();
                    ApplyThrottle(requestedDirection);
                    return;
                }

                BeginDirectionChange(requestedDirection);
            }

            ApplyLongitudinalDeceleration(
                _settings.Deceleration,
                fixedDeltaTime);

            if (Mathf.Abs(_currentForwardSpeed) >
                StopSpeedEpsilon)
            {
                return;
            }

            _remainingDirectionSwitchDelay =
                Mathf.Max(
                    0f,
                    _remainingDirectionSwitchDelay -
                    fixedDeltaTime);

            if (_remainingDirectionSwitchDelay > 0f)
                return;

            int pendingDirection = _pendingDirection;

            CancelDirectionChange();
            ApplyThrottle(pendingDirection);
        }

        private void ApplyThrottle(int requestedDirection)
        {
            float maximumSpeed;
            float acceleration;

            if (requestedDirection > 0)
            {
                maximumSpeed = _settings.ForwardMaxSpeed;
                acceleration = _settings.ForwardAcceleration;
            }
            else
            {
                maximumSpeed = _settings.ReverseMaxSpeed;
                acceleration = _settings.ReverseAcceleration;
            }

            float speedInRequestedDirection =
                _currentForwardSpeed *
                requestedDirection;

            if (speedInRequestedDirection >= maximumSpeed)
                return;

            Vector3 forward =
                _motionBody.Rotation *
                Vector3.forward;

            Vector3 requestedAcceleration =
                forward *
                requestedDirection *
                acceleration *
                Mathf.Abs(_input.Throttle);

            _motionBody.ApplyLinearAcceleration(
                requestedAcceleration);
        }

        private void ApplyLongitudinalDeceleration(float deceleration, float fixedDeltaTime)
        {
            float currentSpeed =
                _currentForwardSpeed;

            if (Mathf.Abs(currentSpeed) <= StopSpeedEpsilon)
                return;

            float requiredAcceleration =
                Mathf.Min(
                    deceleration,
                    Mathf.Abs(currentSpeed) /
                    fixedDeltaTime);

            Vector3 forward =
                _motionBody.Rotation *
                Vector3.forward;

            Vector3 brakingAcceleration =
                -Mathf.Sign(currentSpeed) *
                requiredAcceleration *
                forward;

            _motionBody.ApplyLinearAcceleration(
                brakingAcceleration);
        }


        private void UpdateSteering(float fixedDeltaTime)
        {
            float targetTurnSpeed =
                _input.Steering *
                _settings.TurnSpeed;

            float turnSpeedError =
                targetTurnSpeed -
                _currentTurnSpeed;

            float angularAcceleration =
                Mathf.Clamp(
                    turnSpeedError / fixedDeltaTime,
                    -_settings.TurnAcceleration,
                    _settings.TurnAcceleration);

            if (Mathf.Approximately(
                    angularAcceleration,
                    0f))
            {
                return;
            }

            Vector3 up = _motionBody.Rotation * Vector3.up;

            _motionBody.ApplyAngularAcceleration(
                up *
                angularAcceleration *
                Mathf.Deg2Rad);
        }

        private void BeginDirectionChange(
            int requestedDirection)
        {
            _pendingDirection = requestedDirection;
            _remainingDirectionSwitchDelay =
                _settings.DirectionSwitchDelay;
        }

        private void CancelDirectionChange()
        {
            _pendingDirection = 0;
            _remainingDirectionSwitchDelay = 0f;
        }

        private static int GetDirection(float value)
        {
            return GetDirection(value, 0f);
        }

        private static int GetDirection(
            float value,
            float epsilon)
        {
            if (value > epsilon)
                return 1;

            if (value < -epsilon)
                return -1;

            return 0;
        }
    }
}
