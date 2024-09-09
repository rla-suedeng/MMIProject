using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    public Button maxBtn;

    // Start is called before the first frame update
    void Start()
    {
        maxBtn.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
