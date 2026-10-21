using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    public sealed class ExteriorWeldingInteraction
    {
        private readonly ExteriorMountSlotAuthoring _slot;
        private readonly Func<bool> _canInteract;

        private ExteriorWeldingCommand _command;
        private ulong _actorId;
        private float _duration;

        public bool HasOperation => _command != null;

        public ExteriorWeldingInteraction(
            ExteriorMountSlotAuthoring slot,
            Func<bool> canInteract)
        {
            _slot = slot;

            _canInteract = canInteract
                ?? throw new ArgumentNullException(nameof(canInteract));
        }

        public bool CanHandle(in InteractionContext context)
        {
            return HasOperation ||
                TryGetTool(context, out _);
        }

        public InteractionInfo GetInteractionInfo(
            in InteractionContext context)
        {
            if (HasOperation)
            {
                bool available =
                    context.Actor != null &&
                    context.Actor.Id == _actorId &&
                    _command.CanExecute;

                return InteractionInfo.Hold(
                    InteractionPromts.Weld,
                    _duration,
                    available);
            }

            if (TryGetCandidate(context, out _, out ExteriorAttachmentAuthoring attachment))
            {
                return InteractionInfo.Hold(
                    InteractionPromts.Weld,
                    attachment.Config.WeldingDuration);
            }

            return InteractionInfo.Press(
                InteractionPromts.Weld,
                false);
        }

        public bool Begin(in InteractionContext context)
        {
            if (HasOperation ||
                TryGetCandidate(
                    context,
                    out PickupInteractable tool,
                    out ExteriorAttachmentAuthoring attachment) == false)
            {
                return false;
            }

            InteractionContext operationContext = context;
            long carryRevision = tool.CarryRevision;

            // Сохраняем конкретное положение установки.
            UnityEngine.Transform anchor = _slot.PlacementAnchor;

            _command = new ExteriorWeldingCommand(
                attachment.State,
                _slot.State,
                () =>
                    TryGetCandidate(
                        operationContext,
                        out PickupInteractable currentTool,
                        out ExteriorAttachmentAuthoring currentAttachment) &&
                    currentTool == tool &&
                    currentAttachment == attachment &&
                    _slot.PlacementAnchor == anchor &&
                    tool.CarryRevision == carryRevision);

            _actorId = context.Actor.Id;
            _duration = attachment.Config.WeldingDuration;

            if (_command.CanExecute)
                return true;

            Clear();
            return false;
        }

        public bool TryComplete(in InteractionContext context)
        {
            if (HasOperation == false ||
                context.Actor == null ||
                context.Actor.Id != _actorId)
            {
                return false;
            }

            try
            {
                return _command.TryExecute();
            }
            finally
            {
                Clear();
            }
        }

        public void Cancel(in InteractionContext context)
        {
            if (context.Actor != null &&
                context.Actor.Id == _actorId)
            {
                Clear();
            }
        }

        public void Clear()
        {
            _command = null;
            _actorId = 0;
            _duration = 0f;
        }

        private bool TryGetCandidate(
            in InteractionContext context,
            out PickupInteractable tool,
            out ExteriorAttachmentAuthoring attachment)
        {
            tool = null;
            attachment = null;

            if (_canInteract() == false ||
                _slot == null ||
                _slot.CanInstallNow == false ||
                TryGetTool(context, out tool) == false ||
                _slot.TryGetInstalledAttachment(out attachment) == false)
            {
                return false;
            }

            return
                attachment.Config != null &&
                attachment.Config.IsValid &&
                attachment.State.RequiresWelding &&
                attachment.State.IsSecured == false &&
                ReferenceEquals(attachment.State.Slot, _slot.State);
        }

        private static bool TryGetTool(
            in InteractionContext context,
            out PickupInteractable pickup)
        {
            pickup = null;

            if (context.Actor is not IPickupReceiver receiver)
                return false;

            pickup = receiver.HeldItem as PickupInteractable;

            return
                pickup != null &&
                pickup.isActiveAndEnabled &&
                pickup.IsCarriedBy(context.Actor.Id) &&
                pickup.TryGetComponent(out WeldingTool tool) &&
                tool.isActiveAndEnabled;
        }
    }
}