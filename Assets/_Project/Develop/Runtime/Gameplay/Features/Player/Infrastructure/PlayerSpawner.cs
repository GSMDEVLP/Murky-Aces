using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
public sealed class PlayerSpawner
{
    private readonly PlayerFacade.Factory _factory;
    private readonly PlayerRegistry _playerRegistry;
    private readonly GameLoopRegistry _gameLoopRegistry;

    public PlayerSpawner(
        PlayerFacade.Factory factory,
        PlayerRegistry playerRegistry,
        GameLoopRegistry gameLoopRegistry)
    {
        _factory = factory;
        _playerRegistry = playerRegistry;
        _gameLoopRegistry = gameLoopRegistry;
    }

    public PlayerFacade Spawn(Vector3 position, Quaternion rotation)
    {
        PlayerFacade player = _factory.Create();

        player.transform.SetPositionAndRotation(position, rotation);

        _playerRegistry.Register(player);
        RegisterPhases(player);

        return player;
    }

    public void Despawn(PlayerFacade player)
    {
        UnregisterPhases(player);
        _playerRegistry.Unregister(player);

        Object.Destroy(player.gameObject);
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
