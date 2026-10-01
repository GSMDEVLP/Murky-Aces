namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret
{
    public sealed class TurretAimState
    {
        public float CurrentYaw { get; internal set; }
        public float TargetYaw { get; internal set; }

        public float CurrentPitch { get; internal set; }
        public float TargetPitch { get; internal set; }

        public TurretReferenceMode ReferenceMode { get; internal set; }
        public float WorldDirection { get; internal set; }

        public TurretAimState(
            float initialYaw,
            float initialPitch,
            float initialHullYaw,
            TurretReferenceMode initialMode)
        {
            CurrentYaw = NormalizeAngle(initialYaw);
            TargetYaw = CurrentYaw;

            CurrentPitch = initialPitch;
            TargetPitch = initialPitch;

            ReferenceMode = initialMode;
            WorldDirection =
                NormalizeAngle(initialHullYaw + CurrentYaw);
        }

        private static float NormalizeAngle(float angle)
        {
            float result = angle % 360f;

            if (result > 180f)
                result -= 360f;
            else if (result <= -180f)
                result += 360f;

            return result;
        }
    }
}