using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private CrewStationView _stationView;
        [SerializeField, Min(0.1f)] private float _holdDuration = 1f;
        [SerializeField] private InteractionPromts _prompt = InteractionPromts.Enter;

        private CrewStationController _controller;

        [Inject]
        public void Construct(CrewStationRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            if (_stationView == null)
                throw new InvalidOperationException(
                    $"{nameof(CrewStationView)} is not assigned.");

            if (registry.TryGet(_stationView.RoleId, out _controller) == false)
            {
                throw new InvalidOperationException(
                    $"Station controller for role '{_stationView.RoleId}' was not found.");
            }
        }

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            bool isAvailable = isActiveAndEnabled &&_controller != null &&
                (
                    _controller.CanBeginEnter(context.Actor) ||
                    _controller.CanContinueEnter(context.Actor)
                );

            return InteractionInfo.Hold(_prompt, _holdDuration, isAvailable);
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
