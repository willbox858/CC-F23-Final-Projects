using System.Collections;
using System.Collections.Generic;
using static CollisionDetection;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollisionManager : MonoBehaviour
{
    Octree tree;
    private List<Sphere> mSpheres = new List<Sphere>();

    public enum CollisionType
    {
        Standard,
        Octree
    }

    public static CollisionType collisionType = CollisionType.Octree;

    [SerializeField]
    public uint nStartingParticles = 100;

    [SerializeField]
    private GameObject particlePrefab;


    private void Start()
    {
        tree = Octree.Create(Vector3.zero, 5, 3);

        for (int i = 0; i < nStartingParticles; i++)
        {
            var sphereGO = Instantiate(particlePrefab);
            sphereGO.transform.position = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f));

            var sphere = sphereGO.GetComponent<Sphere>();
            sphere.velocity = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f));

            mSpheres.Add(sphere);
            tree.Insert(sphere);
        }
    }

    private void TreeCollisionResolution()
    {
        tree.ResolveCollisions();

        PlaneCollider[] planes = FindObjectsOfType<PlaneCollider>();
        for (int i = 0; i < mSpheres.Count; i++)
        {
            Sphere sphere = mSpheres[i];
            foreach (PlaneCollider plane in planes)
            {
                ApplyCollisionResolution(sphere, plane);
            }
        }
    }

    private void StandardCollisionResolution()
    {
        Sphere[] spheres = FindObjectsOfType<Sphere>();
        PlaneCollider[] planes = FindObjectsOfType<PlaneCollider>();
        for (int i = 0; i < spheres.Length; i++)
        {
            Sphere s1 = spheres[i];
            for (int j = i + 1; j < spheres.Length; j++)
            {
                Sphere s2 = spheres[j];
                ApplyCollisionResolution(s1, s2);
            }
            foreach (PlaneCollider plane in planes)
            {
                ApplyCollisionResolution(s1, plane);
            }
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks = 0;

        //foreach(Sphere sphere in mSpheres)
        //{
        //    tree.Insert(sphere);
        //}
        switch (collisionType)
        {
            case CollisionType.Standard:
                StandardCollisionResolution();
                break;
            case CollisionType.Octree:
                TreeCollisionResolution();
                break;
        }
        //tree.Clear();
    }

    private void Update()
    {
        if (Keyboard.current.cKey.IsPressed())
        {
            if (collisionType == CollisionType.Standard) collisionType = CollisionType.Octree;
            else collisionType = CollisionType.Standard;
        }
    }
}
