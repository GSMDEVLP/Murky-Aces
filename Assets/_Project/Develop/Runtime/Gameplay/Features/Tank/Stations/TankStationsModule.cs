using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class TankStationsModule : ITankRuntimeModule
    {
        private readonly CrewStationRegistry _stations;

        public TankStationsModule(CrewStationRegistry stations)
        {
            _stations = stations ??
                throw new ArgumentNullException(nameof(stations));
        }

        public void Register(GameLoopRegistry registry)
        {
            foreach (CrewStationController controller in _stations.Controllers)
                registry.RegisterGameplay(controller);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            foreach (CrewStationController controller in _stations.Controllers)
            {
                controller.ClearOutput();
                registry.UnregisterGameplay(controller);
            }
        }
    }
}