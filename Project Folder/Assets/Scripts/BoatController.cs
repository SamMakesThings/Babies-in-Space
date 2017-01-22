using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour {
	public Terrain terrain;
	public TerrainData tdat;
    public Rigidbody rb;
    public float rotSpeed;
    public float speed;
    public float breakSpeed;
	public float downforce;
    public string inputHrz;
    public string inputVrt;
    public string inputFire;
    public GameObject bulletPrefab;
    public GameObject cam;

	void Start() {
		tdat = terrain.terrainData;
	}

	void FixedUpdate() {
        rb.AddForce(cam.transform.forward * -Input.GetAxis(inputVrt) * speed);
        rb.AddForce(cam.transform.right * Input.GetAxis(inputHrz) * speed);

		var localpos = terrain.transform.InverseTransformPoint(transform.position);
		var normal = tdat.GetInterpolatedNormal(localpos.x / tdat.size.x,
												localpos.z / tdat.size.z);

		rb.AddForce(-normal * downforce);
	}
	
	// Update is called once per frame
	void Update () {


        if (Input.GetButtonDown(inputFire))
        {
            Vector3 bulletSource = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            GameObject bullet = Instantiate(bulletPrefab, bulletSource, cam.transform.rotation);
            bullet.GetComponent<BulletController>().shooter = gameObject;
        }

        
    }
    //Die when touching sumo
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Instadeath")
        {
            Destroy(gameObject);
        }
    }


}
