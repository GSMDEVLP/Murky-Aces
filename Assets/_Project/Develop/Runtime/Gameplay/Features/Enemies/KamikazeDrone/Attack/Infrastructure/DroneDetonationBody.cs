using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class DroneDetonationBody : MonoBehaviour, IDroneDetonationBody
    {
        [SerializeField] private KamikazeDroneRoot _droneRoot;

        public KamikazeDroneRoot DroneRoot => _droneRoot;

        public void CompleteDetonation()
        {
            GameObject drone = _droneRoot.gameObject;

            drone.SetActive(false);
            Destroy(drone);
        }
    }
}
