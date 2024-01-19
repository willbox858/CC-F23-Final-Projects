using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle3D : MonoBehaviour
{
    // TODO: YOUR CODE HERE
    //       Hint: It should look very similar to Particle2D
    public Vector3 velocity;
    public float damping;
    public Vector3 acceleration;
    public Vector3 gravity = new Vector3(0, -9.8f);
    public float inverseMass;
    public Vector3 accumulatedForces { get; private set; }

    public void FixedUpdate()
    {
        DoFixedUpdate(Time.fixedDeltaTime);
    }

    public void DoFixedUpdate(float dt)
    {
        // Apply force from each attached ForceGenerator component
        System.Array.ForEach(GetComponents<ForceGenerator3D>(), generator => { if (generator.enabled) generator.UpdateForce(this); });

        Integrator.Integrate(this, dt);
        ClearForces();
    }

    public void ClearForces()
    {
        accumulatedForces = Vector3.zero;
    }

    public void AddForce(Vector3 force)
    {
        accumulatedForces += force;
    }
}
