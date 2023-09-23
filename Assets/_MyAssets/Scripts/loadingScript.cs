using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadingScript : MonoBehaviour
{
    private Image _myImage;
    private bool _bShowing;
    private Image _flying;
    private Image _critter1;
    private Image _critter2;

    [SerializeField] Sprite[] critterImages;
    [SerializeField] Sprite[] flyingCritterImages;

    [SerializeField] Sprite[] backgroundImages;
    [SerializeField] Vector2[] flyingLocation;
    [SerializeField] Vector2[] critterLocation1;
    [SerializeField] Vector2[] critterLocation2;

    public void show()
    {
        _myImage = GetComponent<Image>();
        _flying = transform.Find("flying").GetComponent<Image>();
        _critter1 = transform.Find("critter1").GetComponent<Image>();
        _critter2 = transform.Find("critter2").GetComponent<Image>();

        if (_myImage.fillAmount < 0.96f) GetNewScene();

        _myImage.fillOrigin = 0;
        _bShowing = true;
        _myImage.raycastTarget = true;
    }

    public void hide()
    {
        _myImage.fillOrigin = 1;
        _bShowing = false;
        _myImage.raycastTarget = false;
    }

    private void GetNewScene()
    {
        int random = Random.Range(0, backgroundImages.Length);
        _myImage.sprite = backgroundImages[random];
        _flying.transform.localPosition = flyingLocation[random];
        _critter1.transform.localPosition = critterLocation1[random];
        _critter2.transform.localPosition = critterLocation2[random];

        random = Random.Range(0, flyingCritterImages.Length);
        _flying.sprite = flyingCritterImages[random];

        random = Random.Range(0, critterImages.Length);
        _critter1.sprite = critterImages[random];

        random = Random.Range(0, critterImages.Length);
        _critter2.sprite = critterImages[random];
    }

    void Update()
    {
        int targetValue = (_bShowing) ? 1 : 0;
        _myImage.fillAmount = Mathf.Lerp(_myImage.fillAmount, targetValue, 9 * Time.deltaTime);

        float transparency = Mathf.Lerp(_myImage.color.a, targetValue, 9 * Time.deltaTime);
        _myImage.color = new Color(1, 1, 1, transparency);
        _flying.color = _myImage.color;
        _critter1.color = _myImage.color;
        _critter2.color = _myImage.color;
    }
}
