using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class statNotif : MonoBehaviour
{
    [SerializeField] GameObject statPopup;

    public void spawnStatPopup(Transform spawnParent, float value)
    {
        GameObject newPopup = Instantiate(statPopup);

        Color growthColor = new Color();
        growthColor = Color.green;
        if(value < 0)
        {
            growthColor = Color.red;
        }
        if(value == 0)
        {
            growthColor = new Color(0, 194, 255, 255);
        }

        newPopup.transform.GetChild(0).transform.GetComponent<TMP_Text>().text = (value >= 0) ? "+" + value : value + "";
        newPopup.transform.GetChild(0).transform.GetComponent<TMP_Text>().color = growthColor;
        newPopup.transform.SetParent(spawnParent);
        newPopup.transform.localScale = Vector3.one;
        newPopup.transform.localPosition = Vector3.zero;

        StartCoroutine(deleteMyself(newPopup));
    }

    public IEnumerator deleteMyself(GameObject popup)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(popup);
    }    
}
