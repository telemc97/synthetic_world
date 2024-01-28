using System;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.Randomization.Scenarios;
using EnvController_ns;

public class SimulationScenario : FixedLengthScenario
{
    public bool QuitOnCompletion = false;
    protected override void OnComplete()
    {
        DatasetCapture.ResetSimulation();
        if (QuitOnCompletion)
        {
            Quit();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
