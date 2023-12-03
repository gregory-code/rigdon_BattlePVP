using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menuPage : MonoBehaviour
{
    [SerializeField] menuTab myTab;

    private CanvasGroup canvasGroup;

    private bool isActivePage;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        myTab.onTabSelected += SetMenuStatus;
    }

    public void SetMenuStatus(bool state)
    {
        isActivePage = state;
        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }

    public void Update()
    {
        int alpha = (isActivePage) ? 1 : 0;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, alpha, 18 * Time.deltaTime);
    }
}
