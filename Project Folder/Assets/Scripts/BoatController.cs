using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour {

    Rigidbody rb;
    public float rotSpeed = .5f;
    public float speed = 5f;
    public float breakSpeed = 10f;
    public string inputHrz = "Horizontal_p1";
    public string inputVrt = "Vertical_p1";
    public string inputFire = "Fire1_p1";
    public GameObject bulletPrefab;
    public GameObject cam;

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

        rb.AddForce(cam.transform.forward * -Input.GetAxis(inputVrt) * speed);
        rb.AddForce(cam.transform.right * Input.GetAxis(inputHrz) * speed);

        if (Input.GetButtonDown(inputFire))
        {
            Vector3 bulletSource = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            GameObject bullet = Instantiate(bulletPrefab, bulletSource, transform.rotation);
            bullet.GetComponent<BulletController>().shooter = gameObject;
        }


    }

}
