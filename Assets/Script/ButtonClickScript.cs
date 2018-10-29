using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.UIElements;

public class ButtonClickScript : MonoBehaviour,IPointerClickHandler {


    public void OnPointerClick(PointerEventData eventData)
    {
        this.GetComponentInParent<Canvas>().enabled = false;
        Application.LoadLevel("SampleScene");
    }
}
