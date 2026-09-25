using UnityEngine;
using Zenject;
using System;
using _Project.Develop.Runtime.Gameplay.Presentation;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class PlayerSpawningInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameplayCameraRig _gameplayCameraRig;
        public override void InstallBindings()
        {
            ValidateReferences();
            Container.Bind<GameLoopRegistry>().AsSingle();
            Container.Bind<PlayerRegistry>().AsSingle();
            Container.Bind<PlayerSpawner>().AsSingle();

            Container.BindFactory<PlayerFacade, PlayerFacade.Factory>()
                .FromSubContainerResolve()
                .ByNewContextPrefab(_playerPrefab);
            Container.Bind<GameplayCameraRig>().FromInstance(_gameplayCameraRig).AsSingle();
            
        }

        private void ValidateReferences()
        {
            if (_gameplayCameraRig == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameplayCameraRig)} is not assigned.");
            }
        }
    }
}
