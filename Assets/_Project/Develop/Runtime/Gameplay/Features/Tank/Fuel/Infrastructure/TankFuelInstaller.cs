using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Configs;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class TankFuelInstaller : MonoInstaller
    {
        [SerializeField] private TankFuelView _view;
        [SerializeField] private TankFuelConfig _config;

        public override void InstallBindings()
        {
            ValidateReference();

            var state = new TankFuelState(_config.Capacity, _config.Capacity * _config.InitialFill);

            Container.Bind<TankFuelConfig>()
                .FromInstance(_config)
                .AsSingle();

            Container.Bind<TankFuelState>()
                .FromInstance(state)
                .AsSingle();
                
            Container.Bind<TankFuelView>()
                .FromInstance(_view)
                .AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .To<TankFuelPresenter>()
                .AsSingle()
                .NonLazy();
        }

        private void ValidateReference()
        {
            if (_config == null || !_config.IsValid)
            {
                throw new InvalidOperationException(
                    "Tank fuel config is missing or invalid.");
            }
            if (_view == null || !_view.HasValidScreen)
            {
                throw new InvalidOperationException(
                    "Tank fuel view or screen material is missing or invalid.");
            }
        }
    }
}