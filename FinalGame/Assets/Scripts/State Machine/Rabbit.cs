using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rabbit : MonoBehaviour
{
    public PlayerController m_player;
    public GameObject keyPrefab;

    public enum eState: int
    {
        kIdle,
        kHopStart,
        kHop,
        kCaught,
        kNumStates

    }

    private Color[] stateColors = new Color[(int)eState.kNumStates]
    {
        new Color(255, 0, 0),
        new Color(0,255, 0),
        new Color(0, 0, 255),
        new Color(255, 255, 255)
    };

    //External tunables
    public float m_fHopTime = 0.2f;
    public float m_fHopSpeed = 6.5f;
    public float m_fScaredDistance = 3.0f;
    public int m_nMaxMoveAttemps = 50;
    public float m_fRandAngle = 70f;

    //Internal variables
    public eState m_nState;
    public float m_fHopStart;
    public Vector3 m_vHopStartPos;
    public Vector3 m_vHopEndPos;

    public int attempts = 0;
    public bool validDirection = false;

    public Vector3 playerPos;
    Vector3 hopDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Setup initial state
        m_nState = eState.kIdle;
      
    }

    private void FixedUpdate()
    {
        GetComponentInChildren<Renderer>().material.color = stateColors[(int)m_nState];

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        //looks for player until its found 
        if (m_player == null)
        {
            m_player = FindFirstObjectByType<PlayerController>();
            if (m_player == null) return;
        }

        //In idle state stay in the same place until player gets too close
        if (m_nState == eState.kIdle)
        {
            //change to hop start state if player gets too close
            if(Vector3.Distance(transform.position, m_player.transform.position) <= m_fScaredDistance)
            {
                m_nState = eState.kHopStart;
            }
        }

        //Hop start gives the start and the ending positions of the hop
        else if(m_nState == eState.kHopStart)
        {
            //resets before while loop 
            validDirection = false;
            attempts = 0;

            //gets player and target position
            playerPos = m_player.transform.position;

            //keep movement flat
            playerPos.y = transform.position.y;
            m_vHopStartPos = transform.position;

            Vector3 baseDirection = (transform.position - playerPos).normalized;

            //continues until it finds a valid direction rabbit can move towards 
            while (!validDirection && attempts < m_nMaxMoveAttemps)
            {
                //add randomess to direction 
                float randAngle = Random.Range(-m_fRandAngle, m_fRandAngle);
                Quaternion rotation = Quaternion.Euler(0f, randAngle -45f, 0f);

                hopDirection = rotation * baseDirection;

                //prevent vertical movement 
                hopDirection.y = 0f;
                hopDirection.Normalize();

                //if something is in front -> choose new direction
                if (!Physics.Raycast(transform.position, hopDirection, m_fHopSpeed * m_fHopTime))
                {
                    validDirection = true;
                }

                attempts++;
            }

            //if still not valid after while loop guarantee to move away from player 
            if (!validDirection)
            {
                hopDirection = -baseDirection;
            }

            float hopDist = m_fHopSpeed * m_fHopTime;

            //desired end based on direction
            m_vHopEndPos = m_vHopStartPos + hopDirection * hopDist;

            m_fHopStart = Time.time;

            //change state to hop
            m_nState = eState.kHop;
        }

        else if(m_nState == eState.kHop)
        {
            float t = (Time.time - m_fHopStart) / m_fHopTime;

            float targetAngle = Mathf.Atan2(hopDirection.z, hopDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.position = Vector3.Lerp(m_vHopStartPos, m_vHopEndPos, t);

            if(t >= 1.0f)
            {
                m_nState = eState.kIdle;
                m_fHopStart = Time.time;
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        // Check if this is the player (in this situation it should be!)
        if (collision.gameObject == GameObject.Find("Player"))
        {

            //If the player is diving, it's a catch!
            if (m_player.isDiving)
            {
                Debug.Log("rabbit caught!");

                m_nState = eState.kCaught;

                //spawn position for key 
                Vector3 spawnPos = transform.position + Vector3.up *  1f;

                //remove rabbit and spawn key 
                Destroy(gameObject);
                Instantiate(keyPrefab, spawnPos, Quaternion.identity);

            }
        }
    }

}
