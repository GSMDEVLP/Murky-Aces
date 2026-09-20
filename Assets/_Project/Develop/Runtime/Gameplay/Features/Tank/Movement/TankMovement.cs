using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    public sealed class TankMovement : IFixedGameplayTickable
    {
        private const float StopSpeedEpsilon = 0.05f;

        private readonly TankMovementConfig _config;
        private readonly TankMotionState _state;
        private readonly ITankMotionBody _motionBody;

        private TankDrivingInput _input;

        public TankMovement(
            TankMovementConfig config,
            TankMotionState state,
            ITankMotionBody motionBody)
        {
            _config = config != null
                ? config
                : throw new ArgumentNullException(nameof(config));

            _state = state ??
                throw new ArgumentNullException(nameof(state));

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

            _state.CurrentForwardSpeed =
                Vector3.Dot(
                    _motionBody.LinearVelocity,
                    forward);

            _state.CurrentTurnSpeed =
                Vector3.Dot(
                    _motionBody.AngularVelocity,
                    up) *
                Mathf.Rad2Deg;
        }

        private void UpdateDrive(float fixedDeltaTime)
        {
            _state.IsBraking = _input.IsBraking;

            if (_input.IsBraking)
            {
                CancelDirectionChange();

                ApplyLongitudinalDeceleration(
                    _config.BrakeDeceleration,
                    fixedDeltaTime);

                return;
            }

            int requestedDirection =
                GetDirection(_input.Throttle);

            if (requestedDirection == 0)
            {
                CancelDirectionChange();

                ApplyLongitudinalDeceleration(
                    _config.Deceleration,
                    fixedDeltaTime);

                return;
            }

            if (_state.IsChangingDirection)
            {
                UpdateDirectionChange(
                    requestedDirection,
                    fixedDeltaTime);

                return;
            }

            int currentDirection =
                GetDirection(
                    _state.CurrentForwardSpeed,
                    StopSpeedEpsilon);

            if (currentDirection != 0 &&
                currentDirection != requestedDirection)
            {
                BeginDirectionChange(requestedDirection);

                ApplyLongitudinalDeceleration(
                    _config.Deceleration,
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
                _state.PendingDirection)
            {
                int currentDirection =
                    GetDirection(
                        _state.CurrentForwardSpeed,
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
                _config.Deceleration,
                fixedDeltaTime);

            if (Mathf.Abs(_state.CurrentForwardSpeed) >
                StopSpeedEpsilon)
            {
                return;
            }

            _state.RemainingDirectionSwitchDelay =
                Mathf.Max(
                    0f,
                    _state.RemainingDirectionSwitchDelay -
                    fixedDeltaTime);

            if (_state.RemainingDirectionSwitchDelay > 0f)
                return;

            int pendingDirection =
                _state.PendingDirection;

            CancelDirectionChange();
            ApplyThrottle(pendingDirection);
        }

        private void ApplyThrottle(int requestedDirection)
        {
            float maximumSpeed;
            float acceleration;

            if (requestedDirection > 0)
            {
                maximumSpeed = _config.ForwardMaxSpeed;
                acceleration = _config.ForwardAcceleration;
            }
            else
            {
                maximumSpeed = _config.ReverseMaxSpeed;
                acceleration = _config.ReverseAcceleration;
            }

            float speedInRequestedDirection =
                _state.CurrentForwardSpeed *
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

        private void ApplyLongitudinalDeceleration(
            float deceleration,
            float fixedDeltaTime)
        {
            float currentSpeed =
                _state.CurrentForwardSpeed;

            if (Mathf.Abs(currentSpeed) <=
                StopSpeedEpsilon)
            {
                return;
            }

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
                _config.TurnSpeed;

            float turnSpeedError =
                targetTurnSpeed -
                _state.CurrentTurnSpeed;

            float angularAcceleration =
                Mathf.Clamp(
                    turnSpeedError / fixedDeltaTime,
                    -_config.TurnAcceleration,
                    _config.TurnAcceleration);

            if (Mathf.Approximately(
                    angularAcceleration,
                    0f))
            {
                return;
            }

            Vector3 up =
                _motionBody.Rotation *
                Vector3.up;

            _motionBody.ApplyAngularAcceleration(
                up *
                angularAcceleration *
                Mathf.Deg2Rad);
        }

        private void BeginDirectionChange(
            int requestedDirection)
        {
            _state.IsChangingDirection = true;
            _state.PendingDirection = requestedDirection;
            _state.RemainingDirectionSwitchDelay =
                _config.DirectionSwitchDelay;
        }

        private void CancelDirectionChange()
        {
            _state.IsChangingDirection = false;
            _state.PendingDirection = 0;
            _state.RemainingDirectionSwitchDelay = 0f;
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