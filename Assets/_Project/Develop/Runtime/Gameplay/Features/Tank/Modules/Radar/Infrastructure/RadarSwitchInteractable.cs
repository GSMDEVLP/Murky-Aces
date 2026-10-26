using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Infrastructure
{
    public sealed class RadarSwitchInteractable : SingleActionInteractionTarget
    {
        private TankRadarCommands _commands;

        [Inject]
        public void Construct(TankRadarCommands commands)
        {
            _commands = commands ??
                throw new ArgumentNullException(nameof(commands));
        }

        public override InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            return InteractionInfo.Press(
                InteractionPromts.Toggle,
                CanInteract(context.Actor));
        }

        public override bool Begin(in InteractionContext context)
        {
            return CanInteract(context.Actor);
        }

        public override void Complete(in InteractionContext context)
        {
            if (!CanInteract(context.Actor))
                return;

            _commands.TryToggle(context.Actor);
        }

        public override void Cancel(
            in InteractionContext context,
            InteractionCancelReason reason)
        {
        }

        private bool CanInteract(IInteractionActor actor)
        {
            return isActiveAndEnabled &&
                   _commands != null &&
                   _commands.CanControl(actor);
        }
    }
}
