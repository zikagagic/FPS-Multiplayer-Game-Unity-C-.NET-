using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Bolt;
using UdpKit;
//source for code https://github.com/BoltEngine/Bolt-Sample/blob/master/AdvancedTutorial/scripts/Tokens/Token.cs
public class PlayerTeamToken : IProtocolToken
{
    public string playerName;
    public Team playerTeam;

    public void Read(UdpPacket packet)
    {
        playerName = packet.ReadString();
        playerTeam = (Team)packet.ReadShort();
    }

    public void Write(UdpPacket packet)
    {
        packet.WriteString(playerName);
        packet.WriteShort((short)playerTeam);
    }
}
public enum Team
{
    Blue,
    Red,
    None
}