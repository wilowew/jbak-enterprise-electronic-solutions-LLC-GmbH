using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class ImageData
{
    public Image image;
    public float showDelay;
    public float visibleTime;
    public float hideDelay;
    [Range(0.1f, 2f)]
    public float fadeDuration = 1f; 
}