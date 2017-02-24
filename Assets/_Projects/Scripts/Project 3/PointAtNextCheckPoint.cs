﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAtNextCheckPoint : MonoBehaviour {

    
   // public Transform Player;
    public DroneController dc;
    public LoadCheckPoints lcp;
    public Transform target;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
       int n = dc.nextTargetCheckPoint;
        if (n > lcp.checkPoints.Count) return;

        target = lcp.checkPoints[n-1];
        transform.LookAt(target);



    }
}