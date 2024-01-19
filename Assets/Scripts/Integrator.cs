using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Integrator
{
    public static void Integrate(Particle2D particle, float dt)
    {
        particle.transform.position += (particle.velocity * dt).ToVector3(0);

        particle.acceleration = particle.accumulatedForces * particle.inverseMass + particle.gravity;

        particle.velocity += particle.acceleration * dt;
        particle.velocity *= Mathf.Pow(particle.damping, dt);
    }

    public static void Integrate(Particle3D particle, float dt)
    {
        // TODO: YOUR CODE HERE
        //       Hint: It should look very similar to Integrate(Particle2D...)
        particle.transform.position += particle.velocity * dt;

        particle.acceleration = particle.accumulatedForces * particle.inverseMass + particle.gravity;

        particle.velocity += particle.acceleration * dt;
        particle.velocity *= Mathf.Pow(particle.damping, dt);
    }
}
