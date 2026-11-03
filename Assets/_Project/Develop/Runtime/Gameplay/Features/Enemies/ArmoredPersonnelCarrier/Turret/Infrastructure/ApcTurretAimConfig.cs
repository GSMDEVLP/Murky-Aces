using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Infrastructure
{
    [CreateAssetMenu(
        fileName = "ApcTurretAimConfig",
        menuName = "Murky Aces/Enemies/APC/Turret Aim Config")]
    public sealed class ApcTurretAimConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)]
        private float _yawDegreesPerSecond = 36f;

        [SerializeField, Min(0.01f)]
        private float _pitchDegreesPerSecond = 18f;

        [SerializeField, Range(-90f, 0f)]
        private float _minPitch = -10f;

        [SerializeField, Range(0f, 90f)]
        private float _maxPitch = 20f;

        [SerializeField]
        private Vector3 _localPitchAxis = Vector3.down;

        [SerializeField]
        private Vector3 _localForwardAxis = Vector3.right;

        public float YawDegreesPerSecond => _yawDegreesPerSecond;
        public float PitchDegreesPerSecond => _pitchDegreesPerSecond;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
        public Vector3 LocalPitchAxis => _localPitchAxis;
        public Vector3 LocalForwardAxis => _localForwardAxis;

        public void Validate()
        {
            if (!IsFinite(_yawDegreesPerSecond) ||
                _yawDegreesPerSecond <= 0f ||
                !IsFinite(_pitchDegreesPerSecond) ||
                _pitchDegreesPerSecond <= 0f ||
                !IsFinite(_minPitch) || !IsFinite(_maxPitch) ||
                _minPitch < -90f || _maxPitch > 90f ||
                _minPitch > 0f || _maxPitch < 0f ||
                _minPitch > _maxPitch ||
                !IsValidAxis(_localPitchAxis) ||
                !IsValidAxis(_localForwardAxis))
            {
                throw new InvalidOperationException(
                    "APC turret aim settings are invalid.");
            }
        }

        private static bool IsValidAxis(Vector3 axis)
        {
            return IsFinite(axis.sqrMagnitude) &&
                   axis.sqrMagnitude > 0.000001f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
