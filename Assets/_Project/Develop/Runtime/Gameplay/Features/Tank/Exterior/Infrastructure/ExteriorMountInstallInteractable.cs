using UnityEngine;
using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Abstractions;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class ExteriorMountInstallInteractable : InteractionTargetBehaviour, IExteriorPlacementPreviewSource
    {
        [SerializeField] private ExteriorMountSlotAuthoring _slot;

        private ExteriorWeldingInteraction _welding;
        private Func<bool> _pending;
        private ulong _pendingActorId;
        private IInteractionAction _installAction;
        private IInteractionAction _removeAction;
        private IInteractionAction _weldAction;

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

            _installAction = new InteractionAction(
                InteractionActionId.Install,
                c => GetInstallInfo(c),
                c => GetInstallInfo(c).IsAvailable && TryBeginInstallation(c),
                c => CompletePending(c),
                (c, reason) => CancelOperation(c, reason),
                () => Revision);
            _removeAction = new InteractionAction(
                InteractionActionId.Remove,
                c => GetRemovalInfo(c),
                c => GetRemovalInfo(c).IsAvailable && TryBeginRemoval(c),
                c => CompletePending(c),
                (c, reason) => CancelOperation(c, reason),
                () => Revision);
            _weldAction = new InteractionAction(
                InteractionActionId.Weld,
                c => GetWeldingInfo(c),
                c => GetWeldingInfo(c).IsAvailable && _welding.Begin(c),
                c => { if (this != null) _welding.TryComplete(c); },
                (c, reason) => CancelOperation(c, reason),
                () => Revision);
        }

        public override void CollectActions(
            in InteractionContext context, List<IInteractionAction> actions)
        {
            if (!IsActive || _installAction == null)
                return;
            actions.Add(_installAction);
            actions.Add(_removeAction);
            actions.Add(_weldAction);
        }

        public bool TryGetPlacementPreview(in InteractionContext context,
            out GameObject previewPrefab, out Transform placementAnchor)
        {
            previewPrefab = null;
            placementAnchor = null;
            if (!TryGetCandidate(context, out _, out var attachment))
                return false;
            previewPrefab = attachment.Config.PreviewPrefab;
            placementAnchor = _slot.PlacementAnchor;
            return previewPrefab != null && placementAnchor != null;
        }

        private InteractionInfo GetInstallInfo(in InteractionContext context)
        {
            bool available = this != null && IsActive && _pending == null &&
                !_welding.HasOperation && TryGetCandidate(context, out _, out _);
            return InteractionInfo.Press(InteractionPromts.Install, available,
                this != null && _slot != null ? _slot.State.Revision : 0);
        }

        private InteractionInfo GetRemovalInfo(in InteractionContext context)
        {
            bool available = this != null && IsActive && _pending == null &&
                !_welding.HasOperation && TryGetRemovalCandidate(context, out _, out _);
            return InteractionInfo.Press(InteractionPromts.Remove, available,
                this != null && _slot != null ? _slot.State.Revision : 0);
        }

        private InteractionInfo GetWeldingInfo(in InteractionContext context)
        {
            if (this == null || !IsActive || _pending != null || _welding == null)
                return InteractionInfo.Press(InteractionPromts.Weld, false);

            InteractionInfo info = _welding.GetInteractionInfo(context);
            long revision = _slot != null ? _slot.State.Revision : 0;
            return info.Mode == InteractionMode.Hold
                ? InteractionInfo.Hold(info.Prompt, info.HoldDuration, info.IsAvailable, revision)
                : InteractionInfo.Press(info.Prompt, info.IsAvailable, revision);
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

        private void CompletePending(in InteractionContext context)
        {
            if (_pending == null || context.Actor == null || context.Actor.Id != _pendingActorId)
            {
                return;
            }

            var execute = _pending;
            _pending = null;
            _pendingActorId = 0;

            execute();
        }

        private void CancelOperation(in InteractionContext context, InteractionCancelReason reason)
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

        protected override void OnDisable()
        {
            base.OnDisable();
            _welding?.Clear();

            _pending = null;
            _pendingActorId = 0;
        }
    }
}
