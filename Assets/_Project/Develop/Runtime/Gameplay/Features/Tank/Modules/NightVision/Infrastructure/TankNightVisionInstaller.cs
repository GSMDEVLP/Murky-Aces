using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class TankNightVisionInstaller : MonoInstaller
    {
        [Serializable]
        private sealed class Connection
        {
            [SerializeField] private CrewRoleId _role;
            [SerializeField] private TankNightVisionView _view;
            [SerializeField] private NightVisionSwitchInteractable _switch;

            public CrewRoleId Role => _role;
            public TankNightVisionView View => _view;
            public NightVisionSwitchInteractable Switch => _switch;
        }

        [SerializeField]
        private Connection[] _connections = Array.Empty<Connection>();

        public override void InstallBindings()
        {
            ValidateConnections();

            foreach (Connection connection in _connections)
                InstallConnection(connection);
        }

        private void ValidateConnections()
        {
            if (_connections == null)
            {
                throw new InvalidOperationException(
                    "Night vision connections array is null.");
            }

            for (int i = 0; i < _connections.Length; i++)
            {
                Connection current = _connections[i];

                if (current == null ||
                    current.View == null ||
                    current.View.HasValidReferences == false ||
                    current.Switch == null ||
                    Enum.IsDefined(typeof(CrewRoleId), current.Role) == false)
                {
                    throw new InvalidOperationException(
                        $"Night vision connection {i} is missing or invalid.");
                }

                for (int j = 0; j < i; j++)
                {
                    Connection previous = _connections[j];

                    if (current.Switch == previous.Switch ||
                        current.View == previous.View ||
                        current.View.EffectVolume.gameObject ==
                            previous.View.EffectVolume.gameObject ||
                        current.View.DisplayFeed == previous.View.DisplayFeed)
                    {
                        throw new InvalidOperationException(
                            $"Night vision connections {j} and {i} share a switch, view, volume object or feed.");
                    }
                }
            }
        }

        private void InstallConnection(Connection connection)
        {
            Container.Bind<ITankRuntimeModule>()
                .FromMethod(context =>
                {
                    CrewStationRegistry stations =
                        context.Container.Resolve<CrewStationRegistry>();

                    if (stations.TryGet(connection.Role, out _) == false)
                    {
                        throw new InvalidOperationException(
                            $"Night vision station {connection.Role} is not installed.");
                    }

                    TankNightVision nightVision =
                        new TankNightVision();

                    TankNightVisionCommands commands =
                        new TankNightVisionCommands(
                            nightVision,
                            stations,
                            connection.Role);

                    connection.Switch.Initialize(
                        nightVision,
                        commands,
                        connection.View);

                    return new TankNightVisionPresenter(
                        nightVision,
                        connection.View);
                })
                .AsCached()
                .NonLazy();
        }
    }
}