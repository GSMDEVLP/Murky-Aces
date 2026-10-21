using System;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Application
{
    public sealed class ExteriorWeldingCommand
    {
        private readonly ExteriorAttachmentState _attachment;
        private readonly ExteriorMountSlotState _slot;

        private readonly long _attachmentRevision;
        private readonly long _slotRevision;

        private readonly Func<bool> _canWeldNow;

        private bool _isUsed;

        public bool CanExecute =>
            _isUsed == false &&
            HasValidConditions();

        public ExteriorWeldingCommand(
            ExteriorAttachmentState attachment,
            ExteriorMountSlotState slot,
            Func<bool> canWeldNow)
        {
            _attachment = attachment
                ?? throw new ArgumentNullException(nameof(attachment));

            _slot = slot
                ?? throw new ArgumentNullException(nameof(slot));

            _canWeldNow = canWeldNow
                ?? throw new ArgumentNullException(nameof(canWeldNow));

            _attachmentRevision = attachment.Revision;
            _slotRevision = slot.Revision;
        }

        public bool TryExecute()
        {
            if (_isUsed)
                return false;

            _isUsed = true;

            if (HasValidConditions() == false)
                return false;

            return _attachment.TryWeld(_slot);
        }

        private bool HasValidConditions()
        {
            return
                _attachment.RequiresWelding &&
                _attachment.IsInstalled &&
                _attachment.IsSecured == false &&
                _attachment.Revision == _attachmentRevision &&
                _slot.Revision == _slotRevision &&
                ReferenceEquals(_attachment.Slot, _slot) &&
                ReferenceEquals(_slot.Attachment, _attachment) &&
                _canWeldNow();
        }
    }
}