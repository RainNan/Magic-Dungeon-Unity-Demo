using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributePanel : BasePanel
{
    private Transform HP;
    private Transform MP;
    private Transform Stamina;

    private Transform ATK;
    private Transform ATKRange;
    private Transform ATKSpeed;
    private Transform DEF;
    private Transform Speed;

    private void Awake()
    {
        Init();
        Subcribe();
        OnAttributeChanged(AttributeManager.Instance.Attribute);
    }

    private void Subcribe()
    {
        EventBus.Instance.Subscribe(EventNames.AttributeChanged, OnAttributeChanged);
    }

    private void OnAttributeChanged(object arg)
    {
        if (arg is not Attribute attribute) return;
        HP.GetComponent<Image>().fillAmount = attribute.HP / attribute.MaxHP;
        MP.GetComponent<Image>().fillAmount = attribute.MP / attribute.MaxMP;
        Stamina.GetComponent<Image>().fillAmount = attribute.Stamina / attribute.MaxStamina;

        ATK.GetComponent<TextMeshProUGUI>().text = attribute.ATK.ToString("0.0");
        ATKRange.GetComponent<TextMeshProUGUI>().text = attribute.ATKRange.ToString("0.0");
        ATKSpeed.GetComponent<TextMeshProUGUI>().text = attribute.ATKSpeed.ToString("0.0");
        DEF.GetComponent<TextMeshProUGUI>().text = attribute.DEF.ToString("0.0");
        Speed.GetComponent<TextMeshProUGUI>().text = attribute.Speed.ToString("0.0");
    }
    
    private void Init()
    {
        HP = transform.Find("LeftTop/Avatar/HP");
        MP = transform.Find("LeftTop/Avatar/MP");
        Stamina = transform.Find("LeftTop/Avatar/Stamina");

        ATK = transform.Find("LeftCenter/Number/ATK");
        ATKRange = transform.Find("LeftCenter/Number/ATKRange");
        ATKSpeed = transform.Find("LeftCenter/Number/ATKSpeed");
        DEF = transform.Find("LeftCenter/Number/DEF");
        Speed = transform.Find("LeftCenter/Number/Speed");
    }
}