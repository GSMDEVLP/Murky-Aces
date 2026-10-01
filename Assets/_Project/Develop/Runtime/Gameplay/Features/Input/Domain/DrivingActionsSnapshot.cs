namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct DrivingActionsSnapshot
    {
        public static DrivingActionsSnapshot Neutral =>
            new DrivingActionsSnapshot(
                0f,
                0f,
                false,
                false,
                false);

        public float Throttle { get; }
        public float Steering { get; }
        public bool BrakeHeld { get; }

        public bool ExitPressedThisFrame { get; }
        public bool ExitHeld { get; }

        public DrivingActionsSnapshot(
            float throttle,
            float steering,
            bool brakeHeld,
            bool exitPressedThisFrame,
            bool exitHeld)
        {
            Throttle = throttle;
            Steering = steering;
            BrakeHeld = brakeHeld;
            ExitPressedThisFrame =
                exitPressedThisFrame;
            ExitHeld = exitHeld;
        }
    }
}