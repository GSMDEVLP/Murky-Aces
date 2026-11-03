using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Infrastructure
{
    [CreateAssetMenu(
        fileName = "ApcDetectionConfig",
        menuName = "Murky Aces/Enemies/APC/Detection Config")]
    public sealed class ApcDetectionConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)]
        private float _nearRange = 30f;

        [SerializeField, Min(0.01f)]
        private float _normalRange = 100f;

        [SerializeField, Min(0.01f)]
        private float _largeTargetRange = 200f;

        [SerializeField, Range(1f, 360f)]
        private float _viewAngle = 120f;

        [SerializeField, Min(0.01f)]
        private float _scanInterval = 0.2f;

        [SerializeField]
        private LayerMask _blockingLayers = Physics.DefaultRaycastLayers;

        public float NearRange => _nearRange;
        public float NormalRange => _normalRange;
        public float LargeTargetRange => _largeTargetRange;
        public float ViewAngle => _viewAngle;
        public float ScanInterval => _scanInterval;
        public LayerMask BlockingLayers => _blockingLayers;

        public void Validate()
        {
            if (!IsPositiveFinite(_nearRange) ||
                !IsPositiveFinite(_normalRange) ||
                !IsPositiveFinite(_largeTargetRange) ||
                _nearRange > _normalRange ||
                _normalRange > _largeTargetRange)
            {
                throw new InvalidOperationException(
                    "APC ranges must be positive, finite and ordered: Near <= Normal <= Large.");
            }

            if (!IsPositiveFinite(_viewAngle) || _viewAngle > 360f)
                throw new InvalidOperationException("Invalid APC view angle.");

            if (!IsPositiveFinite(_scanInterval))
                throw new InvalidOperationException("Invalid APC scan interval.");
        }

        private static bool IsPositiveFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value) &&
                   value > 0f;
        }
    }
}
