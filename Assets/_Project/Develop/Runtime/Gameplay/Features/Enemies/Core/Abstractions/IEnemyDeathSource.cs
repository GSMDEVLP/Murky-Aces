using System;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions
{
    public interface IEnemyDeathSource
    {
        event Action DeathRequested;
    }
}
