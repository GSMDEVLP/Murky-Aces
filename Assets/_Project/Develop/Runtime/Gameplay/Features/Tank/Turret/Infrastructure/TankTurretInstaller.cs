using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public sealed class TankTurretInstaller : MonoInstaller
    {
        [SerializeField] private Camera _gunnerCamera;
        [SerializeField] private TurretAimConfig _aimConfig;
        [SerializeField] private UnityTurretRig _rig;
        [SerializeField] private Transform _gunnerCameraMount;
        [SerializeField] private Transform _gunnerEye;

        public override void InstallBindings()
        {
            if (_aimConfig == null || _aimConfig.IsValid == false ||
                _rig == null ||
                _gunnerCameraMount == null ||
                _gunnerEye == null)
            {
                throw new InvalidOperationException(
                    "Turret references are missing or invalid.");
            }

            TurretAimState state = new TurretAimState(
                initialYaw: 0f,
                initialPitch: 0f,
                initialHullYaw: _rig.HullYaw,
                initialMode: _aimConfig.DefaultReferenceMode);

            TurretMechanism mechanism = new TurretMechanism(
                state,
                _aimConfig.TraverseDegreesPerSecond,
                _aimConfig.ElevationDegreesPerSecond,
                _aimConfig.MinPitch,
                _aimConfig.MaxPitch);

            TurretAimRuntime runtime =
                new TurretAimRuntime(mechanism, _rig);
            
            var zoomState = new GunnerZoomState();
            GunnerCameraPresenter camera =
                new GunnerCameraPresenter(
                    _gunnerCameraMount,
                    _gunnerEye,
                    _rig,
                    _gunnerCamera,
                    zoomState);

            Container.Bind<GunnerZoomState>()
                .FromInstance(zoomState).AsSingle();
            Container.Bind<TurretAimRuntime>()
                .FromInstance(runtime).AsSingle();

            Container.Bind<GunnerCameraPresenter>()
                .FromInstance(camera).AsSingle();

            Container.Bind<ITankRuntimeModule>()
                .FromInstance(new TankTurretModule(runtime, camera));
        }
    }
}