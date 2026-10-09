using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class LightsSwitchInteractable : MonoBehaviour, IInteractable
    {

        private TankLights _lights;
        private TankLightsCommands _commands;
        private TankLightsView _view;

        [Inject]
        public void Construct(TankLights lights, TankLightsCommands commands, TankLightsView view)
        {
            _lights = lights ??
                throw new ArgumentNullException(nameof(lights));

            _commands = commands ??
                throw new ArgumentNullException(nameof(commands));

            _view = view ??
                throw new ArgumentNullException(nameof(view));


        }

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            return InteractionInfo.Press(
                InteractionPromts.Toggle,
                CanInteract(context.Actor));
        }

        public bool Begin(in InteractionContext context)
        {
            return CanInteract(context.Actor);
        }

        public void Complete(in InteractionContext context)
        {
            if (!CanInteract(context.Actor))
                return;

            _commands.TrySetEnabled(
                context.Actor,
                !_lights.IsEnabled);
        }

        public void Cancel(
            in InteractionContext context,
            InteractionCancelReason reason)
        {
        }

        private bool CanInteract(IInteractionActor actor)
        {
            return isActiveAndEnabled &&
                   _view != null &&
                   _view.isActiveAndEnabled &&
                   _lights != null &&
                   _commands != null &&
                   _commands.CanControl(actor);
        }
    }
}