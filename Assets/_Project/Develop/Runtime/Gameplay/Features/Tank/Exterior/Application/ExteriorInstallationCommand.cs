using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Application
{
    public sealed class ExteriorInstallationCommand
    {
        private readonly IPickupReceiver _receiver;
        private readonly IHoldableItem _expectedItem;

        private readonly ExteriorAttachmentState _attachment;
        private readonly ExteriorMountSlotState _slot;

        private readonly long _attachmentRevision;
        private readonly long _slotRevision;

        private readonly Func<bool> _canApply;
        private readonly Action _apply;
        private readonly Func<bool> _restore;

        private bool _isUsed;

        public ExteriorInstallationCommand(
            IPickupReceiver receiver,
            IHoldableItem expectedItem,
            ExteriorAttachmentState attachment,
            ExteriorMountSlotState slot,
            Func<bool> canApply,
            Action apply,
            Func<bool> restore)
        {
            _receiver = receiver
                ?? throw new ArgumentNullException(nameof(receiver));

            _expectedItem = expectedItem
                ?? throw new ArgumentNullException(nameof(expectedItem));

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

            if (!ReferenceEquals(_receiver.HeldItem, _expectedItem) ||
                !_expectedItem.IsHeld ||
                _attachment.Revision != _attachmentRevision ||
                _slot.Revision != _slotRevision ||
                !_slot.CanAccept(_attachment) ||
                !_canApply())
            {
                return false;
            }

            if (!_attachment.TryInstall(_slot))
                return false;

            long installedRevision = _attachment.Revision;
            bool completed = false;

            try
            {
                _apply();

                if (!_receiver.TryReleaseHeldItem(_expectedItem))
                    return false;

                completed = true;
                return true;
            }
            finally
            {
                if (!completed)
                {
                    bool restored = false;

                    try
                    {
                        restored = _restore();
                    }
                    finally
                    {
                        if (!_attachment.TryRollbackInstall(
                                _slot,
                                installedRevision))
                        {
                            throw new InvalidOperationException(
                                "Installation state rollback failed.");
                        }
                    }

                    if (!restored)
                    {
                        throw new InvalidOperationException(
                            "Carry pose rollback failed.");
                    }
                }
            }
        }
    }
}