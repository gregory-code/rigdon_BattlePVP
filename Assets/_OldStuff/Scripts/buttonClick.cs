using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class buttonClick : Button
{
    private bool isClicked;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if(name.Contains("critter"))
        {
            StartCoroutine(deleteCritter());
        }

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

    private IEnumerator deleteCritter()
    {
        yield return new WaitForSeconds(1f);

        if(isClicked == true)
        {
            isClicked = false;
            GameObject.Find("builder").GetComponent<builderMenu>().bDeleteCritter = true;
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(this.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            ExecuteEvents.Execute(this.gameObject, pointer, ExecuteEvents.submitHandler);
            yield return new WaitForSeconds(0.25f);
            ExecuteEvents.Execute(this.gameObject, pointer, ExecuteEvents.pointerExitHandler);
            // call builderDelete menu
        }
    }

    private void Update()
    {
        Vector3 scale = (isClicked) ? Vector3.one * 1.1f : Vector3.one;

        transform.localScale = Vector3.Lerp(transform.localScale, scale, 20 * Time.deltaTime);
    }
}
