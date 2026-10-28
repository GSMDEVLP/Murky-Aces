using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Domain
{
    public readonly struct TurretAimState
    {
        public float CurrentYaw { get; }
        public float TargetYaw { get; }

        public float CurrentPitch { get; }
        public float TargetPitch { get; }

        public TurretReferenceMode ReferenceMode { get; }
        public float WorldDirection { get; }

        public TurretAimState(
            float initialYaw,
            float initialPitch,
            float initialHullYaw,
            TurretReferenceMode initialMode)
            : this(initialYaw, initialYaw, initialPitch, initialPitch,
                initialMode, GetWorldDirection(initialHullYaw, initialYaw))
        {
        }

        public TurretAimState(
            float currentYaw,
            float targetYaw,
            float currentPitch,
            float targetPitch,
            TurretReferenceMode referenceMode,
            float worldDirection)
        {
            ValidateFinite(currentYaw, nameof(currentYaw));
            ValidateFinite(targetYaw, nameof(targetYaw));
            ValidateFinite(currentPitch, nameof(currentPitch));
            ValidateFinite(targetPitch, nameof(targetPitch));
            ValidateFinite(worldDirection, nameof(worldDirection));

            if (referenceMode != TurretReferenceMode.HullRelative &&
                referenceMode != TurretReferenceMode.WorldStabilized)
                throw new ArgumentOutOfRangeException(nameof(referenceMode));

            CurrentYaw = NormalizeAngle(currentYaw);
            TargetYaw = NormalizeAngle(targetYaw);
            CurrentPitch = currentPitch;
            TargetPitch = targetPitch;
            ReferenceMode = referenceMode;
            WorldDirection = NormalizeAngle(worldDirection);
        }

        private static float GetWorldDirection(float hullYaw, float turretYaw)
        {
            ValidateFinite(hullYaw, nameof(hullYaw));
            ValidateFinite(turretYaw, nameof(turretYaw));
            return NormalizeAngle(hullYaw) + NormalizeAngle(turretYaw);
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName);
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
