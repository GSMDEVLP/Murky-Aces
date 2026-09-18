using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Infrastructure.Debug;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Infrastructure
{
    public sealed class TankInstaller : MonoInstaller
    {
        [SerializeField] private TankMovementConfig _movementConfig;
        [SerializeField] private KinematicTankMotionBody _motionBody;

        public override void InstallBindings()
        {
            if (_movementConfig == null)
                throw new InvalidOperationException(
                    $"{nameof(TankMovementConfig)} is not assigned.");

            if (_motionBody == null)
                throw new InvalidOperationException(
                    $"{nameof(KinematicTankMotionBody)} is not assigned.");

            Container.Bind<TankRoot>().FromComponentOnRoot().AsSingle();

            Container.Bind<TankMovementConfig>().FromInstance(_movementConfig).AsSingle();

            Container.Bind<TankMotionState>().AsSingle();

            Container.Bind<ITankMotionBody>().FromInstance(_motionBody).AsSingle();

            Container.Bind<TankMovement>().AsSingle();

            Container.Bind<TankDebugInputSource>().FromComponentInHierarchy().AsSingle().NonLazy();
        }
    }
}