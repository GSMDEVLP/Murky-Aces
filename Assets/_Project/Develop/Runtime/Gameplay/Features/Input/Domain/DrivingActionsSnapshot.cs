namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct DrivingActionsSnapshot
    {
        public static DrivingActionsSnapshot Neutral =>
            new DrivingActionsSnapshot(
                0f,
                0f,
                false);

        public float Throttle { get; }
        public float Steering { get; }
        public bool BrakeHeld { get; }

        public DrivingActionsSnapshot(
            float throttle,
            float steering,
            bool brakeHeld)
        {
            Throttle = throttle;
            Steering = steering;
            BrakeHeld = brakeHeld;
        }
    }
}