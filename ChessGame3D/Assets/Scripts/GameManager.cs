using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour {
	public static GameManager ins;
	public GameObject mainMenu;
	public GameObject serverMenu;
	public GameObject connectMenu;

	public GameObject serverPertabs;
	public GameObject clientPertabs;

	public InputField nameInput;
	// Use this for initialization
	private void Start () {
		ins = this;
		mainMenu.SetActive (true);
		serverMenu.SetActive (false);
		connectMenu.SetActive (false);

		DontDestroyOnLoad (gameObject);
	}
	
	public void ConnectButton(){
		mainMenu.SetActive (false);
		connectMenu.SetActive (true);
	}
	public void HostButton(){
		try {
			Server s=Instantiate(serverPertabs).GetComponent<Server>();
			s.Init();

			Client cl=Instantiate(clientPertabs).GetComponent<Client>();
			cl.clientName=nameInput.text;
			cl.isHost=true;
			if(cl.clientName=="")
				cl.clientName="Host";
			cl.ConnectToServer("127.0.0.1",6321);
		} catch (System.Exception ex) {
			Debug.Log (ex.Message);
		}
		mainMenu.SetActive (false);
		serverMenu.SetActive (true);
	}
	public void ConnectToServerButton(){
		string hostAddress =GameObject.Find("HostInput").GetComponent<InputField>().text;;
		if(hostAddress=="")
			hostAddress="127.0.0.1";
		try {
			Client cl=Instantiate(clientPertabs).GetComponent<Client>();
			cl.clientName=nameInput.text;
			cl.isHost=false;
			if(cl.clientName=="")
				cl.clientName="Client";
			cl.ConnectToServer(hostAddress,6321);
			connectMenu.SetActive(false);
		} catch (System.Exception ex) {
			Debug.Log (ex.Message);
		}
	}
	public void BackButton(){
		mainMenu.SetActive (true);
		serverMenu.SetActive (false);
		connectMenu.SetActive (false);

		Server s = FindObjectOfType<Server> ();
		if (s != null)
			Destroy (s.gameObject);
		Client c = FindObjectOfType<Client> ();
		if (c != null)
			Destroy (c.gameObject);
	}
	public void HotseatButton(){
		SceneManager.LoadScene ("Main");
	}
	public void StartGame(){
		SceneManager.LoadScene ("Main");
	}
}
