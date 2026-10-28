using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Application;

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

            var settings = new TankMovementSettings(
                _config.ForwardMaxSpeed,
                _config.ReverseMaxSpeed,
                _config.ForwardAcceleration,
                _config.ReverseAcceleration,
                _config.TurnAcceleration,
                _config.Deceleration,
                _config.BrakeDeceleration,
                _config.TurnSpeed,
                _config.DirectionSwitchDelay);

            TankMovement movement =
                new TankMovement(settings, _body);

            Container.Bind<ITankMotionBody>()
                .FromInstance(_body).AsSingle();
            Container.Bind<TankMovement>()
                .FromInstance(movement).AsSingle();
            Container.Bind<ITankRuntimeModule>()
                .FromInstance(new TankMovementModule(movement));
        }
    }
}
