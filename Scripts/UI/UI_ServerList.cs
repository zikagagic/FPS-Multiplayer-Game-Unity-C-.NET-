using UnityEngine;
using System;
using UdpKit;
using UnityEngine.SceneManagement;
using UdpKit.Platform.Photon;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using Photon.Bolt.Utils;

//source for code https://github.com/BoltEngine/Bolt-Sample/blob/master/BoltInit.cs
namespace Bolt.Samples
{
	public class UI_ServerList : GlobalEventListener
	{
		private enum State
		{
			SelectRoom,
			Started,
		}

		private Rect labelRoom = new Rect(0, 0, 140, 75);
		private GUIStyle labelRoomStyle;

		private State currentState;
		private string map;

		void Awake()
		{
			currentState = State.SelectRoom;
			Application.targetFrameRate = 60;

			labelRoomStyle = new GUIStyle()
			{
				fontSize = 20,
				fontStyle = FontStyle.Bold,
				normal =
				{
					textColor = Color.white
				}
			};
		}

		void OnGUI()
		{
			Rect tex = new Rect(10, 10, 140, 75);
			Rect area = new Rect(10, 90, Screen.width /2, Screen.height - 100);

			GUILayout.BeginArea(area);

			switch (currentState)
			{
				case State.SelectRoom: ServerList(); break;
			}

			GUILayout.EndArea();
		}
		//find all the servers available and list them
		void ServerList()
		{
			GUI.Label(labelRoom, "Looking for rooms:", labelRoomStyle);

			if (BoltNetwork.SessionList.Count > 0)
			{
				GUILayout.BeginVertical();
				GUILayout.Space(30);

				foreach (var session in BoltNetwork.SessionList)
				{
					var photonSession = session.Value as PhotonSession;

					if (photonSession.Source == UdpSessionSource.Photon)
					{
						var matchName = photonSession.HostName;
						var label = string.Format("Join: {0} | {1}/{2}", matchName, photonSession.ConnectionsCurrent, photonSession.ConnectionsMax);

						if (ExpandButton(label))
						{
							BoltMatchmaking.JoinSession(photonSession);
							currentState = State.Started;
						}
					}
				}

				GUILayout.EndVertical();
			}
		}
		bool ExpandButton(string text)
		{
			return GUILayout.Button(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		}
	}
}
