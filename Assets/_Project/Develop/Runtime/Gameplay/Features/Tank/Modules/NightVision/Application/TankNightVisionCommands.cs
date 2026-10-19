using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Application
{
    public sealed class TankNightVisionCommands
    {
        private readonly TankNightVision _nightVision;
        private readonly CrewStationRegistry _stations;
        private readonly CrewRoleId _role;

        public TankNightVisionCommands(TankNightVision nightVision, CrewStationRegistry stations, CrewRoleId role)
        {
            _nightVision = nightVision ??
                throw new ArgumentNullException(nameof(nightVision));

            _stations = stations ??
                throw new ArgumentNullException(nameof(stations));

            _role = role;
        }

        public bool CanControl(IInteractionActor actor)
        {
            return actor != null &&
                   _stations.TryGet(
                       _role,
                       out CrewStationController station) &&
                   station.IsOccupiedBy(actor.Id);
        }

        public bool TrySetEnabled(
            IInteractionActor actor,
            bool enabled)
        {
            if (CanControl(actor) == false)
                return false;

            _nightVision.SetEnabled(enabled);
            return true;
        }
    }
}