using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using KartGame.KartSystems;
using MLAgents.CommunicatorObjects;
using KartGame.Track;

public class KartingAgent : Agent, IInput
{
    TrackManager trackManager;

    IRacer racer;

    KartMovement kartMovement;

    public LayerMask raycastLayers;

    public float debugRaycastTime = 2f;

    public float raycastDistance = 10;

    public Transform[] raycasts;

    #region Getter

    float _acceleration;

    public float Acceleration => _acceleration;

    float _steering;

    public float Steering => _steering;

    bool _boostPressed;

    public bool BoostPressed => _boostPressed;

    bool _firePressed;

    public bool FirePressed => _firePressed;

    public bool HopPressed => false;

    public bool HopHeld => false;

    #endregion

    public float rewardOnCheckPoint = 1;

    Vector3 startingPos;

    Quaternion startingRot;

    void Awake()
    {
        trackManager = FindObjectOfType<TrackManager>();
        racer = GetComponent<IRacer>();
        kartMovement = GetComponent<KartMovement>();
        startingPos = this.transform.position;
        startingRot = this.transform.rotation;
    }

    public override void AgentReset()
    {
        base.AgentReset();
        kartMovement.transform.position = startingPos;
        kartMovement.transform.rotation = startingRot;
        kartMovement.ForceMove(Vector3.zero, Quaternion.identity);
        trackManager.RestartRace();
    }

    public void OnReachCheckpoint(Checkpoint checkpoint)
    {
        this.AddReward(rewardOnCheckPoint);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        base.AgentAction(vectorAction, textAction);
        _acceleration = vectorAction[0];
        if (_acceleration > 0)
        {
            _acceleration = 1;
        }
        _steering = vectorAction[1];

        AddReward(kartMovement.LocalSpeed * 0.001f);
        Debug.Log(kartMovement.LocalSpeed);
    }

    public override void CollectObservations()
    {
        AddVectorObs(kartMovement.LocalSpeed);

        //raycasts
        for (int i = 0; i < raycasts.Length; i++)
        {
            AddRayCastVectorObs(raycasts[i]);
        }

        base.CollectObservations();
    }

    void AddRayCastVectorObs(Transform ray)
    {
        RaycastHit hitInfo = new RaycastHit();
        var hit = Physics.Raycast(ray.position, ray.forward, out hitInfo, raycastDistance, raycastLayers.value, QueryTriggerInteraction.Ignore);
        var distance = hitInfo.distance;
        if (!hit)
        {
            distance = raycastDistance;
        }
        var obs = distance / raycastDistance;
        AddVectorObs(obs);

        if (distance < 1f)
        {
            this.Done();
            this.AgentReset();
        }
        Debug.DrawRay(ray.position, ray.forward * distance, Color.Lerp(Color.red, Color.green, obs), Time.deltaTime * debugRaycastTime);
    }

}
