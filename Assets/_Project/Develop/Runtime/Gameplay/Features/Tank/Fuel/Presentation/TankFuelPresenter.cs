using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Presentation
{
    public sealed class TankFuelPresenter : IPresentationTickable, ITankRuntimeModule
    {
        private readonly TankFuelState _fuel;
        private readonly TankFuelView _view;

        public TankFuelPresenter(TankFuelState fuel, TankFuelView view)
        {
            _fuel = fuel ??
                throw new ArgumentNullException(nameof(fuel));

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
        }

        public void Tick(float deltaTime)
        {
            if (_view != null)
                _view.ApplyFill(_fuel.NormalizedAmount);
        }
    }
}