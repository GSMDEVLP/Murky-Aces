using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank
{
    public interface ITankRuntimeModule
    {
        void Register(GameLoopRegistry registry);
        void Unregister(GameLoopRegistry registry);
    }
}