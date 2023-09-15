using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Patterns;

public enum PlayerFSMStateType
{
    MOVEMENT = 0,
    CROUCH,
    ATTACK,
    RELOAD,
    TAKE_DAMAGE,
    DEAD,
}

public class PlayerFSMState : State
{
    public PlayerFSMStateType ID { get { return _id; } }

    protected Player _player = null;
    protected PlayerFSMStateType _id;
    public PlayerFSMState(FSM fsm, Player player) : base(fsm)
    {
        _player = player;
    }
    public PlayerFSMState(Player player) : base()
    {
        _player = player;
        m_fsm = _player.playerFSM;
    }

    public override void Enter()
    {
        base.Enter();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Update()
    {
        base.Update();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}

public class PlayerFSMState_MOVEMENT : PlayerFSMState
{
    public PlayerFSMState_MOVEMENT(Player player) : base(player)
    {
        _id = PlayerFSMStateType.MOVEMENT;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        _player.playerMovement.Move();

        //_player.playerEffects.Aim();
        if (Input.GetButton("Fire1"))
        {
            PlayerFSMState_ATTACK attackState = (PlayerFSMState_ATTACK)_player.playerFSM.GetState(PlayerFSMStateType.ATTACK);
            attackState.AttackId = 0;
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.ATTACK);
        }
        if (Input.GetButton("Fire2"))
        {
            PlayerFSMState_ATTACK attackState = (PlayerFSMState_ATTACK)_player.playerFSM.GetState(PlayerFSMStateType.ATTACK);
            attackState.AttackId = 1;
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.ATTACK);
        }
        if (Input.GetButton("Fire3"))
        {
            PlayerFSMState_ATTACK attackState = (PlayerFSMState_ATTACK)_player.playerFSM.GetState(PlayerFSMStateType.ATTACK);
            attackState.AttackId = 2;
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.ATTACK);
        }
        if (Input.GetButton("Crouch"))
        {
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.CROUCH);
        }

    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}

public class PlayerFSMState_CROUCH : PlayerFSMState
{
    public PlayerFSMState_CROUCH(Player player) : base(player)
    {
        _id = PlayerFSMStateType.CROUCH;
    }

    public override void Enter()
    {
        _player.playerAnimator.SetBool("Crouch", true);
    }
    public override void Exit()
    {
        _player.playerAnimator.SetBool("Crouch", false);
    }
    public override void Update()
    {
        if (Input.GetButton("Crouch"))
        {
        }
        else
        {
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.MOVEMENT);
        }
    }
    public override void FixedUpdate() { }
}

public class PlayerFSMState_ATTACK : PlayerFSMState
{
    private float m_elaspedTime;

    public GameObject AttackGameObject { get; set; } = null;

    public PlayerFSMState_ATTACK(Player player) : base(player)
    {
        _id = PlayerFSMStateType.ATTACK;
    }

    private int _attackID = 0;
    private string _attackName;

    public int AttackId
    {
        get
        {
            return _attackID;
        }
        set
        {
            _attackID = value;
            _attackName = "Attack" + (_attackID + 1).ToString();
        }
    }

    public override void Enter()
    {
        //Debug.Log("PlayerFSMState_ATTACK");
        _player.playerAnimator.SetBool(_attackName, true);
        m_elaspedTime = 0.0f;
    }
    public override void Exit()
    {
        //Debug.Log("PlayerFSMState_ATTACK - Exit");
        _player.playerAnimator.SetBool(_attackName, false);
    }
    public override void Update()
    {
        //Debug.Log("Ammo count: " + _player.totalAmunitionCount + ", In Magazine: " + _player.bulletsInMagazine);
        if (_player.bulletsInMagazine == 0 && _player.totalAmunitionCount > 0)
        {
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.RELOAD);
            return;
        }

        if (_player.totalAmunitionCount == 0)
        {
            //Debug.Log("No ammo");
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.MOVEMENT);
            //_player.playerEffects.NoAmmo();
            return;
        }

        //_player.playerEffects.Aim();

        if (Input.GetButton("Fire1"))
        {
            _player.playerAnimator.SetBool(_attackName, true);
            if (m_elaspedTime == 0.0f)
            {
                Fire();
            }

            m_elaspedTime += Time.deltaTime;
            if (m_elaspedTime > 1.0f / _player.roundsPerSecond)
            {
                m_elaspedTime = 0.0f;
            }
        }
        else
        {
            m_elaspedTime = 0.0f;
            _player.playerAnimator.SetBool(_attackName, false);
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.MOVEMENT);
        }
    }

    void Fire()
    {
        float secs = 1.0f / _player.roundsPerSecond;
        //_player.playerEffects.DelayedFire(secs);
        _player.bulletsInMagazine -= 1; ;
    }
}

