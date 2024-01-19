using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringForce : ForceGenerator3D
{
    public Transform other = null;

    public float springConstant = .5f;

    public float restLength = 1f;

    public override void UpdateForce(Particle2D particle)
    {
        if (other)
        {
            Vector3 dist = transform.position - other.transform.position;
            float displacement = dist.magnitude - restLength;
            float magnitude = -displacement * springConstant;

            Vector3 force = dist.normalized * magnitude;

            particle.AddForce(force);
        }
        else
        {
            Debug.LogWarning("Anchor not set for Fixed Spring Particle Force");
        }
    }

    public override void UpdateForce(Particle3D particle)
    {
        // TODO: YOUR CODE HERE
        if (other)
        {
            Vector3 dist = transform.position - other.transform.position;
            float displacement = dist.magnitude - restLength;
            float magnitude = -displacement * springConstant;

            Vector3 force = dist.normalized * magnitude;

            particle.AddForce(force);
        }
        else
        {
            Debug.LogWarning("Anchor not set for Fixed Spring Particle Force");
        }
    }
}
