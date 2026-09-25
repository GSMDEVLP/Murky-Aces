using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationRegistry
    {
        private readonly Dictionary<CrewRoleId, CrewStationController> _controllersByRole;

        private readonly IReadOnlyList<CrewStationController> _controllers;

        public IReadOnlyList<CrewStationController> Controllers => _controllers;

        public CrewStationRegistry(List<CrewStationController> controllers)
        {
            if (controllers == null)
            {
                throw new ArgumentNullException(
                    nameof(controllers));
            }

            if (controllers.Count == 0)
            {
                throw new ArgumentException(
                    "At least one crew station is required.",
                    nameof(controllers));
            }

            _controllersByRole = new Dictionary<CrewRoleId, CrewStationController>();

            List<CrewStationController> copy = new List<CrewStationController>(controllers.Count);

            for (int i = 0; i < controllers.Count; i++)
            {
                CrewStationController controller = controllers[i];

                if (controller == null)
                {
                    throw new ArgumentException(
                        "Crew station controller cannot be null.",
                        nameof(controllers));
                }

                CrewRoleId roleId = controller.RoleId;

                if (_controllersByRole.ContainsKey(roleId))
                {
                    throw new InvalidOperationException(
                        $"Duplicate crew station role: {roleId}.");
                }

                _controllersByRole.Add(roleId, controller);
                copy.Add(controller);
            }

            _controllers = copy.AsReadOnly();
        }

        public bool TryGet(CrewRoleId roleId, out CrewStationController controller)
        {
            return _controllersByRole.TryGetValue(roleId, out controller);
        }

        public bool IsOccupied(CrewRoleId roleId)
        {
            return TryGet(roleId, out CrewStationController controller) && controller.IsOccupied;
        }
    }
}