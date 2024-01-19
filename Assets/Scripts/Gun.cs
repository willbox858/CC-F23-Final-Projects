using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public enum Weapon
    {
        Particle = 0,
        Attractor,
        Repulsor,
        FixedSpring,
        PairedSpring,

        Count
    };

    [SerializeField]
    private Transform pointDirection;

    [SerializeField]
    private Transform reticle;

    [SerializeField]
    private float rotationSpeed;

    [SerializeField]
    private float power = 20f;

    [SerializeField]
    private GameObject NormalObject;

    [SerializeField]
    private GameObject SpringObject;

    [SerializeField]
    private GameObject ForceObject;

    int weaponIndex = 0;

    private readonly Dictionary<Weapon, System.Func<GameObject>> weaponsDictionary;


    /// <summary>
    /// The direction of the initial velocity of the fired projectile. That is,
    /// this is the direction the gun is aiming in.
    /// </summary>
    public Vector3 FireDirection
    {
        get
        {
            return transform.up;
        }
    }

    /// <summary>
    /// The position in world space where a projectile will be spawned when
    /// Fire() is called.
    /// </summary>
    public Vector3 SpawnPosition
    {
        get
        {
            return pointDirection.position;
        }
    }

    /// <summary>
    /// Spawns the currently active projectile, firing it in the direction of
    /// FireDirection.
    /// </summary>
    /// <returns>The newly created GameObject.</returns>
    public GameObject Fire()
    {
        if (weaponsDictionary.TryGetValue((Weapon)weaponIndex, out System.Func<GameObject> weaponFunc))
        {
            return weaponFunc();
        }

        Debug.LogError("No weapon func found");
        return null;
    }

    public GameObject Fire(GameObject prototype)
    {
        GameObject firedObject = Instantiate(prototype, SpawnPosition, Quaternion.identity);
        firedObject.GetComponent<Particle3D>().velocity = FireDirection * power;
        return firedObject;
    }

    /// <summary>
    /// Moves to the next weapon. If the last weapon is selected, calling this
    /// again will roll over to the first weapon again. For example, if there
    /// are 4 weapons, calling this 4 times will end up with the same weapon
    /// selected as if it was called 0 times.
    /// </summary>
    public void CycleNextWeapon()
    {
        weaponIndex = (weaponIndex + 1) % ((int)Weapon.Count);
    }

    public GameObject FireParticle()
    {
        return Fire(NormalObject);
    }

    /// <summary>
    /// Spawns a particle that has a SpringForce component attached. Spawns
    /// another particle, and attaches the SpringForce's "other" variable
    /// to the other particle's transform.
    /// </summary>
    /// <returns>The created spring object (NOT the spring object's target)</returns>
    public GameObject FirePairedSpringWeapon()
    {
        GameObject firedObject = Fire(SpringObject);
        GameObject pairedObject = Fire(NormalObject);
        SpringForce spring = firedObject.GetComponent<SpringForce>();
        spring.other = pairedObject.transform;
        pairedObject.transform.position = pairedObject.transform.position + Vector3.one * 0.5f;
        return firedObject;
    }


    /// <summary>
    /// Spawns a particle that has a SpringForce component attached. The spring
    /// force component should have its "other" variable set to some transform
    /// in the scene (Hint: You can use the gun object's transform for this!)
    /// </summary>
    /// <returns>The created spring object</returns>
    public GameObject FireStaticSpringWeapon()
    {
        GameObject firedObject = Fire(SpringObject);
        SpringForce spring = firedObject.GetComponent<SpringForce>();
        spring.other = transform;
        return firedObject;
    }

    /// <summary>
    /// Spawns a particle with an AttractorForce and a ForceMouseController
    /// component attached. The force object should be attracted to the mouse
    /// when the left mouse button is held down.
    /// </summary>
    /// <returns>The fired object.</returns>
    public GameObject FireAttractorForceWeapon()
    {
        GameObject firedObject = Fire(ForceObject);

        return firedObject;
    }

    /// <summary>
    /// Spawns a particle with an AttractorForce and a ForceMouseController
    /// component attached. The force object should be repelled by the mouse
    /// when the right mouse button is held down.
    /// </summary>
    /// <returns></returns>
    public GameObject FireRepulsiveForceWeapon()
    {
        GameObject firedObject = Fire(ForceObject);
        firedObject.GetComponent<AttractorForce>().power = -100f;
        firedObject.GetComponent<ForceMouseController>().activationButton = ForceMouseController.MouseButton.RMB;

        return firedObject;
    }

    public Gun()
    {
        weaponsDictionary = new Dictionary<Weapon, System.Func<GameObject>>
        {
            {Weapon.Particle, FireParticle},
            {Weapon.Attractor, FireAttractorForceWeapon },
            {Weapon.Repulsor, FireRepulsiveForceWeapon },
            {Weapon.FixedSpring, FireStaticSpringWeapon },
            {Weapon.PairedSpring, FirePairedSpringWeapon }
        };

        weaponIndex = (int)Weapon.PairedSpring;
    }

    void Update()
    {
        // TODO: YOUR CODE HERE
        if (Camera.main)
        {
            Vector3 screenPos = Mouse.current.position.ReadValue().ToVector3(10f);
            Vector3 pointPos = Camera.main.ScreenToWorldPoint(screenPos);
            transform.up = (pointPos - transform.position);
            reticle.position = pointPos;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Fire();
        }
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            CycleNextWeapon();
        }
    }
}
