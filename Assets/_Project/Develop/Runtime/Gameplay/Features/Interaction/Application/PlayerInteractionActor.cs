using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class PlayerInteractionActor :IInteractionActor, IPickupReceiver, IStationOccupant, IDrivingIntentSource, IStationIntentSource, IGunnerIntentSource
    {
        private readonly IPickupReceiver _pickupReceiver;
        private readonly IStationOccupant _stationOccupant;
        private readonly IDrivingIntentSource _drivingIntentSource;
        private readonly IStationIntentSource _stationIntentSource;
        private readonly IGunnerIntentSource _gunnerIntentSource;

        public ulong Id { get; }

        public bool IsOccupied => _pickupReceiver.IsOccupied;

        public bool IsInStation => _stationOccupant.IsInStation;

        public PlayerInteractionActor(ulong id, IPickupReceiver pickupReceiver, IStationOccupant stationOccupant, IDrivingIntentSource drivingIntentSource, IGunnerIntentSource gunnerIntentSource)
        {
            Id = id;

            _pickupReceiver = pickupReceiver
                ?? throw new ArgumentNullException(
                    nameof(pickupReceiver));

            _stationOccupant = stationOccupant
                ?? throw new ArgumentNullException(
                    nameof(stationOccupant));

            _drivingIntentSource = drivingIntentSource
                ?? throw new ArgumentNullException(
                    nameof(drivingIntentSource));

            _stationIntentSource =
                stationOccupant as IStationIntentSource
                ?? throw new ArgumentException(
                    $"{nameof(stationOccupant)} must implement " +
                    $"{nameof(IStationIntentSource)}.",
                    nameof(stationOccupant));

            _gunnerIntentSource = gunnerIntentSource
                ?? throw new ArgumentNullException(
                    nameof(gunnerIntentSource));
        }

        public bool TryReceive(IInteractable item)
        {
            return _pickupReceiver.TryReceive(item);
        }

        public bool TryDrop()
        {
            return _pickupReceiver.TryDrop();
        }

        public bool TryEnterStation(Transform seatAnchor,Transform cameraAnchor,IInteractionScope interactionScope, StationCapabilityProfile capabilityProfile)
        {
            return _stationOccupant.TryEnterStation(
                seatAnchor,
                cameraAnchor,
                interactionScope,
                capabilityProfile);
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
            return _stationIntentSource.ConsumeExitRequest();
        }

        public GunnerIntentSnapshot ConsumeGunnerIntent()
        {
            return _gunnerIntentSource.ConsumeGunnerIntent();
        }
    }
}