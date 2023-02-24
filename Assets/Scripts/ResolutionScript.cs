using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var res = Screen.currentResolution;

        var tmp = GetComponent<TMPro.TMP_Text>();
        tmp.text = res.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
