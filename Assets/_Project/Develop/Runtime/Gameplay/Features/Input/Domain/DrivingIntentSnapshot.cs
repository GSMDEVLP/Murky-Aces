namespace _Project.Develop.Runtime.Gameplay.Features.Input.Domain
{
    public readonly struct DrivingIntentSnapshot
    {
        public static DrivingIntentSnapshot Neutral =>new DrivingIntentSnapshot(0f,0f,false);

        public float Throttle { get; }
        public float Steering { get; }
        public bool IsBraking { get; }

        public DrivingIntentSnapshot(float throttle, float steering, bool isBraking)
        {
            Throttle = ClampAxis(throttle);
            Steering = ClampAxis(steering);
            IsBraking = isBraking;
        }

        private static float ClampAxis(float value)
        {
            if (value < -1f)
                return -1f;

            if (value > 1f)
                return 1f;

            return value;
        }
    }
}