public class PlayerFSMState_RELOAD : PlayerFSMState
{
    public float ReloadTime = 3.0f;
    float dt = 0.0f;
    public int previousState;
    public PlayerFSMState_RELOAD(Player player) : base(player)
    {
        _id = PlayerFSMStateType.RELOAD;
    }

    public override void Enter()
    {
        //Debug.Log("PlayerFSMState_RELOAD");
        _player.playerAnimator.SetTrigger("Reload");
        dt = 0.0f;
    }
    public override void Exit()
    {
        if (_player.totalAmunitionCount > _player.maxAmunitionBeforeReload)
        {
            _player.bulletsInMagazine += _player.maxAmunitionBeforeReload;
            _player.totalAmunitionCount -= _player.bulletsInMagazine;
        }
        else if (_player.totalAmunitionCount > 0 && _player.totalAmunitionCount < _player.maxAmunitionBeforeReload)
        {
            _player.bulletsInMagazine += _player.totalAmunitionCount;
            _player.totalAmunitionCount = 0;
        }
        //Debug.Log("PlayerFSMState_RELOAD - Exit");
    }
    public override void Update()
    {
        dt += Time.deltaTime;
        //_player.playerAnimator.SetTrigger("Reload");
        //_player.playerEffects.Reload();
        if (dt >= ReloadTime)
        {
            //Debug.Log("Reload complete in " + dt);
            _player.playerFSM.SetCurrentState(PlayerFSMStateType.MOVEMENT);
        }
    }
    public override void FixedUpdate() { }
}

public class PlayerFSMState_TAKE_DAMAGE : PlayerFSMState
{
    public PlayerFSMState_TAKE_DAMAGE(Player player) : base(player)
    {
        _id = PlayerFSMStateType.TAKE_DAMAGE;
    }

    public override void Enter() { }
    public override void Exit() { }
    public override void Update() { }
    public override void FixedUpdate() { }
}

public class PlayerFSMState_DEAD : PlayerFSMState
{
    public PlayerFSMState_DEAD(Player player) : base(player)
    {
        _id = PlayerFSMStateType.DEAD;
    }

    public override void Enter()
    {
        Debug.Log("Player dead");
        _player.playerAnimator.SetTrigger("Die");
    }
    public override void Exit() { }
    public override void Update() { }
    public override void FixedUpdate() { }
}

public class PlayerFSM : FSM
{
    public PlayerFSM() : base()
    {
    }

    public void Add(PlayerFSMState state)
    {
        m_states.Add((int)state.ID, state);
    }

    public PlayerFSMState GetState(PlayerFSMStateType key)
    {
        return (PlayerFSMState)GetState((int)key);
    }

    public void SetCurrentState(PlayerFSMStateType stateKey)
    {
        State state = m_states[(int)stateKey];
        if (state != null)
        {
            SetCurrentState(state);
        }
    }
}

public class Player : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public Animator playerAnimator;
    public PlayerFSM playerFSM = null;

    public int maxAmunitionBeforeReload = 40;
    public int totalAmunitionCount = 100;
    [HideInInspector]
    public int bulletsInMagazine = 40;
    public float roundsPerSecond = 10;

    // Start is called before the first frame update
    void Start()
    {

        playerFSM = new PlayerFSM();

        // create the FSM.
        playerFSM.Add(new PlayerFSMState_MOVEMENT(this));
        playerFSM.Add(new PlayerFSMState_ATTACK(this));
        playerFSM.Add(new PlayerFSMState_RELOAD(this));
        playerFSM.Add(new PlayerFSMState_TAKE_DAMAGE(this));
        playerFSM.Add(new PlayerFSMState_DEAD(this));
        playerFSM.Add(new PlayerFSMState_CROUCH(this));

        playerFSM.SetCurrentState(PlayerFSMStateType.MOVEMENT);
    }

    // Update is called once per frame
    void Update()
    {
        playerFSM.Update();
    }
}
