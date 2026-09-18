using System.Collections.Generic;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application
{
    public sealed class PlayerRegistry
    {
        private readonly List<PlayerFacade> _players = new();

        public IReadOnlyList<PlayerFacade> Players => _players;

        public void Register(PlayerFacade player) => _players.Add(player);
        public void Unregister(PlayerFacade player) => _players.Remove(player);
    }
}
