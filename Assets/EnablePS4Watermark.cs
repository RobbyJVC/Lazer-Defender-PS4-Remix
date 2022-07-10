using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnablePS4Watermark : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.PS4)
            //#if UNITY_PS4
            this.gameObject.GetComponent<Image>().enabled = true;
        else
            //#else
            this.gameObject.GetComponent<Image>().enabled = false;
    }
//#endif
        // Update is called once per frame
        void Update()
    {
        
    }
}
