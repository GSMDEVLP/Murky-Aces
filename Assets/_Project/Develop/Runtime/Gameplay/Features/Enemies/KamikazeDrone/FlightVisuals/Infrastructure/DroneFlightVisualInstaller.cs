using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.FlightVisuals.Presentation;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.FlightVisuals.Infrastructure
{
    public sealed class DroneFlightVisualInstaller : MonoInstaller
    {
        [SerializeField] private EnemyRoot _root;
        [SerializeField] private DroneFlightVisualConfig _config;
        [SerializeField] private DroneFlightVisualView _view;

        public override void InstallBindings()
        {
            ValidateReferences();

            _config.Validate();
            _view.ValidateReferences(_root.transform);

            BindDependencies();
        }

        private void ValidateReferences()
        {
            if (_root == null || _config == null || _view == null ||
                !transform.IsChildOf(_root.transform) || transform == _root.transform)
            {
                throw new InvalidOperationException(
                    "Drone flight visual installer requires its own root, config and view references.");
            }
        }

        private void BindDependencies()
        {
            Container.Bind<DroneFlightVisualConfig>().FromInstance(_config).AsSingle();
            Container.Bind<DroneFlightVisualView>().FromInstance(_view).AsSingle();
            Container.Bind<IEnemyRuntimeModule>()
                .WithId(EnemyModuleGroup.Presentation)
                .To<DroneFlightVisualPresenter>()
                .AsSingle();
        }
    }
}
