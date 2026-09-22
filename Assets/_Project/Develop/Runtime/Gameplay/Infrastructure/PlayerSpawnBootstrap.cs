using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System;
using _Project.Develop.Runtime.Gameplay.Presentation;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
public sealed class PlayerSpawnBootstrap : MonoBehaviour
{
    [SerializeField] private List<Transform> _playerSpawnPoints;

    private PlayerSpawner _playerSpawner;
    private GameplayCameraRig _cameraRig;

    [Inject]
    public void Construct(PlayerSpawner playerSpawner, GameplayCameraRig cameraRig)
    {
        _playerSpawner = playerSpawner;
        _cameraRig = cameraRig;
    }

    private void Start()
    {
        Transform point = _playerSpawnPoints[0];
        PlayerFacade player = _playerSpawner.Spawn(point.position, point.rotation);

        if (!_cameraRig.TryBindWalkingAnchor(player.CameraPivot))
        {
            throw new InvalidOperationException(
                "Gameplay Camera could not bind to Player.CameraPivot.");
        }
        
        _cameraRig.transform.localPosition = Vector3.zero;
        _cameraRig.transform.localRotation = Quaternion.identity;

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
