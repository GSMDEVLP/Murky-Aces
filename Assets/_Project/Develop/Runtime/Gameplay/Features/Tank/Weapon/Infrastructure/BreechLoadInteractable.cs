using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Ammunition;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Infrastructure
{
    public sealed class BreechLoadInteractable : MonoBehaviour, IInteractable
    {
        private TankWeaponView _weapon;
        private GunLoadingService _loading;

        [Inject]
        public void Construct(TankWeaponView weapon, GunLoadingService loading)
        {
            _weapon = weapon
                ?? throw new ArgumentNullException(nameof(weapon));

            _loading = loading
                ?? throw new ArgumentNullException(nameof(loading));
        }

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            bool available =
                TryGetShell(
                    context,
                    out IHoldableItem heldItem,
                    out ShellItem shell) &&
                _loading.CanLoad(
                    context.Actor,
                    heldItem,
                    shell.Definition);

            return InteractionInfo.Press(
                InteractionPromts.Load,
                available);
        }

        public bool Begin(in InteractionContext context)
        {
            return GetInteractionInfo(context).IsAvailable;
        }

        public void Complete(in InteractionContext context)
        {
            if (!TryGetShell(context, out IHoldableItem heldItem, out ShellItem shell))
            {
                return;
            }

            if (!_loading.TryLoad(context.Actor, heldItem, shell.Definition))
            {
                return;
            }

            GameObject shellObject = shell.gameObject;
            shellObject.SetActive(false);
            Destroy(shellObject);
        }

        public void Cancel(
            in InteractionContext context,
            InteractionCancelReason reason)
        {
        }

        private bool TryGetShell(
            in InteractionContext context,
            out IHoldableItem heldItem,
            out ShellItem shell)
        {
            heldItem = null;
            shell = null;

            if (!isActiveAndEnabled ||
                _loading == null ||
                _weapon == null ||
                !_weapon.isActiveAndEnabled)
            {
                return false;
            }

            if (context.Actor is not IPickupReceiver receiver)
                return false;

            heldItem = receiver.HeldItem;

            if (heldItem is not Component itemComponent ||
                itemComponent == null)
            {
                return false;
            }

            shell = itemComponent.GetComponent<ShellItem>();
            return shell != null;
        }
    }
}