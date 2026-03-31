using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class DescPanel : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField]
    private GameObject panelDesc;
    [SerializeField]
    private TextMeshPro itemName;
    [SerializeField]
    private TextMeshPro itemDesc;
    [SerializeField]
    private Button btnEquipOrUnequip;
    
    private void Awake()
    {
        panelDesc.SetActive(false);
    }
    
}