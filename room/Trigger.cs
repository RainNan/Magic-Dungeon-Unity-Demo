using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    /// <summary>
    /// 用于当 nextRoom == null 时，不显示门
    /// </summary>
    [Tooltip("当没有下个房间时,门关闭")]
    [SerializeField]
    private GameObject door;
    
    [SerializeField]
    private RoomController currentRoom;

    [SerializeField]
    private RoomController nextRoom;

    private void Start()
    {
        if (nextRoom is null) door.SetActive(false);
    }

    public void SetRooms(RoomController current, RoomController next)
    {
        currentRoom = current;
        nextRoom = next;
    }

    public Vector2Int GetGridDirection()
    {
        Vector3 localOffset = transform.localPosition;

        if (Mathf.Abs(localOffset.x) >= Mathf.Abs(localOffset.z))
        {
            return localOffset.x >= 0f ? Vector2Int.up : Vector2Int.down;
        }

        return localOffset.z >= 0f ? Vector2Int.right : Vector2Int.left;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (nextRoom == null)
        {
            return;
        }

        nextRoom.EnterRoom();
        currentRoom?.ExitRoom();
    }
}
