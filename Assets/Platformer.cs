using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Platformer : MonoBehaviour
{
	Vector3 _movement;
	public float _walkSpeed;
	public float _jumpSpeed;
	
	//collision
	public LayerMask _collisionMask;
	BoxCollider[] _boxes;//only used for gizmos atm
	Collider[] _colliders;
	int _maxColliders=10;
	public float _wallHitRadius;
	float _playerHeight;
	public LayerMask _walkable;
	Vector3 _groundPoint;
	Vector3 _prevPos;
	public float _stepHeight;
	float _fallVel;
	public float _grav;
	public float _yLerp;
	public bool _grounded;
	public float _gameOverHeight;

	//ui
	CanvasGroup _blink;
	int _blinkState;
	Text _poem;
	Button _play;

	//game
	int _levelIndex;
	Transform _key;
	bool _hasKey;
	Vector3 _startPos;
	Vector3 _keyStartPos;
	Transform _lock;
	bool _keyHidden;
	bool _keyHiddenOnStart;
	TriggerZone [] _triggers;
	FallCube [] _fallCubes;
	public UnityEvent _allFallen;

    // Start is called before the first frame update
    void Start()
    {
		_startPos=transform.position;
		_boxes = FindObjectsOfType<BoxCollider>();
		_colliders = new Collider[_maxColliders];
		_playerHeight=_wallHitRadius;
		SnapStartingHeight();
		_blink = GameObject.Find("Blink").GetComponent<CanvasGroup>();
		_blink.alpha=1f;
		_poem = GameObject.Find("Poem").GetComponent<Text>();
		string poemPath = Application.streamingAssetsPath+"/poem.txt";
		string[] lines = File.ReadAllLines(poemPath);
		_levelIndex = SceneManager.GetActiveScene().buildIndex;
		_poem.text=lines[_levelIndex];
		_play=GameObject.Find("PlayButton").GetComponent<Button>();
		_play.onClick.AddListener(delegate {StartLevel();});
		_play.Select();
		//_blinkState=1;
		_key = GameObject.Find("Key").transform;
		_lock = GameObject.Find("Lock").transform;
		_keyStartPos=_key.position;
		if(!_key.GetComponent<BoxCollider>().enabled)
		{
			_key.GetComponent<BoxCollider>().enabled=true;
			_keyHidden=true;
			_keyHiddenOnStart=true;
			_key.gameObject.SetActive(false);
		}
		_triggers = FindObjectsOfType<TriggerZone>();
		_fallCubes = FindObjectsOfType<FallCube>();
    }

	void SnapStartingHeight(){
		RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, _walkable))
        {
			_groundPoint=hit.point;
			transform.position=_groundPoint+Vector3.up*_playerHeight;
        }
		_grounded=true;
	}

    // Update is called once per frame
    void Update()
    {
		//try movement
		if(_blinkState==1){
			_movement = Vector3.zero;
			_movement+=Vector3.right*Input.GetAxis("Horizontal");
			transform.position+=_movement*Time.deltaTime*_walkSpeed;

			//check jump
			if(_grounded && Input.GetButtonDown("Jump")){
				_grounded=false;
				_fallVel=_jumpSpeed;
			}
		}
		
		//check for wall
		int numColliders = Physics.OverlapSphereNonAlloc(transform.position, 
				_wallHitRadius, _colliders,_collisionMask,QueryTriggerInteraction.Ignore);
		Vector3 closePoint=Vector3.zero;

		//check for key n such
		for(int i=0; i<numColliders; i++){
			if(!_hasKey && (_colliders[i].name=="Key" || _colliders[i].name=="Shaft")){
				_hasKey=true;
				_key.SetParent(transform);
				_key.localPosition=Vector3.back+Vector3.right*0.4f;
				_key.GetComponent<BoxCollider>().enabled=false;
				_key.GetChild(0).GetComponent<BoxCollider>().enabled=false;
			}
			else if(_hasKey && _colliders[i].name=="Lock"){
				_lock.gameObject.SetActive(false);
				_key.gameObject.SetActive(false);
			}
			else if(_colliders[i].name=="WinZone"){
				if(_levelIndex+1<SceneManager.sceneCountInBuildSettings)
					SceneManager.LoadScene(_levelIndex+1);
			}
		}

		int attempts=0;
		while(numColliders>0&&attempts<5){
			for(int i=0; i<numColliders; i++){
				closePoint=_colliders[i].ClosestPoint(transform.position);

				//check if bonk head then fall
				Vector3 v = closePoint-_colliders[i].transform.position;
				if(Vector3.Dot(v.normalized,Vector3.down)>0.8f){
					_fallVel=-0.1f;
				}
				//check if transform is still within collider
				if((closePoint-_colliders[i].transform.position).sqrMagnitude <
						(transform.position-_colliders[i].transform.position).sqrMagnitude)
				{
					//offset player from wall
					transform.position=closePoint+
						(transform.position-closePoint).normalized*_wallHitRadius;
				}
			}

			numColliders= Physics.OverlapSphereNonAlloc(transform.position, 
					_wallHitRadius, _colliders);
			attempts++;
		}
        
		//check for ground
		RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100f, _walkable))
        {
			//stepping up
			if(hit.distance<_playerHeight && hit.distance>_playerHeight-_stepHeight){
				_groundPoint=hit.point;
			}
			//stepping down
			else if(hit.distance>_playerHeight){
				//step down
				if(hit.distance<_playerHeight+_stepHeight){
					_groundPoint=hit.point;
				}
				//drop down
				else{
					_groundPoint=hit.point;
					if(_grounded)
					{
						_grounded=false;
						_fallVel=-0.1f;
					}
				}
			}
			//flat walking
			else{
				_groundPoint=hit.point;
			}
        }

		//animate y coord
		Vector3 p = transform.position;
		//falling
		if(!_grounded){
			_fallVel-=_grav*Time.deltaTime;
			p.y+=_fallVel*Time.deltaTime;
			//done falling
			if(p.y<_groundPoint.y+_playerHeight)//-_stepHeight*_fallFactor)
				_grounded=true;
		}
		//normal walking or step up/down
		else
			p.y=Mathf.Lerp(p.y,_groundPoint.y+_playerHeight,_yLerp*Time.deltaTime);
		transform.position=p;
		
		//check game over condition
		if(p.y<_gameOverHeight){
			ResetLevel();
		}

		_prevPos=transform.position;

		switch(_blinkState){
			case 0:
			default:
				break;
			case 1:
				if(_blink.alpha>0)
					_blink.alpha-=Time.deltaTime;
				break;
			case 2:
				if(_blink.alpha<1)
					_blink.alpha+=Time.deltaTime;
				break;
		}
    }

	void StartLevel(){
		_blinkState=1;
	}

	public void ResetLevel(){
		//reset
		_fallVel=0;
		foreach(FallCube f in _fallCubes){
			f.Reset();
		}
		RiverBlock [] rbs = FindObjectsOfType<RiverBlock>();
		foreach(RiverBlock rb in rbs)
			rb.Reset();
		foreach(TriggerZone tz in _triggers)
			tz.gameObject.SetActive(true);
		transform.position=_startPos;
		ResetKey();
		_lock.gameObject.SetActive(true);
	}

	public void ResetKey(){
		_key.gameObject.SetActive(!_keyHiddenOnStart);
		_key.GetComponent<BoxCollider>().enabled=true;
		_key.GetChild(0).GetComponent<BoxCollider>().enabled=true;
		_key.SetParent(null);
		_key.position=_keyStartPos;
		_hasKey=false;
		_keyHidden=_keyHiddenOnStart;
	}

	public void RevealKey(){
		_key.gameObject.SetActive(true);
		_keyHidden=false;
	}

	public void CheckAllFall(){
		bool allFallen=true;
		foreach(FallCube fc in _fallCubes){
			if(!fc._fallen)
				allFallen=false;
		}
		if(allFallen)
			_allFallen.Invoke();
	}

	void OnDrawGizmos(){
		Gizmos.color=Color.green;
		Gizmos.DrawWireSphere(transform.position,0.5f);
		if(_boxes==null || _boxes.Length==0)
			_boxes = FindObjectsOfType<BoxCollider>();

		foreach(BoxCollider b in _boxes)
		{
			if(b!=null)
				Gizmos.DrawWireCube(b.transform.position,Vector3.one);
		}
		Gizmos.color=Color.yellow;
		Gizmos.DrawWireSphere(_groundPoint,0.25f);
	}
}
