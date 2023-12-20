using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selectParticleScript : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particles;

    public void Init(Color matchingColor)
    {
        foreach (ParticleSystem particle in particles)
        {
            var module = particle.main;
            module.startColor = matchingColor;
        }
    }
}
