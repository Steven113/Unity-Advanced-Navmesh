using Assets.Scripts.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    public class AudioManager : SingletonMonobehaviour
    {
        [SerializeField]
        SoundProvider[] soundProviders;
        [SerializeField]
        RandomSoundProvider[] randomSoundProviders;
        [SerializeField]
        AudioSource audioSource;

        private Dictionary<SoundType, ISoundProvider> SoundLookup;        

        public new void Awake()
        {
            base.Awake();

            SoundLookup = soundProviders.Cast<ISoundProvider>()
                .Union(randomSoundProviders.Cast<ISoundProvider>())
                .ToDictionary(x => x.SoundType, x => x);
        }

        public void PlaySound(SoundType soundType, Vector3? soundPoint = null)
        {
            if (SoundLookup.TryGetValue(soundType, out ISoundProvider soundProvider))
            {
                if (soundPoint.HasValue)
                {
                    AudioSource.PlayClipAtPoint(soundProvider.GetClip(), soundPoint.Value, audioSource.volume);
                }
                else
                {
                    audioSource.PlayOneShot(soundProvider.GetClip());
                }

                return;
            }

            throw new InvalidOperationException($"No sound provider exists for the sound of type {soundType}");
        }

        [Serializable]
        public class SoundProvider : ISoundProvider
        {
            [SerializeField]
            SoundType soundType;
            [SerializeField]
            AudioClip soundClip;

            SoundType ISoundProvider.SoundType => soundType;

            AudioClip ISoundProvider.GetClip()
            {
                return soundClip;
            }
        }

        [Serializable]
        public class RandomSoundProvider : ISoundProvider
        {
            [SerializeField]
            SoundType soundType;
            [SerializeField]
            AudioClip [] soundClips;
            private IEnumerable<AudioClip> _randomlyOrderedClips;
            private IEnumerable<AudioClip> RandomlyOrderedClips
            {
                get
                {
                    _randomlyOrderedClips = _randomlyOrderedClips ?? Utils.Utils.InfiniteRandomlyShuffledEnumerator(soundClips);
                    return _randomlyOrderedClips;
                }
            }


            SoundType ISoundProvider.SoundType => soundType;

            AudioClip ISoundProvider.GetClip()
            {
                return RandomlyOrderedClips.First();
            }
        }
    }
}
