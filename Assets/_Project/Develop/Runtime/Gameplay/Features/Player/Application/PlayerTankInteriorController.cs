using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Application;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerTankInteriorController :
        ITankInteriorOccupant
    {
        private readonly CrewTransitionCoordinator _transitions;
        private readonly IPlayerInteriorBody _body;

        public CrewLocation Current => _transitions.Current;
        public long Revision => _transitions.Revision;

        public bool IsInInterior =>
            Current.Kind == CrewLocationKind.Interior;

        public bool CanUseHatch =>
            _body.CanTeleport &&
            (Current.Kind == CrewLocationKind.Outside ||
             Current.Kind == CrewLocationKind.Interior);

        public PlayerTankInteriorController(
            CrewTransitionCoordinator transitions,
            IPlayerInteriorBody body)
        {
            _transitions = transitions
                ?? throw new ArgumentNullException(nameof(transitions));

            _body = body
                ?? throw new ArgumentNullException(nameof(body));
        }

        public bool TryEnterTank(
            long expectedRevision,
            Transform insideAnchor)
        {
            return TryMove(
                expectedRevision,
                CrewLocationKind.Outside,
                CrewLocation.Interior,
                insideAnchor);
        }

        public bool TryExitTank(
            long expectedRevision,
            Transform outsideAnchor)
        {
            return TryMove(
                expectedRevision,
                CrewLocationKind.Interior,
                CrewLocation.Outside,
                outsideAnchor);
        }

        private bool TryMove(
            long expectedRevision,
            CrewLocationKind requiredSource,
            CrewLocation destination,
            Transform anchor)
        {
            if (Revision != expectedRevision ||
                Current.Kind != requiredSource ||
                !CanUseHatch ||
                anchor == null)
            {
                return false;
            }

            if (!_body.TryPrepareTeleport(
                    anchor,
                    out Func<bool> tryApply,
                    out Func<bool> tryRollback))
            {
                return false;
            }

            return _transitions.TryTransition(
                expectedRevision,
                destination,
                tryApply,
                tryRollback);
        }
    }
}