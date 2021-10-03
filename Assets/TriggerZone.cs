using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
	Platformer _p;
	public UnityEvent _onTrigger;
	public float _mag;
    // Start is called before the first frame update
    void Start()
    {
		_p = FindObjectOfType<Platformer>();
    }

    // Update is called once per frame
    void Update()
    {
		if((transform.position-_p.transform.position).sqrMagnitude<=_mag*_mag)
			_onTrigger.Invoke();
    }

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position, _mag);
	}
}
