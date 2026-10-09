using System;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Application
{
    public sealed class TankNightVisionCommands
    {
        private readonly TankNightVision _nightVision;
        private readonly CrewStationRegistry _stations;

        public TankNightVisionCommands(TankNightVision nightVision, CrewStationRegistry stations)
        {
            _nightVision = nightVision ??
                throw new ArgumentNullException(nameof(nightVision));

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

        public bool TrySetEnabled(IInteractionActor actor, bool enabled)
        {
            if (!CanControl(actor))
                return false;

            _nightVision.SetEnabled(enabled);
            return true;
        }
    }
}