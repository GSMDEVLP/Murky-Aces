using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
public sealed class PlayerSpawnBootstrap : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private List<Transform> _playerSpawnPoints;

    private PlayerSpawner _playerSpawner;

    [Inject]
    public void Construct(PlayerSpawner playerSpawner)
    {
        _playerSpawner = playerSpawner;
    }

    private void Start()
    {
        Transform point = _playerSpawnPoints[0];
        PlayerFacade player = _playerSpawner.Spawn(point.position, point.rotation);

        _mainCamera.transform.SetParent(player.CameraPivot, false);
        _mainCamera.transform.localPosition = Vector3.zero;
        _mainCamera.transform.localRotation = Quaternion.identity;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // foreach(Transform spawnPoint in _playerSpawnPoints)
        // {
        //     _playerSpawner.Spawn(spawnPoint.position, spawnPoint.rotation);
        // }
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
}
