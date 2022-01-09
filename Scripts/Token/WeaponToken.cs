using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Bolt;
using UdpKit;

public class WeaponToken : IProtocolToken
{
    public WeaponID ID;
    public int currentAmmo;
    public int totalAmmo;
    public NetworkId networkID;
    public void Read(UdpPacket packet)
    {
        ID = (WeaponID)packet.ReadInt();
        currentAmmo = packet.ReadInt();
        totalAmmo = packet.ReadInt();
        networkID = new NetworkId(packet.ReadULong());
    }

    public void Write(UdpPacket packet)
    {
        packet.WriteInt((int)ID);
        packet.WriteInt(currentAmmo);
        packet.WriteInt(totalAmmo);
        packet.WriteULong(networkID.PackedValue);
    }
}
    public enum WeaponID
    {
        None = 0,
        AK47,
        M4A1,
        Remington,
        L96,
        PrimaryEnd,
        Glock,
        Colt,
        Berreta_M9
    }

