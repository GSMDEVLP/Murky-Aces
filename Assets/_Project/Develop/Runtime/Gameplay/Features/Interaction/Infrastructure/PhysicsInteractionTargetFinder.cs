using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
    public sealed class PhysicsInteractionTargetFinder : MonoBehaviour, IInteractionTargetFinder
    {
        [SerializeField] private Transform _origin;

        [SerializeField, Min(0.1f)]
        private float _maxDistance = 3f;

        [SerializeField]
        private LayerMask _collisionMask = ~0;

        public bool TryFindTarget(
            IInteractionScope scope,
            in InteractionContext context,
            out IInteractable target)
        {
            target = null;

            if (_origin == null || context.Actor == null)
                return false;

            RaycastHit[] hits = Physics.RaycastAll(
                _origin.position,
                _origin.forward,
                _maxDistance,
                _collisionMask,
                QueryTriggerInteraction.Collide);

            Array.Sort(
                hits,
                (left, right) =>
                    left.distance.CompareTo(right.distance));

            foreach (RaycastHit hit in hits)
            {
                Collider collider = hit.collider;

                if (collider == null)
                    continue;

                if (collider.TryGetComponent(
                        out InteractionTargetLink link) &&
                    link.TryGetInteractable(
                        out IInteractable candidate))
                {
                    bool allowedByScope =
                        scope == null || scope.Allows(candidate);

                    if (allowedByScope &&
                        candidate.GetInteractionInfo(context).IsAvailable)
                    {
                        target = candidate;
                        return true;
                    }
                }

                if (!collider.isTrigger)
                    return false;

            }

            return false;
        }
    }
}