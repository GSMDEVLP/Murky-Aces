using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Infrastructure
{
    public sealed class DroneDetectionModule : IDroneRuntimeModule
    {
        private readonly DroneTargetScanner _scanner;
        private readonly DroneTargetSelector _selector;

        public DroneDetectionModule(
            DroneTargetScanner scanner,
            DroneTargetSelector selector)
        {
            _scanner = scanner ??
                throw new ArgumentNullException(nameof(scanner));

            _selector = selector ??
                throw new ArgumentNullException(nameof(selector));
        }

        public void Register(GameLoopRegistry registry)
        {
            _scanner.Clear();
            _selector.Clear();

            _scanner.OnScanCompleted += _selector.Refresh;
            registry.RegisterGameplay(_scanner);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterGameplay(_scanner);
            _scanner.OnScanCompleted -= _selector.Refresh;

            _scanner.Clear();
            _selector.Clear();
        }
    }
}