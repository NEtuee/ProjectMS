using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHp : IUIElement
{
    private EnemyHpBinder _binder;

    private Queue<EnemyHpObject> _pool = new Queue<EnemyHpObject>();
    private List<EnemyHpObject> _allObjects = new List<EnemyHpObject>();

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
    }

    private void InitPool()
    {
        for (int i = 0; i < 10; i++)
        {
            _pool.Enqueue(NewObject());
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
    
    private void AddCharacter(CharacterEntityBase character)
    {
        if (character.getStatusInfo().getStatusInfoData()._useHPEffect == false ||
            (int)character.getStatusInfo().getMaxStatus("HP") <= 1)
        {
            return;
        }

        var offset = character.getHeadUpOffset();
        var enemyHpObj = GetObject();
        
        enemyHpObj.Active(character, new Vector3(0.0f, offset, 0.0f), (returnEnemyHp) => _pool.Enqueue(returnEnemyHp));
    }
    
    private void DisableCharacter(CharacterEntityBase character)
    {
        
    }
}
