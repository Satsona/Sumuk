using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]

public class S_FpsLimit : MonoBehaviour
{

    [SerializeField] private int frameRate = 60;
    
    
    private void Start()
    {
     #if UNITY_EDITOR
     QualitySettings.vSyncCount = 0;
     Application.targetFrameRate = frameRate;
     #endif
    }
}
