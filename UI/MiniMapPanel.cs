using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room
{
    public Vector2 Pos { get; }

    public RoomState State { get; }

    public Room(Vector2 pos, RoomState state)
    {
        Pos = pos;
        State = state;
    }
}

public class MiniMapPanel : BasePanel
{
    [SerializeField]
    private float padding;
    
    [SerializeField]
    private Color seenRoomColor = new Color(180, 180, 180);

    [SerializeField]
    private Color unknownRoomColor = new Color(130, 130, 130);
    
    private Transform _contentTrans;
    private RectTransform _viewportRect;

    private Dictionary<Vector2, Image> _roomIconMap = new();
    private bool _eventsBound;
    private Color _defaultRoomColor;
    private Vector2 _currentRoomPos;
    private bool _hasCurrentRoom;

    private void Awake()
    {
        _viewportRect = transform.Find("Scroll View/Viewport") as RectTransform;
        _contentTrans = transform.Find("Scroll View/Viewport/Content");
        InitEvent();
    }

    private void InitEvent()
    {
        if (_eventsBound) return;
        _eventsBound = true;

        EventBus.Instance.Subscribe(EventNames.OnMiniMapInit, arg0 =>
        {
            if (arg0 is Room[] rooms)
                InitRooms(rooms);
        });

        EventBus.Instance.Subscribe(EventNames.OnMiniMapChanged, arg =>
        {
            if (arg is Vector2 pos)
            {
                Enter(pos);
            }
        });
    }

    /// <summary>
    /// 玩家进入新房间
    /// </summary>
    private void Enter(Vector2 pos)
    {
        if (!_roomIconMap.TryGetValue(pos, out var roomImage))
            return;

        if (_hasCurrentRoom &&
            _currentRoomPos != pos &&
            _roomIconMap.TryGetValue(_currentRoomPos, out var previousRoomImage))
        {
            previousRoomImage.color = seenRoomColor;
        }

        roomImage.color = _defaultRoomColor;
        _currentRoomPos = pos;
        _hasCurrentRoom = true;

        var contentRect = _contentTrans as RectTransform;
        var roomRect = roomImage.rectTransform;
        if (contentRect == null || _viewportRect == null || roomRect == null)
            return;

        Vector3 viewportCenter = _viewportRect.TransformPoint(_viewportRect.rect.center);
        Vector3 roomCenter = roomRect.TransformPoint(roomRect.rect.center);
        contentRect.position += viewportCenter - roomCenter;
    }

    private void InitRooms(Room[] rooms)
    {
        _roomIconMap.Clear();
        _hasCurrentRoom = false;

        foreach (var room in rooms)
        {
            if (room == null) continue;

            var pos = room.Pos;

            var roomIconPrefab = Resources.Load<GameObject>("Prefabs/RoomIcon");
            var roomIcon = Instantiate(roomIconPrefab, _contentTrans);

            var img = roomIcon.GetComponent<Image>();
            if (_roomIconMap.Count == 0)
                _defaultRoomColor = img.color;

            _roomIconMap[pos] = img;
            if (pos != Vector2.zero)
            {
                img.color = unknownRoomColor;
            }
            else
            {
                _currentRoomPos = pos;
                _hasCurrentRoom = true;
            }

            RectTransform rect = roomIcon.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.one * pos * padding;
        }
    }
}
