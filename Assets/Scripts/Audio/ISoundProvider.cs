using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Audio
{
    interface ISoundProvider
    {
        SoundType SoundType { get; }
        AudioClip GetClip();
    }
}
