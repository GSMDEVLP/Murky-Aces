using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class PlayerInteractionActor :IInteractionActor, IPickupReceiver, IStationOccupant
    {
        private readonly IPickupReceiver _pickupReceiver;
        private readonly IStationOccupant _stationOccupant;

        public ulong Id { get; }

        public bool IsOccupied => _pickupReceiver.IsOccupied;

        public bool IsInStation => _stationOccupant.IsInStation;

        public PlayerInteractionActor(ulong id, IPickupReceiver pickupReceiver, IStationOccupant stationOccupant)
        {
            Id = id;

            _pickupReceiver = pickupReceiver
                ?? throw new ArgumentNullException(
                    nameof(pickupReceiver));

            _stationOccupant = stationOccupant
                ?? throw new ArgumentNullException(
                    nameof(stationOccupant));
        }

        public bool TryReceive(IInteractable item)
        {
            return _pickupReceiver.TryReceive(item);
        }

        public bool TryDrop()
        {
            return _pickupReceiver.TryDrop();
        }

        public bool TryEnterStation(Transform seatAnchor)
        {
            return _stationOccupant.TryEnterStation(seatAnchor);
        }

        public bool TryExitStation(Transform exitAnchor)
        {
            return _stationOccupant.TryExitStation(exitAnchor);
        }
    }
}