using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

//Source/Template for code https://doc.photonengine.com/en-us/bolt/current/gameplay/state
public class PlayerCallbacks : EntityEventListener<IPlayerState>
{
    private PlayerMotor _playerMotor;
    private PlayerWeapons _playerWeapons;
    private PlayerController _playerController;
    private PlayerRenderer _playerRenderer;
    public PlayerSetupController playerSetup;
    private void Awake()
    {
        _playerMotor = GetComponent<PlayerMotor>();
        _playerWeapons = GetComponent<PlayerWeapons>();
        _playerController= GetComponent<PlayerController>();
        _playerRenderer = GetComponent<PlayerRenderer>();
        playerSetup = FindObjectOfType<PlayerSetupController>();
    }
    //Assign functions to all the state properties
    public override void Attached()
    {
        state.AddCallback("LifePoints", UpdatePlayerLife);
        state.AddCallback("Pitch", _playerMotor.SetPitch);
        state.AddCallback("IsDead", UpdatePlayerDeath);
        state.AddCallback("WeaponIndex", UpdateWeaponIndex);
        state.AddCallback("WeaponArray[].ID", UpdateWeaponList);
        state.AddCallback("WeaponArray[].CurrentAmmo",UpdateWeaponAmmo);
        state.AddCallback("WeaponArray[].TotalAmmo", UpdateWeaponAmmo);

        //assign the player properties when the player is attached to the server
        if (entity.IsOwner)
        {
            state.IsDead = false;
            state.LifePoints = _playerMotor.GetLife;
            GameController.Current.UpdateGameState();
            GameController.Current.state.AlivePlayers++;
        }
    }
    //when a weapon is added to the player's inventory
    private void UpdateWeaponList(IState state, string propertyPath, ArrayIndices arrayIndices)
    {
        int index = arrayIndices[0];
        IPlayerState s = (IPlayerState)state;
        if (s.WeaponArray[index].ID == -1)
            _playerWeapons.RemoveWapon(index);
        else
            _playerWeapons.AddWeaponToArray((WeaponID)s.WeaponArray[index].ID);
    }

    //every time tha player fires update the player's ammo
    public void UpdateWeaponAmmo(IState state, string propertyPath,ArrayIndices arrayIndices)
    {
        int index = arrayIndices[0];
        IPlayerState s = (IPlayerState)state;
        _playerWeapons.SetAmmo(index,s.WeaponArray[index].CurrentAmmo, s.WeaponArray[index].TotalAmmo);
    }
    //when the player changes the current weapon 
    public void UpdateWeaponIndex()
    {
        _playerWeapons.ChangeActiveWeapon(state.WeaponIndex);
    }
    //simulate the players weapon effects

    public void FireEvent(float currentSpreadX,float currentSpreadY)
    {
        GunFireEvent evnt = GunFireEvent.Create(entity, EntityTargets.EveryoneExceptController);
        evnt.SpreadX = currentSpreadX;
        evnt.SpreadY = currentSpreadY;
        evnt.Send();
    }

    public override void OnEvent(GunFireEvent evnt)
    {
        _playerWeapons.FireEffect(evnt.SpreadX,evnt.SpreadY);
    }

    //when a player receives damage update the HP bar and show a hit effect
    public void UpdatePlayerLife()
    {
        if (entity.HasControl)
        {
            GUI_Controller.Current.UpdateLife(state.LifePoints, _playerMotor.GetLife);
            if (state.LifePoints != _playerMotor.GetLife)
            {
                GUI_Controller.Current.ShowHitEffect();
            }
        }
    }
    //when a player is killed remove that player from the alive players and call all the associated functions for the player's death
    private void UpdatePlayerDeath()
    {
        PlayerTeamToken token = (PlayerTeamToken)entity.AttachToken;
        if (entity.IsOwner)
        {
            if (state.IsDead)
            {
                GameController.Current.state.AlivePlayers--;
            }
            else
                GameController.Current.state.AlivePlayers++;
        }
        if (entity.HasControl)
            GUI_Controller.Current.Show_GUI(!state.IsDead);
            
        
        _playerMotor.OnDeath(state.IsDead);
        _playerRenderer.OnDeath(state.IsDead);
        _playerWeapons.OnDeath(state.IsDead);

        transform.position = playerSetup.GetSpawnPoint(token.playerTeam);
    }

    //function that is called when resetting a game round, refresh the player's health
    public void RoundReset()
    {
        if(entity.IsOwner)
        {
            Debug.Log("Round reset called");
            if (GameController.Current.CurrentGamePhase!=GamePhase.Starting)
            {
                if(state.IsDead==true)
                {
                    state.IsDead = false;
                    if(GameController.Current.CurrentGamePhase==GamePhase.WaitForPlayers)
                    {
                        state.LifePoints = _playerMotor.GetLife;
                        PlayerTeamToken token = (PlayerTeamToken)entity.AttachToken;
                    }
                }
                else
                {
                    _playerWeapons.OnDeath(state.IsDead);
                }

                if(GameController.Current.CurrentGamePhase==GamePhase.StartRound || GameController.Current.CurrentGamePhase==GamePhase.EndRound)
                {
                    PlayerTeamToken token = (PlayerTeamToken)entity.AttachToken;

                    state.LifePoints = _playerMotor.GetLife;
                }
            }
        }
    }
    
    //when a player chooses a weapon through the menu assign that weapon to the player inventory via WeaponID
    public override void OnEvent(ChooseWeaponEvent evnt)
    {
        WeaponID weaponID = GUI_Controller.Current.WeaponPanel.GetWeaponID(evnt.index);
        _playerWeapons.AddWeaponToSlot(weaponID);        
        
    }

    //event raised when a player click on a weapon button
    public void RaiseChooseWeaponEvent(int index)
    {
        ChooseWeaponEvent evnt = ChooseWeaponEvent.Create(entity, EntityTargets.OnlyOwner);
        evnt.index = index;
        evnt.Send();
    }
}
