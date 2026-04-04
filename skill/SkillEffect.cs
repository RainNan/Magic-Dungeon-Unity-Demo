using System;
using System.Collections;
using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    private Transform _playerTrans;
    
    private void Awake()
    {
        _playerTrans = GameObject.FindWithTag("Player").GetComponent<Transform>();
    }

    private void OnEnable()
    {
        
    }
}