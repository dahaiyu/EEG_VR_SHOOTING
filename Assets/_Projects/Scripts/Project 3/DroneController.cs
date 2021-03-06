﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;
using UnityEngine.UI;

public class DroneController : MonoBehaviour {
    public GameObject distance;
    public GameObject particle;
    public Transform particlePos;
    public Text t;
    public DroneState state = DroneState.Stop;
    public float flySpeed = 4.0f;
    public float speedUpFactor = 1.3f;
    public bool speedUp = false;
    public LoadCheckPoints lcp;
    [Header("Line Render")]
    public LineRenderer lr;
    public float lineLength = 100.0f;
    [Header("Hands")]
    public GameObject leftHand;
    public GameObject rightHand;
    [Header("Sounds")]
    public AudioClip gameEndSound;
    public AudioClip hitSound;
    public AudioClip motorSound;
    private AudioSource audio;
    public AudioSource engineAudio;
    // [HideInInspector]
    public int nextTargetCheckPoint = 2;  //1st checkpoint is where the player is at (1st set of coord in the competition file)
    private CharacterController c_controller;
    private Vector3 flyTowards = Vector3.zero;
    private LeapProvider provider;

    private bool useIndexFingerAsDirection = false;
    [Header("UI")]
    public GameObject timerUI;
    public GameObject stopwatch;
    public Text gameEndText;


  
   
    [HideInInspector]
    public bool canStart = false;


    private float defaultVolume;
    public static DroneController Instance;
    private void Awake()
    {
        Instance = this;
    }


	// Use this for initialization
	void Start () {
        particle.SetActive(false);
        c_controller = GetComponent<CharacterController>();
        audio = GetComponent<AudioSource>();
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
        gameEndText.enabled = false;
        nextTargetCheckPoint = 2;
        defaultVolume = engineAudio.volume;
        if (!provider)
            Debug.LogError("Leap Provider not found");
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!canStart) return;
        //if(timerUI.GetComponent<Timer>().timeElapsed)
            PerformDroneMovement();
	}

    public void PerformDroneMovement()
    {
        if (state == DroneState.Stop) return;

        if (useIndexFingerAsDirection)
        {
            Frame frame = provider.CurrentFrame;

            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsRight)
                {
                    //draws where the index finger is pointing at
                    Bone[] bones = hand.Fingers[1].bones;
                    Bone bone = bones[3];
                    Vector d = bone.Direction;    
                    Vector3 center = transform.position;
                    Vector3 direction = new Vector3(d.x, d.y, d.z);

                    flyTowards = direction;
                }
            }

        }
        float factor = flySpeed * Time.deltaTime;
        if (speedUp)
        {
            factor = factor * speedUpFactor;
        }

        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, transform.position + flyTowards * 500f);
        c_controller.Move(flyTowards * factor);


    }



    public void MoveWhereLeftHandisPointingAt()
    {
        state = DroneState.Forward;
    }

    public void MoveWhereRightHandisPointingAt()
    {
        state = DroneState.Forward;
        useIndexFingerAsDirection = true;
    }

    public void StoppedUsingIndexFinger()
    {
        useIndexFingerAsDirection = false;
    }


    public void StopMoving()
    {
        state = DroneState.Stop;
    }

    public void CheckPointReached()
    {
        nextTargetCheckPoint++;
        if(nextTargetCheckPoint > LoadCheckPoints.totalNumCheckPoint)
        {
            Debug.Log("Game should end");
            // game ending here
            audio.PlayOneShot(gameEndSound);
            gameEndText.enabled = true;
            distance.GetComponent<Distance>().gameover = true;
            stopwatch.GetComponent<Stopwatch>().setGameOver(true);
            particle.transform.position = particlePos.position;
            particle.SetActive(true);
        }
    }


    IEnumerator startFireWork()
    {
        yield return new WaitForSeconds(0.3f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        string s = hit.transform.name;
        audio.PlayOneShot(hitSound);
        state = DroneState.Stop;
        MoveToLastCheckPoint();

    }

    public void StartToSpeedUp()
    {
        speedUp = true;
        engineAudio.volume = defaultVolume * 3.5f;
       // audio.PlayOneShot(motorSound);
    }


    public void StopSpeedUp()
    {
        speedUp = false;
        engineAudio.volume = defaultVolume;
    }

    private void MoveToLastCheckPoint()
    {
        transform.position = lcp.posOfCheckPointNumber(nextTargetCheckPoint - 1);
    }


}


public enum DroneState
{
    Forward, Stop
}