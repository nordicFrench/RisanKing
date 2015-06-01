using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    private Transform myTransform;
    private Vector3 tempPos;

    private Rigidbody2D body;

    public float speed = 1f;

	// Use this for initialization
	void Start () {
        this.myTransform = gameObject.transform;
        this.tempPos = this.myTransform.position;
        body = gameObject.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
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

        if (xa != 0 || ya != 0) {
            tempPos.x = myTransform.position.x + xa * Time.deltaTime;
            tempPos.y = myTransform.position.y + ya * Time.deltaTime;
            body.MovePosition(tempPos);
        }        
	}
}
