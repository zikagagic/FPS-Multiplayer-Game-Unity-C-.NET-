using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUI_Controller : MonoBehaviour
{
    #region Singleton
    private static GUI_Controller _instance = null;
    public static GUI_Controller Current
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GUI_Controller>();
            return _instance;
        }
    }
    #endregion
    public Team SetTeam { set => Team_UI = value; }


    [SerializeField] private UI_HealthBar _hpBar = null;
    [SerializeField] private UI_AmmoPanel _ammoUI = null;
    [SerializeField] private TextMeshProUGUI _Blue_Score=null;
    [SerializeField] private TextMeshProUGUI _Red_Score=null;
    [SerializeField] private UI_WeaponPanel _weaponPanel = null;
    [SerializeField] private UI_Timer _timer = null;
    public UI_QuitGamePanel _QuitGamePanel = null;
    public UI_EndGamePanel _endGamePanel = null;
    public  GameObject scopeOverlay = null;
    public GameObject crosshair = null;
    public GameObject splatterImage = null;
    public GameObject hitEffectImage = null;
    private float hitTimer = 0.13f;

    public UI_WeaponPanel WeaponPanel { get => _weaponPanel;}
    Team Team_UI = Team.Blue;
   

    private void Start()
    {
        Show_GUI(false);
    }
    public void Show_GUI(bool active)
    {
        _hpBar.gameObject.SetActive(active);
        _ammoUI.gameObject.SetActive(active);
        crosshair.SetActive(active);

        //if the player dies while he is in a menu, deactivate the menu
        if (_weaponPanel.gameObject.activeSelf)
        {
            _weaponPanel.gameObject.SetActive(active);
        }
        //if the player dies while scoping or the round resets, deactivate the menu
        if (scopeOverlay.gameObject.activeSelf)
        {
            scopeOverlay.gameObject.SetActive(active);
        }
    }
    
    public void UpdateLife(int current, int total)
    {
        _hpBar.UpdateLife(current, total);
    }
    
    public void UpdateAmmo(int current, int total)
    {
        _ammoUI.UpdateAmmo(current,total);
    }

    public void UpdateGunName(string name)
    {
        _ammoUI.UpdateGunName(name);
    }

    public void UpdatePoints(int bluePoints, int redPoints)
    {
      _Blue_Score.text = bluePoints.ToString();
      _Red_Score.text = redPoints.ToString();
    }

    public void UpdateTimer(float f)
    {
       _timer.SetTimer(f);
    }

    public void ShowWeaponPanel(bool active)
    {
        _weaponPanel.gameObject.SetActive(active);
    }

    public void ShowScope(bool active)
    {
        scopeOverlay.SetActive(active);
    }

    public void ShowCrosshair(bool active)
    {
        crosshair.SetActive(active);
    }
    public void ShowEndGamePanel(bool active)
    {
        _endGamePanel.gameObject.SetActive(active);
        _ammoUI.gameObject.SetActive(!active);
        _hpBar.gameObject.SetActive(!active);
        crosshair.gameObject.SetActive(!active);
    }
    public void ShowQuitGamePanel(bool active)
    {
        _QuitGamePanel.gameObject.SetActive(active);
    }
    public void ShowHitEffect()
    {
        StartCoroutine(HitEffect());
    }
    IEnumerator HitEffect()
    {
        hitEffectImage.SetActive(true);
        yield return new WaitForSeconds(hitTimer);
        hitEffectImage.SetActive(false);
    }
}
