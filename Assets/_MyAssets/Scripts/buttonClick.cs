using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class buttonClick : Button
{
    private bool isClicked;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        isClicked = true;
        if(name.Contains("Switch"))
        {
            GameObject.Find("switch-").GetComponent<AudioSource>().Play();
        }
        else
        {
            GameObject.Find("light-switch").GetComponent<AudioSource>().Play();
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        isClicked = false;
    }

    private void Update()
    {
        Vector3 scale = (isClicked) ? Vector3.one * 1.1f : Vector3.one;

        transform.localScale = Vector3.Lerp(transform.localScale, scale, 20 * Time.deltaTime);
    }
}
