using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_AmmoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentAmmoText;
    [SerializeField] private TextMeshProUGUI _totalAmmoText;
    [SerializeField] private TextMeshProUGUI _gunNameText;

    public void UpdateAmmo(int current, int total)
    {
        _currentAmmoText.text = current.ToString();
        _totalAmmoText.text = total.ToString();
    }  
    
    public void UpdateGunName(string gunName)
    {
       _gunNameText.text = gunName;
    }
    
}
