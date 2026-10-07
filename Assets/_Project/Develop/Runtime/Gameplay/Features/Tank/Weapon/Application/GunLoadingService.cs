using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Ammunition;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application
{
    public sealed class GunLoadingService
    {
        private readonly GunChamber _chamber;
        private ShellDefinition _loadedShell;

        public bool IsLoaded => _chamber.IsLoaded;

        public string LoadedShellId => _chamber.LoadedShellId;

        public ShellDefinition LoadedShell
        {
            get
            {
                if (_loadedShell == null ||
                    !_chamber.IsLoaded ||
                    !string.Equals(
                        _chamber.LoadedShellId,
                        _loadedShell.Id,
                        StringComparison.Ordinal))
                {
                    return null;
                }

                return _loadedShell;
            }
        }

        public GunLoadingService(GunChamber chamber)
        {
            _chamber = chamber
                ?? throw new ArgumentNullException(nameof(chamber));
        }

        public bool CanLoad(
            IInteractionActor actor,
            IHoldableItem expectedItem,
            ShellDefinition definition)
        {
            if (actor is not IPickupReceiver receiver ||
                actor is not ITankInteriorOccupant interior ||
                !interior.IsInInterior)
            {
                return false;
            }

            if (expectedItem == null ||
                !ReferenceEquals(receiver.HeldItem, expectedItem) ||
                !expectedItem.IsHeld ||
                definition == null ||
                !definition.IsValid)
            {
                return false;
            }

            return _chamber.CanLoad(
                definition.Id,
                definition.CompatibleWeaponId);
        }

        public bool TryLoad(
            IInteractionActor actor,
            IHoldableItem expectedItem,
            ShellDefinition definition)
        {
            if (!CanLoad(actor, expectedItem, definition))
                return false;

            string shellId = definition.Id;

            if (!_chamber.TryLoad(
                    shellId,
                    definition.CompatibleWeaponId))
            {
                return false;
            }

            var receiver = (IPickupReceiver)actor;

            if (!receiver.TryReleaseHeldItem(expectedItem))
            {
                _chamber.TryUnload(shellId);
                return false;
            }

            _loadedShell = definition;
            return true;
        }
    }
}