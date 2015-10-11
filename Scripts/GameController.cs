using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.Text;

public class GameController : MonoBehaviour {

	public Transform questList;
	public Text questDetails;
	public GetGeolocation playerLocation;
	public GameObject loadingBar;
	public UILoadingBar loadingScript;
	public Text addressText;
	public GameObject editDetailsPanel;
	public Text qName, qType, qMedia, qPre, qPost, qAnswer;

	private List<Quest> quests = new List<Quest>();

	private string baseUrl = "http://www.mastercava.it/digitalquest/";

	private GameObject objectToDisplay;

	private UnityEngine.Object[] objects;

	private int questCurrentlyDisplayed;
	private int objectCurrentlyDisplayed = 0;

	private static GameController instance;
	
	private GameController() {}
	
	public static GameController Instance {
		get {
			if (instance == null) {
				instance = new GameController();
			}
			return instance;
		}
	}

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		objects = Resources.LoadAll ("Objects");
	}

	public void LoadQuestData() {
		StartCoroutine (LoadQuestDataCoroutine(2));
	}

	private IEnumerator LoadQuestDataCoroutine(int questId) {
		Debug.Log ("Gathering database data...");
		SetLoadingBar (0f);
		WWW www = new WWW (baseUrl + "quest.php?id=" + questId);
		Debug.Log (baseUrl + "quest.php?id=" + questId);
		while (!www.isDone)
			SetLoadingBar (www.progress);
			yield return www.progress;
		Debug.Log (www.text);
		SetLoadingBar (1f);
		if (www.error != null) {
			Debug.Log ("No connection available!!");
		} else {
			if(www.text != "") {
				JSONNode objectsDatabase = JSON.Parse (www.text);
				for(int i=0; i<objectsDatabase["elements"].AsInt ; i++) {

					Quest newQuest = new Quest();
					JSONNode obj = objectsDatabase["objects"][i];
					newQuest.id = obj["id"].AsInt;
					newQuest.name = obj["name"].Value;
					newQuest.lat = obj["lat"].AsFloat;
					newQuest.lon = obj["lon"].AsFloat;
					newQuest.portal = obj["object"].Value;
					newQuest.pre = obj["pre"].Value;
					newQuest.post = obj["post"].Value;
					newQuest.answer = obj["answer"].Value;
					newQuest.type = obj["type"].AsInt;
					newQuest.media = obj["media"].Value;
					quests.Add (newQuest);

					//Create item in list
					UnityEngine.Object res = Resources.Load ("QuestTemplate");
					GameObject newElement = (GameObject) GameObject.Instantiate (res);
					newElement.transform.SetParent (questList);
					newElement.transform.localScale = Vector3.one;
					newQuest.questElement = newElement;
					Text questName = newElement.GetComponentInChildren<Text>();
					questName.text = newQuest.name;
					
					//Asign OnClick
					Button btn = newElement.GetComponent<Button> ();
					AddClickListener(btn, newQuest.id);
					
					//Put object on map
					UnityEngine.Object resource = Resources.Load ("MapObject");
					GameObject mapObject = (GameObject) GameObject.Instantiate (resource);
					SetGeolocation loc = mapObject.GetComponent<SetGeolocation> ();
					loc.lat = newQuest.lat;
					loc.lon = newQuest.lon;
					loc.height = playerLocation.height - 100;
					loc.scaleX = 1f;
					loc.scaleY = 1f;
					loc.scaleZ = 1f;
					GetGeolocation realLoc = mapObject.GetComponent<GetGeolocation> ();
					newQuest.location = realLoc;
					QuestObject script = mapObject.AddComponent<QuestObject> ();
					script.id = newQuest.id;
					newQuest.mapObject = mapObject;

					if(newQuest.id == 1) {
						MoveCursor(newQuest.lat, newQuest.lon);
					}
				}
			}
		}
	}

	private void AddClickListener(Button btn, int questId) {
		btn.onClick.AddListener (delegate {
			ShowQuestDetails(questId);
		});
	}

	public void DisplayObject(string name) {
		if (objectToDisplay != null) {
			Destroy (objectToDisplay);
		}
		UnityEngine.Object resource = Resources.Load ("Objects/" + name);
		objectToDisplay = (GameObject)GameObject.Instantiate (resource, Vector3.zero, Quaternion.identity);
	}

	public void NextObject() {
		if (objectToDisplay != null & questCurrentlyDisplayed > 0) {
			Destroy (objectToDisplay);
			objectCurrentlyDisplayed++;
			if(objectCurrentlyDisplayed >= objects.Length) objectCurrentlyDisplayed = 0;
			objectToDisplay = (GameObject)GameObject.Instantiate (objects[objectCurrentlyDisplayed], Vector3.zero, Quaternion.identity);
			quests[questCurrentlyDisplayed-1].portalObj = objectToDisplay;
			quests[questCurrentlyDisplayed-1].portal = objects[objectCurrentlyDisplayed].name;
			questDetails.text = quests [questCurrentlyDisplayed - 1].Summary ();
		}
	}

	public void PreviousObject() {
		if (objectToDisplay != null & questCurrentlyDisplayed > 0) {
			Destroy (objectToDisplay);
			objectCurrentlyDisplayed--;
			if(objectCurrentlyDisplayed <0) objectCurrentlyDisplayed = objects.Length - 1;
			objectToDisplay = (GameObject)GameObject.Instantiate (objects[objectCurrentlyDisplayed], Vector3.zero, Quaternion.identity);
			quests[questCurrentlyDisplayed-1].portalObj = objectToDisplay;
			quests[questCurrentlyDisplayed-1].portal = objects[objectCurrentlyDisplayed].name;
			questDetails.text = quests [questCurrentlyDisplayed - 1].Summary ();
		}
	}

	public void AddQuestHere() {
		AddQuest (playerLocation.lat, playerLocation.lon);
	}

	public void DeleteCurrentQuest() {
		Debug.Log ("Trying to delete quest #" + questCurrentlyDisplayed);
		if (questCurrentlyDisplayed > 0) {
			foreach(Quest q in quests) {
				if(q.id == questCurrentlyDisplayed) {
					Destroy(q.questElement);
					Destroy (q.mapObject);
					Destroy(objectToDisplay);
					objectToDisplay = null;
					questDetails.text = "";
					Debug.Log("Quest " + q.id + " removed.");
					quests.Remove(q);
					ResetQuestIds();
					break;
				}
			}
			questCurrentlyDisplayed = 0;
		}
	}

	private void ResetQuestIds() {
		int i = 1;
		foreach(Quest q in quests) {
			q.id = i;
			i++;
		}
	}

	public void AddQuest(float lat, float lon) {
		//Create new quest
		Quest newQuest = new Quest ();
		newQuest.id = quests.Count + 1;
		newQuest.portal = "Plasma_02";
		newQuest.pre = "";
		newQuest.answer = "";
		newQuest.post = "";
		//Add previous as precedence
		if(quests.Count > 0) newQuest.precedences.Add (quests.Count);
		quests.Add (newQuest);

		//Create item in list
		UnityEngine.Object res = Resources.Load ("QuestTemplate");
		GameObject newElement = (GameObject) GameObject.Instantiate (res);
		newElement.transform.SetParent (questList);
		newElement.transform.localScale = Vector3.one;
		newQuest.questElement = newElement;

		//Asign OnClick
		Button btn = newElement.GetComponent<Button> ();
		btn.onClick.AddListener (delegate {
			ShowQuestDetails(newQuest.id);
		});

		//Put object on map
		UnityEngine.Object resource = Resources.Load ("MapObject");
		GameObject mapObject = (GameObject) GameObject.Instantiate (resource);
		SetGeolocation loc = mapObject.GetComponent<SetGeolocation> ();
		loc.lat = lat;
		loc.lon = lon;
		loc.height = playerLocation.height - 100;
		loc.scaleX = 1f;
		loc.scaleY = 1f;
		loc.scaleZ = 1f;
		GetGeolocation realLoc = mapObject.GetComponent<GetGeolocation> ();
		newQuest.location = realLoc;
		QuestObject script = mapObject.AddComponent<QuestObject> ();
		script.id = newQuest.id;
		newQuest.mapObject = mapObject;

	}

	public void ShowQuestDetails(int questId) {
		questCurrentlyDisplayed = questId;
		DisplayObject (quests[questId - 1].portal);
		questDetails.text = quests [questId - 1].Summary ();
	}

	public void EditQuestDetails() {
		if(questCurrentlyDisplayed > 0) {
			Quest quest = quests[questCurrentlyDisplayed-1];
			qName.text = quest.name;
			qType.text = "" + quest.type;
			qMedia.text = quest.media;
			qPre.text = quest.pre;
			qPost.text = quest.post;
			qAnswer.text = quest.answer;
			editDetailsPanel.SetActive (true);
		}
	}

	public void SaveEditDetailsPanel() {
		Quest quest = quests[questCurrentlyDisplayed-1];
		quest.name = qName.text;
		Int32.TryParse (qType.text, out quest.type);
		quest.media = qMedia.text;
		quest.pre = qPre.text;
		quest.post = qPost.text;
		quest.answer = qAnswer.text;
		editDetailsPanel.SetActive (false);

		Text questName = quest.questElement.GetComponentInChildren<Text>();
		questName.text = quest.name;
		ShowQuestDetails (questCurrentlyDisplayed);
	}

	public void CloseEditDetailsPanel() {
		editDetailsPanel.SetActive (false);
	}

	public void ExportQuest() {
		StartCoroutine ("UploadToServer");
	}

	private IEnumerator UploadToServer() {
		JSONNode json = new JSONClass();
		json ["id"].AsInt = 2;
		json ["name"] = "Chicago Demo";
		json ["elements"].AsInt = quests.Count;
		
		//Add quests
		JSONArray questArray = new JSONArray ();
		int i = 0;
		foreach(Quest q in quests) {
			JSONNode questNode = new JSONClass();
			questNode["id"].AsInt = i+1;
			questNode["name"] = q.name;
			questNode["object"] = q.portal;
			questNode["type"].AsInt = q.type;
			questNode["media"] = q.media;
			questNode["points"].AsInt = q.points;
			questNode["scale"].AsInt = 1;
			questNode["map"] = "yes";
			questNode["pre"] = q.pre;
			questNode["answer"] = q.answer;
			questNode["post"] = q.post;
			questNode["lat"].AsFloat = q.location.lat;
			questNode["lon"].AsFloat = q.location.lon;
			questArray[i] = questNode;
			i++;
		}
		json ["objects"] = questArray;
		
		Debug.Log(json.ToString());
		//Debug.Log (json.SaveToBase64());
		string url = "http://www.mastercava.it/digitalquest/save_quest.php?data=" + WWW.EscapeURL (Convert.ToBase64String (Encoding.UTF8.GetBytes (json.ToString ())));
		Debug.Log (url);
		WWW www = new WWW (url);
		while (!www.isDone) {
			SetLoadingBar(www.progress);
			yield return null;
		}
		SetLoadingBar (1f);
	}

	public void SetLoadingBar(float value) {
		loadingBar.SetActive (true);
		loadingScript.SetValue (value);
		if (value == 1f) {
			loadingBar.SetActive (false);
		}
	}

	public void SearchAddress() {
		if (addressText.text != "") {
			StartCoroutine (SetPositionOnMap (WWW.EscapeURL(addressText.text)));
		}
	}


	private IEnumerator SetPositionOnMap(string address) {
		WWW www = new WWW ("https://maps.googleapis.com/maps/api/geocode/json?address" + address);

		while(!www.isDone) {
			yield return null;
		}
		Debug.Log (www.text);
		JSONNode response = JSON.Parse (www.text);
		float lat = response["results"][0]["geometry"]["location"]["lat"].AsFloat;
		float lon = response["results"][0]["geometry"]["location"]["lng"].AsFloat;
		MoveCursor (lat, lon);
	}

	private void MoveCursor(float lat, float lon){
		SetGeolocation oldScript = playerLocation.gameObject.GetComponent<SetGeolocation> ();
		if (oldScript != null) {
			Destroy (oldScript);
		}
		SetGeolocation script = playerLocation.gameObject.AddComponent<SetGeolocation> ();
		script.lat = lat;
		script.lon = lon;
		script.height = playerLocation.height;
		script.scaleX = playerLocation.scaleX;
		script.scaleY = playerLocation.scaleY;
		script.scaleZ = playerLocation.scaleZ;
	}

}
