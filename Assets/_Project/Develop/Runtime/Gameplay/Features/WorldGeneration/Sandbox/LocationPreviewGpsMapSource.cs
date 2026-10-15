using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.WorldGeneration.Sandbox
{
    public sealed class LocationPreviewGpsMapSource : MonoBehaviour, IGpsMapSource
    {
        [SerializeField] private LocationGenerationPreview _source;

        private LocationLayout _cachedLayout;
        private GpsMapData _cachedMap;

        public bool TryGetMap(out GpsMapData map)
        {
            map = null;

            if (_source == null)
                return false;

            if (_source.Layout == null)
                _source.Generate();

            LocationLayout layout = _source.Layout;

            if (layout == null)
                return false;

            if (!ReferenceEquals(_cachedLayout, layout))
            {
                _cachedMap = new GpsMapData(
                    layout.Center,
                    layout.Size,
                    layout.RoadPoints);

                _cachedLayout = layout;
            }

            map = _cachedMap;
            return true;
        }
    }
}