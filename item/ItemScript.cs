using System;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private ItemConfig itemConfig;

    private void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed,Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventBus.Instance.Publish(EventNames.PickUp, itemConfig);
        }

        Destroy(gameObject);
    }
}