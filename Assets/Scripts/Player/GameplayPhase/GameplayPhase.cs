public sealed class GameplayPhase
{
    private readonly PlayerMovement _playerMovement;
    private readonly PlayerLook _playerLook;
    private readonly PlayerInteraction _playerInteraction;

    public GameplayPhase(PlayerMovement playerMovement, PlayerLook playerLook, PlayerInteraction playerInteraction)
    {
        _playerMovement = playerMovement;
        _playerLook = playerLook;
        _playerInteraction = playerInteraction;
    }

    public void Tick(float deltaTime)
    {
        _playerLook.Tick(deltaTime);
        _playerInteraction.Tick(deltaTime);
    }

    public void FixedTick(float fixedDeltaTime)
    {

        _playerLook.FixedTick(fixedDeltaTime);
        _playerMovement.FixedTick(fixedDeltaTime);
    }
}
