using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.WorldGeneration.Sandbox
{
    public sealed class TestGpsInstaller : MonoInstaller
    {
        [SerializeField] private TestMapView _view;
        [SerializeField] private TestMapView _commanderView;

        public override void InstallBindings()
        {
            if (_view == null)
            {
                throw new InvalidOperationException(
                    "Driver GPS map view is not assigned.");
            }

            if (_commanderView != null && _commanderView == _view)
            {
                throw new InvalidOperationException(
                    "GPS screens must use separate map views.");
            }

            InstallView(_view);

            if (_commanderView != null)
                InstallView(_commanderView);
        }

        private void InstallView(TestMapView view)
        {
            Container.Bind<ITankRuntimeModule>()
                .FromMethod(context => new TestGpsPresenter(
                    context.Container.Resolve<ITankMotionBody>(),
                    view,
                    context.Container.Resolve<IGpsRouteReader>()))
                .AsCached()
                .NonLazy();
        }
    }
}