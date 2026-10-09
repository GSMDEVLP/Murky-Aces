using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Presentation
{
    public sealed class TankNightVisionPresenter : IPresentationTickable, ITankRuntimeModule
    {
        private readonly TankNightVision _nightVision;
        private readonly TankNightVisionView _view;

        public TankNightVisionPresenter(
            TankNightVision nightVision,
            TankNightVisionView view)
        {
            _nightVision = nightVision ??
                throw new ArgumentNullException(nameof(nightVision));

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
                _view.ApplyState(_nightVision.IsEnabled);
        }
    }
}