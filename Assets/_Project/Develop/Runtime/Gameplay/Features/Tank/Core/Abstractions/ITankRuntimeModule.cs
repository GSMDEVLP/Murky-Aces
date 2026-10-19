using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions
{
    public interface ITankRuntimeModule
    {
        void Register(GameLoopRegistry registry);
        void Unregister(GameLoopRegistry registry);
    }
}