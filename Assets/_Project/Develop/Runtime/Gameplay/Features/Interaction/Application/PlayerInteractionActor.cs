using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Application
{
    public sealed class PlayerInteractionActor : 
        IInteractionActor, 
        IPickupReceiver, 
        IStationOccupant, 
        ITankInteriorOccupant, 
        IDrivingIntentSource, 
        IStationIntentSource, 
        IGunnerIntentSource

    {
        private readonly IPickupReceiver _pickupReceiver;
        private readonly IStationOccupant _stationOccupant;
        private readonly ITankInteriorOccupant _tankInteriorOccupant;
        private readonly IDrivingIntentSource _drivingIntentSource;
        private readonly IStationIntentSource _stationIntentSource;
        private readonly IGunnerIntentSource _gunnerIntentSource;
        
        public ulong Id { get; }

        public bool IsOccupied => _pickupReceiver.IsOccupied;

        public bool IsInStation => _stationOccupant.IsInStation;

        public CrewLocation Current =>
            _tankInteriorOccupant.Current;

        public long Revision =>
            _tankInteriorOccupant.Revision;

        public bool IsInInterior =>
            _tankInteriorOccupant.IsInInterior;

        public bool CanUseHatch =>
            _tankInteriorOccupant.CanUseHatch;
            
        public PlayerInteractionActor(ulong id, 
            IPickupReceiver pickupReceiver, 
            IStationOccupant stationOccupant, 
            IDrivingIntentSource drivingIntentSource, 
            IGunnerIntentSource gunnerIntentSource,
            ITankInteriorOccupant tankInteriorOccupant)
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
            _tankInteriorOccupant = tankInteriorOccupant
                 ?? throw new ArgumentNullException(nameof(tankInteriorOccupant));

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

        public bool TryEnterStation(
            long expectedRevision,
            CrewRoleId roleId,
            Transform seatAnchor,
            Transform cameraAnchor,
            IInteractionScope interactionScope,
            StationCapabilityProfile capabilityProfile)
        {
            return _stationOccupant.TryEnterStation(
                expectedRevision,
                roleId,
                seatAnchor,
                cameraAnchor,
                interactionScope,
                capabilityProfile);
        }

        public bool TryRollbackStationEntry()
        {
            return _stationOccupant.TryRollbackStationEntry();
        }
        public bool TryExitStation(Transform exitAnchor)
        {
            return _stationOccupant.TryExitStation(exitAnchor);
        }

        public bool TryForceExitStation(Transform exitAnchor)
        {
            return _stationOccupant.TryForceExitStation(exitAnchor);
        }


        public bool TryEnterTank(
            long expectedRevision,
            Transform insideAnchor)
        {
            return _tankInteriorOccupant.TryEnterTank(
                expectedRevision,
                insideAnchor);
        }

        public bool TryExitTank(
            long expectedRevision,
            Transform outsideAnchor)
        {
            return _tankInteriorOccupant.TryExitTank(
                expectedRevision,
                outsideAnchor);
        }

        public bool TrySetReleaseHandler(Func<bool> releaseHandler)
        {
            return _stationOccupant.TrySetReleaseHandler(releaseHandler);
        }

        public void ClearReleaseHandler(Func<bool> releaseHandler)
        {
            _stationOccupant.ClearReleaseHandler(releaseHandler);
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