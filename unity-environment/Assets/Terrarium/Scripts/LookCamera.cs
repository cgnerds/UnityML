using System.Collections;
using UnityEngine;

public class LookCamera : MonoBehaviour {
	public float mouseSensitivityX = 5.0f;
	public float mouseSensitivityY = 5.0f;
	float rotY = 0.0f;

	// Use this for initialization
	void Start () {
	    if(GetComponent<Rigidbody>()) 
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(1))
		{
			float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
			rotY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
			rotY = Mathf.Clamp(rotY, -89.5f, 89.5f);
			transform.localEulerAngles = new Vector3(-rotY, rotX, 0.0f);
		}

		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			transform.position += transform.forward * 0.1f;
		}
		else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			transform.position -= transform.forward * 0.1f;
		}
		
		if(Input.GetKey(KeyCode.U))
		{
			gameObject.transform.localPosition = new Vector3(0.0f, 50.0f, 0.0f);
			transform.localEulerAngles = new Vector3(90f, 0.0f, 0.0f);
		}
	}
}
