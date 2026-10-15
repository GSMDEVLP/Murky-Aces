using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Infrastructure
{
    public sealed class TankGpsInstaller : MonoInstaller
    {
        [SerializeField] private GpsMapView _view;
        [SerializeField] private GpsMapView _commanderView;

        public override void InstallBindings()
        {
            ValidateViews();

            var route = new GpsRouteState();

            Container.Bind<GpsPolylineRoutePlanner>().AsSingle();
            Container.BindInterfacesAndSelfTo<GpsRouteState>()
                .FromInstance(route)
                .AsSingle();

            Container.Bind<GpsRouteService>()
                .AsSingle()
                .NonLazy();

            Container.Bind<GpsScreenInteractable>()
                .FromComponentsInHierarchy()
                .AsCached()
                .NonLazy();

            InstallView(_view);

            if (_commanderView != null)
                InstallView(_commanderView);
        }

        private void ValidateViews()
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
        }

        private void InstallView(GpsMapView view)
        {
            Container.Bind<ITankRuntimeModule>()
                .FromMethod(context => new GpsPresenter(
                    context.Container.Resolve<ITankMotionBody>(),
                    view,
                    context.Container.Resolve<IGpsRouteReader>()))
                .AsCached()
                .NonLazy();
        }
    }
}