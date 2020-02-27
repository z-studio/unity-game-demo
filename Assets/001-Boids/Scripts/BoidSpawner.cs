using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {
    public static BoidSpawner Instance { get; private set; }

    public int numBoids = 100;
    public GameObject boidPrefab;
    public Transform boidParent;
    public float spawnRadius = 100f;
    public float spawnVelocity = 10f;
    public float minVelocity = 0f;
    public float maxVelocity = 30f;
    public float nearDist = 30f;
    public float collisionDist = 5f;
    public float velocityMatchingAmt = 0.01f;
    public float flockCenteringAmt = 0.15f;
    public float collisionAvoidanceAmt = -0.5f;
    public float mouseAttractionAmt = 0.01f;
    public float mouseAvoidanceAmt = 0.75f;
    public float mouseAvoidanceDist = 15f;
    public float velocityLerpAmt = 0.25f;

    private Transform _transform;
    private Camera _camera;

    public Vector3 MouseWorldPos { get; private set; }
    
    private void Start() {
        Instance = this;
        
        _transform = transform;
        _camera = GetComponent<Camera>();
        
        for (var i = 0; i < numBoids; i++) {
            Instantiate(boidPrefab, boidParent);
        }
    }

    private void LateUpdate() {
        var screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _transform.position.y);
        MouseWorldPos = _camera.ScreenToWorldPoint(screenPos);
    }
}