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

        public TankLightsCommands(
            TankLights lights,
            CrewStationRegistry stations)
        {
            _lights = lights ??
                throw new ArgumentNullException(nameof(lights));

            _stations = stations ??
                throw new ArgumentNullException(nameof(stations));
        }

        public bool CanControl(IInteractionActor actor)
        {
            return actor != null &&
                   _stations.TryGet(
                       CrewRoleId.Driver,
                       out CrewStationController driver) &&
                   driver.IsOccupiedBy(actor.Id);
        }

        public bool TrySetEnabled(
            IInteractionActor actor,
            bool enabled)
        {
            if (!CanControl(actor))
                return false;

            _lights.SetEnabled(enabled);
            return true;
        }
    }
}