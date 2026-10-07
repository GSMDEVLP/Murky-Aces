using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Domain
{
    public sealed class GunChamber
    {
        public string WeaponId { get; }

        public string LoadedShellId { get; private set; }

        public bool IsLoaded => LoadedShellId != null;

        public GunChamber(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                throw new ArgumentException(
                    "Weapon ID must be specified.",
                    nameof(weaponId));
            }

            WeaponId = weaponId;
        }

        public bool CanLoad(string shellId, string compatibleWeaponId)
        {
            if (IsLoaded)
                return false;

            if (string.IsNullOrWhiteSpace(shellId))
                return false;

            return string.Equals(
                WeaponId,
                compatibleWeaponId,
                StringComparison.Ordinal);
        }

        public bool TryLoad(string shellId, string compatibleWeaponId)
        {
            if (!CanLoad(shellId, compatibleWeaponId))
                return false;

            LoadedShellId = shellId;
            return true;
        }
        
        public bool TryUnload(string expectedShellId)
        {
            if (!IsLoaded ||
                !string.Equals(
                    LoadedShellId,
                    expectedShellId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            LoadedShellId = null;
            return true;
        }
    }
}