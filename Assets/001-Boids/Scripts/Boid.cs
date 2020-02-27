using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour {
    public static List<Boid> Boids = new List<Boid>();

    public Vector3 velocity;
    
    private Vector3 _newVelocity;
    private Vector3 _newPosition;

    private List<Boid> _neighbors = new List<Boid>();
    private List<Boid> _collisionRisks = new List<Boid>();

    private Boid _closest;
    private Transform _transform;
    
    private void Awake() {
        _transform = transform;
        
        Boids.Add(this);

        // 让Boid只在XZ平面上移动
        var randPos = Random.insideUnitSphere * BoidSpawner.Instance.spawnRadius;
        randPos.y = 0;
        _transform.position = randPos;
        velocity = Random.onUnitSphere * BoidSpawner.Instance.spawnVelocity;

        var randColor = Color.black;

        while (randColor.r + randColor.g + randColor.b < 1f) {
            randColor = new Color(Random.value, Random.value, Random.value);
        }

        var rends = _transform.GetComponentsInChildren<MeshRenderer>();
        var mat = rends[0].material;
        mat.color = randColor;
        
        foreach (var r in rends) {
            r.sharedMaterial = mat;
        }
    }

    private void Update() {
        var neighbors = GetNeighbors(this);
        _newVelocity = velocity;
        _newPosition = _transform.position;

        // 速度匹配：使当前Boid的速度接近其临近Boid对象的平均速度
        var neighborVel = GetAverageVelocity(neighbors);
        _newVelocity += neighborVel * BoidSpawner.Instance.velocityMatchingAmt;
        
        // 凝聚向心性：向邻近对象的中心移动
        var neighborCenterOffset = GetAveragePosition(neighbors) - _transform.position;
        _newVelocity += neighborCenterOffset * BoidSpawner.Instance.flockCenteringAmt;

        // 排斥性：避免撞到邻近对象
        var dist = Vector3.zero;

        if (_collisionRisks.Count > 0) {
            var collisionAveragePos = GetAveragePosition(_collisionRisks);
            dist = collisionAveragePos - _transform.position;
            _newVelocity += dist * BoidSpawner.Instance.collisionAvoidanceAmt;
        }
        
        // 跟随鼠标移动：不论距离多远都向鼠标移动
        dist = BoidSpawner.Instance.MouseWorldPos - _transform.position;

        if (dist.magnitude > BoidSpawner.Instance.mouseAvoidanceDist) {
            _newVelocity += dist * BoidSpawner.Instance.mouseAttractionAmt;
        } else {
            // 如果距离鼠标过近，则快速离开
            _newVelocity -= dist.normalized * BoidSpawner.Instance.mouseAvoidanceDist *
                            BoidSpawner.Instance.mouseAvoidanceAmt;
        }
    }

    private void LateUpdate() {
        velocity = (1 - BoidSpawner.Instance.velocityLerpAmt) * velocity +
                    BoidSpawner.Instance.velocityLerpAmt * _newVelocity;

        if (velocity.magnitude > BoidSpawner.Instance.maxVelocity) {
            velocity = velocity.normalized * BoidSpawner.Instance.maxVelocity;
        }

        if (velocity.magnitude < BoidSpawner.Instance.minVelocity) {
            velocity = velocity.normalized * BoidSpawner.Instance.minVelocity;
        }

        _newPosition = _transform.position + (velocity * Time.deltaTime);
        _newPosition.y = 0;
        _transform.LookAt(_newPosition);
        _transform.position = _newPosition;
    }

    private List<Boid> GetNeighbors(Boid boid) {
        var closestDist = float.MaxValue;
        var delta = Vector3.zero;
        var dist = 0f;
        _neighbors.Clear();
        _collisionRisks.Clear();

        foreach (var b in Boids) {
            if (b == boid) {
                continue;
            }

            delta = b.transform.position - boid.transform.position;
            dist = delta.magnitude;

            if (dist < closestDist) {
                closestDist = dist;
                _closest = b;
            }

            if (dist < BoidSpawner.Instance.nearDist) {
                _neighbors.Add(b);
            }

            if (dist < BoidSpawner.Instance.collisionDist) {
                _collisionRisks.Add(b);
            }
        }

        if (_neighbors.Count == 0) {
            _neighbors.Add(_closest);
        }
        
        return _neighbors;
    }

    private Vector3 GetAverageVelocity(List<Boid> boids) {
        var sum = Vector3.zero;

        foreach (var b in boids) {
            sum += b.velocity;
        }

        return sum / boids.Count;
    }
    
    private Vector3 GetAveragePosition(List<Boid> boids) {
        var sum = Vector3.zero;

        foreach (var b in boids) {
            sum += b.transform.position;
        }

        return sum / boids.Count;
    }
}