using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used to test the various components of the Space-Filling Curve Algorithm on the CPU side
public class InterleaveTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float SpatialLength = 25.0f;
        int numberOfPoints = 100;
        int K = 2;

        List<Vector3> points = GenerateRandomPoints(numberOfPoints, SpatialLength);
        List<Vector3Int> sfcPoints = new List<Vector3Int>();
        List<uint> interleavedPoints = new List<uint>();

        foreach(var point in points)
        {
            sfcPoints.Add(sfcPoint(point, SpatialLength, K));
        }

        foreach(var point in sfcPoints)
        {
            interleavedPoints.Add(InterleaveBits(point));
        }

        OutputUintList(interleavedPoints);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3Int sfcPoint(Vector3 p, float _SpaceLength, int K)
    {
        //translate the x,y,z components of our vector into integers
        int xInt = (int)(Mathf.Pow(2, K) * p.x / _SpaceLength);
        int yInt = (int)(Mathf.Pow(2, K) * p.y / _SpaceLength);
        int zInt = (int)(Mathf.Pow(2, K) * p.z / _SpaceLength);

        //initializing the new point
        Vector3Int sfcPoint = new Vector3Int(xInt, yInt, zInt);
        return sfcPoint;
    }

    uint InterleaveBits(Vector3Int temp)
    {
        //break it up into individual values
        int x = temp.x;
        int y = temp.y;
        int z = temp.z;

        //The mask is used to isolate the last 2 bits of each number.
        uint mask = (1 << 2) - 1;

        //Isolate last two bits
        uint lastXBits = (uint)x & mask;
        uint lastYBits = (uint)y & mask;
        uint lastZBits = (uint)z & mask;

        //Shift everything into position for combining together
        lastXBits <<= 4;
        lastYBits <<= 2;
        lastZBits <<= 0;

        //Interleave the first 2 bits of each value
        uint Result = 0;

        Result |= lastXBits;
        Result |= lastYBits;
        Result |= lastZBits;

        return Result;
    }

    List<Vector3> GenerateRandomPoints(int numberOfPoints,float euclideanLength)
    {
        List<Vector3> points = new List<Vector3>();
        for(int i = 0; i < numberOfPoints; i++)
        {
            points.Add(new Vector3(Random.Range(0.0f, euclideanLength), Random.Range(0.0f, euclideanLength), Random.Range(0.0f, euclideanLength)));
        }
        return points;
    }

    void OutputUintList(List<uint> list)
    {
        foreach(uint x in list)
        {
            Debug.Log(x);
        }
    }
}
