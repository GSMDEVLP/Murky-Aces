using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Infrastructure
{
    public sealed class DroneDetectionModule : IDroneRuntimeModule
    {
        private readonly DroneTargetScanner _scanner;

        public DroneDetectionModule(DroneTargetScanner scanner)
        {
            _scanner = scanner ??
                throw new ArgumentNullException(nameof(scanner));
        }

        public void Register(GameLoopRegistry registry)
        {
            _scanner.Clear();
            registry.RegisterGameplay(_scanner);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterGameplay(_scanner);
            _scanner.Clear();
        }
    }
}