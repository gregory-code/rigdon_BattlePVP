using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Photon.Pun.UtilityScripts.TabViewManager;

public class monsterTab : Button
{
    [SerializeField] float clickedAmount = 1.1f;
    [SerializeField] float selectAmount = 1.15f;

    private monsterTab[] tabs;

    private bool isClicked;
    private bool isSelected;

    public delegate void OnMonsterSelected(int which);
    public event OnMonsterSelected onMonsterSelected;

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

        onMonsterSelected?.Invoke(GetID());

        isClicked = false;
    }

    private int GetID()
    {
        int id = 0;
        if (name.Contains("1"))
            id = 1;
        if (name.Contains("2"))
            id = 2;

        return id;
    }

    public void SetTab(bool state)
    {
        isSelected = state;
    }

    private void FixTabs()
    {
        if (tabs == null)
        {
            tabs = GameObject.FindObjectsOfType<monsterTab>();
        }

        foreach (monsterTab tab in tabs)
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
        Vector3 scale = new Vector3(0.03f, 0.03f, 1);
        if (isSelected) scale *= selectAmount;
        if (isClicked) scale *= clickedAmount;

        transform.localScale = Vector3.Lerp(transform.localScale, scale, 5 * Time.deltaTime);
    }

    private void LerpMove()
    {
        Vector3 move = (isSelected) ? new Vector3(transform.localPosition.x, -10, 0) : new Vector3(transform.localPosition.x, 0, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, move, 5 * Time.deltaTime);
    }
}
