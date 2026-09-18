using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
public sealed class PhysicsInteractionTargetFinder : MonoBehaviour, IInteractionTargetFinder
{
    [SerializeField] private Transform _origin;
    [SerializeField, Min(0.1f)] private float _maxDistance = 3f;
    [SerializeField] private LayerMask _collisionMask = ~0;
    [SerializeField] private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

    public bool TryFindTarget(out IInteractable target)
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

        if (!hasHit)
            return false;

        if (!hit.collider.TryGetComponent(out InteractionTargetLink targetLink))
        {
            return false;
        }

        return targetLink.TryGetInteractable(out target);
    }
}
}
