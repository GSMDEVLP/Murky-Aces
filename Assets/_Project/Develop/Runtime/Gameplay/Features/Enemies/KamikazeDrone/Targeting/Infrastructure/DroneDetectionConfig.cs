using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Infrastructure
{
    [CreateAssetMenu(fileName = "DroneDetectionConfig", menuName = "Murky Aces/Enemies/Drone Detection")]
    public sealed class DroneDetectionConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)]
        private float _range = 50f;

        [SerializeField, Range(1f, 360f)]
        private float _viewAngle = 120f;

        [SerializeField, Min(0.01f)]
        private float _scanInterval = 0.2f;

        [SerializeField]
        private LayerMask _blockingLayers = Physics.DefaultRaycastLayers;

        public float Range => _range;
        public float ViewAngle => _viewAngle;
        public float ScanInterval => _scanInterval;
        public LayerMask BlockingLayers => _blockingLayers;

        public void Validate()
        {
            if (!IsPositiveFinite(_range))
                throw new InvalidOperationException(
                    "Drone detection range must be positive and finite.");

            if (!IsPositiveFinite(_viewAngle) || _viewAngle > 360f)
                throw new InvalidOperationException(
                    "Drone view angle must be within (0, 360].");

            if (!IsPositiveFinite(_scanInterval))
                throw new InvalidOperationException(
                    "Drone scan interval must be positive and finite.");
        }

        private static bool IsPositiveFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > 0f;
        }
    }
}