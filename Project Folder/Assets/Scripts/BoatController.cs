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
	public float fire_cd;
	public float prev_fire;
    public string inputHrz;
    public string inputVrt;
    public string inputFire;
    public GameObject bulletPrefab;
    public GameObject cam;
    public AudioSource rollsound;
	public aplay_mb apmb;
	public Transform death_ps;
    Vector3 normal;
    // testing

	void Start() {
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
           inputFire += "_mac";
           #endif

		
		tdat = terrain.terrainData;
        rollsound = GetComponent<AudioSource>();
    }

	void FixedUpdate() {
        rb.AddForce((cam.transform.forward -
					 Vector3.Dot(cam.transform.forward, normal) * normal).normalized *
					-Input.GetAxis(inputVrt) *
					speed);
        rb.AddForce((cam.transform.right -
					 Vector3.Dot(cam.transform.right, normal) * normal).normalized *
					Input.GetAxis(inputHrz) *
					speed);

		var localpos = terrain.transform.InverseTransformPoint(transform.position);
		normal = tdat.GetInterpolatedNormal(localpos.x / tdat.size.x,
												localpos.z / tdat.size.z);

		rb.AddForce(-normal * downforce);
	}
	
	// Update is called once per frame
	void Update () {


        if (Input.GetButtonDown(inputFire) && (Time.time - prev_fire > fire_cd))
        {
			prev_fire = Time.time;
			
			var asrc = apmb.get_src();
			asrc.clip = apmb.clip_dict["asteroid_fire"];
			asrc.Play();

            GameObject bullet = Instantiate(bulletPrefab,
											transform.position,
											cam.transform.rotation);
			var bc = bullet.GetComponent<BulletController>();
			bc.shooter = gameObject;
            bc.terrain = terrain;
            bc.tdat = tdat;
            bc.GetComponent<Rigidbody>().velocity =
				((cam.transform.forward -
				  Vector3.Dot(cam.transform.forward, normal) * normal).normalized *
				 bc.speed);
        }

        float magnitude = rb.velocity.magnitude;
        rollsound.volume = magnitude / 12;
        rollsound.pitch = magnitude / 15;


    }
    //Die when touching sumo
    void OnCollisionEnter(Collision col)
    {
		if (col.gameObject.tag == "Wall") {
			var asrc = apmb.get_src();
			asrc.clip = apmb.clip_dict["bump"];
			asrc.Play();
		}
		
        if (col.gameObject.tag == "Instadeath")
        {
			Instantiate(death_ps, transform.position, transform.rotation);

			var asrc2 = apmb.get_src();
			asrc2.clip = apmb.clip_dict["explosion"];
			asrc2.Play();
            Destroy(gameObject);
        }
    }


}
