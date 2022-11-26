using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : Singleton<CameraControll> {

	public Camera mainCam{get{return _main;}}
	public Vector3 position{get{return _position;}}

	private ObjectBase _followTarget = null;
	private float _followSpeed = 4f;

	private Transform _tp;
	private Camera _main;


	private Vector3 _position;
	private Vector3 _velocity;


	private bool _targetSet = false;

	private float _zDist = -10f;
	private float _shakeTimer = 0f;
	private float _shakeTurm = .1f;
	private Vector2 _shakeFactor = new Vector2(1f,1f);

	private float _mainCamSize = 0f;
	// private float _zoomScale;
	// private float _glitchTimer = 0f;
	// private float _timeSaver = 0f;
	private float _delayTimer = 0f;
	// private float _lemTimer = 0f;

	private float camWidth;
	private float camHeight;

	public void assign()
	{
		_main = Camera.main;
		_tp = _main.transform;
		_position = _tp.position;

		camHeight = _main.orthographicSize;
		camWidth = camHeight * ((float)Screen.width / (float)Screen.height);

		_mainCamSize = _main.orthographicSize;
	}

	public Vector4 GetCamBounds() //x min x max y min y max
	{
		return new Vector4(_position.x - camWidth, _position.x + camWidth,
							_position.y - camHeight, _position.y + camHeight);
	}

	public bool IsInCamera(Vector3 pos)
	{
		var bound = GetCamBounds();
		return (pos.x >= bound.x && pos.x <= bound.y && pos.y >= bound.z && pos.y <= bound.w);
	}

	public void SetTarget(ObjectBase target)
	{
		_followTarget = target;

		_targetSet = true;
	}

	public void SetSpeed(float value)
	{
		_followSpeed = value;
	}


	public void initialize()
	{

	}

	public Vector3 ScreenToWorldMouse()
	{
		Vector3 pos = _main.ScreenToWorldPoint(Input.mousePosition);
		pos.z = 0f;
		return pos;
	}

	public void progress(float deltaTime)
	{

		if(_followTarget != null)
		{
			Follow(deltaTime);

			if(_delayTimer != 0f)
			{
				_delayTimer -= Time.deltaTime;
				if(_delayTimer <= 0f)
				{
					//Timer.SetViTimeScaleTimer(1,1f,0.3f,true);
					_delayTimer = 0f;
				}
			}
		}

		if(_shakeTimer > 0f)
		{
			_shakeTimer -= deltaTime;
			_shakeTurm -= deltaTime;

			if(_shakeTurm <= 0)
			{
				float x = _shakeFactor.x;
				float y = _shakeFactor.y;
			
				_position += new Vector3(Random.Range(-x, x), Random.Range(-y, y));

				_shakeTurm = 0.01f;
			}
		}

		if(MathEx.equals(_main.orthographicSize,_mainCamSize,float.Epsilon) == true)
			_main.orthographicSize = _mainCamSize;
		else	
			_main.orthographicSize = Mathf.Lerp(_main.orthographicSize,_mainCamSize,4f * deltaTime);
	}

	public void Shake(float time, Vector2 factor)
	{
		_shakeTimer = time;
		_shakeFactor = factor;
		_shakeTurm = 0f;
	}

	public void Zoom(float scale)
	{
		if(_main.orthographicSize > scale)
			_main.orthographicSize = scale;
	}

	public void FollowDelay(float delay)
	{
		_delayTimer = delay;
	}

	public void release()
	{

	}

	Vector3 targetPos;

	float cameraAngle = 0f;

	public void Follow(float deltaTime)
	{
		Vector3 pos = (Vector2)_position;
		Vector3 followPos = _followTarget.transform.position;

		Vector3 direction = _followTarget.getDirection();
		float angle = Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg;

		float angleDist = MathEx.abs(cameraAngle - angle);
		cameraAngle = Mathf.LerpAngle(cameraAngle, angle, 480f * deltaTime / angleDist);

		targetPos = followPos;// + /*_followTarget.direction.normalized*/ MathEx.angleToDirection(cameraAngle * Mathf.Deg2Rad) * 0.65f;

		float dist = Vector2.Distance(targetPos,pos);
		_velocity = (targetPos - pos).normalized * dist * _followSpeed;

		_position += _velocity * deltaTime;
		_position.z = _zDist;
	}

	public void SyncPosition()
	{
		
		_tp.SetPositionAndRotation(_position, _tp.rotation);
	}
}
