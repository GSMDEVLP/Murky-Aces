using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Infrastructure
{
    public sealed class ApcVisionSensor : MonoBehaviour, IApcVisionSensor
    {
        private const float POSITION_TOLERANCE_SQUARED = 0.000001f;

        [SerializeField] private Transform _eyes;
        [SerializeField] private Transform _apcRoot;

        private float _nearRangeSquared;
        private float _normalRangeSquared;
        private float _largeTargetRangeSquared;
        private float _minimumDot;
        private int _blockingLayers;
        private bool _isInitialized;

        public Vector3 WorldPosition => _eyes.position;

        public void Initialize(ApcDetectionConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.Validate();

            if (_eyes == null || _apcRoot == null)
                throw new InvalidOperationException(
                    "APC vision requires Eyes and APC Root.");

            if (!_eyes.IsChildOf(_apcRoot) ||
                !transform.IsChildOf(_apcRoot))
            {
                throw new InvalidOperationException(
                    "APC vision and Eyes must belong to APC Root.");
            }

            _nearRangeSquared = config.NearRange * config.NearRange;
            _normalRangeSquared = config.NormalRange * config.NormalRange;
            _largeTargetRangeSquared =
                config.LargeTargetRange * config.LargeTargetRange;

            _minimumDot = Mathf.Cos(
                config.ViewAngle * 0.5f * Mathf.Deg2Rad);

            _blockingLayers = config.BlockingLayers;
            _isInitialized = true;
        }

        public bool CanSee(ICombatTarget target)
        {
            return CanObserve(target, true);
        }

        public bool CanTrack(ICombatTarget target)
        {
            return CanObserve(target, false);
        }

        private bool CanObserve(ICombatTarget target, bool requireSector)
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    "APC vision is not initialized.");

            if (!isActiveAndEnabled ||
                target == null ||
                !target.IsAvailable)
            {
                return false;
            }

            if (target is CombatTarget ownTarget &&
                ownTarget.transform.IsChildOf(_apcRoot))
            {
                return false;
            }

            Vector3 offset = target.WorldPosition - _eyes.position;
            float distanceSquared = offset.sqrMagnitude;

            bool usesLargeRange =
                target.Kind == CombatTargetKind.Tank ||
                (target.Kind == CombatTargetKind.Monster && target.IsLarge);

            float rangeSquared = usesLargeRange
                ? _largeTargetRangeSquared
                : _normalRangeSquared;

            if (distanceSquared > rangeSquared)
                return false;

            if (distanceSquared <= POSITION_TOLERANCE_SQUARED)
                return true;

            if (requireSector &&
                distanceSquared > _nearRangeSquared &&
                !IsInsideSector(offset))
            {
                return false;
            }

            return HasClearSight(target, offset, distanceSquared);
        }

        private bool IsInsideSector(Vector3 offset)
        {
            Vector3 horizontalOffset =
                Vector3.ProjectOnPlane(offset, Vector3.up);

            if (horizontalOffset.sqrMagnitude <= POSITION_TOLERANCE_SQUARED)
                return true;

            Vector3 horizontalForward =
                Vector3.ProjectOnPlane(_eyes.forward, Vector3.up);

            if (horizontalForward.sqrMagnitude <= POSITION_TOLERANCE_SQUARED)
                return false;

            return Vector3.Dot(
                horizontalForward.normalized,
                horizontalOffset.normalized) >= _minimumDot;
        }

        private bool HasClearSight(
            ICombatTarget target,
            Vector3 offset,
            float distanceSquared)
        {
            float distance = Mathf.Sqrt(distanceSquared);

            RaycastHit[] hits = Physics.RaycastAll(
                _eyes.position,
                offset / distance,
                distance,
                _blockingLayers,
                QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits)
            {
                Collider collider = hit.collider;

                if (collider == null ||
                    collider.transform.IsChildOf(_apcRoot))
                {
                    continue;
                }

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
