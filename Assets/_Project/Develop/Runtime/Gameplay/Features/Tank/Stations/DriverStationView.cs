using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class DriverStationView : MonoBehaviour
    {
        [Header("Station Anchors")]
        [SerializeField]
        private Transform _driverSeatAnchor;

        [SerializeField]
        private Transform _driverExitAnchor;

        [SerializeField]
        private Transform _driverCameraAnchor;

        [SerializeField]
        private Transform _interactionPoint;

        [Header("Cockpit")]
        [SerializeField]
        private StationPanelRoot _panelRoot;

        public Transform DriverSeatAnchor =>
            _driverSeatAnchor;

        public Transform DriverExitAnchor =>
            _driverExitAnchor;

        public Transform DriverCameraAnchor =>
            _driverCameraAnchor;

        public Transform InteractionPoint =>
            _interactionPoint;

        public StationPanelRoot PanelRoot =>
            _panelRoot;

        public bool CanEnter =>
            _driverSeatAnchor != null &&
            _driverExitAnchor != null &&
            _driverCameraAnchor != null &&
            _panelRoot != null &&
            _panelRoot.HasValidPrimaryScreen;

        public bool CanExit =>
            _driverExitAnchor != null;
    }
}