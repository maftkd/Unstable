using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverBlock : MonoBehaviour
{
	public float _left;
	public float _right;
	bool _rightward;
	public float _flowSpeed;
	Vector3 _startPos;
    // Start is called before the first frame update
    void Start()
    {
		_rightward=false;
		_startPos=transform.position;
    }

    // Update is called once per frame
    void Update()
    {
		if(_rightward){
			transform.position+=Vector3.right*Time.deltaTime*_flowSpeed;
			if(transform.position.x>_right)
				_rightward=false;
		}
		else{
			transform.position+=Vector3.left*Time.deltaTime*_flowSpeed;
			if(transform.position.x<_left)
				_rightward=true;
		}
    }

	public void Reset(){
		transform.position=_startPos;
		_rightward=false;
	}
}
