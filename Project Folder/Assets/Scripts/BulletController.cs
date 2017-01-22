using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {

    public float speed = 15f;
    public GameObject shooter;
    public Terrain terrain;
    public TerrainData tdat;
    public float downforce = 10f;
    public float gracePeriod = 0.3f;
 	public Transform death_ps;
    public float lifeTime = 5f;
    Rigidbody rb;
    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        Physics.IgnoreCollision(GetComponent<Collider>(),
								shooter.GetComponent<Collider>());
    }
	
	// Update is called once per frame
	void Update () {

        if (gracePeriod <= 0)
        {
			if (shooter) {
				Physics.IgnoreCollision(GetComponent<Collider>(),
										shooter.GetComponent<Collider>(), false);
			}
        }
        if (lifeTime <= 0)
        {
            Instantiate(death_ps, transform.position, transform.rotation);
            Destroy(gameObject);
        }

        gracePeriod -= Time.deltaTime;
        lifeTime -= Time.deltaTime;

        //transform.Translate(Vector3.forward * speed * Time.deltaTime);
        var localpos = terrain.transform.InverseTransformPoint(transform.position);
        var normal = tdat.GetInterpolatedNormal(localpos.x / tdat.size.x,
                                                localpos.z / tdat.size.z);

        rb.AddForce(-normal * downforce);

    }

    //Die when touching sumo
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Instadeath" || col.gameObject.tag == "Player")
        {
			Instantiate(death_ps, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
