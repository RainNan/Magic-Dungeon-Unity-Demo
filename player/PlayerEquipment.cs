using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField]
    private GameObject jointItemR;

    private void Start()
    {
        EventBus.Instance.Subscribe(EventNames.OnEquipWithModel, (o) =>
        {
            if (o is null)
            {
                RemoveSonWeapon();
                return;
            }
            
            if (o is EquipmentData equipmentData)
            {
                RemoveSonWeapon();
                    
                var equipmentConfig = equipmentData.equipmentConfig;
                Instantiate(equipmentConfig.prefab, jointItemR.transform);
            }
        });
    }

    private void RemoveSonWeapon()
    {
        foreach (Transform son in jointItemR.transform)
        {
            Destroy(son.gameObject);
        }
    }
}