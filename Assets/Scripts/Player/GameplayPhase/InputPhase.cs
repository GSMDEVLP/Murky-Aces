public sealed class InputPhase
{
    private readonly PlayerInputSystem _playerInputSystem;

    public InputPhase(PlayerInputSystem playerInputSystem)
    {
        _playerInputSystem = playerInputSystem;
    }

    public void Tick(float deltaTime) => _playerInputSystem.Tick(deltaTime);
}