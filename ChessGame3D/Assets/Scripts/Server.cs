﻿using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;


public class Server : MonoBehaviour {
	public int port=6321;

	public List<ServerClient> clients;
	public List<ServerClient> disconnectList;

	private TcpListener server;
	private bool serverStarted;

	public void Init(){
		DontDestroyOnLoad (gameObject);
		clients = new List<ServerClient> ();
		disconnectList = new List<ServerClient> ();
		try {
			server=new TcpListener(IPAddress.Any,port);
			server.Start();

			StartListening();
			serverStarted=true;
		} catch (System.Exception ex) {
			Debug.Log ("Socket error:" + ex.Message);
		}
	}
	private void Update(){
		if (!serverStarted)
			return;
		foreach (ServerClient i in clients) {
			//Is the client still connected
			if (!IsConnected (i.tcp)) {
				i.tcp.Close ();
				disconnectList.Add (i);
				continue;
			} else {
				NetworkStream s = i.tcp.GetStream ();
				if (s.DataAvailable) {
					StreamReader reader = new StreamReader (s, true);
					string data = reader.ReadLine ();
					if (data != null)
						OnInComingData (i, data);
					
				}

			}
		}
		for (int i = 0; i < disconnectList.Count-1; i++) {
			//tell our player somebody has disconnected

			clients.Remove (disconnectList [i]);
			disconnectList.RemoveAt (i);
		}
	}
	//Server read
	private void OnInComingData(ServerClient c,string data){
		Debug.Log ("Server:" + data);
		string[] aData = data.Split ('|');
		switch (aData[0]) {
		case "CWHO":
			c.clientName = aData [1];
			c.isHost = ((aData [2] == "0")) ? false : true;
			BroadCast ("SCNN|" + c.clientName, clients);
			break;
		case "CMOV":
			data = data.Replace ('C', 'S');
			BroadCast (data, clients);
			break;
		case "CMSG":
			BroadCast ("SMSG|" + c.clientName + ":" + aData [1], clients);
			break;
		}
	}
	//Server writer
	private void BroadCast(string data,List<ServerClient> cl){
		foreach (ServerClient sc in cl) {
			try {
				StreamWriter writer=new StreamWriter(sc.tcp.GetStream());
				writer.WriteLine(data);
				writer.Flush();
			} catch (Exception ex) {
				Debug.Log ("Write error:" + ex.Message);
			}
		}
	}
	private void BroadCast(string data,ServerClient cl){
		List<ServerClient> sc = new List<ServerClient>{ cl };
		BroadCast (data, sc);
	}
	private void StartListening(){
		server.BeginAcceptTcpClient (AcceptTcpClient,server);
	}
	private void AcceptTcpClient(IAsyncResult ar){
		TcpListener listener =(TcpListener) ar.AsyncState;

		string allUser = "";
		foreach (ServerClient s in clients) {
			allUser+=s.clientName+'|';
		}
		ServerClient sc = new ServerClient (listener.EndAcceptTcpClient (ar));
		clients.Add (sc);

		StartListening ();
		BroadCast ("SWHO|"+allUser,clients [clients.Count - 1]);
	}
	private bool IsConnected(TcpClient c){
		try {
			if(c!=null &&c.Client!=null&&c.Client.Connected){
				if(c.Client.Poll(0,SelectMode.SelectRead)){
					return !(c.Client.Receive(new byte[1],SocketFlags.Peek)==0);
				}
				return true;
			}else
				return false;
		} catch (Exception ex) {
			return false;
		}
	}

}
[Serializable]
public class ServerClient{
	public string clientName;
	public bool isHost;
	public TcpClient tcp;
	public ServerClient(TcpClient tcp){
		this.tcp = tcp;
	}
}
