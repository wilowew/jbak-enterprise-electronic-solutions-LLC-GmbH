using UnityEngine;
using System.Collections;

public class ObjectActivator : MonoBehaviour
{
    public GameObject targetObject; 
    public float delaySeconds = 5.4f; 

    void Start()
    {
        StartCoroutine(ActivateObjectWithDelay());
    }

    IEnumerator ActivateObjectWithDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        targetObject.SetActive(true);
    }
}