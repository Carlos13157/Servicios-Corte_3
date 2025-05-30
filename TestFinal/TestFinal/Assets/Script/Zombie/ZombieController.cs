using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class ZombieController : NetworkBehaviour, IHealth
{
    public NavMeshAgent navMeshAgent { get; set; }
    private List<Transform> playerTransforms = new();
    private Transform currentPlayerObjective;

    [SerializeField] private AudioClip[] idleAudioClips;
    [SerializeField] private AudioClip[] screamAudioClips;
    [SerializeField] private AudioClip[] chaseAudioClips;
    [SerializeField] private AudioClip[] attackAudioClips;
    [SerializeField] private AudioClip[] deathAudioClips;

    private Dictionary<string, AudioClip[]> audioFiles = new();
    public AudioSource audioSource { get; set; }
    public Animator animator { get; set; }

    public ZombieStateMachine stateMachine { get; set; }
    public float distanceToPlayer { get; set; }

    public bool playerInSight { get; set; } = false;

    public float defaultVelocity { get; set; } = 100f;

    public float jumpVelocity { get; set; } = -450f;

    public float maxHealth { get; set; } = 300f;

    public float chaseDistance { get; set; } = 15.0f;

    public float attackDistance { get; set; } = 3f;

    public float hitDamage { get; set; } = 20f;

    public float attackDelay { get; set; } = 1000f;

    public float attackRate { get; set; } = 850f;

    public float NetworkedHealth { get; set; } = 300;

    public bool IsAlive { get; set; } = true;

    public Vector3? targetLastSeenPosition { get; set; } = null;

    void Awake()
    {
        audioFiles.Add("IdleSounds", idleAudioClips);
        audioFiles.Add("ScreamSounds", screamAudioClips);
        audioFiles.Add("ChaseSounds", chaseAudioClips);
        audioFiles.Add("AttackSounds", attackAudioClips);
        audioFiles.Add("DeathsSounds", deathAudioClips);
    }

    void Start()
    {
        LookUpForPlayers();

        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        stateMachine = GetComponent<ZombieStateMachine>();
        StartCoroutine(WaitForStateMachine());
    }

    private void LookUpForPlayers()
    {
        var players = GameObject.FindGameObjectsWithTag("Player").ToList();
        playerTransforms = players.Where(player => player != null).Select(player => player.transform).ToList();
    }

    IEnumerator WaitForStateMachine()
    {
        yield return new WaitUntil(() => stateMachine != null);
        stateMachine.Initialize();
        StartCoroutine(LookUpForPlayersByTime());
        StartCoroutine(CheckCloserPlayerByTime());
    }

    IEnumerator LookUpForPlayersByTime()
    {
        while (true)
        {
            LookUpForPlayers();
            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator CheckCloserPlayerByTime()
    {
        while (true)
        {
            if (playerTransforms.Count > 0)
                CheckCloserPlayer();

            yield return new WaitForSeconds(1);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (currentPlayerObjective == null) return;
        SetDistanceToPlayer();
        UpdateRaycaster();

    }

    private void CheckCloserPlayer()
    {
        currentPlayerObjective = playerTransforms
            .Where(playerTransform => playerTransform != null)
            .OrderBy(playerTransform => Vector3.Distance(transform.position, playerTransform.position))
            .FirstOrDefault();
    }

    public void SetDistanceToPlayer()
    {
        distanceToPlayer = Vector3.Distance(transform.position, currentPlayerObjective.position);
    }

    public float GetDistanceToPlayer()
    {
        return distanceToPlayer;
    }

    public Dictionary<string, AudioClip[]> GetAudioFiles()
    {
        return audioFiles;
    }

    public Transform GetPlayerTransform()
    {
        return currentPlayerObjective;
    }

    private void UpdateRaycaster()
    {
        var headPosition = new Vector3(transform.position.x, transform.position.y + navMeshAgent.height, transform.position.z);
        Vector3 directionToPlayer = (currentPlayerObjective.position - headPosition).normalized;

        Debug.DrawRay(headPosition, directionToPlayer * distanceToPlayer, Color.red);

        if (Physics.Raycast(headPosition, directionToPlayer, out RaycastHit hit, chaseDistance))
        {
            playerInSight = hit.collider.gameObject == currentPlayerObjective.gameObject && GetDistanceToPlayer() <= chaseDistance;
        }
        else if (playerInSight)
        {
            playerInSight = false;
        }

        animator.SetBool("isPlayerInSight", playerInSight);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcDecreaseHealthBy(float damageValue)
    {
        if (IsAlive)
        {
            if (NetworkedHealth - damageValue <= 0)
            {
                stateMachine.TransitionToState(ZombieStateMachine.EStates.Dead);
                NetworkedHealth = 0;
                IsAlive = false;
                if (!TryGetComponent<NetworkObject>(out var networkObject))
                {
                    Debug.LogError("NetworkComponent not found.");
                    return;
                }
                StartCoroutine(DespawnByTime(networkObject));
            }
            else
            {
                NetworkedHealth -= damageValue;
            }
        }
    }
    
    IEnumerator DespawnByTime(NetworkObject networkObject){
        yield return new WaitForSeconds(6);
        Runner.Despawn(networkObject);
    }
}
