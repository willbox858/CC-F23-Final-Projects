using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static MovementTest;

public class SceneTests : InputTestFixture
{
    Mouse mouse { get => Mouse.current; }
    const string testSceneId = "Assets/Scenes/TestScene.unity";

    public override void Setup()
    {
        base.Setup();
        InputSystem.AddDevice<Keyboard>();
        InputSystem.AddDevice<Mouse>();
        SceneManager.LoadScene(testSceneId);
    }

    // Test parser expects all tests to be iterated, so we add this
    // variable to make each test run once.
    public static int[] dummyData = new int[] { 0 };

    [UnityTest]
    public IEnumerator SceneTest([ValueSource("dummyData")] int _)
    {
        CollisionManager collisionManager = Object.FindObjectOfType<CollisionManager>();
        Sphere[] spheres = Object.FindObjectsOfType<Sphere>();
        Assert.That(collisionManager.nStartingParticles, Is.EqualTo(spheres.Length), "Starting number of spheres is not the number requested in CollisionManager.");
        Assert.That(CollisionManager.collisionType, Is.EqualTo(CollisionManager.CollisionType.Standard), "Scene starts in Octree collision, but should start in Standard");
        yield return null;

        int normalCollisions = CollisionDetection.CollisionChecks;

        Press(Keyboard.current.cKey);

        yield return null;
        yield return new WaitForFixedUpdate();

        Assert.That(CollisionManager.collisionType, Is.EqualTo(CollisionManager.CollisionType.Octree), "Scene does not switch to Octree collision after 'c' is pressed.");

        int octreeCollisions = CollisionDetection.CollisionChecks;

        Assert.That(octreeCollisions, Is.LessThan(normalCollisions * .5f), "Octree collision mode makes too many collision checks");
    }
}
