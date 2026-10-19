using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Infrastructure
{
    public sealed class TankLightsInstaller : MonoInstaller
    {
        [Serializable]
        private sealed class Connection
        {
            [SerializeField] private CrewRoleId _role;
            [SerializeField] private TankLightsView _view;
            [SerializeField] private LightsSwitchInteractable _switch;

            public CrewRoleId Role => _role;
            public TankLightsView View => _view;
            public LightsSwitchInteractable Switch => _switch;
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
                    "Lights connections array is null.");
            }

            for (int i = 0; i < _connections.Length; i++)
            {
                Connection current = _connections[i];

                if (current == null ||
                    current.View == null ||
                    current.View.HasValidLights == false ||
                    current.Switch == null ||
                    Enum.IsDefined(typeof(CrewRoleId), current.Role) == false)
                {
                    throw new InvalidOperationException(
                        $"Lights connection {i} is missing or invalid.");
                }

                for (int j = 0; j < i; j++)
                {
                    Connection previous = _connections[j];

                    if (current.Switch == previous.Switch ||
                        current.View == previous.View ||
                        current.View.SharesLightsWith(previous.View))
                    {
                        throw new InvalidOperationException(
                            $"Lights connections {j} and {i} share a switch, view or Light.");
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
                            $"Lights station {connection.Role} is not installed.");
                    }

                    TankLights lights = new TankLights();

                    TankLightsCommands commands =
                        new TankLightsCommands(
                            lights,
                            stations,
                            connection.Role);

                    connection.Switch.Initialize(
                        lights,
                        commands,
                        connection.View);

                    return new TankLightsPresenter(
                        lights,
                        connection.View);
                })
                .AsCached()
                .NonLazy();
        }
    }
}