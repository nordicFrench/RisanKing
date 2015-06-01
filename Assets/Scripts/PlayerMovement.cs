using UnityEngine;
using System.Collections;

[System.Serializable]
public class Boundary {
    public float xMin, xMax, yMin, yMax;
}

public class PlayerMovement : MonoBehaviour {

    private Transform myTransform;
    private Vector3 tempPos;

    private Rigidbody2D body;

    public float speed;
    public float tilt;
    public Boundary boundary;

	// Use this for initialization
	void Start () {
        this.myTransform = gameObject.transform;
        this.tempPos = this.myTransform.position;
        body = gameObject.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        float xa = 0;
        float ya = 0;
        if (Input.GetKey(KeyCode.UpArrow)) {
            ya = 1f * speed;
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            ya = -1f * speed;
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            xa = 1f * speed;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            xa = -1f * speed;
        }




        Vector2 movement = new Vector2(xa, ya);
        body.velocity = movement * speed;
        body.position = new Vector2
        (
            Mathf.Clamp(body.position.x, boundary.xMin, boundary.xMax),
            Mathf.Clamp(body.position.y, boundary.yMin, boundary.yMax)
        );
        body.rotation = 0f;
               
	}
}
