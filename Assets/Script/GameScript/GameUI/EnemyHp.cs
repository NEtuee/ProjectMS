using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHp : IUIElement
{
    private EnemyHpBinder _binder;

    private Queue<EnemyHpObject> _pool = new Queue<EnemyHpObject>();
    private Queue<EnemyHpObjectMax3> _max3Pool = new Queue<EnemyHpObjectMax3>();
    private List<EnemyHpObject> _allObjects = new List<EnemyHpObject>();
    private List<EnemyHpObjectMax3> _allMax3Objects = new List<EnemyHpObjectMax3>();

    private SceneCharacterManager.OnCharacterEnabled _onCharacterEnabled;
    private SceneCharacterManager.OnCharacterDisabled _onCharacterDisabled;

    public bool CheckValidBinderLink(out string reason)
    {
        if (_binder == null)
        {
            reason = "적 hp 바인더가 없음";
            return false;
        }
        
        reason = string.Empty;
        return true;
    }

    public void SetBinder<T>(T binder) where T : UIObjectBinder
    {
        _binder = binder as EnemyHpBinder;
    }

    public void Initialize()
    {
        _onCharacterEnabled = AddCharacter;
        _onCharacterDisabled = DisableCharacter;

        InitPool();
    }

    public void InitValue()
    {
        foreach (var obj in _allObjects)
        {
            obj.Stop();
        }
        
        foreach (var obj in _allMax3Objects)
        {
            obj.Stop();
        }
        
        var characterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;

        characterManager.deleteCharacterEnableDelegate(_onCharacterEnabled);
        characterManager.deleteCharacterDisableDelegate(_onCharacterDisabled);
        
        characterManager.addCharacterEnableDelegate(_onCharacterEnabled);
        characterManager.addCharacterDisableDelegate(_onCharacterDisabled);
    }

    public void UpdateByManager(float deltaTime)
    {
        foreach (var enemyHp in _allObjects)
        {
            enemyHp.UpdateByManager(deltaTime);
        }

        foreach (var enemyHp in _allMax3Objects)
        {
            enemyHp.UpdateByManager(deltaTime);
        }
    }

    private void InitPool()
    {
        for (int i = 0; i < 10; i++)
        {
            _pool.Enqueue(NewObject());
            _max3Pool.Enqueue(NewObjectForMax3());
        }
    }

    private EnemyHpObject GetObject()
    {
        if (_pool.Count <= 0)
        {
            return NewObject();
        }

        return _pool.Dequeue();
    }

    private EnemyHpObject NewObject()
    {
        var newInstance = Object.Instantiate(_binder.Prefab, _binder.transform);
        newInstance.Init();
        newInstance.gameObject.SetActive(false);
        
        _allObjects.Add(newInstance);

        return newInstance;
    }
    
    private EnemyHpObjectMax3 GetObjectForMax3()
    {
        if (_max3Pool.Count <= 0)
        {
            return NewObjectForMax3();
        }

        return _max3Pool.Dequeue();
    }

    private EnemyHpObjectMax3 NewObjectForMax3()
    {
        var newInstance = Object.Instantiate(_binder.PrefabMax3, _binder.transform);
        newInstance.Init();
        newInstance.gameObject.SetActive(false);
        
        _allMax3Objects.Add(newInstance);

        return newInstance;
    }
    
    private void AddCharacter(CharacterEntityBase character)
    {
        var maxHp = (int) character.getStatusInfo().getMaxStatus("HP");
        if (character.getUseHPInterface() == false || maxHp <= 1)
        {
            return;
        }

        var offset = character.getHeadUpOffset();

        var type = maxHp == 2 ? EnemyHpObjectMax3.Type.Max2 : EnemyHpObjectMax3.Type.Max3;
        var enemyHpMax3Obj = GetObjectForMax3();
        enemyHpMax3Obj.Active(character, new Vector3(0.0f, offset, 0.0f), type, (returnEnemyHp) => _max3Pool.Enqueue(returnEnemyHp));
    }
    
    private void DisableCharacter(CharacterEntityBase character)
    {
        
    }
}
