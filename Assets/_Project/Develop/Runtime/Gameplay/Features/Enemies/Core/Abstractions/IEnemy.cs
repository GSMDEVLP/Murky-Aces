namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions
{
    public interface IEnemy
    {
        ulong Id { get; }
        bool IsAlive { get; }
    }
}
