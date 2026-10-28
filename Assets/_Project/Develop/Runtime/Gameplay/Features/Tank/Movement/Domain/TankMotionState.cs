using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Domain
{
    public readonly struct TankMotionState
    {
        public float CurrentForwardSpeed { get; }
        public float CurrentTurnSpeed { get; }
        public bool IsBraking { get; }
        public bool IsChangingDirection => PendingDirection != 0;
        public float RemainingDirectionSwitchDelay { get; }
        public int PendingDirection { get; }

        public int CurrentDirection
        {
            get
            {
                if (CurrentForwardSpeed > 0f)
                    return 1;

                if (CurrentForwardSpeed < 0f)
                    return -1;

                return 0;
            }
        }

        public TankMotionState(float currentForwardSpeed, float currentTurnSpeed, bool isBraking, int pendingDirection, float remainingDirectionSwitchDelay)
        {
            ValidateFinite(currentForwardSpeed, nameof(currentForwardSpeed));
            ValidateFinite(currentTurnSpeed, nameof(currentTurnSpeed));
            ValidateFinite(remainingDirectionSwitchDelay, nameof(remainingDirectionSwitchDelay));

            if (pendingDirection < -1 || pendingDirection > 1)
                throw new ArgumentOutOfRangeException(nameof(pendingDirection));

            if (remainingDirectionSwitchDelay < 0f)
                throw new ArgumentOutOfRangeException(nameof(remainingDirectionSwitchDelay));

            if ((pendingDirection == 0 && remainingDirectionSwitchDelay != 0f) ||
                (isBraking && pendingDirection != 0))
            {
                throw new ArgumentException("Inconsistent direction change state.");
            }

            CurrentForwardSpeed = currentForwardSpeed;
            CurrentTurnSpeed = currentTurnSpeed;
            IsBraking = isBraking;
            PendingDirection = pendingDirection;
            RemainingDirectionSwitchDelay = remainingDirectionSwitchDelay;
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
