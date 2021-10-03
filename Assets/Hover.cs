using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
	public float _amplitude;
	public float _frequency;
	Vector3 _startPos;
    // Start is called before the first frame update
    void Start()
    {
		_startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
		if(transform.parent==null)
			transform.position = _startPos+Vector3.up*Mathf.Sin(Time.time*Mathf.PI*2*_frequency)*_amplitude;
    }
}
