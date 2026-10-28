using System;
using System.Collections.Generic;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application
{
    public sealed class RadarScanSettings
    {
        public float Range { get; }
        public float ScanInterval { get; }
        public IReadOnlyList<string> TargetTypeIds { get; }

        public RadarScanSettings(
            float range,
            float scanInterval,
            IEnumerable<string> targetTypeIds)
        {
            if (float.IsNaN(range) || float.IsInfinity(range) || range <= 0f)
                throw new ArgumentOutOfRangeException(nameof(range));

            if (float.IsNaN(scanInterval) || float.IsInfinity(scanInterval) || scanInterval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(scanInterval));

            if (targetTypeIds == null)
                throw new ArgumentNullException(nameof(targetTypeIds));

            var ids = new List<string>();
            var uniqueIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (string id in targetTypeIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !uniqueIds.Add(id))
                {
                    throw new ArgumentException(
                        "Radar target type IDs must be non-empty and unique.",
                        nameof(targetTypeIds));
                }

                ids.Add(id);
            }

            Range = range;
            ScanInterval = scanInterval;
            TargetTypeIds = ids.AsReadOnly();
        }
    }
}
