using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject drone;
    public GameObject DronePrefab;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(DronePrefab, drone.transform.position, Quaternion.identity);
    }
}
