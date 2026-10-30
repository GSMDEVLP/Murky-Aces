using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Infrastructure
{
    public sealed class DroneVisionSensor : MonoBehaviour, IDroneVisionSensor
    {
        [SerializeField] private Transform _eyes;
        [SerializeField] private Transform _droneRoot;

        private float _rangeSquared;
        private float _minimumDot;
        private int _blockingLayers;
        private bool _isInitialized;

        public Vector3 WorldPosition => _eyes.position;
        
        public void Initialize(DroneDetectionConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.Validate();

            if (_eyes == null || _droneRoot == null)
                throw new InvalidOperationException(
                    "Drone vision requires Eyes and Drone Root.");

            if (!_eyes.IsChildOf(_droneRoot) ||
                !transform.IsChildOf(_droneRoot))
            {
                throw new InvalidOperationException(
                    "Drone vision and Eyes must belong to Drone Root.");
            }

            _rangeSquared = config.Range * config.Range;
            _minimumDot = Mathf.Cos(
                config.ViewAngle * 0.5f * Mathf.Deg2Rad);

            _blockingLayers = config.BlockingLayers;
            _isInitialized = true;
        }

        public bool CanSee(ICombatTarget target)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "Drone vision is not initialized.");

            if (!isActiveAndEnabled ||
                target == null ||
                !target.IsAvailable)
            {
                return false;
            }

            if (target is CombatTarget ownTarget &&
                ownTarget.transform.IsChildOf(_droneRoot))
            {
                return false;
            }

            Vector3 origin = _eyes.position;
            Vector3 offset = target.WorldPosition - origin;
            float distanceSquared = offset.sqrMagnitude;

            if (distanceSquared > _rangeSquared)
                return false;

            if (distanceSquared <= 0.000001f)
                return true;

            float distance = Mathf.Sqrt(distanceSquared);
            Vector3 direction = offset / distance;

            if (Vector3.Dot(_eyes.forward, direction) < _minimumDot)
                return false;

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                direction,
                distance,
                _blockingLayers,
                QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits)
            {
                Collider collider = hit.collider;

                if (collider == null)
                    continue;

                if (collider.transform.IsChildOf(_droneRoot))
                    continue;

                CombatTarget hitTarget =
                    collider.GetComponentInParent<CombatTarget>();

                if (ReferenceEquals(hitTarget, target))
                    continue;

                return false;
            }

            return true;
        }
    }
}