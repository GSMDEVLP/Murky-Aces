using UnityEngine;

public sealed class PlayerSpawner
{
    private readonly PlayerFacade.Factory _factory;
    private readonly PlayerRegistry _registry;

    public PlayerSpawner(PlayerFacade.Factory factory, PlayerRegistry registry)
    {
        _factory = factory;
        _registry = registry;
    }

    public PlayerFacade Spawn(Vector3 position, Quaternion rotation)
    {
        PlayerFacade player = _factory.Create();
        player.transform.SetPositionAndRotation(position, rotation);
        _registry.Register(player);
        return player;
    }

    public void Despawn(PlayerFacade player)
    {
        _registry.Unregister(player);
        Object.Destroy(player.gameObject);
    }
}