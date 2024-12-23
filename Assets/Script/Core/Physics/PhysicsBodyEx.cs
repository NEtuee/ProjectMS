using System.Threading;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct PhysicsBodyDescription : SerializableStructure
{
    public Vector3Ex _velocity;
    public FloatEx _torque;

    public float _friction;
    public float _angularFriction;
    public float _gravityRatio;

    public bool _useGravity;

    public PhysicsBodyDescription(object nullObj)
    {
        _velocity = new Vector3Ex();
        _torque = new FloatEx();
        _friction = 0f;
        _angularFriction = 0f;
        _useGravity = false;
        _gravityRatio = 1f;
    }

    public void clearPhysicsBody()
    {
        if(_velocity == null)
            _velocity = new Vector3Ex();
        else
            _velocity.setValue(UnityEngine.Vector3.zero);

        if(_torque == null)
            _torque = new FloatEx();
        else
            _torque.setValue(0f);

        _friction = 0f;
        _angularFriction = 0f;
        _useGravity = false;
    }
#if UNITY_EDITOR
    public void serialize(ref BinaryWriter binaryWriter)
    {
        _velocity.serialize(ref binaryWriter);
        _torque.serialize(ref binaryWriter);
        binaryWriter.Write(_friction);
        binaryWriter.Write(_angularFriction);
        binaryWriter.Write(_useGravity);
    }
#endif
    public void deserialize(ref BinaryReader binaryReader)
    {
        _velocity.deserialize(ref binaryReader);
        _torque.deserialize(ref binaryReader);
        _friction = binaryReader.ReadSingle();
        _angularFriction = binaryReader.ReadSingle();
        _useGravity = binaryReader.ReadBoolean();
    }
}

public class PhysicsBodyEx
{
    public static Vector3 _gravity = new Vector3(0f,-12f,0f);

    private Vector3 _currentVelocity;
    private float _currentTorque;

    private float _angularFriction;
    private float _friction;
    private bool _useGravity;
    private float _gravityRatio = 1f;

    public void initialize(bool useGravity, float friction, float angularFriction)
    {
        _currentVelocity = Vector3.zero;
        _currentTorque = 0f;
        _useGravity = useGravity;
        _friction = friction;
        _angularFriction = angularFriction;
    }

    public void initialize(PhysicsBodyDescription desc)
    {
        _currentVelocity = desc._velocity.getValue();
        _currentTorque = desc._torque;
        _useGravity = desc._useGravity;
        _friction = desc._friction;
        _angularFriction = desc._angularFriction;
        _gravityRatio = desc._gravityRatio;
    }

    public void progress(float deltaTime)
    {
        _currentVelocity = MathEx.convergence0(_currentVelocity, _friction * deltaTime);
        _currentTorque = MathEx.convergence0(_currentTorque, _angularFriction * deltaTime);
        addForce(_gravity * _gravityRatio * deltaTime);
    }

    public void addTorque(float torque)
    {
        _currentTorque += torque;
    }

    public void setTorque(float torque)
    {
        _currentTorque = torque;
    }

    public void addForce(Vector3 force)
    {
        _currentVelocity += force;
    }

    public void setForce(Vector3 force)
    {
        _currentVelocity = force;
    }

    public Vector3 getCurrentVelocity()
    {
        return _currentVelocity;
    }

    public Quaternion getCurrentTorque(float deltaTime)
    {
        return Quaternion.Euler(0f,0f,(_currentTorque * Mathf.Rad2Deg) * deltaTime);
    }

    public float getCurrentTorqueValue()
    {
        return (_currentTorque * Mathf.Rad2Deg);
    }
}
