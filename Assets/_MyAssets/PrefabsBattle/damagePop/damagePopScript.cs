using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class damagePopScript : MonoBehaviour
{
    [SerializeField] Animator popAnimator;
    [SerializeField] TMP_Text damageText;

    [SerializeField] Color damagedColor;
    [SerializeField] Color healedColor;
    [SerializeField] Color shieldedColor;

    [SerializeField] GameObject crit;

    public void Init(int change, bool shielededColor, bool crit)
    {
        popAnimator.SetTrigger("pop" + Random.Range(0, 5)); // do 1 over the amount you want. So (0, 3) is really (0, 2)

        transform.localEulerAngles = Vector3.one;
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        if (crit)
            this.crit.SetActive(true);

        damageText.color = (change < 0) ? damagedColor : healedColor;

        if (shielededColor)
            damageText.color = shieldedColor;

        damageText.text = (change < 0) ? "" + change : "+" + change ;
        Destroy(this.gameObject, 1.5f);
    }
}
