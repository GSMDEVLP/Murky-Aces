using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationView : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField]
        private StationInteractionScope _interactionScope;
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
        
        [Header("Entry")]
        [SerializeField]private CrewLocationKind _requiredEntryLocation = CrewLocationKind.Interior;
        public Transform SeatAnchor =>_seatAnchor;
        public Transform ExitAnchor => _exitAnchor;
        public Transform CameraAnchor => _cameraAnchor;
        public Transform InteractionPoint => _interactionPoint;
        public StationPanelRoot PanelRoot => _panelRoot;
        public CrewRoleId RoleId => _roleId;
        public StationCapabilityProfile CapabilityProfile => _capabilityProfile;
        public CrewLocationKind RequiredEntryLocation =>_requiredEntryLocation;
        public IInteractionScope InteractionScope => _interactionScope;


        public bool CanEnter =>
            (_requiredEntryLocation == CrewLocationKind.Outside ||
            _requiredEntryLocation == CrewLocationKind.Interior) &&
            _capabilityProfile != null &&
            _seatAnchor != null &&
            _exitAnchor != null &&
            _cameraAnchor != null &&
            _interactionScope != null &&
            _panelRoot != null &&
            _panelRoot.HasValidPrimaryScreen;

        public bool CanExit =>
            _exitAnchor != null;
    }
}