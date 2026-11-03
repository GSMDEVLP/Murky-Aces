using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Infrastructure
{
    public sealed class ApcDetectionInstaller : MonoInstaller
    {
        [SerializeField] private ApcDetectionConfig _config;
        [SerializeField] private ApcVisionSensor _sensor;

        private ApcTargetScanner _scanner;
        private ApcTargetSelector _selector;

        public override void InstallBindings()
        {
            if (_config == null || _sensor == null)
            {
                throw new InvalidOperationException(
                    "APC detection requires Config and Sensor.");
            }

            _sensor.Initialize(_config);

            Container.Bind<IApcVisionSensor>()
                .FromInstance(_sensor)
                .AsSingle();

            Container.Bind<ApcTargetScanner>()
                .FromMethod(CreateScanner)
                .AsSingle();

            Container.Bind<ApcTargetSelector>()
                .FromMethod(CreateSelector)
                .AsSingle();

            Container.Bind<IEnemyRuntimeModule>()
                .WithId(EnemyModuleGroup.Gameplay)
                .To<ApcDetectionModule>()
                .AsSingle();
        }

        private ApcTargetScanner CreateScanner(InjectContext context)
        {
            _scanner = new ApcTargetScanner(
                context.Container.Resolve<CombatTargetRegistry>(),
                _sensor,
                _config.ScanInterval);

            return _scanner;
        }

        private ApcTargetSelector CreateSelector(InjectContext context)
        {
            _selector = new ApcTargetSelector(
                context.Container.Resolve<ApcTargetScanner>(),
                context.Container.Resolve<IApcVisionSensor>());

            return _selector;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Print Detected Targets")]
        private void PrintDetectedTargets()
        {
            if (!UnityEngine.Application.isPlaying || _scanner == null)
            {
                Debug.Log("Enter Play Mode first.", this);
                return;
            }

            Debug.Log(
                $"Detected targets: {_scanner.DetectedTargets.Count}",
                this);

            foreach (var target in _scanner.DetectedTargets)
                Debug.Log($"{target.Kind} | ID: {target.TargetId}", this);

            var selected = _selector?.CurrentTarget;
            Debug.Log(
                selected != null
                    ? $"Selected target: {selected.Kind} | ID: {selected.TargetId}"
                    : "Selected target: None",
                this);
        }
#endif
    }
}
