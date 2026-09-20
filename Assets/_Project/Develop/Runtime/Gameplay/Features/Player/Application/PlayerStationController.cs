using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{ 
    public sealed class PlayerStationController : IStationOccupant
    {
        private readonly IPlayerStationBody _body;
        private readonly PlayerStationCapabilities _capabilities;

        public bool IsInStation { get; private set; }

        public PlayerStationController(IPlayerStationBody body, PlayerStationCapabilities capabilities)
        {
            _body = body
                ?? throw new ArgumentNullException(nameof(body));

            _capabilities = capabilities
                ?? throw new ArgumentNullException(
                    nameof(capabilities));
        }

        public bool TryEnterStation(Transform seatAnchor)
        {
            if (IsInStation || seatAnchor == null)
                return false;

            if (!_capabilities.TryDisableForStation())
                return false;

            if (!_body.TryAttach(seatAnchor))
            {
                _capabilities.TryRestore();
                return false;
            }

            IsInStation = true;

            return true;
        }

        public bool TryExitStation(Transform exitAnchor)
        {
            if (!IsInStation || exitAnchor == null)
                return false;

            if (!_body.TryDetach(exitAnchor))
                return false;

            if (!_capabilities.TryRestore())
                return false;

            IsInStation = false;

            return true;
        }
    }
}