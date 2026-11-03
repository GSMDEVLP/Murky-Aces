using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Application
{
    public sealed class ApcTurretAim
    {
        private readonly float _minPitch;
        private readonly float _maxPitch;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public ApcTurretAim(float minPitch, float maxPitch)
        {
            if (!IsFinite(minPitch) || !IsFinite(maxPitch) ||
                minPitch < -90f || maxPitch > 90f ||
                minPitch > 0f || maxPitch < 0f ||
                minPitch > maxPitch)
            {
                throw new ArgumentOutOfRangeException(nameof(minPitch));
            }

            _minPitch = minPitch;
            _maxPitch = maxPitch;
        }

        public void Tick(
            float desiredYaw,
            float desiredPitch,
            float yawSpeed,
            float pitchSpeed,
            float deltaTime)
        {
            if (!IsFinite(desiredYaw) || !IsFinite(desiredPitch) ||
                !IsFinite(yawSpeed) || yawSpeed <= 0f ||
                !IsFinite(pitchSpeed) || pitchSpeed <= 0f ||
                !IsFinite(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (deltaTime <= 0f)
                return;

            Yaw = Mathf.DeltaAngle(
                0f,
                Mathf.MoveTowardsAngle(
                    Yaw, desiredYaw, yawSpeed * deltaTime));

            Pitch = Mathf.MoveTowards(
                Pitch,
                Mathf.Clamp(desiredPitch, _minPitch, _maxPitch),
                pitchSpeed * deltaTime);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
