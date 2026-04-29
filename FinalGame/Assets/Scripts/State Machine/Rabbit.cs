using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rabbit : MonoBehaviour
{
    public PlayerController m_player;

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

    //Internal variables
    public eState m_nState;
    public float m_fHopStart;
    public Vector3 m_vHopStartPos;
    public Vector2 m_vHopEndPos;

    public Vector3 playerPos;
    Vector3 hopDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Setup initial state
        m_nState = eState.kIdle;
        m_player = GameObject.FindFirstObjectByType(typeof(PlayerController)) as PlayerController;
    }

    private void FixedUpdate()
    {
        GetComponent<Renderer>().material.color = stateColors[(int)m_nState];

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);

        //In idle state stay in the same place until player gets too close
        if(m_nState == eState.kIdle)
        {
            //change to hop start state if player gets too close
            if(Vector2.Distance(transform.position, m_player.transform.position) <= m_fScaredDistance)
            {
                m_nState = eState.kHopStart;
            }
        }

        //Hop start gives the start and the ending positions of the hop
        else if(m_nState == eState.kHopStart)
        {
            //gets player and target position
            playerPos = m_player.transform.position;
            //keep movement flat
            playerPos.y = transform.position.y;
            m_vHopStartPos = transform.position;

            hopDirection = (transform.position - playerPos).normalized;
            //prevent vertical movement 
            hopDirection.y = 0f;
            float hopDist = m_fHopSpeed * m_fHopTime;

            Vector3 desiredEnd = m_vHopStartPos + hopDirection * hopDist;
            Vector3 screenEnd = Camera.main.WorldToScreenPoint(desiredEnd);

            Vector3 vScreenPos = Camera.main.WorldToScreenPoint(transform.position);

            float edgeBuffer = 5f;

            if(vScreenPos.y > Screen.height - edgeBuffer)
            {
                hopDirection.z = -Mathf.Abs(hopDirection.z);
            }

            else if(vScreenPos.y < edgeBuffer)
            {
                hopDirection.z = Mathf.Abs(hopDirection.z);
            }

            else if(vScreenPos.x > Screen.width - edgeBuffer)
            {
                hopDirection.x = -Mathf.Abs(hopDirection.x);
            }

            hopDirection = hopDirection.normalized;

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
            transform.rotation = Quaternion.Euler(0f, -targetAngle, 0f);
            transform.position = Vector3.Lerp(m_vHopStartPos, m_vHopEndPos, t);

            if(t >= 1.0f)
            {
                m_nState = eState.kIdle;
                m_fHopStart = Time.time;
            }
        }
    }


    void OnTriggerStay(Collider collision)
    {
        // Check if this is the player (in this situation it should be!)
        if (collision.gameObject == GameObject.Find("Player"))
        {
            //If the player is diving, it's a catch!
            if (m_player.isDiving)
            {
                m_nState = eState.kCaught;
                transform.parent = m_player.transform;
                transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);
            }
        }
    }

}
