using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField]
    private Gradient _gradient = null;

    [SerializeField]
    private Image _bg = null;
    [SerializeField]
    private Image _hpBar = null;
    [SerializeField]
    private TextMeshProUGUI _hpText = null;

    //change the fill of the hp bar based on the player's health
    public void UpdateLife(int hp, int totalHP)
    {
        float f = (float)hp / (float)totalHP;
        _hpBar.fillAmount = f;
        Color c = _gradient.Evaluate(f);
        _bg.color = new Color(c.r, c.g, c.b, _bg.color.a);
        _hpBar.color = c;
        _hpText.text = hp.ToString();
        _hpText.color = c;
    }
}
