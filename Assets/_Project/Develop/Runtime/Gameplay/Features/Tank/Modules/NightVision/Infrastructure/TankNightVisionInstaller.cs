using UnityEngine;
using Zenject;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class TankNightVisionInstaller : MonoInstaller
    {
        [SerializeField] private TankNightVisionView _view;
        public override void InstallBindings()
        {

            if (_view == null || !_view.HasValidReferences)
            {
                throw new InvalidOperationException(
                    "Night vision view, volume profile or display feed is invalid.");
            }
            
            Container.Bind<TankNightVision>()
                .AsSingle()
                .NonLazy();

            Container.Bind<TankNightVisionCommands>()
                .AsSingle()
                .NonLazy();

            Container.Bind<TankNightVisionView>()
                .FromInstance(_view)
                .AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .To<TankNightVisionPresenter>()
                .AsSingle()
                .NonLazy();

            Container.Bind<NightVisionSwitchInteractable>()
                .FromComponentsInHierarchy()
                .AsCached()
                .NonLazy();
        }
    }
}