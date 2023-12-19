using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class menuTab : Button
{
    [SerializeField] float clickedAmount = 1.1f;
    [SerializeField] float selectAmount = 1.25f;

    private menuTab[] tabs;

    private bool isClicked;
    private bool isSelected;

    public delegate void OnTabSelected(bool state);
    public event OnTabSelected onTabSelected;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        isClicked = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);


        FixTabs();

        SetTab(true);

        isClicked = false;
    }

    public void SetTab(bool state)
    {
        isSelected = state;
        onTabSelected?.Invoke(state);
    }

    private void FixTabs()
    {
        if (tabs == null)
        {
            tabs = GameObject.FindObjectsOfType<menuTab>();
        }

        foreach(menuTab tab in tabs)
        {
            tab.SetTab(false);
        }
    }

    private void Update()
    {
        LerpScale();
        LerpMove();
    }

    private void LerpScale()
    {
        Vector3 scale = Vector3.one;
        if (isSelected) scale *= selectAmount;
        if (isClicked) scale *= clickedAmount;

        transform.localScale = Vector3.Lerp(transform.localScale, scale, 12 * Time.deltaTime);
    }

    private void LerpMove()
    {
        Vector3 move = (isSelected) ? new Vector3(-475, transform.localPosition.y, 0) : new Vector3(-525, transform.localPosition.y, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, move, 12 * Time.deltaTime);
    }
}
