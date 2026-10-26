using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
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

        private readonly List<IInteractionAction> _actions = new List<IInteractionAction>(4);

        public bool TryFindTarget(
            IInteractionScope scope,
            in InteractionContext context,
            out IInteractionTarget  target)
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

                if (TryResolveTarget(collider, out IInteractionTarget candidate))
                {
                    bool allowedByScope =
                        scope == null || scope.Allows(candidate);

                    if (allowedByScope)
                        InteractionActions.CollectAvailable(candidate, context, _actions);

                    if (allowedByScope && _actions.Count > 0)
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
        private static bool TryResolveTarget(Collider collider, out IInteractionTarget target)
        {
            target = null;

            if (collider == null)
                return false;

            Rigidbody body = collider.attachedRigidbody;

            Transform boundary = body != null
                ? body.transform
                : null;

            Transform current = collider.transform;

            while (current != null)
            {
                if (current.TryGetComponent(
                        out InteractionTargetLink link))
                {
                    return link.TryGetTarget(out target);
                }

                if (current == boundary)
                    return false;

                current = current.parent;
            }

            return false;
        }
    }
}
