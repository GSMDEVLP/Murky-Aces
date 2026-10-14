using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Infrastructure
{
    public sealed class TankGpsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var route = new GpsRouteState();

            Container.BindInterfacesAndSelfTo<GpsRouteState>()
                .FromInstance(route)
                .AsSingle();

            Container.Bind<GpsRouteService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<GpsScreenInteractable>()
                .FromComponentsInHierarchy()
                .AsCached()
                .NonLazy();
        }
    }
}