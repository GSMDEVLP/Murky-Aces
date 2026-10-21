using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain
{
    public sealed class ExteriorMountSlotState
    {
        public string MountTypeId { get; }

        public ExteriorAttachmentState Attachment
        {
            get;
            private set;
        }

        public bool IsOccupied => Attachment != null;

        public long Revision { get; private set; }

        public ExteriorMountSlotState(string mountTypeId)
        {
            if (string.IsNullOrWhiteSpace(mountTypeId))
            {
                throw new ArgumentException(
                    "Mount type ID is required.",
                    nameof(mountTypeId));
            }

            MountTypeId = mountTypeId;
        }

        public bool CanAccept(ExteriorAttachmentState attachment)
        {
            return attachment != null &&
                !IsOccupied &&
                !attachment.IsInstalled &&
                string.Equals(
                    MountTypeId,
                    attachment.CompatibleMountTypeId,
                    StringComparison.Ordinal);
        }

        internal bool TryOccupy(ExteriorAttachmentState attachment)
        {
            if (!CanAccept(attachment))
                return false;

            Attachment = attachment;
            Revision++;

            return true;
        }

        internal bool TryRelease(ExteriorAttachmentState expectedAttachment, bool allowSecured = false)
        {
            if (!IsOccupied ||
                !ReferenceEquals(Attachment, expectedAttachment) ||
                !ReferenceEquals(expectedAttachment.Slot, this) ||
                (!allowSecured && expectedAttachment.IsSecured))
            {
                return false;
            }

            Attachment = null;
            Revision++;

            return true;
        }
    }
}