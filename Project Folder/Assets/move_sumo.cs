using UnityEngine;
using System.Collections;

public class move_sumo : MonoBehaviour {
	public float theta;
	public float theta_per_second;
	public float revolve_radius;
	public Terrain terrain;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		theta += theta_per_second * Time.deltaTime;

		var pos = transform.position;
		var terr_center = terrain.transform.position + (terrain.terrainData.size)/2;

		pos.x = terr_center.x + revolve_radius * Mathf.Sin(theta);
		pos.z = terr_center.z + revolve_radius * Mathf.Cos(theta);

		transform.position = pos;
	}
}
