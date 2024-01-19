using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Codice.Client.Common.TreeGrouper;

public enum indexPosition
{
    left = 0,
    right = 1,
    down = 0,
    up = 2,
    back = 0,
    front = 4
}

public interface Octree
{
    /// <summary>
    /// Inserts a particle into the octree, descending its children as needed.
    /// </summary>
    /// <param name="particle"></param>
    public void Insert(Sphere particle);

    /// <summary>
    /// Does all necessary collision detection tests.
    /// </summary>
    public void ResolveCollisions();

    /// <summary>
    /// Removes all objects from the Octree.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Creates a new Octree, properly creating children.
    /// </summary>
    /// <param name="pos">The position of this Octree</param>
    /// <param name="halfWidth">The width of this Octree node, from the center to one edge (only needs to be used to calculate children's positions)</param>
    /// <param name="depth">The number of levels beneath this one to create (i.e., depth = 1 means create one node with 8 children. depth = 0 means create only this node. depth = 2 means create one node with 8 children, each of which are Octree's with depth 1.</param>
    /// <returns>The newly created Octree</returns>
    public static Octree Create(Vector3 pos, float halfWidth = 1f, uint depth = 1)
    {
        float childHalf = halfWidth / 2;
        if (depth == 0)
        {
            var node = new OctreeObjects();

            return node;
        }
        if (depth != 0)
        {
            var node = new OctreeNode();
            node.position = pos;
            for (int i = 0; i < 8; i++)
            {
                BitArray bits = new BitArray(new int[] { i });

                int xScalar = bits[0] ? 1 : -1;
                int yScalar = bits[1] ? 1 : -1;
                int zScalar = bits[2] ? 1 : -1;

                Vector3 childOffSet = new Vector3(xScalar * childHalf, yScalar * childHalf, zScalar * childHalf);

                Vector3 childPosition = pos + childOffSet;

                node.children[i] = Create(childPosition, childHalf, depth - 1);
            }
            return node;
        }
        return null;
    }
}

/// <summary>
/// An octree that holds 8 children, all of which are Octree's.
/// </summary>
public class OctreeNode : Octree
{
    public Vector3 position;
    public Octree[] children;

    public OctreeNode()
    {
        position = new Vector3();
        children = new Octree[8];
    }

    public OctreeNode(Vector3 position, Octree[] children)
    {
        this.position = position;
        this.children = children;
    }

    /// <summary>
    /// Inserts the given particle into the appropriate children. The particle
    /// may need to be inserted into more than one child.
    /// </summary>
    /// <param name="sphere">The bounding sphere of the particle to insert.</param>
    public void Insert(Sphere sphere)
    {
        Vector3 relativePosition = sphere.position - position;
        int index = 0;

        //insert sphere into child it is contained within
        index += (relativePosition.x > 0) ? 1 : 0;
        index += (relativePosition.y > 0) ? 2 : 0;
        index += (relativePosition.z > 0) ? 4 : 0;

        children[index].Insert(sphere);

        BitArray indexPosition = new BitArray(new int[] { index });

        //determine where (if any) the overlaps are
        indexPosition[0] = sphere.Radius > Mathf.Abs(relativePosition.x); //zy
        indexPosition[1] = sphere.Radius > Mathf.Abs(relativePosition.y); //zx
        indexPosition[2] = sphere.Radius > Mathf.Abs(relativePosition.z); //yx

        if (indexPosition[0]) //1
        {
            children[1].Insert(sphere);
        }
        if (indexPosition[1]) //2
        {
            children[2].Insert(sphere);
        }
        if (indexPosition[0] && indexPosition[1]) //3
        {
            children[3].Insert(sphere);
        }
        if (indexPosition[2]) //4
        {
            children[4].Insert(sphere);
        }
        if (indexPosition[2] && indexPosition[0]) //5
        {
            children[5].Insert(sphere);
        }
        if (indexPosition[2] && indexPosition[1]) //6
        {
            children[6].Insert(sphere);
        }
        if (indexPosition[2] && indexPosition[1] && indexPosition[0]) //7
        {
            children[7].Insert(sphere);
        }
    }

    /// <summary>
    /// Resolves collisions in all children, as only leaf nodes can hold particles.
    /// </summary>
    public void ResolveCollisions()
    {
        foreach (var child in children)
        {
            child.ResolveCollisions();
        }
    }

    /// <summary>
    /// Removes all particles in each child.
    /// </summary>
    public void Clear()
    {
        foreach (var child in children)
        {
            child.Clear();
        }

    }
}

/// <summary>
/// An octree that holds only particles.
/// </summary>
public class OctreeObjects : Octree
{
    HashSet<Sphere> children = new HashSet<Sphere>();

    public ICollection<Sphere> Objects
    {
        get
        {
            return children;
        }
    }

    /// <summary>
    /// Inserts the particle into this node. It will be compared with all other
    /// particles in this node in ResolveCollisions().
    /// </summary>
    /// <param name="particle">The particle to insert.</param>
    public void Insert(Sphere particle)
    {
        children.Add(particle);
    }

    /// <summary>
    /// Calls CollisionDetection.ApplyCollisionResolution() on every pair of
    /// spheres in this node.
    /// </summary>
    public void ResolveCollisions()
    {
        foreach (var sphere1 in children)
        {
            foreach (var sphere2 in children)
            {
                if (sphere1 != sphere2) CollisionDetection.ApplyCollisionResolution(sphere1, sphere2);
            }
        }
    }

    /// <summary>
    /// Removes all objects from this node.
    /// </summary>
    public void Clear()
    {
        children.Clear();
    }
}
