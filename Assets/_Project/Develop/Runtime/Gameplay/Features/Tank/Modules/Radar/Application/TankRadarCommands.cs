using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application
{
    public sealed class TankRadarCommands
    {
        private readonly TankRadar _radar;
        private readonly TankRadarScanner _scanner;
        private readonly CrewStationRegistry _stations;

        public TankRadarCommands(
            TankRadar radar,
            TankRadarScanner scanner,
            CrewStationRegistry stations)
        {
            _radar = radar ??
                throw new ArgumentNullException(nameof(radar));

            _scanner = scanner ??
                throw new ArgumentNullException(nameof(scanner));

            _stations = stations ??
                throw new ArgumentNullException(nameof(stations));
        }

        public bool CanControl(IInteractionActor actor)
        {
            return actor != null &&
                   _scanner.IsRegistered &&
                   _stations.TryGet(
                       CrewRoleId.Driver,
                       out CrewStationController driver) &&
                   driver.IsOccupiedBy(actor.Id);
        }

        public bool TrySetEnabled(IInteractionActor actor, bool enabled)
        {
            if (!CanControl(actor))
                return false;

            int contactsBefore = _radar.Contacts.Count;

            _radar.SetEnabled(enabled);

            UnityEngine.Debug.Log(
                $"Radar: IsEnabled={_radar.IsEnabled}, " +
                $"Contacts: {contactsBefore} -> {_radar.Contacts.Count}");

            return true;
        }

        public bool TryToggle(IInteractionActor actor)
        {
            return TrySetEnabled(actor, !_radar.IsEnabled);
        }
    }
}