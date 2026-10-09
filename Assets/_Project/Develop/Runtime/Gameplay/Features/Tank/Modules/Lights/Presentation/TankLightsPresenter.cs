using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Presentation
{
    public sealed class TankLightsPresenter : IPresentationTickable, ITankRuntimeModule
    {
        private readonly TankLights _lights;
        private readonly TankLightsView _view;

        public TankLightsPresenter(TankLights lights, TankLightsView view)
        {
            _lights = lights ??
                throw new ArgumentNullException(nameof(lights));

            _view = view ??
                throw new ArgumentNullException(nameof(view));
        }

        public void Register(GameLoopRegistry registry)
        {
            registry.RegisterPresentation(this);
            Tick(0f);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterPresentation(this);

            if (_view != null)
                _view.TurnOff();
        }

        public void Tick(float deltaTime)
        {
            if (_view != null)
                _view.ApplyState(_lights.IsEnabled);
        }
    }
}