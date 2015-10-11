using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Quest {

	public int id;
	public string portal;

	public GameObject portalObj;
	public GameObject questElement;

	public GameObject mapObject;

	public GetGeolocation location;

	public string name, media, pre, answer, post;
	public int type = 1, points = 100;
	public float lat, lon;

	public List<int> precedences = new List<int>();

	public string Summary() {
		string summary = "Name:\t" + name + "\n" +
			"Portal:\t" + portal + "\n" +
			"Location:\t" + location.lat + ", " + location.lon;
		if (precedences.Count > 0)
			summary += "\nPrecedences:\t" + DisplayPrecedences();
		if (pre != "")
			summary += "\nPre:\t" + pre;
		if (answer != "")
			summary += "\nAnswer:\t" + answer;
		if (post != "")
			summary += "\nPost:\t" + post;
		return summary;
	}

	public string DisplayPrecedences() {
		int i = 0;
		string text = "";
		foreach (int p in precedences) {
			text += (i!=0 ? ", " : "") + p;
		}
		return text;
	}

}
