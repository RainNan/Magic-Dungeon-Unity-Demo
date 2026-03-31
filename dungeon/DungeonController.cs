using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 负责随机生成房间
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    private const byte _mapSize = 10;

    /// <summary>
    /// 2d地图，从(0,0)出发随机走一格
    /// </summary>
    private Vector2[] _map = new Vector2[_mapSize];
    
    /// <summary>
    /// 怪物房间
    /// </summary>
    private GameObject[] _commonRooms = new GameObject[10];
    
    /// <summary>
    /// 出生点
    /// </summary>
    private GameObject _spawnRooms;

    /// <summary>
    /// 房间和房间之间的2维偏移
    /// </summary>
    private float offset = 19.58f;

    private readonly Dictionary<Vector2Int, RoomController> _roomControllers = new();
    private readonly List<GameObject> _spawnedRooms = new();
    private Room[] _miniMapRooms;

    private void Awake()
    {
        GenerateMap();
        InitRooms();
    }

    private void Start()
    {
        if (_miniMapRooms != null)
        {
            EventBus.Instance.Publish(EventNames.OnMiniMapInit, _miniMapRooms);
        }
    }

    private void InitRooms()
    {
        // todo AB
        _commonRooms = Resources.LoadAll<GameObject>("Room/Common");
        _spawnRooms = Resources.Load<GameObject>("Room/Spawn/Spawn");

        _roomControllers.Clear();
        _spawnedRooms.Clear();

        var rooms = new Room[_mapSize];
        var index = 0;
        

        foreach (var pos in _map)
        {
            Vector2Int gridPos = Vector2Int.RoundToInt(pos);

            if (pos == Vector2.zero)
            {
                GameObject spawnRoomInstance = Instantiate(_spawnRooms, Vector3.zero, Quaternion.identity);
                _spawnedRooms.Add(spawnRoomInstance);

                RoomController spawnRoomController = spawnRoomInstance.GetComponent<RoomController>();
                if (spawnRoomController != null)
                {
                    _roomControllers[gridPos] = spawnRoomController;
                }
                
                rooms[index++] = new Room(pos, RoomState.Clear);
                
                continue;
            }

            var tZ = pos.x * offset;
            var tX = pos.y * offset;

            var rand = Random.Range(0, _commonRooms.Length);
            GameObject roomInstance = Instantiate(_commonRooms[rand], new Vector3(tX, 0f, tZ), Quaternion.identity);
            roomInstance.SetActive(false);
            _spawnedRooms.Add(roomInstance);
            
            
            // mini map
            rooms[index++] = new Room(pos, RoomState.Inactive);

            
            RoomController roomController = roomInstance.GetComponent<RoomController>();
            if (roomController)
            {
                _roomControllers[gridPos] = roomController;
                roomController.SetLogicPos(pos);
            }
        }
        
        _miniMapRooms = rooms;

        BindTriggers();
    }

    private void GenerateMap()
    {
        int index = 0;
        _map[index++] = Vector2.zero;

        while (index < _mapSize)
        {
            int randomIndex = Random.Range(0, index);
            Vector2 currentPoint = _map[randomIndex];

            Vector2 newPoint = currentPoint;
            int randomDir = Random.Range(0, 4);
            switch (randomDir)
            {
                case 0: newPoint += Vector2.up; break; // 上
                case 1: newPoint += Vector2.right; break; // 右
                case 2: newPoint += Vector2.down; break; // 下
                case 3: newPoint += Vector2.left; break; // 左
            }

            bool isDuplicate = false;
            for (int i = 0; i < index; i++)
            {
                if (_map[i] == newPoint)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                _map[index] = newPoint;
                index++;
            }
        }

        for (int i = 0; i < _map.Length; i++)
        {
            Debug.Log($"地图点 [{i}] : {_map[i]}");
        }
    }

    private void BindTriggers()
    {
        foreach (GameObject roomObject in _spawnedRooms)
        {
            if (roomObject == null) continue;
            
            RoomController currentRoom = roomObject.GetComponent<RoomController>();
            Vector2Int currentGridPos = WorldToGrid(roomObject.transform.position);
            Trigger[] triggers = roomObject.GetComponentsInChildren<Trigger>(true);

            foreach (Trigger trigger in triggers)
            {
                Vector2Int nextGridPos = currentGridPos + trigger.GetGridDirection();
                _roomControllers.TryGetValue(nextGridPos, out RoomController nextRoom);
                trigger.SetRooms(currentRoom, nextRoom);
            }
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int gridX = Mathf.RoundToInt(worldPosition.z / offset);
        int gridY = Mathf.RoundToInt(worldPosition.x / offset);
        return new Vector2Int(gridX, gridY);
    }
}
