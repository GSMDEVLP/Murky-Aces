using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
    public sealed class PhysicsInteractionTargetFinder : MonoBehaviour, IInteractionTargetFinder
    {
        [SerializeField] private Transform _origin;

        [SerializeField, Min(0.1f)]
        private float _maxDistance = 3f;

        [SerializeField]
        private LayerMask _collisionMask = ~0;

        [SerializeField]
        private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        public bool TryFindTarget(IInteractionScope scope, out IInteractable target)
        {
            target = null;

            if (_origin == null)
                return false;

            bool hasHit = Physics.Raycast(
                _origin.position,
                _origin.forward,
                out RaycastHit hit,
                _maxDistance,
                _collisionMask,
                _triggerInteraction);

            if (hasHit == false)
                return false;

            if (hit.collider.TryGetComponent(
                    out InteractionTargetLink targetLink) == false)
            {
                return false;
            }

            if (targetLink.TryGetInteractable(
                    out target) == false)
            {
                return false;
            }

            if (scope != null &&
                scope.Allows(target) == false)
            {
                target = null;
                return false;
            }

            return true;
        }
    }
}