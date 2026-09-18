using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    public sealed class TankMovement : IFixedGameplayTickable
    {
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

            UpdateForwardSpeed(fixedDeltaTime);
            UpdateTurnSpeed();
            ApplyMotion(fixedDeltaTime);
        }

        private void UpdateForwardSpeed(float fixedDeltaTime)
        {
            _state.IsBraking = _input.IsBraking;

            if (_input.IsBraking)
            {
                CancelDirectionChange();

                _state.CurrentForwardSpeed = Mathf.MoveTowards(
                    _state.CurrentForwardSpeed,
                    0f,
                    _config.BrakeDeceleration * fixedDeltaTime);

                return;
            }

            int requestedDirection =
                GetDirection(_input.Throttle);

            if (requestedDirection == 0)
            {
                CancelDirectionChange();

                _state.CurrentForwardSpeed = Mathf.MoveTowards(
                    _state.CurrentForwardSpeed,
                    0f,
                    _config.Deceleration * fixedDeltaTime);

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
                _state.CurrentDirection;

            if (currentDirection != 0 &&
                currentDirection != requestedDirection)
            {
                BeginDirectionChange(requestedDirection);

                _state.CurrentForwardSpeed = Mathf.MoveTowards(
                    _state.CurrentForwardSpeed,
                    0f,
                    _config.Deceleration * fixedDeltaTime);

                return;
            }

            Accelerate(requestedDirection, fixedDeltaTime);
        }

        private void UpdateDirectionChange(
            int requestedDirection,
            float fixedDeltaTime)
        {
            if (requestedDirection !=
                _state.PendingDirection)
            {
                int currentDirection =
                    _state.CurrentDirection;

                if (currentDirection != 0 &&
                    requestedDirection == currentDirection)
                {
                    CancelDirectionChange();
                    Accelerate(
                        requestedDirection,
                        fixedDeltaTime);

                    return;
                }

                BeginDirectionChange(requestedDirection);
            }

            _state.CurrentForwardSpeed = Mathf.MoveTowards(
                _state.CurrentForwardSpeed,
                0f,
                _config.Deceleration * fixedDeltaTime);

            if (_state.CurrentForwardSpeed != 0f)
                return;

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
            Accelerate(pendingDirection, fixedDeltaTime);
        }

        private void Accelerate(
            int requestedDirection,
            float fixedDeltaTime)
        {
            float targetSpeed;
            float acceleration;

            if (requestedDirection > 0)
            {
                targetSpeed =
                    _input.Throttle *
                    _config.ForwardMaxSpeed;

                acceleration =
                    _config.ForwardAcceleration;
            }
            else
            {
                targetSpeed =
                    _input.Throttle *
                    _config.ReverseMaxSpeed;

                acceleration =
                    _config.ReverseAcceleration;
            }

            _state.CurrentForwardSpeed =
                Mathf.MoveTowards(
                    _state.CurrentForwardSpeed,
                    targetSpeed,
                    acceleration * fixedDeltaTime);
        }

        private void UpdateTurnSpeed()
        {
            _state.CurrentTurnSpeed =
                _input.Steering *
                _config.TurnSpeed;
        }

        private void ApplyMotion(float fixedDeltaTime)
        {
            Quaternion currentRotation =
                _motionBody.Rotation;

            float turnAngle =
                _state.CurrentTurnSpeed *
                fixedDeltaTime;

            Quaternion desiredRotation =
                Quaternion.AngleAxis(
                    turnAngle,
                    Vector3.up) *
                currentRotation;

            Vector3 forward =
                desiredRotation *
                Vector3.forward;

            Vector3 desiredDisplacement =
                forward *
                _state.CurrentForwardSpeed *
                fixedDeltaTime;

            _motionBody.ResolveAndApplyMotion(
                desiredDisplacement,
                desiredRotation);
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
            if (value > 0f)
                return 1;

            if (value < 0f)
                return -1;

            return 0;
        }
    }
}