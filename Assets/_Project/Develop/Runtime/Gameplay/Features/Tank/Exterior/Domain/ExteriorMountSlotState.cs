using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain
{
    public sealed class ExteriorMountSlotState
    {
        public ExteriorMountType MountType { get; }

        public ExteriorAttachmentState Attachment
        {
            get;
            private set;
        }

        public bool IsOccupied => Attachment != null;

        public long Revision { get; private set; }

        public ExteriorMountSlotState(ExteriorMountType mountType)
        {
            if (mountType == ExteriorMountType.None ||
                !Enum.IsDefined(typeof(ExteriorMountType), mountType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(mountType),
                    mountType,
                    "A valid mount type is required.");
            }

            MountType = mountType;
        }

        public bool CanAccept(ExteriorAttachmentState attachment)
        {
            return attachment != null &&
                !attachment.IsDestroyed &&
                !IsOccupied &&
                !attachment.IsInstalled &&
                MountType == attachment.CompatibleMountType;
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