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
    public AudioSource rollsound;
    Vector3 normal;
	public int m_PlayerNumber = 1;
    // testing

	void Start() {
		tdat = terrain.terrainData;
        rollsound = GetComponent<AudioSource>();
    }

	void FixedUpdate() {
        rb.AddForce((cam.transform.forward - Vector3.Dot(cam.transform.forward, normal) * normal).normalized * -Input.GetAxis(inputVrt) * speed);
        rb.AddForce((cam.transform.right - Vector3.Dot(cam.transform.right, normal) * normal).normalized * Input.GetAxis(inputHrz) * speed);

		var localpos = terrain.transform.InverseTransformPoint(transform.position);
		normal = tdat.GetInterpolatedNormal(localpos.x / tdat.size.x,
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
            bullet.GetComponent<BulletController>().terrain = terrain;
            bullet.GetComponent<BulletController>().tdat = tdat;
            bullet.GetComponent<Rigidbody>().velocity = (cam.transform.forward - Vector3.Dot(cam.transform.forward, normal) * normal).normalized * bullet.GetComponent<BulletController>().speed;
        }

        float magnitude = rb.velocity.magnitude;
        rollsound.volume = magnitude / 12;
        rollsound.pitch = magnitude / 15;


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
