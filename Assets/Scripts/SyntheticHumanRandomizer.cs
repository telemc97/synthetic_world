using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using Unity.CV.SyntheticHumans.Randomizers;
using Random = UnityEngine.Random;
using UnityEditorInternal;

[Serializable]
[AddRandomizerMenu("Synthetic Humans/Human Generation Randomizer")]

public class SyntheticHumanRandomizer : HumanGenerationRandomizer
{
    [Header("Human Areas Settings")]
    public int humanAreaCapacity = 4;
    public float humanAreaSize = 20f;

    [Header("Spawn Areas Min and Max Coordinates")]
    public Vector3 minCoords = new Vector3(100f, 0f, 100f);
    public Vector3 maxCoords = new Vector3(900f, 0f, 900f);

    protected override void OnIterationEnd()
    {
        //Do nothing
    }

    protected override void OnScenarioComplete()
    {
        PlaceHumans();
    }

    private void PlaceHumans()
    {
        List<GameObject> humans = new List<GameObject>(GameObject.FindGameObjectsWithTag("SyntheticHuman"));
        List<GameObject> humanAreas = new List<GameObject>(GameObject.FindGameObjectsWithTag("SyntheticHumanArea"));

        if (humans.Count > 0)
        {

            if (humanAreas.Count == 0 || humans.Count / humanAreaCapacity > humanAreas.Count || humans.Count % humanAreaCapacity > 0)
            {
                int extraAreasNeeded = (int)Math.Ceiling((double)humans.Count / humanAreaCapacity) - humanAreas.Count;
                Debug.Log("To meet the requirement of 4 humans per area " + extraAreasNeeded + " areas need to be created");
                for (int i = 0; i < extraAreasNeeded; i++)
                {
                    GameObject newHumanArea = CreateActorArea(i);
                    humanAreas.Add(newHumanArea);
                }
            }

            int humanIt = 0;
            foreach (GameObject area in humanAreas)
            {
                for (int i = 0; i < humanAreaCapacity; i++)
                {
                    Vector3 newHumanPos = new Vector3(Random.Range((area.transform.position.x - humanAreaSize), (area.transform.position.x + humanAreaSize)),
                                                      0f,
                                                      Random.Range((area.transform.position.z - humanAreaSize), (area.transform.position.z + humanAreaSize)));
                    newHumanPos.y = Terrain.activeTerrain.SampleHeight(newHumanPos) + Terrain.activeTerrain.GetPosition().y + 0.5f; // get terrain height at that point
                    humans[humanIt].transform.SetParent(area.transform);
                    humans[humanIt].transform.position = newHumanPos;
                    humanIt++;
                    if (humanIt > (humans.Count - 1))
                    {
                        break;
                    }
                }
                if (humanIt > (humans.Count - 1))
                {
                    break;
                }
            }
        }
    }

    private GameObject CreateActorArea(int it = 0)
    {
        GameObject actorArea = new GameObject("HumanArea" + (it+1));
        actorArea.tag = "SyntheticHumanArea";
        Vector3 newPos = new Vector3(Random.Range(minCoords.x, maxCoords.x), 0f, Random.Range(minCoords.z, maxCoords.z));
        actorArea.transform.position = newPos;
        return actorArea;
    }
}