using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum RoomState
{
    Inactive,
    Combat,
    Clear
}

/// <summary>
/// 负责进入房间刷怪
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private GameObject[] locks;

    public RoomState roomState;
    private List<Entity> _enemies;
    private bool _spawnFinished;

    public Vector2 LogicPos { private set; get; }

    public void SetLogicPos(Vector2 pos) => LogicPos = pos;

    private void Awake()
    {
        roomState = RoomState.Inactive;
        _enemies = new List<Entity>(10);
        OpenDoor();
    }

    public void EnterRoom()
    {
        gameObject.SetActive(true);

        // 出生点不战斗
        if (spawnPoints.Length == 0) roomState = RoomState.Clear;

        if (IsCombat() || IsClear()) return;

        CloseDoor();
        roomState = RoomState.Combat;
        _enemies.Clear();
        _spawnFinished = false;

        ABManager.Instance.LoadResAsync<GameObject>("model", "Skeleton", (obj) =>
        {
            GameObject skeletonTemplate = obj as GameObject;
            if (skeletonTemplate is null)
            {
                Debug.LogError("Skeleton load failed.");
                roomState = RoomState.Inactive;
                OpenDoor();
                _spawnFinished = true;
                return;
            }

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform spawnPoint = spawnPoints[i];
                if (!spawnPoint) continue;

                Vector3 spawnPosition = spawnPoint.position;
                Quaternion spawnRotation = spawnPoint.rotation;

                if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    spawnPosition = hit.position;
                }

                GameObject skeletonInstance = Instantiate(skeletonTemplate, spawnPosition, spawnRotation);
                skeletonInstance.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

                var enemy = skeletonInstance.GetComponent<Entity>();
                if (enemy) _enemies.Add(enemy);
            }

            _spawnFinished = true;
        });

        EventBus.Instance.Publish(EventNames.OnMiniMapChanged, LogicPos);
    }

    private void Update()
    {
        if (IsClear()) return;

        if (IsCombat())
        {
            if (!_spawnFinished || _enemies.Count == 0)
            {
                return;
            }

            foreach (var enemy in _enemies)
            {
                if (!enemy.IsDead)
                {
                    return;
                }
            }

            roomState = RoomState.Clear;
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        foreach (var alock in locks)
        {
            alock.SetActive(false);
        }
    }

    private void CloseDoor()
    {
        foreach (var alock in locks)
        {
            alock.SetActive(true);
        }
    }

    private bool IsCombat() => roomState == RoomState.Combat;
    private bool IsClear() => roomState == RoomState.Clear;

    public void ExitRoom()
    {
        gameObject.SetActive(false);
    }
}