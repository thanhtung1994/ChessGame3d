using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class CheckerBroad : MonoBehaviour {
	public static CheckerBroad ins;

	public Piece[,] pieces = new Piece[8, 8];
	public GameObject whitePiecePertab;
	public GameObject blackPiecePertabs;
	public CanvasGroup alertCanvas;
	public GameObject highLightContainer;

	public Transform chatpanelTranform;
	public GameObject messagerPertabs;

	private float lasAlert;
	private bool isAlertActive;

	private Vector3 broadOffet=new Vector3(-4f,0,-4f);
	private Vector3 pieceOffset=new Vector3(0.5f,0.125f,0.5f);

	public bool isWhite;
	private bool isWhiteTurn;
	private bool hasKilled;

	private Piece pieceSeleted;
	private List<Piece>forcedPieces;

	private Vector2 mouseOver;
	private Vector2 startDrag;
	private Vector2 endDrag;

	private Client client;
	private void Start(){
		if (ins == null)
			ins = this;
		else
			Destroy (gameObject);
		foreach (Transform t in highLightContainer.transform) {
			t.position = Vector3.down * 10f;
		}
		client = FindObjectOfType<Client> ();
		if (client) {
			isWhite = client.isHost;
			Alert (client.players[0].name+"vs"+client.players[1].name);
		} else {
			isWhite = true;
			Alert ("White player starts");
			Transform c = GameObject.Find ("Canvas").transform;
			foreach (Transform item in c) {
				item.gameObject.SetActive (false);
			}
			c.GetChild (0).gameObject.SetActive (true);
		}
		isWhiteTurn = true;
		forcedPieces = new List<Piece> ();
		GenerateBroad ();

	}
	private void Update(){
		foreach (Transform t in highLightContainer.transform) {
			t.Rotate (Vector3.right * Time.deltaTime*2.5f);
		}
		UpdateAlert ();
		UpdateMouseOver ();
		if (isWhite ? !isWhiteTurn : isWhiteTurn)
			return;
		//if its my turn
		int x = (int)mouseOver.x;
		int y = (int)mouseOver.y;
		if (pieceSeleted != null)
			UpdatePieceDrag (pieceSeleted);
		if (Input.GetMouseButtonDown (0)) {
			SelectPiece (x, y);
			Debug.Log (x + ":" + y);
		}
		if (Input.GetMouseButtonUp (0)) {
			TryMove ((int)startDrag.x, (int)startDrag.y, x, y);
		}
	}

	private void UpdateMouseOver(){
	if(!Camera.main){
		Debug.Log ("Unable to find main camera");
		return;
	}
	RaycastHit hit;
	if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 25, LayerMask.GetMask ("Board"))) {
		mouseOver.x = (int)(hit.point.x-broadOffet.x);
		mouseOver.y = (int)(hit.point.z-broadOffet.z);
	} else {
		mouseOver.y = -1;
		mouseOver.x = -1;
	}
	}
	private void UpdatePieceDrag(Piece p){
	if(!Camera.main){
		Debug.Log ("Unable to find main camera");
		return;
	}
	RaycastHit hit;
	if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 25, LayerMask.GetMask ("Board"))) {
			p.transform.position = hit.point + Vector3.up;
	} 
	}
	private void SelectPiece(int x,int y){
		//out of bounds
		if(x<0||x>=8||y<0||y>=8){
			return;
		}
		Piece p = pieces [x, y];
		if (p != null&&p.isWhite==isWhite) {
			if (forcedPieces.Count == 0) {
				pieceSeleted = p;
				startDrag = mouseOver;
			} else {
				//Look for the pieces under our forced pieces list
				if (forcedPieces.Find (a => a== p) == null)
					return;
				pieceSeleted = p;
				startDrag = mouseOver;
			}
		}
			
	}
	public void TryMove(int x1,int y1,int x2,int y2){

		forcedPieces = ScanForPossibleMove ();
		//Multipplayer Support
		startDrag=new Vector2(x1,y1);
		endDrag = new Vector2 (x2, y2);
		pieceSeleted = pieces [x1, y1];
		// Out of broad
		if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8) {
			if (pieceSeleted != null)
				MovePiece (pieceSeleted, x1, y1);
			startDrag = Vector2.zero;
			pieceSeleted = null;
			HightLight ();
			return;
		}
		if (pieceSeleted != null) {
			if (startDrag == endDrag) {
				MovePiece (pieceSeleted, x1, y1);
				startDrag = Vector2.zero;
				pieceSeleted = null;
				HightLight ();
				return;
			}
			//Check if its a valid move
			if (pieceSeleted.ValidMove (pieces, x1, y1, x2, y2)) {
				// Did we kill anything 
				//If this is  a jump 
				Debug.Log ("3");
				if (Mathf.Abs (x2 - x1) == 2) {
					Piece p = pieces [(x1 + x2) / 2, (y1 + y2) / 2];
					if (p != null) {
						pieces [(x1 + x2) / 2, (y1 + y2) / 2] = null;
						Destroy (p.gameObject);
						hasKilled = true;
					}
				}

				//Were we suposed to kill anything?
				if(forcedPieces.Count!=0&&!hasKilled){
					MovePiece (pieceSeleted, x1, y1);
					startDrag = Vector2.zero;
					pieceSeleted = null;
					HightLight ();
					return;
				}
				pieces [x2, y2] = pieceSeleted;
				pieces [x1, y1] = null;
				MovePiece (pieceSeleted, x2, y2);
				EndTurn ();

			} else {
				MovePiece (pieceSeleted, x1, y1);
				startDrag = Vector2.zero;
				pieceSeleted = null;
				HightLight ();
				return;
			}

		}
	}
	private void EndTurn(){
		int x = (int)endDrag.x;
		int y = (int)endDrag.y;
		//Promotions 
		if (pieceSeleted != null) {
			if (pieceSeleted.isWhite && !pieceSeleted.isKing && y == 7) {
				pieceSeleted.isKing = true;
				pieceSeleted.transform.Rotate (Vector3.right * 180);
			}else if  (!pieceSeleted.isWhite && !pieceSeleted.isKing && y == 0) {
				pieceSeleted.isKing = true;
				pieceSeleted.transform.Rotate (Vector3.right * 180);
			}
		}
		if (client) {
			string msg = "CMOV|";
			msg += startDrag.x.ToString () + '|';
			msg += startDrag.y.ToString () + '|';
			msg += endDrag.x.ToString () + '|';
			msg += endDrag.y.ToString ();
			client.Send (msg);
		}
		pieceSeleted = null;
		startDrag = Vector2.zero;

		if (pieces[x,y].IsForceToMove(pieces,x,y) && hasKilled) {
			return;
		}
		isWhiteTurn = !isWhiteTurn;
		if (isWhiteTurn)
			Alert ("White player turn");
		else
			Alert ("Black player turn");
		hasKilled = false;
		CheckVictory ();

		if (!client)
			isWhite = !isWhite;

		ScanForPossibleMove ();
	}
	private void CheckVictory(){
		var ps = FindObjectsOfType<Piece> ();
		bool hasWhite = false, hasBlack = false;
		for (int i = 0; i < ps.Length; i++) {
			if (ps [i].isWhite)
				hasWhite = true;
			else
				hasBlack = true;
		}
		if (!hasWhite)
			Victory (false);
		if (!hasBlack)
			Victory (true);
	}
	private void Victory(bool isWhite){
		if (isWhite)
			Debug.Log ("White team has won");
		else
			Debug.Log ("Black team has won");
	}
