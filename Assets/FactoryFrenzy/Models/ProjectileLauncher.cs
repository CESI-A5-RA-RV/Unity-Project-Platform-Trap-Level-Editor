using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : LevelElement
{
    public override void Initialize(ElementData data)
    {
        base.Initialize(data);

        // Search for the Launcher component in child objects
        Launcher launcher = GetComponentInChildren<Launcher>();

        if (data.parameters != null)
        {
            foreach (var param in data.parameters)
            {
                if (param.key == "detectionRadius")
                {
                    launcher.detectionRadius = param.value;
                }
                else if (param.key == "rotationSpeed")
                {
                    launcher.rotationSpeed = param.value;
                }
                else if (param.key == "shootingForce")
                {
                    launcher.shootingForce = param.value;
                }
            }
        }
        else
        {
            Debug.Log("No parameters found for this element.");
        }
    }
}

