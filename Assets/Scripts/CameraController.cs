using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public float speed=100.0F;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float depth = 0.0F;
		if (Input.GetKey (KeyCode.LeftShift))
			depth=1.0F;


		transform.Translate(new Vector3(-Input.GetAxis("Horizontal") * speed * Time.deltaTime, -Input.GetAxis("Vertical") * speed * (1.0F-depth) * Time.deltaTime, 
		                                -Input.GetAxis("Vertical") *depth* speed * Time.deltaTime));

	}
}

