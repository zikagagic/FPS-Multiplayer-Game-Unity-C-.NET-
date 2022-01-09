using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;

[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{
    public override void BoltStartBegin()
    {
        //register room properties and the player token
        BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
        BoltNetwork.RegisterTokenClass<PlayerTeamToken>();
    }

    public override void SceneLoadLocalDone(string scene, IProtocolToken token)
    {
       if(BoltNetwork.IsServer)
        {
            if(scene == HeadlessServerManager.Map())
            {
                //Create a game controller for the server 
                if(!GameController.Current)
                {
                    BoltNetwork.Instantiate(BoltPrefabs.GameController);
                }
            }
        }
    }
}
