using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Infrastructure
{
    public sealed class RadarTargetRegistryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<RadarTargetRegistry>()
                .AsSingle()
                .NonLazy();
        }
    }
}