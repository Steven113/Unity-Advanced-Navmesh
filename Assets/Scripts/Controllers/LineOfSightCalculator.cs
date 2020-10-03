
using Assets.Scripts.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class LineOfSightCalculator : SingletonMonobehaviour
    {
        [SerializeField]
        internal LayerMask rayCastLayerMask;

        public bool CanSeeGivenPoint(Vector3 targetPoint, Vector3 eye)
        {
            return !Physics.Raycast(eye, targetPoint - eye, (targetPoint - eye).magnitude, rayCastLayerMask, QueryTriggerInteraction.Ignore);
        }
    }
}
