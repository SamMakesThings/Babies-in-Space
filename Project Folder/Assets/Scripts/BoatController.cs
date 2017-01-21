using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour {

    Rigidbody rb;
    public float rotSpeed = .5f;
    public float speed = 5f;
    public float breakSpeed = 10f;
    //public string inputThrottle = "up";
    //public string inputBreak = "down";
    //public string inputLeft = "left";
    //public string inputRight = "right";
    //public string inputFire = "right ctrl";
    public GameObject bulletPrefab;

    // Use this for initialization
    void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        //if (Input.GetKey(inputRight))
        //{
        //    transform.Rotate(0, rotSpeed, 0);
        //}
        //if (Input.GetKey(inputLeft))
        //{
        //    transform.Rotate(0, -rotSpeed, 0);

        //}
        //if (Input.GetKey(inputThrottle))
        //{
        //    rb.AddForce(transform.forward * speed);
        //}
        //if (Input.GetKey(inputBreak))
        //{
        //    if (rb.velocity.x > 0)
        //    {
        //        rb.AddForce(-transform.forward * speed);
        //    }
        //}
        //if (Input.GetKeyDown(inputFire))
        //{
        //    Vector3 bulletSource = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //    GameObject bullet = Instantiate(bulletPrefab, bulletSource, transform.rotation);
        //    //set bullet speed?
        //    //set shooter to this to prevent friendly fire

        //}

        transform.Rotate(0, Input.GetAxis("Horizontal") * rotSpeed, 0);
        rb.AddForce(transform.forward * Input.GetAxis("Vertical") * rotSpeed);

        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 bulletSource = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            GameObject bullet = Instantiate(bulletPrefab, bulletSource, transform.rotation);
            bullet.GetComponent<BulletController>().shooter = gameObject;
        }


    }

}
