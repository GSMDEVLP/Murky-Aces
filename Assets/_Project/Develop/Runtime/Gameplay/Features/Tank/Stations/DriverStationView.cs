using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class DriverStationView : MonoBehaviour
    {
        [SerializeField] private Transform _driverSeatAnchor;
        [SerializeField] private Transform _driverExitAnchor;
        [SerializeField] private Transform _driverCameraAnchor;
        [SerializeField] private Transform _interactionPoint;

        public Transform DriverSeatAnchor => _driverSeatAnchor;
        public Transform DriverExitAnchor => _driverExitAnchor;
        public Transform DriverCameraAnchor => _driverCameraAnchor;
        public Transform InteractionPoint => _interactionPoint;

        public bool CanEnter => _driverSeatAnchor != null && _driverCameraAnchor != null;
    }
}