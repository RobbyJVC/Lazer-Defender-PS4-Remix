using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class disableQuit : MonoBehaviour
{
    public GameObject gameObj;//the gameobject you want to disable in the scene



    void Start()
    {

        if (Application.platform == RuntimePlatform.PS4)
            gameObj.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }
}