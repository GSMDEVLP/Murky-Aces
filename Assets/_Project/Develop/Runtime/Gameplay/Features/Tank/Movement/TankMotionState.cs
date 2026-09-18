namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    public sealed class TankMotionState
    {
        public float CurrentForwardSpeed { get; internal set; }
        public float CurrentTurnSpeed { get; internal set; }
        public bool IsBraking { get; internal set; }
        public bool IsChangingDirection { get; internal set; }
        public float RemainingDirectionSwitchDelay { get; internal set; }
        public int PendingDirection { get; internal set; }

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

        public void Reset()
        {
            CurrentForwardSpeed = 0f;
            CurrentTurnSpeed = 0f;
            IsBraking = false;
            IsChangingDirection = false;
            RemainingDirectionSwitchDelay = 0f;
            PendingDirection = 0;
        }
    }
}