using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePumpkin : MonoBehaviour
{
    public GameObject DisableObject;

    // Start is called before the first frame update
    void Start()
    {
        if(DisableObject != null)
            DisableObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
