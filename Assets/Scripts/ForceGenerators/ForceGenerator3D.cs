using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ForceGenerator3D : ForceGenerator
{
    public abstract void UpdateForce(Particle3D particle);
}
