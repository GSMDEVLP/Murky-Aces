using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class TankLightsInstaller : MonoInstaller
    {
        [SerializeField] private TankLightsView _view;

        public override void InstallBindings()
        {
            if (_view == null || !_view.HasValidLights)
            {
                throw new InvalidOperationException(
                    "Tank lights view or light references are not assigned.");
            }

            Container.Bind<TankLights>()
                .AsSingle()
                .NonLazy();

            Container.Bind<TankLightsCommands>()
                .AsSingle()
                .NonLazy();

            Container.Bind<TankLightsView>()
                .FromInstance(_view)
                .AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .To<TankLightsPresenter>()
                .AsSingle()
                .NonLazy();

            Container.Bind<LightsSwitchInteractable>()
                .FromComponentsInHierarchy()
                .AsCached()
                .NonLazy();
        }
    }
}