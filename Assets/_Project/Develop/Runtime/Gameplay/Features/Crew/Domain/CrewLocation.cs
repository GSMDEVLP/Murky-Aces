using System;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Crew.Domain
{
    public sealed class CrewLocation
    {
        public CrewLocationKind Kind { get; }
        public CrewRoleId? StationRole { get; }

        public bool IsInTank => Kind != CrewLocationKind.Outside;

        public static CrewLocation Outside { get; } =
            new CrewLocation(CrewLocationKind.Outside, null);

        public static CrewLocation Interior { get; } =
            new CrewLocation(CrewLocationKind.Interior, null);

        private CrewLocation(
            CrewLocationKind kind,
            CrewRoleId? stationRole)
        {
            Kind = kind;
            StationRole = stationRole;
        }

        public static CrewLocation AtStation(CrewRoleId role)
        {
            if (!Enum.IsDefined(typeof(CrewRoleId), role))
            {
                throw new ArgumentOutOfRangeException(nameof(role));
            }

            return new CrewLocation(CrewLocationKind.Station, role);
        }
    }
}