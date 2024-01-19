using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using static TestHelpers;
using System.Collections.Generic;

public class MovementTest : InputTestFixture
{
    public static int[] dummyData = { 0 };
    [Test]
    public void OctreeCreateTest([ValueSource("dummyData")] int _)
    {
        {
            Octree newTree = Octree.Create(Vector3.zero, 1, 0);
            Assert.That(newTree.GetType(), Is.EqualTo(typeof(OctreeObjects)), "Octree of depth 0 is of the wrong type.");
        }

        {
            Octree newTree = Octree.Create(Vector3.zero, 1, 2);
            Assert.That(newTree.GetType(), Is.EqualTo(typeof(OctreeNode)), "Octree of depth 1 is of the wrong type.");
            OctreeNode node = newTree as OctreeNode;
            Assert.That(node.children.Length, Is.EqualTo(8), "Octree Node has wrong number of children.");

            HashSet<Vector3> correctPositions = new HashSet<Vector3>();

            for (int i = 0; i < 8; i++)
            {
                float step = 0.5f;
                Vector3 offset = Vector3.zero;
                offset += Vector3.right * (((i & 1) != 0) ? step : -step);
                offset += Vector3.up * (((i & 2) != 0) ? step : -step);
                offset += Vector3.forward * (((i & 4) != 0) ? step : -step);
                correctPositions.Add(offset);
            }

            HashSet<Vector3> foundPositions = new HashSet<Vector3>();

            for (int i = 0; i < 8; i++)
            {
                Assert.That(node.children[i].GetType(), Is.EqualTo(typeof(OctreeNode)), "Octree of depth 2 has children of wrong type.");
                OctreeNode child = node.children[i] as OctreeNode;
                foundPositions.Add(child.position);
            }

            Assert.That(foundPositions.SetEquals(correctPositions), "Children positions aren't all correct.");

            for (int i = 0; i < 8; i++)
            {
                OctreeNode child = node.children[i] as OctreeNode;
                Assert.That(child.children[i].GetType(), Is.EqualTo(typeof(OctreeObjects)), "Octree of depth 2's children's children are wrong type.");
            }
        }
    }

    [Test]
    public void OctreeObjectsTest([ValueSource("dummyData")] int _)
    {
        Sphere s1 = new GameObject().AddComponent<Sphere>();
        Sphere s2 = new GameObject().AddComponent<Sphere>();

        s1.gameObject.AddComponent<Particle3D>();
        s2.gameObject.AddComponent<Particle3D>();

        s1.position = Vector3.right * 0.5f;
        s1.invMass = 1f;
        s2.invMass = 1f;
        s1.Radius = 1f;
        s2.Radius = 1f;

        Octree tree = new OctreeObjects();
        tree.Insert(s1);
        tree.Insert(s2);

        tree.ResolveCollisions();

        Assert.That(s1.position, Is.EqualTo(Vector3.right * 1.25f), "OctreeObjects did not resolve collisions correctly. s1 final position is wrong.");
        Assert.That(s2.position, Is.EqualTo(Vector3.right * -0.75f), "OctreeObjects did not resolve collisions correctly. s2 final position is wrong.");

        s1.position = Vector3.right * 0.5f;
        s2.position = Vector3.zero;

        tree.Clear();

        tree.ResolveCollisions();

        Assert.That(s1.position, Is.EqualTo(Vector3.right * 0.5f), "Clear() failed to prevent collision resolution");
        Assert.That(s2.position, Is.EqualTo(Vector3.zero), "Clear() failed to prevent collision resolution");
    }

    [Test]
    public void OctreeNodeTest([ValueSource("dummyData")] int _)
    {
        OctreeNode tree = new OctreeNode();
        Assert.That(tree.children.Length, Is.EqualTo(8), "OctreeNode has wrong number of children");
        for (int i = 0; i < 8; i++)
        {
            tree.children[i] = new OctreeObjects();
        }
        Sphere s1 = new GameObject().AddComponent<Particle3D>().gameObject.AddComponent<Sphere>();
        tree.Insert(s1);

        foreach(Octree child in tree.children)
        {
            OctreeObjects objects = child as OctreeObjects;
            ICollection<Sphere> spheres = objects.Objects;
            Assert.That(spheres.Contains(s1), "Sphere is missing from a child of an OctreeNode");
        }

        tree.Clear();

        foreach (Octree child in tree.children)
        {
            OctreeObjects objects = child as OctreeObjects;
            ICollection<Sphere> spheres = objects.Objects;
            Assert.That(spheres.Contains(s1), Is.False, "Clear() did not remove child spheres");
        }

        s1.position = Vector3.one * 2f;

        tree.Insert(s1);

        {
            uint numChildren = 0;
            foreach (Octree child in tree.children)
            {
                OctreeObjects objects = child as OctreeObjects;
                ICollection<Sphere> spheres = objects.Objects;
                if (spheres.Contains(s1))
                {
                    numChildren++;
                }
            }
            Assert.That(numChildren, Is.EqualTo(1), "Inserted sphere should be in one child.");
        }

        tree.Clear();

        s1.Radius = 1f;
        s1.invMass = 1f;
        Sphere s2 = new GameObject().AddComponent<Particle3D>().gameObject.AddComponent<Sphere>();

        s2.position = Vector3.one * 1.5f;
        s2.Radius = 1f;
        s2.invMass = 1f;

        tree.Insert(s1);
        tree.Insert(s2);
        tree.ResolveCollisions();

        Assert.That(s1.position, Is.EqualTo(new Vector3(2.32735f, 2.32735f, 2.32735f)).Using(Vec3Comparer), "ResolveCollision() failed between spheres in same node");
        Assert.That(s2.position, Is.EqualTo(new Vector3(1.17265f, 1.17265f, 1.17265f)).Using(Vec3Comparer), "ResolveCollision() failed between spheres in same node");

        tree.Clear();

        s1.position = new Vector3(1f, 1f, 0f);
        s2.position = new Vector3(1f, .4f, 0f);

        tree.Insert(s1);
        tree.Insert(s2);
        tree.ResolveCollisions();

        Assert.That(s1.position, Is.EqualTo(new Vector3(1f, 1.7f, 0f)).Using(Vec3Comparer), "ResolveCollision() failed between spheres in same node");
        Assert.That(s2.position, Is.EqualTo(new Vector3(1f, -0.3f, 0f)).Using(Vec3Comparer), "ResolveCollision() failed between spheres in same node");
    }
}
