using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Obstacle_ns
{
    internal class Obstacle
    {
        private Vector3 actorPosition;
        private Vector3 obstalcePosition;

        private bool obstacleAhead;
        private float distance;
        private string obstacleTag;

        public Obstacle()
        {
            actorPosition = Vector3.zero;
            obstalcePosition = Vector3.zero;
            obstacleAhead = false;
            obstacleTag = string.Empty;
        }

        public void SetObstacleData(Vector3 _actorPosition, Vector3 _obstalcePosition, string _obstacleTag)
        {
            actorPosition = _actorPosition;
            obstalcePosition = _obstalcePosition;
            obstacleAhead = true;
            distance = Mathf.Sqrt(Mathf.Pow((obstalcePosition.x - actorPosition.x), 2) + Mathf.Pow((obstalcePosition.y - actorPosition.y), 2));
            obstacleTag = _obstacleTag;
        }

        public float DistanceToObstacle()
        {
            return distance;
        }

        public bool ObstacleExists()
        {
            return obstacleAhead;
        }

        public string ObstacleTag()
        {
            return obstacleTag;
        }

        public Vector3 ObstaclePosition() 
        {
            return obstalcePosition;
        }
    }
}
