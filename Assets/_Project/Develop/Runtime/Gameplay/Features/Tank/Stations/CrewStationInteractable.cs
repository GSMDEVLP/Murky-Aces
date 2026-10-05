using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private CrewStationView _stationView;

        [SerializeField]
        private InteractionMode _interactionMode =
            InteractionMode.Press;

        [SerializeField, Min(0.1f)]
        private float _holdDuration = 1f;

        [SerializeField]
        private bool _allowExitInteraction;

        private CrewStationController _controller;

        [Inject]
        public void Construct(CrewStationRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            if (_stationView == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CrewStationView)} is not assigned.");
            }

            if (!registry.TryGet(
                    _stationView.RoleId,
                    out _controller))
            {
                throw new InvalidOperationException(
                    $"Station controller for role " +
                    $"'{_stationView.RoleId}' was not found.");
            }
        }

        public InteractionInfo GetInteractionInfo(
            in InteractionContext context)
        {
            bool canExit = CanExit(context.Actor);
            bool canEnter = CanEnter(context.Actor);

            InteractionPromts prompt = canExit
                ? InteractionPromts.Exit
                : InteractionPromts.Enter;

            bool available = canEnter || canExit;

            if (_interactionMode == InteractionMode.Hold)
            {
                return InteractionInfo.Hold(
                    prompt,
                    _holdDuration,
                    available);
            }

            return InteractionInfo.Press(
                prompt,
                available);
        }

        public bool Begin(
            in InteractionContext context)
        {
            if (!isActiveAndEnabled ||
                _controller == null)
            {
                return false;
            }

            if (CanExit(context.Actor))
                return true;

            return _controller.TryBeginEnter(
                context.Actor);
        }

        public void Complete(
            in InteractionContext context)
        {
            if (_controller == null)
                return;

            if (CanExit(context.Actor))
            {
                _controller.TryExit(context.Actor);
                return;
            }

            _controller.TryCompleteEnter(
                context.Actor);
        }

        public void Cancel(
            in InteractionContext context,
            InteractionCancelReason reason)
        {
            if (_controller == null)
                return;

            if (CanExit(context.Actor))
                return;

            _controller.TryCancelEnter(
                context.Actor);
        }

        private bool CanEnter(
            IInteractionActor actor)
        {
            return isActiveAndEnabled &&
                   _controller != null &&
                   (_controller.CanBeginEnter(actor) ||
                    _controller.CanContinueEnter(actor));
        }

        private bool CanExit(
            IInteractionActor actor)
        {
            return isActiveAndEnabled &&
                   _allowExitInteraction &&
                   _controller != null &&
                   _controller.CanExit(actor);
        }

        private void OnValidate()
        {
            _holdDuration = Mathf.Max(
                0.1f,
                _holdDuration);
        }
    }
}