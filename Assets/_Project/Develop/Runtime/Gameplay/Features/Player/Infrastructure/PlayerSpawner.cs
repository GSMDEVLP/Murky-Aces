using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Presentation;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
    public sealed class PlayerSpawner
    {
        private readonly PlayerFacade.Factory _factory;
        private readonly PlayerRegistry _playerRegistry;
        private readonly GameLoopRegistry _gameLoopRegistry;
        private readonly GameplayCameraRig _cameraRig;

        public PlayerSpawner(
            PlayerFacade.Factory factory,
            PlayerRegistry playerRegistry,
            GameLoopRegistry gameLoopRegistry,
            GameplayCameraRig cameraRig)
        {
            _factory = factory;
            _playerRegistry = playerRegistry;
            _gameLoopRegistry = gameLoopRegistry;
            _cameraRig = cameraRig;
        }

        public PlayerFacade Spawn(Vector3 position, Quaternion rotation)
        {
            PlayerFacade player = _factory.Create();

            player.transform.SetPositionAndRotation(position, rotation);

            _playerRegistry.Register(player);
            RegisterPhases(player);

            return player;
        }

        public bool Despawn(PlayerFacade player)
        {
            if (player == null)
                return false;

            if (player.Gameplay.TryPrepareDespawn() == false)
            {
                Debug.LogError(
                    "Player despawn cancelled: station release failed.",
                    player);
                return false;
            }

            _cameraRig.TryUnbindPlayerCameraPivot(player.CameraPivot);
            UnregisterPhases(player);
            _playerRegistry.Unregister(player);
            Object.Destroy(player.gameObject);

            return true;
        }

        private void RegisterPhases(PlayerFacade player)
        {
            _gameLoopRegistry.RegisterInput(player.Input);
            _gameLoopRegistry.RegisterGameplay(player.Gameplay);
            _gameLoopRegistry.RegisterFixedGameplay(player.Gameplay);
            _gameLoopRegistry.RegisterPresentation(player.Presentation);
        }

        private void UnregisterPhases(PlayerFacade player)
        {
            _gameLoopRegistry.UnregisterInput(player.Input);
            _gameLoopRegistry.UnregisterGameplay(player.Gameplay);
            _gameLoopRegistry.UnregisterFixedGameplay(player.Gameplay);
            _gameLoopRegistry.UnregisterPresentation(player.Presentation);
        }
    }
}
