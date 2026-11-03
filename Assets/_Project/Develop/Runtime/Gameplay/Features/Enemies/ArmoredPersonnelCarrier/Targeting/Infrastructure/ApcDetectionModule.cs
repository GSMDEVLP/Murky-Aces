using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Infrastructure
{
    public sealed class ApcDetectionModule : IEnemyRuntimeModule
    {
        private readonly ApcTargetScanner _scanner;
        private readonly ApcTargetSelector _selector;
        private bool _isActive;

        public ApcDetectionModule(
            ApcTargetScanner scanner,
            ApcTargetSelector selector)
        {
            _scanner = scanner ??
                throw new ArgumentNullException(nameof(scanner));
            _selector = selector ??
                throw new ArgumentNullException(nameof(selector));
        }

        public void Activate(GameLoopRegistry registry)
        {
            if (_isActive)
                return;

            _scanner.Clear();
            _selector.Clear();
            _scanner.OnScanCompleted += _selector.Refresh;
            registry.RegisterGameplay(_scanner);
            _isActive = true;
        }

        public void Deactivate(GameLoopRegistry registry)
        {
            if (_isActive == false)
                return;

            _isActive = false;
            registry.UnregisterGameplay(_scanner);
            _scanner.OnScanCompleted -= _selector.Refresh;
            _scanner.Clear();
            _selector.Clear();
        }
    }
}
