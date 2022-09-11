using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : MonoBehaviour
{
    private AnimationTimeProcessor _timeProcessor = new AnimationTimeProcessor();
    private MovementGraph _movementGraph = null;

    private Vector3 _direction = Vector3.right;
    private Sprite[] _sprites;
    private SpriteRenderer _spriteRenderer;

    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _movementGraph = ResourceContainerEx.Instance().GetScriptableObject("Sprites/PLAYER_OLD1") as MovementGraph;
        _sprites = ResourceContainerEx.Instance().GetSpriteAll("Resources/Sprites/",false);

        _timeProcessor.initialize();

       // _timeProcessor.setFrame(_sprites.Length,12f);
        _timeProcessor.setLoop(true);
    }

    public void Update()
    {
        _timeProcessor.updateTime(Time.deltaTime);

        _spriteRenderer.sprite = _sprites[_timeProcessor.getCurrentIndex()];
        Vector3 moveValue = _movementGraph.getMoveValuePerFrameFromTime(_timeProcessor.getPrevNormalizedTime(),_timeProcessor.getCurrentNormalizedTime(),_timeProcessor.getTotalLoopCount());

        _direction = MathEx.deleteZ(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - transform.position;
        _direction.Normalize();

        if(Input.GetKey(KeyCode.A))
            transform.localPosition += new Vector3(moveValue.x, moveValue.y, 0f);
    }
}
