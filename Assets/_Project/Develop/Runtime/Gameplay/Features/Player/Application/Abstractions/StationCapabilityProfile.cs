using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public enum StationControlContext
    {
        Driving = 0,
        Gunner = 1,
        Commander = 2
    }

    [Serializable]
    public sealed class StationCapabilityProfile
    {
        [SerializeField] private StationControlContext _controlContext = StationControlContext.Driving;

        [SerializeField] private bool _blocksLocomotion = true;

        [SerializeField] private bool _usesCockpitLook = true;

        [SerializeField] private bool _allowsInteraction = true;

        [SerializeField] private Vector2 _yawLimits = new Vector2(-75f, 75f);
        [SerializeField] private Vector2 _pitchLimits = new Vector2(-35f, 55f);
        [SerializeField] private Vector2 _initialLookAngles = Vector2.zero;
        public StationControlContext ControlContext => _controlContext;
        public bool BlocksLocomotion => _blocksLocomotion;
        public bool UsesCockpitLook => _usesCockpitLook;
        public bool AllowsInteraction => _allowsInteraction;
        public Vector2 YawLimits => _yawLimits;
        public Vector2 PitchLimits => _pitchLimits;
        public Vector2 InitialLookAngles => _initialLookAngles;

        public bool HasValidLookSettings =>
            IsValidRange(_yawLimits) &&
            IsValidRange(_pitchLimits) &&
            IsFinite(_initialLookAngles.x) &&
            IsFinite(_initialLookAngles.y);

        private static bool IsValidRange(Vector2 range)
        {
            return IsFinite(range.x) &&
                IsFinite(range.y) &&
                range.x <= range.y;
        }

        private static bool IsFinite(float value)
        {
            return float.IsNaN(value) == false &&
                float.IsInfinity(value) == false;
        }
    }
}