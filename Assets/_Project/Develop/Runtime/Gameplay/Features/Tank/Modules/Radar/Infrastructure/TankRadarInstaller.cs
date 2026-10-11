using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Configs;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Infrastructure
{
    public sealed class TankRadarInstaller : MonoInstaller
    {
        [SerializeField] private TankRadarConfig _config;
        [SerializeField] private RadarTarget _ownTarget;
        [SerializeField] private TankRadarDisplayView _view;

        public override void InstallBindings()
        {
            if (_view == null || !_view.HasValidReferences)
            {
                throw new InvalidOperationException(
                    "Radar display view or screen material is missing or invalid.");
            }

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
                    _config,
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