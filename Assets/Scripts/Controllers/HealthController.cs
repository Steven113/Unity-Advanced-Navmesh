using Assets.Scripts.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class HealthController : MonoBehaviour, General.IStateProvider<HealthLevel>
    {
        [SerializeField]
        private HealthLevel healthLevel;
        private ObservableState<HealthLevel> healthStateObservable = new ObservableState<HealthLevel>(new HealthLevel());

        private void Awake()
        {
            healthStateObservable.Set(healthLevel);
        }

        public void Damage(float damage)
        {
            Debug.Assert(damage > 0);

            healthLevel.Current -= damage;
            healthLevel.Current = Math.Max(healthLevel.Current, 0);

            healthStateObservable.Set(healthLevel);
        }

        public IDisposable Subscribe(General.IStateListener<HealthLevel> observer)
        {
            return ((General.IStateProvider<HealthLevel>)healthStateObservable).Subscribe(observer);
        }
    }

    [Serializable]
    public struct HealthLevel
    {
        public float Max;
        public float Current;
    }
}
