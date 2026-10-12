using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Application
{
    public sealed class TankLightsCommands
    {
        private readonly TankLights _lights;
        private readonly CrewStationRegistry _stations;
        private readonly CrewRoleId _role;

        public TankLightsCommands(
            TankLights lights,
            CrewStationRegistry stations,
            CrewRoleId role)
        {
            _lights = lights ??
                throw new ArgumentNullException(nameof(lights));

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

            _lights.SetEnabled(enabled);
            return true;
        }
    }
}