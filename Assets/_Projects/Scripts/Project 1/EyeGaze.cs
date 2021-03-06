﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class EyeGaze : MonoBehaviour {

    public int mode;
    public int level;
    public GameObject displayAttention;

    public Transform transformForTP;

    public float laserSpeed = 100.0f;
    public float cannonballSpeed = 200.0f;
    public float TimeRequiredForAction = 2.0f; //Threshold
    public Text statusText;

    public Text raycastResult;

    public Image widget;
    public Image floorWidget;
    public Image laserIcon;
    public Image cannonIcon;
    public Image tpIcon;

    public FireMode fireMode = FireMode.None;
    public GameObject laserPrefab;
    
    public Transform laserSpawnPoint;
    public GameObject cannonBallPrefab;
    public Color defaultColor = Color.white;

    public AudioClip cannonSound;
    public AudioClip laserSound;
    public AudioClip teleportSound;
    private Transform hitLocation;
    private AudioSource audioS;
    private RaycastHit hit;
    private float totalTimeGazed;
    private Interactable previousFocus; //gameObject from result of raycast from last frame;
    private Interactable focusedObject; //currently focus object
    private Transform camT;
    private List<GameObject> cannonballs;

    private bool lastFrameFloorGazed;
    private bool currentFrameFloorGazed;
    private float timeGazedOnFloor;
    private Vector3 hitPoint;
    private Vector3 cameraDefaultPosition;
    private int numOfTargetDestroyed;

    //CSE189 BCI
    private DisplayData attentionScript;
    private int attentionValue;
    public int attentionThreshold;
    // Use this for initialization
    void Start() {
        level = 1;
        if (mode == 0)
        {
            TimeRequiredForAction = 3.0f;
            attentionThreshold = 40;
        }
        if (mode == 1)
        {
            attentionThreshold = 50;
            TimeRequiredForAction = 2.0f;
        }
        numOfTargetDestroyed = 0;
        totalTimeGazed = 0.0f;
        timeGazedOnFloor = 0.0f;
        cannonIcon.color = defaultColor;
        laserIcon.color = defaultColor;
        tpIcon.color = defaultColor;
         camT = Camera.main.transform;
        cannonballs = new List<GameObject>();
        audioS = GetComponent<AudioSource>();
        attentionScript = GetComponent<DisplayData>();

        lastFrameFloorGazed = false;
        currentFrameFloorGazed = false;
        cameraDefaultPosition = transformForTP.position;

    }

    // Update is called once per frame
    void Update() {
        attentionValue = attentionScript.getAttention();
        if (numOfTargetDestroyed >= 5) {
            if (mode == 0)
                attentionThreshold += 10;
            else if (mode == 1)
                TimeRequiredForAction += 1.0f;
            numOfTargetDestroyed = 0;
            level++;
            Debug.Log("Final time: " + displayAttention.GetComponent<displayAttention>().time);
            displayAttention.GetComponent<displayAttention>().time = 0.0f;
        }
        Debug.Log("attention threshold: " + attentionThreshold);
        PerformRaycast();
        CheckFocus();

        CheckFloorGazeForTP();

        previousFocus = focusedObject;
        lastFrameFloorGazed = currentFrameFloorGazed;
        

        if (Input.GetKeyDown(KeyCode.S))
        {
             ShootLaser();
        }
        

    }


    void PerformRaycast()
    {
        Debug.DrawRay(camT.position, camT.forward*15, Color.red);
        Ray ray = new Ray(camT.position, camT.forward);
        if (Physics.Raycast(ray, out hit))
        {
            hitLocation = hit.transform;
            focusedObject = hitLocation.GetComponent<Interactable>();
         //   raycastResult.text = "Raycast: " + hitLocation.name;
            hitPoint = hit.point;
           // string s = "Point: " + hitPoint.x + "  " + hitPoint.y + "  " + hitPoint.z;
           // raycastResult.text = s;
            //check if user is gazing at the floor
            if (hitLocation.GetComponent<Floor>())
            {
                currentFrameFloorGazed = true;
            }
            else
            {
                currentFrameFloorGazed = false;
            }
            
        }
        else
        {
            raycastResult.text = "Raycast: null" ;
            focusedObject = null;
            hitLocation = null;
            currentFrameFloorGazed = false;
        }
    }

    void CheckFocus()
    {

        if (!focusedObject || (!previousFocus && focusedObject))
        {
            totalTimeGazed = 0f;
        }
        else if (previousFocus != focusedObject || attentionValue < attentionThreshold)
        {
            totalTimeGazed = 0.0f;   //reset timer if focus changed
        }
        else
        {
            totalTimeGazed += Time.deltaTime;
            //totalTimeGazed = attentionScript.getAttention();
        }


            

        if ((fireMode == FireMode.None || fireMode == FireMode.Teleport) && ((!focusedObject) || (!(focusedObject.GetType() == typeof(SelectFireMode)))))
        {
            widget.fillAmount = 0.0f;
        }
        else
        {
            float pctComplete = totalTimeGazed / TimeRequiredForAction;
            widget.fillAmount = pctComplete;
        }


        if (totalTimeGazed >= TimeRequiredForAction)
        {
            performAction();
            totalTimeGazed = 0.0f;
        }
    }


    void CheckFloorGazeForTP()
    {
        if (fireMode != FireMode.Teleport)
            return;

        if (lastFrameFloorGazed && currentFrameFloorGazed)
        {
            timeGazedOnFloor += Time.deltaTime;
            //timeGazedOnFloor = attentionScript.getAttention();
        }
        else
        {
            timeGazedOnFloor = 0.0f;
        }
        
        float pctComplete = timeGazedOnFloor / TimeRequiredForAction;
        floorWidget.fillAmount = pctComplete;


        if (timeGazedOnFloor >= TimeRequiredForAction)
        {
            //perform tp here
            Vector3 p = this.transform.position;
            transformForTP.position = new Vector3(hitPoint.x, p.y, hitPoint.z);
            audioS.PlayOneShot(teleportSound);
            timeGazedOnFloor = 0.0f;
        }


    }

    

    void performAction()
    {

        if (focusedObject.GetType() == typeof(SelectFireMode))
        {

            SelectFireMode sfm = focusedObject as SelectFireMode;
            fireMode = sfm.fireMode;
            audioS.PlayOneShot(sfm.sound);
            if (fireMode == FireMode.None)
            {
                cannonIcon.color = defaultColor;
                laserIcon.color = defaultColor;
                tpIcon.color = defaultColor;
            }
            else if (fireMode == FireMode.Cannon)
            {
                cannonIcon.color = Color.white;
                laserIcon.color = defaultColor;
                tpIcon.color = defaultColor;
            }
            else if(fireMode == FireMode.Laser)
            {
                cannonIcon.color = defaultColor;
                laserIcon.color = Color.white;
                tpIcon.color = defaultColor;
            }
            else if(fireMode == FireMode.Teleport)
            {
                cannonIcon.color = defaultColor;
                laserIcon.color = defaultColor;
                tpIcon.color = Color.white;
            }
            return;
        }
        else if (focusedObject.GetType() == typeof(Brick))
        {
            numOfTargetDestroyed++;
            if (fireMode == FireMode.Laser)
            {
                ShootLaser();
            }
            else if (fireMode == FireMode.Cannon)
            {
                FireCannonBall();
            }

        }
        
    }


    void ShootLaser()
    {

        GameObject laserGO = Instantiate(laserPrefab) as GameObject;
        laserGO.transform.parent = laserSpawnPoint;
        laserGO.transform.position = laserSpawnPoint.position;
        laserGO.transform.rotation = laserSpawnPoint.rotation;
        laserGO.GetComponent<Rigidbody>().AddForce(laserSpawnPoint.forward*laserSpeed);
        audioS.PlayOneShot(laserSound);
        (focusedObject as Brick).RespondToLaserAttack();
    }


    void FireCannonBall()
    {
        GameObject cannonballGO = Instantiate(cannonBallPrefab) as GameObject;
        cannonballs.Add(cannonballGO);
        cannonballGO.transform.position = camT.position;
        cannonballGO.GetComponent<Rigidbody>().AddForce(camT.forward*cannonballSpeed);
        audioS.PlayOneShot(cannonSound);
    }


    public void ResetCannonBalls()
    {
        foreach (GameObject g in cannonballs)
        {
            GameObject.Destroy(g);
        }
        cannonballs.Clear();
        transformForTP.position = cameraDefaultPosition;
    }


}

public enum FireMode
{
    None, Laser, Cannon, Teleport
}
