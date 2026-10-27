using System;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public sealed class TankTurretModule : ITankRuntimeModule
    {
        private readonly TurretAimRuntime _runtime;
        private readonly GunnerCameraPresenter _cameraPresenter;

        public TankTurretModule(
            TurretAimRuntime runtime,
            GunnerCameraPresenter cameraPresenter)
        {
            _runtime = runtime ??
                throw new ArgumentNullException(nameof(runtime));

            _cameraPresenter = cameraPresenter ??
                throw new ArgumentNullException(nameof(cameraPresenter));
        }

        public void Register(GameLoopRegistry registry)
        {
            registry.RegisterGameplay(_runtime);
            registry.RegisterPresentation(_cameraPresenter);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            _runtime.ClearInput();
            _cameraPresenter.ResetZoom();
            registry.UnregisterGameplay(_runtime);
            registry.UnregisterPresentation(_cameraPresenter);
        }
    }
}
