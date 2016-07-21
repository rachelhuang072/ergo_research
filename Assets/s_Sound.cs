using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class s_Sound : MonoBehaviour
{
	public AudioClip hit;
	public AudioClip nothit;
	public AudioClip fivetoone;
	public AudioClip letsgo;
	public AudioClip stop;

	private AudioSource source;

	// Use this for initialization
	void Start ()
	{
		source = GetComponent<AudioSource>();
	}

	void Awake()
	{
		source = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update ()
	{
	
	}

	public void PlayHit()
	{
		source.PlayOneShot(hit, 1);
	}

	public void PlayNotHit()
	{
		source.PlayOneShot(nothit, 1.5f);
	}

	public void Play5to1()
	{
		source.PlayOneShot(fivetoone, 1);
	}

	public void PlayLetsGo()
	{
		source.PlayOneShot(letsgo, 1);
	}

	public void PlayStop()
	{
		source.PlayOneShot(stop, 1);
	}
}
