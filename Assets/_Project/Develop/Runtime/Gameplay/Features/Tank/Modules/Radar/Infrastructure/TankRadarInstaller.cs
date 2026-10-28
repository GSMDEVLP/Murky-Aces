using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Infrastructure
{
    public sealed class TankRadarInstaller : MonoInstaller
    {
        [SerializeField] private TankRadarConfig _config;
        [SerializeField] private RadarTarget _ownTarget;
        [SerializeField] private TankRadarDisplayView _view;

        public override void InstallBindings()
        {
            if (_config == null || !_config.IsValid)
            {
                throw new InvalidOperationException(
                    "Radar config is missing or invalid.");
            }

            if (_view == null || !_view.HasValidReferences)
            {
                throw new InvalidOperationException(
                    "Radar display view or screen material is missing or invalid.");
            }

            var targetTypeIds = new string[_config.TargetTypes.Count];

            for (int i = 0; i < targetTypeIds.Length; i++)
                targetTypeIds[i] = _config.TargetTypes[i].Id;

            var scanSettings = new RadarScanSettings(
                _config.Range,
                _config.ScanInterval,
                targetTypeIds);

            var radar = new TankRadar();
            radar.SetEnabled(_config.InitiallyEnabled);

            Container.Bind<TankRadarConfig>()
                .FromInstance(_config)
                .AsSingle();

            Container.Bind<TankRadar>()
                .FromInstance(radar)
                .AsSingle();

            Container.Bind<TankRadarScanner>()
                .FromMethod(context => new TankRadarScanner(
                    radar,
                    context.Container.Resolve<RadarTargetRegistry>(),
                    context.Container.Resolve<ITankMotionBody>(),
                    scanSettings,
                    _ownTarget != null
                        ? (ulong?)_ownTarget.TargetId
                        : null))
                .AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .To<TankRadarScanner>()
                .FromResolve()
                .AsCached()
                .NonLazy();

            Container.Bind<TankRadarCommands>()
                .AsSingle()
                .NonLazy();

            Container.Bind<RadarSwitchInteractable>()
                .FromComponentsInHierarchy()
                .AsCached()
                .NonLazy();

            Container.Bind<TankRadarDisplayView>()
                .FromInstance(_view)
                .AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .To<TankRadarPresenter>()
                .AsSingle()
                .NonLazy();
        }
    }
}
