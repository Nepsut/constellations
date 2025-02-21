using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private string keyName = "gate_key_1";
    
    public string KeyName() { return keyName; }
}
