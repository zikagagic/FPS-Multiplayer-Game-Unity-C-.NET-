using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UI_WeaponPanel : MonoBehaviour
{
    [SerializeField] private PanelItem[] _panelItems;
    

    //add the selected weapon to player inventory through the panel menu
    public void GetWeapon(int index)
    {
        GameController.Current.LocalPlayer.GetComponent<PlayerCallbacks>().RaiseChooseWeaponEvent(index);
    }
    //get the weapon id from the weapon panel item
    public WeaponID GetWeaponID(int index)
    {

        return _panelItems[index].id;
    }
}

[System.Serializable]
public struct PanelItem
{
    public UI_WeaponPanel_Item item;
    public WeaponID id;
}