//	private List<Piece> ScanForPossibleMove(Piece p,int x,int y){
//		forcedPieces = new List<Piece> ();
//
//
//		return forcedPieces;
//	}
	private List<Piece> ScanForPossibleMove(){
		forcedPieces = new List<Piece> ();
		//Check all the pieces 
		for (int i = 0; i < 8; i++) {
			for (int j = 0; j < 8; j++) {
				if (pieces [i, j] != null) {
					if (pieces [i, j] != null && pieces [i, j].isWhite == isWhiteTurn) {
						if (pieces [i, j].IsForceToMove (pieces, i, j)) {
							forcedPieces.Add (pieces [i, j]);
						}
					}
				}
			}
		}
		HightLight ();
		return forcedPieces;
	}
	private void HightLight(){
		foreach (Transform t in highLightContainer.transform) {
			t.position = Vector3.down * 10f;
		}
		if (forcedPieces.Count>0)
			highLightContainer.transform.GetChild (0).transform.position = forcedPieces [0].transform.position+Vector3.down*0.1f;
		
		if (forcedPieces.Count>1)
			highLightContainer.transform.GetChild (1).transform.position = forcedPieces [1].transform.position+Vector3.down*0.1f;
	}
	private void GenerateBroad(){
		//generate white piece team 
		for (int y = 0; y < 3; y++) {
			bool oddRod = (y % 2 == 0);
			for (int x = 0; x < 8; x+=2) {
				//generate out piece
				GeneratePiece((oddRod)? x:x+1,y,whitePiecePertab);
			}
		}
		//generate black piece team 
		for (int y = 7; y >4; y--) {
			bool oddRod = (y % 2 == 0);
			for (int x = 0; x < 8; x+=2) {
				//generate out piece
				GeneratePiece((oddRod)? x:x+1,y,blackPiecePertabs);
			}
		}
	}
	private void GeneratePiece(int x, int y,GameObject g){
		GameObject go = Instantiate (g)as GameObject;
		go.transform.SetParent (transform);
		Piece p = go.GetComponent<Piece> ();
		pieces [x, y] = p;
		MovePiece (p, x, y);
	}
	private void MovePiece(Piece p,int x, int y){
		p.transform.position = (Vector3.right * x) + (Vector3.forward * y)+broadOffet+pieceOffset;
	}
	public void Alert(string text){
		alertCanvas.GetComponentInChildren<Text> ().text = text;
		lasAlert = Time.time;
		alertCanvas.alpha = 1;
		isAlertActive = true;
	}
	public void UpdateAlert(){
		if (isAlertActive) {
			if (Time.time - lasAlert > 1.5f) {
				alertCanvas.alpha = 1-((Time.time - lasAlert) - 1.5f);
				if (Time.time - lasAlert > 2.5f) {
					isAlertActive = false;
				}
			}
		}
			
	}
	public void ChatMessage(string data){
		GameObject go = Instantiate (messagerPertabs)as GameObject;
		go.transform.SetParent (chatpanelTranform);

		go.GetComponentInChildren<Text> ().text = data;
	}
	public void SendChatMessage(){
		InputField i = GameObject.Find ("MessageInput").GetComponentInChildren<InputField> ();
		if (i.text == "")
			return;
		client.Send ("CMSG|"+i.text);
		i.text = "";
	}
}
