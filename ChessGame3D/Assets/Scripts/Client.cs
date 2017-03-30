using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;

public class Client : MonoBehaviour {
	public string clientName;
	public bool isHost;

	private bool socketReady;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader read;

	public List<GameClient> players=new List<GameClient>();
	private void Start(){
		DontDestroyOnLoad (gameObject);
	}
	private void Update(){
		if (!socketReady)
			return;
		if (stream.DataAvailable) {
			string data = read.ReadLine ();
			if (data != null)
				OnInComingData (data);
		}
	}
	public bool ConnectToServer(string host,int port){
		if (socketReady)
			return false;
		try {
			socket=new TcpClient(host,port);
			stream=socket.GetStream();
			writer=new StreamWriter(stream);
			read=new StreamReader(stream);
			socketReady=true;
		} catch (System.Exception ex) {
			Debug.Log ("Socket error:" + ex.Message);
		}
		return socketReady;
	}
	//Send messages to the server
	public void Send(string data){
		if (!socketReady)
			return;
		writer.WriteLine (data);
		writer.Flush ();
	}
	// Read messages from the server
	private void OnInComingData(string data){
		Debug.Log ("Client:" + data);
		string[] aData = data.Split ('|');
		switch (aData[0]) {
		//just read 1 time when reicever
		case "SWHO":
			for (int i = 1; i < aData.Length - 1; i++) {
				UserConnected (aData [i], false);
			}
			Send ("CWHO|" + clientName+'|'+((isHost)?1:0));
			break;
			//when every player reicever
		case "SCNN":
			UserConnected (aData [1], false);
			break;
		case "SMOV":
			CheckerBroad.ins.TryMove (int.Parse(aData [1]), int.Parse(aData [2]), int.Parse(aData [3]), int.Parse(aData [4]));
			break;
		case "SMSG":
			CheckerBroad.ins.ChatMessage (aData [1]);
			break;
		}
	}
	private void UserConnected(string name,bool host){
		GameClient c = new GameClient ();
		c.name = name;

		players.Add (c);

		if (players.Count == 2)
			GameManager.ins.StartGame ();
		
	}

	private void OnApplicationQuit(){
		CloseSocket ();
	}
	private void OnDisable(){
		CloseSocket ();
	}
	private void CloseSocket(){
		if (!socketReady)
			return;
		writer.Close ();
		read.Close ();
		socket.Close ();
		socketReady = false;
	}
}
public class GameClient{
	public string name;
	public string isHost;
}
