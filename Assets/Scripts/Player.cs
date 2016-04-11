using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public float moveSpeed;

	public Animator anim;

	private bool PlayerMoving = false;
	private bool wasMoving = false;
	private bool destinationReached = false;
	private Vector2 lastMove;

	private Vector3 moveTarget;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

		float horiz = 0;
		float vert = 0;

		float velocity = moveSpeed * GameController.getDeltaTime ();

		if (!transform.Equals (moveTarget)) {
			// move to target
			transform.position = Vector3.MoveTowards (transform.position, moveTarget, velocity);
		
			horiz = (moveTarget - transform.position).x * velocity;
			vert = (moveTarget - transform.position).y * velocity;
		}

		anim.SetFloat ("MoveX", horiz);
		anim.SetFloat ("MoveY", vert);

		wasMoving = PlayerMoving;
		PlayerMoving = false;

		if (horiz != 0) {
			PlayerMoving = true;
			lastMove = new Vector2(horiz,0);
		}

		if (vert != 0) {
			PlayerMoving = true;
			lastMove = new Vector2(0,vert);
		}

		if (wasMoving == true && PlayerMoving == false)
			destinationReached = true;
			

		anim.SetFloat ("LastMoveX", lastMove.x);
		anim.SetFloat ("LastMoveY", lastMove.y);
		anim.SetBool ("PlayerMoving", PlayerMoving);
	}

	public void moveTo( Vector3 location ) {
		moveTarget = location;
	}

	public bool getDestinationReached() {
		return destinationReached;
	}

	public void resetDestinationReached() {
		destinationReached = false;
	}
}
