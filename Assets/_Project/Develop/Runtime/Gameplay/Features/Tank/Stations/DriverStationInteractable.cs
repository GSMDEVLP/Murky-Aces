using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class DriverStationInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private InteractionPromts _prompt = InteractionPromts.Enter;

        private DriverStationController _controller;

        [Inject]
        public void Construct(DriverStationController controller)
        {
            _controller = controller;
        }

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            bool isAvailable =
                isActiveAndEnabled &&
                _controller != null &&
                (
                    _controller.CanBeginEnter(context.Actor) ||
                    _controller.CanContinueEnter(context.Actor)
                );

            return InteractionInfo.Hold(_prompt, 1f, isAvailable);
        }

        public bool Begin(in InteractionContext context)
        {
            return isActiveAndEnabled &&
                   _controller != null &&
                   _controller.TryBeginEnter(context.Actor);
        }

        public void Complete(in InteractionContext context)
        {
            _controller?.TryCompleteEnter(context.Actor);
        }

        public void Cancel(in InteractionContext context, InteractionCancelReason reason)
        {
            _controller?.TryCancelEnter(context.Actor);
        }
    }
}
