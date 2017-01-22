using UnityEngine;
using System.Collections;

public class change_terrain : MonoBehaviour {
	public Terrain terrain;
	public TerrainData tdat;
	public bool inited;
	public float[,] hmap;
	public float[,] vmap;
	public float[,] vmap_new;
	public int window_rad;
	public int sumo_pos_rad;
	public float sumo_weight;
	public float base_elast;
	public float inter_elast;
	public float max_elast;
	public float elem_mass;
	public float equil_height;
	public float max_vel;
	public float linear_damp;
	public float quad_damp;
	public SphereCollider[] sumos;
	public float[,] window;
	public int[,] sphere_poses;

	// Use this for initialization
	void Start () {
		int x = tdat.heightmapWidth;
		int y = tdat.heightmapHeight;
		
		hmap = new float[x, y];
		vmap = new float[x, y];
		vmap_new = new float[x, y];

		for (int i=0; i<x; i++) {
			for (int j=0; j<y; j++) {
				vmap[i,j] = 0;
				vmap_new[i,j] = 0;
			}
		}

		var ws = window_rad*2+1;
		window = new float[ws, ws];
		for (int i=0; i<ws; i++) {
			for (int j=0; j<ws; j++) {
				if (i != ws || j != ws) {
					window[i,j] = inter_elast/((i-window_rad)*(i-window_rad) +
											   (j-window_rad)*(j-window_rad));
				}
			}
		}

		sphere_poses = new int[sumos.Length, 2];
	}

	void p3(int i, int j) {
				vmap_new[i,j] += base_elast*(equil_height-hmap[i,j]);
				vmap[i,j] += Mathf.Clamp(vmap_new[i,j], -max_elast, max_elast);
				vmap[i,j] -= quad_damp*(vmap[i,j]*vmap[i,j]);
				vmap[i,j] *= (1.0f - linear_damp);
				vmap[i,j] = Mathf.Clamp(vmap[i,j], -max_vel, max_vel);

	}

	void p1() {
		int tdatw = tdat.heightmapWidth;
		int tdath = tdat.heightmapHeight;

		hmap = tdat.GetHeights(0,0,tdatw,tdath);
		/*if (!inited) {		
			for (int i=0; i<tdatw; i++) {
				for (int j=0; j<tdath; j++) {
					/*if (i<tdatw/2 && j < tdath/2) {
						hmap[i,j] = 0.4f;
					}
					else {* /
						hmap[i,j] = 0.5f;
				//}
				}
			}
			inited = true;
		}*/

		for (int i=0; i<tdatw; i++) {
			var kmin = Mathf.Max(0, i-window_rad);
			var klim = Mathf.Min(tdatw, i+window_rad+1);
			for (int j=0; j<tdath; j++) {
				vmap_new[i,j] = 0;

				var hmapij = hmap[i,j];
				var win_jmin = j-window_rad;
				var lmin = Mathf.Max(0, win_jmin);
				var llim = Mathf.Min(tdath, j+window_rad+1);				
				for (int k=kmin; k<klim; k++) {
					var windowk = k-(i-window_rad);
					
					for (int l=lmin; l<llim; l++) {
						if (k != i || j != l) {
							vmap_new[i,j] += window[windowk,
													l-win_jmin]*(hmap[k,l] -
																 hmapij);
						}
					}
				}
				p3(i,j);
			}
		}

	}
	// Update is called once per frame
	void Update () {
		int tdatw = tdat.heightmapWidth;
		int tdath = tdat.heightmapHeight;

		p1();
		/* width corresponds to x, height (aka length in the editor) corresponds to z */
		for (int idx=0; idx<sumos.Length; idx++) {
			var localpos = terrain.transform.
				InverseTransformPoint(sumos[idx].transform.position);
			
			sphere_poses[idx,1] = (int)(localpos.x / tdat.heightmapScale.x);
			sphere_poses[idx,0] = (int)(localpos.z / tdat.heightmapScale.z);

			var win_rad_x = (int)(sumos[idx].radius *
								  sumos[idx].transform.localScale.x /
								  tdat.heightmapScale.x);
			var win_rad_y = (int)(sumos[idx].radius *
								  sumos[idx].transform.localScale.z /
								  tdat.heightmapScale.z);

			var ilim = Mathf.Min(tdatw, sphere_poses[idx, 0] + win_rad_x);
			for (int i=Mathf.Max(0, sphere_poses[idx, 0] - win_rad_x);
				 i < ilim;
				 i++) {
				var jlim = Mathf.Min(tdath, sphere_poses[idx, 1] + win_rad_y);
				for (int j=Mathf.Max(0, sphere_poses[idx, 1] - win_rad_y);
					 j < jlim;
					 j++) {
					var r0 = i-sphere_poses[idx, 0];
					var r1 = j-sphere_poses[idx, 1];
					if (r0*r0 + r1*r1 < win_rad_x * win_rad_y) {
						vmap[i,j] = Mathf.Clamp(vmap[i,j] + base_elast*(sumo_weight - equil_height),
												-max_vel,
												max_vel);
					}
				}
			}
		}


		for (int i=0; i<tdatw; i++) {
			for (int j=0; j<tdath; j++) {
				hmap[i,j] += vmap[i,j];
			}
		}
		tdat.SetHeights(0, 0, hmap);
		p2();
		/*for (int idx=0; idx<sumos.Length; idx++) {
			var pos = sumos[idx].transform.position;
			pos.y = (sumos[idx].radius*sumos[idx].transform.localScale.y +
					 (sumo_weight+.2f) * tdat.heightmapScale.y +
					 terrain.transform.position.y);

			sumos[idx].transform.position = pos;
		}*/


	}

	void p2() {
		int tdatw = tdat.heightmapWidth;
		int tdath = tdat.heightmapHeight;

		for (int idx=0; idx<sumos.Length; idx++) {
			var pos = sumos[idx].transform.position;
			var localpos = terrain.transform.InverseTransformPoint(pos);
			var height = tdat.GetInterpolatedHeight(localpos.x / tdat.size.x,
													localpos.z / tdat.size.z);
			pos.y = (//sumos[idx].radius*sumos[idx].transform.localScale.y +
					 height + //* tdat.heightmapScale.y +
					 terrain.transform.position.y);
			sumos[idx].transform.position = pos;
		}
	}
}
