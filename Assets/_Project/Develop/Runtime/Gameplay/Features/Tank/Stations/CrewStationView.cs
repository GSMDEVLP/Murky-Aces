using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class CrewStationView : MonoBehaviour
    {
        [Header("Station Anchors")]
        [SerializeField]
        private Transform _seatAnchor;

        [SerializeField]
        private Transform _exitAnchor;

        [SerializeField]
        private Transform _cameraAnchor;

        [SerializeField]
        private Transform _interactionPoint;

        [Header("Cockpit")]
        [SerializeField]
        private StationPanelRoot _panelRoot;

        public Transform SeatAnchor =>_seatAnchor;

        public Transform ExitAnchor => _exitAnchor;

        public Transform CameraAnchor => _cameraAnchor;

        public Transform InteractionPoint => _interactionPoint;

        public StationPanelRoot PanelRoot => _panelRoot;

        public bool CanEnter =>
            _seatAnchor != null &&
            _exitAnchor != null &&
            _cameraAnchor != null &&
            _panelRoot != null &&
            _panelRoot.HasValidPrimaryScreen;

        public bool CanExit =>
            _exitAnchor != null;
    }
}