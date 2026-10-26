using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Infrastructure
{
    public sealed class NightVisionSwitchInteractable : SingleActionInteractionTarget
    {
        private TankNightVision _nightVision;
        private TankNightVisionCommands _commands;
        private TankNightVisionView _view;

        public void Initialize(TankNightVision nightVision, TankNightVisionCommands commands, TankNightVisionView view)
        {
            _nightVision = nightVision ??
                throw new ArgumentNullException(nameof(nightVision));

            _commands = commands ??
                throw new ArgumentNullException(nameof(commands));

            _view = view ??
                throw new ArgumentNullException(nameof(view));
        }

        public override InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            return InteractionInfo.Press(InteractionPromts.Toggle, CanInteract(context.Actor));
        }

        public override bool Begin(in InteractionContext context)
        {
            return CanInteract(context.Actor);
        }

        public override void Complete(in InteractionContext context)
        {
            if (!CanInteract(context.Actor))
                return;

            _commands.TrySetEnabled(
                context.Actor,
                !_nightVision.IsEnabled);
        }

        public override void Cancel(
            in InteractionContext context,
            InteractionCancelReason reason)
        {
        }

        private bool CanInteract(IInteractionActor actor)
        {
            return isActiveAndEnabled &&
                   _nightVision != null &&
                   _commands != null &&
                   _view != null &&
                   _view.IsAvailable &&
                   _commands.CanControl(actor);
        }
    }
}
