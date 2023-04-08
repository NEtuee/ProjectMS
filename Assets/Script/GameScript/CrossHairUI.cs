using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHairUI : MonoBehaviour
{
    public static CrossHairUI _instance;

    public Sprite _guidLineSprite;
    public GameObject _gagueObject;

    public Transform _targetPosition;

    public float _lineObjectRatio = 0.7f;
    public float _gagueStack;


    private List<GameObject> _guidLineObjects = new List<GameObject>();

    public void Awake()
    {
        _instance = this;
        createGuidLineObjects(2);
    }

    public void createGuidLineObjects(int count)
    {
        for(int index = 0; index < count; ++index)
        {
            GameObject guidLineObject = new GameObject("CrossHairGuid");
            guidLineObject.AddComponent<SpriteRenderer>().sprite = _guidLineSprite;

            _guidLineObjects.Add(guidLineObject);
        }
    }

    public void Update()
    {
        if(_targetPosition == null)
            return;

        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMousePosition.z = 0f;

        Vector3 toMouseVector = worldMousePosition - _targetPosition.position;
        Quaternion rotation = Quaternion.Euler(0f,0f,MathEx.directionToAngle(toMouseVector.normalized));

        for(int index = 0; index < _guidLineObjects.Count; ++index)
        {
            _guidLineObjects[index].transform.position = _targetPosition.position + (toMouseVector) * (_lineObjectRatio / (float)(index + 1));
            _guidLineObjects[index].transform.rotation = rotation;
        }

        transform.position = worldMousePosition;
        transform.rotation = rotation;
        
    }

    public void setActive(bool value)
    {
        gameObject.SetActive(value);

        for(int index = 0; index < _guidLineObjects.Count; ++index)
        {
            _guidLineObjects[index].SetActive(value);
        }

        Update();
    }

    public void setTarget(Transform target)
    {
        _targetPosition = target;

        Update();
    }
}
