using UnityEngine;
using System.Collections;

public class SimpleCharacterController : MonoBehaviour {
	public float speed = 10.0f;
	public float rotationSpeed = 100.0f;
	public float jumpSpeed = 8.0f;
	public float gravity = 20.0f;
	private Vector3 moveDirection = Vector3.zero;
	public bool flipCharacter = false;
	
	private CharacterController controller;
	// Use this for initialization
	void Start () {
		controller = collider as CharacterController;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 moveDirection = Vector3.zero;
		if (controller.isGrounded) {
			float rotation = Input.GetAxis ("Horizontal") * Time.deltaTime*rotationSpeed;
        	// Rotate around our y-axis
    		transform.Rotate (0, rotation, 0);
			
			// We are grounded, so recalculate
        	// move direction directly from axes
        	moveDirection.z = Input.GetAxis("Vertical")* Time.deltaTime*speed*(flipCharacter?-1:1);
			moveDirection = transform.TransformDirection(moveDirection);
			
    	}

    	// Apply gravity
    	moveDirection.y -= gravity * Time.deltaTime;
    
    	// Move the controller
    	controller.Move(moveDirection);
	}
}
