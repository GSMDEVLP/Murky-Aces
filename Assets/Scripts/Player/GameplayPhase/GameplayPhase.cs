public sealed class GameplayPhase
{
    private readonly PlayerMovement _playerMovement;
    private readonly PlayerLook _playerLook;

    public GameplayPhase(PlayerMovement playerMovement, PlayerLook playerLook)
    {
        _playerMovement = playerMovement;
        _playerLook = playerLook;
    }

    public void Tick()
    {
        _playerLook.Tick();
    }

    public void FixedTick()
    {
        _playerLook.FixedTick();
        _playerMovement.FixedTick();
    }
}
