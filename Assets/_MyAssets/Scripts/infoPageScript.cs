using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class infoPageScript : canvasGroupRenderer
{
    [SerializeField] GameMaster gameMaster;

    [SerializeField] TextMeshProUGUI levelText;

    [SerializeField] Image monsterImage;
    [SerializeField] Image monsterSillio;

    [SerializeField] TextMeshProUGUI[] baseStatTexts;
    [SerializeField] TextMeshProUGUI[] currentStatTexts;
    [SerializeField] TextMeshProUGUI[] currentTexts;

    [SerializeField] Image[] growthImages;
    [SerializeField] Sprite[] growthSprites;
    [SerializeField] Color[] growthColors;

    [SerializeField] Transform attackParent;
    [SerializeField] Transform abilityParent;
    [SerializeField] Transform passiveParent;

    [SerializeField] Transform effectsList;
    [SerializeField] GameObject[] effectsPrefabs;
    List<GameObject> listOfEffects = new List<GameObject>();

    [SerializeField] List<moveContent> moveContentList = new List<moveContent>();

    [SerializeField] string spellShieldInfinite;

    public void DisplayMonster(monster mon)
    {
        gameMaster.inInfoScreen = true;
        SetCanvasStatus(true);
        
        monsterImage.sprite = mon.stages[mon.GetSpriteIndexFromLevel()];
        monsterSillio.sprite = mon.stages[mon.GetSpriteIndexFromLevel()];
        monsterImage.SetNativeSize();
        monsterSillio.SetNativeSize();

        levelText.text = "Level " + mon.GetLevel();

        baseStatTexts[0].text = mon.GetMaxHealth() + "";
        baseStatTexts[1].text = mon.GetBaseStrength() + "";
        baseStatTexts[2].text = mon.GetBaseMagic() + "";
        baseStatTexts[3].text = mon.GetBaseSpeed() + "";

        currentStatTexts[0].text = mon.GetCurrentHealth() + "";
        currentStatTexts[1].text = (mon.GetBaseStrength() == mon.GetCurrentStrength()) ? "" : mon.GetCurrentStrength() + "";
        currentStatTexts[2].text = (mon.GetBaseMagic() == mon.GetCurrentMagic()) ? "" : mon.GetCurrentMagic() + "";
        currentStatTexts[3].text = (mon.GetBaseSpeed() == mon.GetCurrentSpeed()) ? "" : mon.GetCurrentSpeed() + "";

        currentTexts[0].text = (mon.GetBaseStrength() == mon.GetCurrentStrength()) ? "" : "Current: ";
        currentTexts[1].text = (mon.GetBaseMagic() == mon.GetCurrentMagic()) ? "" : "Current: ";
        currentTexts[2].text = (mon.GetBaseSpeed() == mon.GetCurrentSpeed()) ? "" : "Current: ";

        int[] growths = mon.GetGrowths();

        for(int i = 0; i < growthImages.Length; i++)
        {
            growthImages[i].color = growthColors[growths[i]];
            growthImages[i].sprite = growthSprites[growths[i]];
        }

        foreach (moveContent content in moveContentList)
        {
            Destroy(content.gameObject);
        }
        moveContentList.Clear();

        moveContent[] contents = mon.GetMoveContents();

        if(mon.GetAttackID() != 0)
        {
            moveContent newAttack = Instantiate(contents[mon.GetAttackID() - 1], attackParent);
            newAttack.transform.localPosition = Vector3.zero;
            moveContentList.Add(newAttack);
        }

        if (mon.GetAbilityID() != 0)
        {
            moveContent newAbility = Instantiate(contents[mon.GetAbilityID() + 1], abilityParent);
            newAbility.transform.localPosition = Vector3.zero;
            moveContentList.Add(newAbility);
        }

        if (mon.GetPassiveID() != 0)
        {
            moveContent newPassive = Instantiate(contents[mon.GetPassiveID() + 3], passiveParent);
            newPassive.transform.localPosition = Vector3.zero;
            moveContentList.Add(newPassive);
        }

        foreach (moveContent content in moveContentList)
        {
            content.SetTier(mon.GetSpriteIndexFromLevel(), mon.GetCurrentStatBlock());
        }

        foreach(GameObject effect in listOfEffects)
        {
            Destroy(effect);
        }
        listOfEffects.Clear();

        if(mon.statusEffects.Count <= 0)
        {
            return;
        }

        foreach(statusEffectUI status in mon.statusEffects)
        {
            GameObject effect = Instantiate(effectsPrefabs[status.GetIndex()], effectsList);
            effect.GetComponent<Image>().sprite = status.GetComponent<Image>().sprite;
            effect.transform.Find("statusCounter").GetComponent<TextMeshProUGUI>().text = (status.GetCounter() >= 160) ? "" : status.GetCounter() + "";
            switch(status.GetIndex())
            {
                case 1:
                    effect.transform.Find("power").GetComponent<TextMeshProUGUI>().text = status.GetBurnDamage() + "";
                    break;

                case 2:
                    effect.transform.Find("power").GetComponent<TextMeshProUGUI>().text = status.GetPower() + "";
                    break;

                case 5:
                    effect.transform.Find("statusCounter").GetComponent<TextMeshProUGUI>().text = (status.GetCounter() >= 160) ? spellShieldInfinite : "(" + status.GetCounter() + " turns)";
                    break;

                case 6:
                    int tier = status.GetLevelIndex();
                    int reduction = 40 + (tier * 10);
                    effect.transform.Find("power").GetComponent<TextMeshProUGUI>().text = $"{reduction}%";
                    break;

                case 8:
                    effect.transform.Find("power").GetComponent<TextMeshProUGUI>().text = status.GetPower() + "";
                    effect.transform.Find("secondaryPower").GetComponent<TextMeshProUGUI>().text = status.GetSecondaryPower() + "";
                    break;

                case 9:
                    effect.transform.Find("power").GetComponent<TextMeshProUGUI>().text = status.GetPower() + "";
                    break;

                case 12:
                    effect.transform.Find("power").GetComponent<TextMeshProUGUI>().text = status.GetPower() + "%";
                    break;

                case 13:
                    effect.transform.Find("power").GetComponent<TextMeshProUGUI>().text = status.GetPower() + "%";
                    break;
            }

            listOfEffects.Add(effect);
        }

        RectTransform rect = effectsList.GetComponent<RectTransform>();
        if (listOfEffects.Count > 4)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, listOfEffects.Count * 58.75f);
        }
        else
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 235);
        }
    }

    public void HideDisplay()
    {
        gameMaster.inInfoScreen = false;
        SetCanvasStatus(false);
    }
}
