using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class canvasGroupRenderer : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private bool isActiveGroup;

    public void SetCanvasStatus(bool state)
    {
        isActiveGroup = state;
        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }

    public bool isActive()
    {
        return isActiveGroup;
    }

    public void Update()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        int alpha = (isActiveGroup) ? 1 : 0;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, alpha, 18 * Time.deltaTime);
    }
}
