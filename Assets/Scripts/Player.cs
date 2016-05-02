using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public enum SwapIndex
	{
		TeamColor = 56,
		TeamColorHighlight = 128,
		TeamColorShading = 32
	}

	public float moveSpeed;

	public Animator anim;

	private bool PlayerMoving = false;
	private bool wasMoving = false;
	private bool destinationReached = false;
	private Vector2 lastMove;

	private Vector3 moveTarget;

	Texture2D mColorSwapTex;
	Color[] mSpriteColors;
	SpriteRenderer mSpriteRenderer;

	/******************/
	/*  Color Values  */
	/******************/

	float hue = 0f;
	float mainSat = 1.0f;
	float mainVal = 0.53f;
	float highlightSat = 0.73f;
	float highlightVal = 0.82f;
	float shadingSat = 0.75f;
	float shadingVal = 0.25f;

	/******************/

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		mSpriteRenderer = GetComponent<SpriteRenderer> ();

		InitColorSwapTex ();

		SwapColor (SwapIndex.TeamColor, Color.HSVToRGB(hue, mainSat, mainVal));
		SwapColor (SwapIndex.TeamColorHighlight, Color.HSVToRGB(hue, highlightSat, highlightVal));
		SwapColor (SwapIndex.TeamColorShading, Color.HSVToRGB(hue, shadingSat, shadingVal));

		mColorSwapTex.Apply ();
	}
	
	// Update is called once per frame
	void Update () {

		float horiz = 0;
		float vert = 0;

		float velocity = moveSpeed * (float) GameController.getDeltaTime ();

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
		if (!location.Equals (moveTarget)) {
			destinationReached = false;
			moveTarget = location;
		}
	}

	public bool getDestinationReached() {
		return destinationReached;
	}

	public void resetDestinationReached() {
		destinationReached = false;
	}

	public void InitColorSwapTex() {
		Texture2D colorSwapTex = new Texture2D (256, 1, TextureFormat.RGBA32, false, false);
		colorSwapTex.filterMode = FilterMode.Point;

		for (int i = 0; i < colorSwapTex.width; ++i)
			colorSwapTex.SetPixel (i, 0, new Color (0.0f, 0.0f, 0.0f, 0.0f));

		colorSwapTex.Apply ();

		mSpriteRenderer.material.SetTexture ("_SwapTex", colorSwapTex);

		mSpriteColors = new Color[colorSwapTex.width];
		mColorSwapTex = colorSwapTex;
	}

	public void SwapColor( SwapIndex index, Color color) {
		mSpriteColors [(int)index] = color;
		mColorSwapTex.SetPixel ((int)index, 0, color);
	}

	public void SetColor( float newHue ) {
		hue = newHue;
	}

}
