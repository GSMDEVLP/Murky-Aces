using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationView : MonoBehaviour
    {
        [Header("Station Anchors")]
        [SerializeField] private Transform _seatAnchor;

        [SerializeField] private Transform _exitAnchor;

        [SerializeField] private Transform _cameraAnchor;

        [SerializeField] private Transform _interactionPoint;

        [Header("Cockpit")]
        [SerializeField] private StationPanelRoot _panelRoot;

        [Header("Capabilities")] 
        [SerializeField] private CrewRoleId _roleId = CrewRoleId.Driver;

        [SerializeField] private StationCapabilityProfile _capabilityProfile = new StationCapabilityProfile();
        public Transform SeatAnchor =>_seatAnchor;

        public Transform ExitAnchor => _exitAnchor;

        public Transform CameraAnchor => _cameraAnchor;

        public Transform InteractionPoint => _interactionPoint;

        public StationPanelRoot PanelRoot => _panelRoot;

        public CrewRoleId RoleId => _roleId;

        public StationCapabilityProfile CapabilityProfile => _capabilityProfile;


        public bool CanEnter =>
            _capabilityProfile != null &&
            _seatAnchor != null &&
            _exitAnchor != null &&
            _cameraAnchor != null &&
            _panelRoot != null &&
            _panelRoot.HasValidPrimaryScreen;

        public bool CanExit =>
            _exitAnchor != null;
    }
}