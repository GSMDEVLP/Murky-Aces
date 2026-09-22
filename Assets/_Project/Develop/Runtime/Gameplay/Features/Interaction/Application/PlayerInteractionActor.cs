using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class PlayerInteractionActor :IInteractionActor, IPickupReceiver, IStationOccupant, IDrivingIntentSource
    {
        private readonly IPickupReceiver _pickupReceiver;
        private readonly IStationOccupant _stationOccupant;
        private readonly IDrivingIntentSource _drivingIntentSource;

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

            _drivingIntentSource = stationOccupant as IDrivingIntentSource
                ?? throw new ArgumentException(
                    $"{nameof(stationOccupant)} must implement " +
                    $"{nameof(IDrivingIntentSource)}.",
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

        public bool TryEnterStation(Transform seatAnchor, Transform cameraAnchor)
        {
            return _stationOccupant.TryEnterStation(seatAnchor, cameraAnchor);
        }

        public bool TryExitStation(Transform exitAnchor)
        {
            return _stationOccupant.TryExitStation(exitAnchor);
        }
        public DrivingIntentSnapshot ReadDrivingIntent()
        {
            return _drivingIntentSource.ReadDrivingIntent();
        }

        public bool ConsumeExitRequest()
        {
            return _drivingIntentSource.ConsumeExitRequest();
        }
    }
}