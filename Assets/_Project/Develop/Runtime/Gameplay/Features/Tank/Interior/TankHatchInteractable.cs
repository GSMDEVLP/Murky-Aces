using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Interior
{
    public sealed class TankHatchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _insideAnchor;
        [SerializeField] private Transform _outsideAnchor;

        [SerializeField, Min(0.1f)]
        private float _holdDuration = 1f;

        private ulong? _activeInteractorId;
        private long _expectedRevision;

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            var interior = context.Actor as ITankInteriorOccupant;

            bool isInside = interior != null && interior.IsInInterior;

            bool available = CanInteract(context.Actor);

            if (_activeInteractorId.HasValue)
            {
                available =
                    available &&
                    _activeInteractorId.Value == context.Actor.Id &&
                    _expectedRevision == interior.Revision;
            }

            return InteractionInfo.Hold(
                isInside
                    ? InteractionPromts.Exit
                    : InteractionPromts.Enter,
                _holdDuration,
                available);
        }

        public bool Begin(in InteractionContext context)
        {
            if (!CanInteract(context.Actor))
                return false;

            var interior = (ITankInteriorOccupant)context.Actor;

            if (_activeInteractorId.HasValue)
            {
                return
                    _activeInteractorId.Value == context.Actor.Id &&
                    _expectedRevision == interior.Revision;
            }

            _activeInteractorId = context.Actor.Id;
            _expectedRevision = interior.Revision;

            return true;
        }

        public void Complete(in InteractionContext context)
        {
            if (context.Actor == null ||
                _activeInteractorId != context.Actor.Id)
            {
                return;
            }

            long expectedRevision = _expectedRevision;
            ReleaseInteraction();

            if (!CanInteract(context.Actor))
                return;

            var interior =
                (ITankInteriorOccupant)context.Actor;

            if (interior.Revision != expectedRevision)
                return;

            if (interior.IsInInterior)
            {
                interior.TryExitTank(expectedRevision, _outsideAnchor);
            }
            else
            {
                interior.TryEnterTank(expectedRevision, _insideAnchor);
            }
        }

        public void Cancel(in InteractionContext context, InteractionCancelReason reason)
        {
            if (context.Actor != null &&
                _activeInteractorId == context.Actor.Id)
            {
                ReleaseInteraction();
            }
        }

        private bool CanInteract(IInteractionActor actor)
        {
            if (!isActiveAndEnabled ||
                _insideAnchor == null ||
                _outsideAnchor == null)
            {
                return false;
            }

            var interior = actor as ITankInteriorOccupant;

            return interior != null &&
                   interior.CanUseHatch;
        }

        private void ReleaseInteraction()
        {
            _activeInteractorId = null;
            _expectedRevision = 0;
        }

        private void OnDisable()
        {
            ReleaseInteraction();
        }
    }
}