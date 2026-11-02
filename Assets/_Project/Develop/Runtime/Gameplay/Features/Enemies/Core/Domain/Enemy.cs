using System;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Domain
{
    public sealed class Enemy : IEnemy
    {
        public ulong Id { get; }

        public bool IsAlive { get; private set; } = true;

        public Enemy(ulong id)
        {
            if (id == 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Id = id;
        }

        internal bool TryDie()
        {
            if (IsAlive == false)
                return false;

            IsAlive = false;
            return true;
        }
    }
}
