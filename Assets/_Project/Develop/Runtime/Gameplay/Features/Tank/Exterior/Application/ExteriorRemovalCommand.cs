using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Application
{
    public sealed class ExteriorRemovalCommand
    {
        private readonly IPickupReceiver _receiver;
        private readonly IInteractable _item;
        private readonly IHoldableItem _holdableItem;

        private readonly ExteriorAttachmentState _attachment;
        private readonly ExteriorMountSlotState _slot;

        private readonly long _attachmentRevision;
        private readonly long _slotRevision;

        private readonly Func<bool> _canApply;
        private readonly Action _apply;
        private readonly Func<bool> _restore;

        private bool _isUsed;

        public ExteriorRemovalCommand(
            IPickupReceiver receiver,
            IInteractable item,
            ExteriorAttachmentState attachment,
            ExteriorMountSlotState slot,
            Func<bool> canApply,
            Action apply,
            Func<bool> restore)
        {
            _receiver = receiver
                ?? throw new ArgumentNullException(nameof(receiver));

            _item = item
                ?? throw new ArgumentNullException(nameof(item));

            _holdableItem = item as IHoldableItem
                ?? throw new ArgumentException(
                    "Item must support carrying.", nameof(item));

            _attachment = attachment
                ?? throw new ArgumentNullException(nameof(attachment));

            _slot = slot
                ?? throw new ArgumentNullException(nameof(slot));

            _canApply = canApply
                ?? throw new ArgumentNullException(nameof(canApply));

            _apply = apply
                ?? throw new ArgumentNullException(nameof(apply));

            _restore = restore
                ?? throw new ArgumentNullException(nameof(restore));

            _attachmentRevision = attachment.Revision;
            _slotRevision = slot.Revision;
        }

        public bool TryExecute()
        {
            if (_isUsed)
                return false;

            _isUsed = true;

            if (_receiver.IsOccupied ||
                _holdableItem.IsHeld ||
                _attachment.IsSecured ||
                _attachment.Revision != _attachmentRevision ||
                _slot.Revision != _slotRevision ||
                !ReferenceEquals(_attachment.Slot, _slot) ||
                !ReferenceEquals(_slot.Attachment, _attachment) ||
                !_canApply())
            {
                return false;
            }

            if (!_attachment.TryRemove(_slot))
                return false;

            bool completed = false;

            try
            {
                _apply();

                if (!_receiver.TryReceive(_item))
                    return false;

                completed = true;
                return true;
            }
            finally
            {
                if (!completed)
                    Rollback();
            }
        }

        private void Rollback()
        {
            bool restored = false;

            try
            {
                restored = _restore();
            }
            finally
            {
                if (!_attachment.TryInstall(_slot))
                {
                    throw new InvalidOperationException(
                        "Removal state rollback failed.");
                }
            }

            if (!restored)
            {
                throw new InvalidOperationException(
                    "Mounted pose rollback failed.");
            }
        }
    }
}