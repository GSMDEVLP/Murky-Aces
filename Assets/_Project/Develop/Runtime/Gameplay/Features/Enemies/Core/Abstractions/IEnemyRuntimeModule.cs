using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions
{
    public interface IEnemyRuntimeModule
    {
        void Activate(GameLoopRegistry registry);
        void Deactivate(GameLoopRegistry registry);
    }
}
