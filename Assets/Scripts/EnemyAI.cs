using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Chasing,
        Roaming
    }

    public State currentState;
    public float detectionRadius = 5f;  // How far the enemy can detect the player
    public float roamTime = 3f;  // Time spent roaming before switching back to roaming mode
    public float chaseSpeed = 3.5f;
    public float roamSpeed = 2f;

    private Transform player;
    private NavMeshAgent agent;
    private float roamTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;  // Assuming the player has the "Player" tag
        currentState = State.Roaming;

        roamTimer = roamTime;
        agent.speed = roamSpeed;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Chasing:
                ChasePlayer();
                break;
            case State.Roaming:
                RoamBehavior();
                break;
        }

        DetectPlayer();
    }

    void DetectPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRadius)
        {
            currentState = State.Chasing;
            agent.speed = chaseSpeed;
        }
        else if (currentState == State.Chasing && distanceToPlayer > detectionRadius)
        {
            currentState = State.Roaming;
            agent.speed = roamSpeed;
        }
    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    void RoamBehavior()
    {
        roamTimer -= Time.deltaTime;

        if (roamTimer <= 0f)
        {
            // Reset roam time and pick a new random destination
            roamTimer = roamTime;
        }

        // Roaming behavior: move to random points within a certain range
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas);
        agent.SetDestination(hit.position);
    }
}
