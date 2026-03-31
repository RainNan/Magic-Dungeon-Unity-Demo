using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField]
    private GameObject inventoryPanel;
    [SerializeField]
    private Button btnOpen;
    [SerializeField]
    private Button btnClose;

    private void Awake()
    {
        OnClose();
    }

    private void Start()
    {
        btnOpen.onClick.AddListener(OnOpen);
        btnClose.onClick.AddListener(OnClose);
    }

    private void OnOpen() => inventoryPanel.SetActive(true);
    private void OnClose() => inventoryPanel.SetActive(false);
}