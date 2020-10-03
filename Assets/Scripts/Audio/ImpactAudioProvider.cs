using Assets.Scripts.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    [RequireComponent(typeof(HealthController))]
    public class ImpactAudioProvider : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] impactAudioClips;
        private IEnumerable<AudioClip> randomAudioClipProvider;
        [SerializeField]
        private AudioSource audioSource;

        private void Awake()
        {
            randomAudioClipProvider = Utils.Utils.InfiniteRandomlyShuffledEnumerator(impactAudioClips);
        }

        public void PlayImpactAudioEffect()
        {
            audioSource.PlayOneShot(randomAudioClipProvider.First(), 0.5f);
            audioSource.PlayOneShot(randomAudioClipProvider.First(), 0.5f);
        }
    }
}
