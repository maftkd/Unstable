using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallCube : MonoBehaviour
{
	int _state;
	Vector3 _startPos;
	Platformer _p;
	public float _colX;
	public float _colYMin;
	public float _colYMax;
	float _fallSpeed;
	public float _gravity;
	public bool _return;
	[HideInInspector]
	public bool _fallen;
	AudioSource _audio;
	Material _mat;
	public Color _normalColor;
	public Color _fallColor;
	public Color _returnColor;

    // Start is called before the first frame update
    void Start()
    {
		_p = FindObjectOfType<Platformer>();
		_startPos=transform.position;
		_audio = transform.GetChild(0).GetComponent<AudioSource>();
		_mat=GetComponent<MeshRenderer>().material;
		_mat.SetColor("_Color",_normalColor);
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 pos = _p.transform.position;
		switch(_state){
			case 0:
				if(pos.x>=transform.position.x-_colX && pos.x<=transform.position.x+_colX)
				{
					if(pos.y>=transform.position.y+_colYMin&&pos.y<=transform.position.y+_colYMax){
						_state=1;
						if(_return)
							_mat.SetColor("_Color",_returnColor);
						else
							_mat.SetColor("_Color",_fallColor);
					}
				}
				break;
			case 1:
				if(pos.x<transform.position.x-_colX || pos.x>transform.position.x+_colX)
				{
					Fall();
				}
				else if(pos.y<transform.position.y+_colYMin || pos.y>transform.position.y+_colYMax){
					Fall();
				}
				break;
			case 2:
				if(transform.position.y>-30f){
					_fallSpeed+=_gravity*Time.deltaTime;
					transform.position+=Vector3.down*Time.deltaTime*_fallSpeed;
				}
				else if(_return)
				{
					_state=3;
					_fallSpeed=0;
				}
				break;
			case 3:
				if(transform.position.y<_startPos.y){
					_fallSpeed-=_gravity*Time.deltaTime;
					transform.position+=Vector3.down*Time.deltaTime*_fallSpeed;
				}
				else{
					Reset();
				}
				break;
			default:
				break;
		}
    }

	public void Reset(){
		transform.position=_startPos;
		_fallSpeed=0;
		_state=0;
		_fallen=false;
		_mat.SetColor("_Color",_normalColor);
	}

	public void Fall(){
		_fallen=true;
		_state=2;
		_p.CheckAllFall();
		_audio.Play();

	}

	void OnDrawGizmos(){
		float yHalf = (_colYMin+_colYMax)*0.5f;
		Vector3 p = new Vector3(transform.position.x,transform.position.y+yHalf,0);
		Vector3 e = new Vector3(_colX*2,_colYMax-_colYMin,0);
		if(_state==0)
			Gizmos.color=Color.blue;
		else if(_state==1)
			Gizmos.color=Color.red;
		else
			Gizmos.color=Color.magenta;
		Gizmos.DrawWireCube(p,e);
	}
}
