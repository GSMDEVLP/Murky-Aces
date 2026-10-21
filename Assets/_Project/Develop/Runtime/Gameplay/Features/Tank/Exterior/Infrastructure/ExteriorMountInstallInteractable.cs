using UnityEngine;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class ExteriorMountInstallInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private ExteriorMountSlotAuthoring _slot;

        private ExteriorWeldingInteraction _welding;
        private Func<bool> _pending;
        private ulong _pendingActorId;

        public ExteriorMountSlotAuthoring Slot => _slot;

        private void Awake()
        {
            ExteriorMountSlotAuthoring configuredSlot = _slot;

            _welding = new ExteriorWeldingInteraction(
                configuredSlot,
                () =>
                    this != null &&
                    isActiveAndEnabled &&
                    _slot == configuredSlot);
        }

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            if (_pending != null)
            {
                return InteractionInfo.Press(InteractionPromts.Install, false);
            }

            if (_welding != null && _welding.CanHandle(context))
            {
                return _welding.GetInteractionInfo(context);
            }

            if (TryGetRemovalCandidate(context, out _, out _))
            {
                return InteractionInfo.Press(
                    InteractionPromts.Remove);
            }

            return InteractionInfo.Press(
                InteractionPromts.Install,
                TryGetCandidate(context, out _, out _));
        }

        private bool TryBeginInstallation(in InteractionContext context)
        {
            if (_pending != null ||
                !TryGetCandidate(
                    context,
                    out var pickup,
                    out var attachment))
            {
                return false;
            }

            var slot = _slot;
            var anchor = slot.PlacementAnchor;
            var mountBody = attachment.MountBody;

            if (!mountBody.TryPrepareMount(
                    anchor,
                    out var apply,
                    out var restore))
            {
                return false;
            }

            // Копируем in-параметр для использования в замыкании.
            InteractionContext operationContext = context;
            long carryRevision = pickup.CarryRevision;

            var command = new ExteriorInstallationCommand(
                (IPickupReceiver)context.Actor,
                pickup,
                attachment.State,
                slot.State,
                () =>
                    this != null &&
                    _slot == slot &&
                    TryGetCandidate(
                        operationContext,
                        out var currentPickup,
                        out var currentAttachment) &&
                    currentPickup == pickup &&
                    currentAttachment == attachment &&
                    slot.PlacementAnchor == anchor &&
                    pickup.CarryRevision == carryRevision,
                apply,
                restore);

            _pendingActorId = context.Actor.Id;
            _pending = command.TryExecute;
            return true;
        }

        public bool Begin(in InteractionContext context)
        {
            if (_pending != null)
                return false;

            if (_welding != null &&
                _welding.CanHandle(context))
            {
                return _welding.Begin(context);
            }

            return TryBeginRemoval(context) ||
                TryBeginInstallation(context);
        }

        public void Complete(in InteractionContext context)
        {
            if (_welding != null && _welding.HasOperation)
            {
                _welding.TryComplete(context);
                return;
            }

            if (_pending == null || context.Actor == null || context.Actor.Id != _pendingActorId)
            {
                return;
            }

            var execute = _pending;
            _pending = null;

            execute();
        }

        public void Cancel(in InteractionContext context, InteractionCancelReason reason)
        {
            _welding?.Cancel(context);
            if (context.Actor != null &&
                context.Actor.Id == _pendingActorId)
            {
                _pending = null;
            }
        }

        private bool TryGetCandidate(in InteractionContext context, out PickupInteractable pickup, out ExteriorAttachmentAuthoring attachment)
        {
            pickup = null;
            attachment = null;

            if (!isActiveAndEnabled ||
                _slot == null ||
                !_slot.CanInstallNow ||
                context.Actor is not IPickupReceiver receiver)
            {
                return false;
            }

            pickup = receiver.HeldItem as PickupInteractable;

            return pickup != null &&
                pickup.IsCarriedBy(context.Actor.Id) &&
                pickup.TryGetComponent(out attachment) &&
                _slot.CanAccept(attachment) &&
                attachment.MountBody.CanMount(
                    _slot.PlacementAnchor);
        }
        private bool TryBeginRemoval(in InteractionContext context)
        {
            if (!TryGetRemovalCandidate(
                    context,
                    out var pickup,
                    out var attachment))
            {
                return false;
            }

            var slot = _slot;
            var anchor = slot.PlacementAnchor;

            if (!attachment.MountBody.TryPrepareRemoval(
                    anchor,
                    out var apply,
                    out var restore))
            {
                return false;
            }

            InteractionContext operationContext = context;
            long carryRevision = pickup.CarryRevision;

            var command = new ExteriorRemovalCommand(
                (IPickupReceiver)context.Actor,
                pickup,
                attachment.State,
                slot.State,
                () =>
                    this != null &&
                    _slot == slot &&
                    TryGetRemovalCandidate(
                        operationContext,
                        out var currentPickup,
                        out var currentAttachment) &&
                    currentPickup == pickup &&
                    currentAttachment == attachment &&
                    slot.PlacementAnchor == anchor &&
                    pickup.CarryRevision == carryRevision,
                apply,
                restore);

            _pending = command.TryExecute;
            _pendingActorId = context.Actor.Id;

            return true;
        }

        private bool TryGetRemovalCandidate(
            in InteractionContext context,
            out PickupInteractable pickup,
            out ExteriorAttachmentAuthoring attachment)
        {
            pickup = null;
            attachment = null;

            if (!isActiveAndEnabled ||
                _slot == null ||
                !_slot.CanRemoveNow ||
                context.Actor is not IPickupReceiver receiver ||
                receiver.IsOccupied ||
                !_slot.TryGetInstalledAttachment(out attachment) ||
                attachment.State.IsSecured)
            {
                return false;
            }

            pickup = attachment.GetComponent<PickupInteractable>();

            return pickup != null &&
                !pickup.IsHeld &&
                attachment.MountBody.CanRemove(
                    _slot.PlacementAnchor);
        }

        private void OnDisable()
        {
            _welding?.Clear();

            _pending = null;
            _pendingActorId = 0;
        }
    }
}