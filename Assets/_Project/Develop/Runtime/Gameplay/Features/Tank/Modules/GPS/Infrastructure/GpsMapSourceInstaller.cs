using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Infrastructure
{
    public sealed class GpsMapSourceInstaller : MonoInstaller
    {
        [SerializeField] private MonoBehaviour _source;

        public override void InstallBindings()
        {
            if (_source == null ||
                !(_source is IGpsMapSource mapSource))
            {
                throw new InvalidOperationException(
                    "GPS map source must implement IGpsMapSource.");
            }

            Container.Bind<IGpsMapSource>()
                .FromInstance(mapSource)
                .AsSingle();
        }
    }
}