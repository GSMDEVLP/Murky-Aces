using _Project.Develop.Runtime.Core.GameLoop.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions
{
    public interface IDroneRuntimeModule
    {
        void Register(GameLoopRegistry registry);
        void Unregister(GameLoopRegistry registry);
    }
}
