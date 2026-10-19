using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Infrastructure
{
    public sealed class TankMovementInstaller : MonoInstaller
    {
        [SerializeField] private TankMovementConfig _config;
        [SerializeField] private DynamicTankMotionBody _body;

        public override void InstallBindings()
        {
            if (_config == null)
                throw new InvalidOperationException(
                    "Tank movement config is not assigned.");

            if (_body == null)
                throw new InvalidOperationException(
                    "Tank motion body is not assigned.");

            TankMotionState state = new TankMotionState();
            TankMovement movement =
                new TankMovement(_config, state, _body);

            Container.Bind<TankMovementConfig>()
                .FromInstance(_config).AsSingle();
            Container.Bind<TankMotionState>()
                .FromInstance(state).AsSingle();
            Container.Bind<ITankMotionBody>()
                .FromInstance(_body).AsSingle();
            Container.Bind<TankMovement>()
                .FromInstance(movement).AsSingle();
            Container.Bind<ITankRuntimeModule>()
                .FromInstance(new TankMovementModule(movement));
        }
    }
}