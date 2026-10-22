using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain
{
    public sealed class ExteriorAttachmentState
    {
        public string CompatibleMountTypeId { get; }
        public bool RequiresWelding { get; }

        public ExteriorMountSlotState Slot { get; private set; }

        public bool IsInstalled => Slot != null;
        public bool IsSecured { get; private set; }
        public long Revision { get; private set; }
        public bool IsDestroyed { get; private set; }

        public ExteriorAttachmentState(
            string compatibleMountTypeId,
            bool requiresWelding)
        {
            if (string.IsNullOrWhiteSpace(compatibleMountTypeId))
            {
                throw new ArgumentException(
                    "Compatible mount type ID is required.",
                    nameof(compatibleMountTypeId));
            }

            CompatibleMountTypeId = compatibleMountTypeId;
            RequiresWelding = requiresWelding;
        }

        public bool TryInstall(ExteriorMountSlotState slot)
        {
            if (slot == null || !slot.TryOccupy(this))
                return false;

            Slot = slot;
            IsSecured = !RequiresWelding;
            Revision++;

            return true;
        }

        public bool TryRemove(ExteriorMountSlotState expectedSlot)
        {
            if (!IsInstalled ||
                IsSecured ||
                !ReferenceEquals(Slot, expectedSlot))
            {
                return false;
            }

            if (!Slot.TryRelease(this))
                return false;

            Slot = null;
            IsSecured = false;
            Revision++;

            return true;
        }
        internal bool TryRollbackInstall(ExteriorMountSlotState expectedSlot, long expectedRevision)
        {
            if (!IsInstalled ||
                Revision != expectedRevision ||
                !ReferenceEquals(Slot, expectedSlot) ||
                !Slot.TryRelease(this, allowSecured: true))
            {
                return false;
            }

            Slot = null;
            IsSecured = false;
            Revision++;

            return true;
        }

        public bool TryWeld(ExteriorMountSlotState expectedSlot)
        {
            if (!RequiresWelding ||
                !IsInstalled ||
                IsSecured ||
                !ReferenceEquals(Slot, expectedSlot))
            {
                return false;
            }

            IsSecured = true;
            Revision++;

            return true;
        }
        public bool TryDestroy()
        {
            if (IsDestroyed)
                return true;

            if (IsInstalled &&
                Slot.TryRelease(this, allowSecured: true) == false)
            {
                return false;
            }

            Slot = null;
            IsSecured = false;
            IsDestroyed = true;
            Revision++;

            return true;
        }
    }
}