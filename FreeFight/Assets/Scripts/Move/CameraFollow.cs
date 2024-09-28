using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	// public float rotSpeed = 0.2f;
	// public float rollSpeed = 0.2f;
	// public float zoomspeed = 0.2f;

	private float distance = 60.0f;
	private float rot = 0;
	private float roll = 45.0f * Mathf.PI * 2 / 360;
	private GameObject target;
	// private float maxroll = 70.0f * Mathf.PI * 2 / 360;
	// private float minroll = -10.0f * Mathf.PI * 2 / 360;
	// private float maxdistance = 100.0f;
	// private float mindistance = 50.0f;

	// update is called once per frame
	void LateUpdate()
	{
		if (NetManager.socket == null) return;

		if (target == null)
			target = GameObject.Find("EthanCtr");
		if (Camera.main == null)
			return;

			// Rotate();
			// Roll();
			// Zoom();
		
		Vector3 targetpos = target.transform.position;
		Vector3 cameraPos;
		float d = distance * Mathf.Cos(roll);
		float height = distance * Mathf.Sin(roll);
		cameraPos.x = targetpos.x + d * Mathf.Cos(rot);
		cameraPos.z = targetpos.z - d * Mathf.Sin(rot);
		cameraPos.y = targetpos.y + height;

		Camera.main.transform.position = cameraPos;
		Camera.main.transform.LookAt(target.transform);

		foreach (GameObject ethan in GameObject.FindGameObjectsWithTag("Ethan"))
		{
			ethan.transform.Find("Canvas").transform.rotation =
			Camera.main.transform.rotation;
		}
	}

	//void Rotate()
	//{
	//	float w = Input.GetAxis("Mouse X") * rotSpeed;
	//	rot -= w;
	//}

	//void Roll()
	//{
	//	float w = Input.GetAxis("Mouse Y") * rollSpeed;
	//	roll -= w;
	//	if (roll > maxroll)
	//		roll = maxroll;
	//	if (roll < minroll)
	//		roll = minroll;
	//}

	//void Zoom()
	//{
	//	if (Input.GetAxis("Mouse ScrollWheel") > 0)
	//	{
	//		distance -= zoomspeed;
	//		if (distance < mindistance)
	//			distance = mindistance;
	//	}
	//	else if (Input.GetAxis("Mouse ScrollWheel") < 0)
	//	{
	//		distance += zoomspeed;
	//		if (distance > maxdistance)
	//			distance = maxdistance;
	//	}
	//}
}