using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class aplay_mb : MonoBehaviour {

	public Dictionary<string, AudioClip> clip_dict;
	public AudioClip[] clip_arr;
	public AudioSource[] as_arr;
	public int as_idx;

	public AudioSource get_src() {
		var ret = as_arr[as_idx];
		while (true) {
			if (!ret.isPlaying) {
				break;
			}
			
			as_idx++;
			as_idx = as_idx % as_arr.Length;
			ret = as_arr[as_idx];		
		}
		ret.volume = 1.0f;
		ret.pitch = 1.0f;
		return ret;
	}

	void Start () {
		int i;
		for (i=0; i<as_arr.Length; i++) {
			as_arr[i] = gameObject.AddComponent<AudioSource>();
		}
			
		clip_dict = new Dictionary<string, AudioClip>();
		for (i=0; i<clip_arr.Length; i++) {
			var n = clip_arr[i].name;
			clip_dict[n] = clip_arr[i];
		}
	}
}
