using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Presentation
{
    public sealed class CrewStationDisplayPresenter : IPresentationTickable, ITankRuntimeModule
    {
        private readonly CrewStationController _station;
        private readonly StationDisplayFeed _display;

        private bool _wasOccupied;

        public CrewStationDisplayPresenter(CrewStationController station, StationDisplayFeed display)
        {
            _station = station
                ?? throw new ArgumentNullException(nameof(station));

            _display = display
                ?? throw new ArgumentNullException(nameof(display));
        }

        public void Register(GameLoopRegistry registry)
        {
            _wasOccupied = false;

            registry.RegisterPresentation(this);
            Tick(0f);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterPresentation(this);

            if (_display != null)
                _display.Deactivate();

            _wasOccupied = false;
        }

        public void Tick(float deltaTime)
        {
            bool occupied = _station.IsOccupied;

            if (occupied == _wasOccupied)
                return;

            _wasOccupied = occupied;

            if (_display == null)
                return;

            if (occupied == false)
            {
                _display.Deactivate();
                return;
            }

            if (_display.TryActivate() == false)
            {
                Debug.LogWarning(
                    $"Station {_station.RoleId} was occupied " +
                    "without an active display.");
            }
        }
    }
}