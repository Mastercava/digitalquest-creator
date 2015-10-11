using UnityEngine;
using System.Collections;

public class QuestObject : MonoBehaviour {

	public int id;

	private Vector3 screenPoint, offset;

	void OnMouseDown() {
		screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
	}

	void OnMouseDrag() {
		Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
		transform.position = new Vector3(curPosition.x, transform.position.y, curPosition.z);
	}

	void OnMouseUp() {
		GameController.Instance.ShowQuestDetails (id);
	}

}
