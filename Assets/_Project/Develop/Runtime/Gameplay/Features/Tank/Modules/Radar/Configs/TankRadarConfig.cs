using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Configs
{
    [CreateAssetMenu(fileName = "TankRadarConfig", menuName = "Murky Aces/Tank/Radar/Config")]
    public sealed class TankRadarConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)]
        private float _range = 100f;

        [SerializeField, Min(0.01f)]
        private float _scanInterval = 0.5f;

        [SerializeField]
        private RadarTargetDefinition[] _targetTypes = Array.Empty<RadarTargetDefinition>();

        [SerializeField] private bool _initiallyEnabled;
        public bool InitiallyEnabled => _initiallyEnabled;
        
        public float Range => _range;
        public float ScanInterval => _scanInterval;

        public IReadOnlyList<RadarTargetDefinition> TargetTypes =>
            _targetTypes;

        public bool IsValid
        {
            get
            {
                if (!IsPositiveFinite(_range) ||
                    !IsPositiveFinite(_scanInterval) ||
                    _targetTypes == null)
                {
                    return false;
                }

                var ids = new HashSet<string>(StringComparer.Ordinal);

                foreach (RadarTargetDefinition definition in _targetTypes)
                {
                    if (definition == null ||
                        !definition.IsValid ||
                        !ids.Add(definition.Id))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsInfinity(value);
        }
    }
}