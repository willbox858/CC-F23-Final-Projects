using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static CollisionDetection;
using UnityEngine;

public class OctreeShaderDispatcher : MonoBehaviour
{
    private List<Sphere> mSpheres = new List<Sphere>();

    [SerializeField]
    public int nStartingParticles = 100;

    [SerializeField]
    private GameObject particlePrefab;

    [SerializeField]
    private ComputeShader mComputeShader;

    [SerializeField]
    private float mEuclideanLength;

    [SerializeField]
    private int K = 2;
    [SerializeField]
    private int closenessThreshold = 1;

    ComputeBuffer mPositionBuffer;
    string mPositionBufferName = "_Positions";
    ComputeBuffer mSFCBuffer;
    string mSFCPointsName = "_SFCPoints";
    ComputeBuffer mMortonNumbersBuffer;
    string mMortonNumbersName = "_InterleavedPoints";

    struct float3
    {
        public float x;
        public float y;
        public float z;
    }

    struct int3
    {
        public int x;
        public int y;
        public int z;
    }

    struct MortonIndexPair
    {
        public int index;
        public uint morton;

        public static bool Compare(MortonIndexPair a, MortonIndexPair b)
        {
            return a.morton < b.morton;
        }
        public static bool operator <(MortonIndexPair a,MortonIndexPair b)
        {
            return a.morton < b.morton;
        }
        public static bool operator >(MortonIndexPair a, MortonIndexPair b)
        {
            return a.morton > b.morton;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < nStartingParticles; i++)
        {
            var sphereGO = Instantiate(particlePrefab);
            sphereGO.transform.position = new Vector3(Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));

            var sphere = sphereGO.GetComponent<Sphere>();
            sphere.velocity = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f));

            mSpheres.Add(sphere);
        }
        CreateBuffers();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CollisionChecks = 0;

        //pass particle positions into the _Positions buffer
        LoadData();

        //dispatch CSSpaceFillingCurve kernel to populate the _SFCPoints buffer
        DispatchCSSpaceFillingCurve();

        //dispatch CSInterleaveBits kernal to get the Morton Number of each SFC-Point.
        DispatchCSInterleaveBits();

        //Wait for GPU to finish work

        //Iterate over the Morton Number buffer to find matching numbers
        //Matching numbers means colliding particles means test collision of particles
        //CheckMortonNumbers();
        NewMortonCheck();

        //Test for Sphere-Plane collisions
        SpherePlaneCheck();
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void ReleaseBuffers()
    {
        if(mPositionBuffer != null)
        {
            mPositionBuffer.Release();
            mPositionBuffer = null;
        }
        if(mSFCBuffer != null)
        {
            mSFCBuffer.Release();
            mSFCBuffer = null;
        }
        if(mMortonNumbersBuffer != null)
        {
            mMortonNumbersBuffer.Release();
            mMortonNumbersBuffer = null;
        }
    }

    private void SpherePlaneCheck()
    {
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

    private void CreateBuffers()
    {
        mPositionBuffer = CreateStructuredBuffer<float3>(nStartingParticles);
        mSFCBuffer = CreateStructuredBuffer<int3>(nStartingParticles);
        mMortonNumbersBuffer = CreateStructuredBuffer<uint>(nStartingParticles);
    }

    private void LoadData()
    {
        mComputeShader.SetInt("_NumberOfParticles",nStartingParticles);
        mComputeShader.SetFloat("_SpaceLength", mEuclideanLength);

        List<float3> positions = SpherePositionsToFloat3Array();
        mPositionBuffer.SetData(positions);

        List<int3> sfcPoints = new List<int3>(new int3[nStartingParticles]);
        mSFCBuffer.SetData(sfcPoints);

        List<uint> mortonNumbers = new List<uint>(new uint[nStartingParticles]);
        mMortonNumbersBuffer.SetData(mortonNumbers);
    }
    List<float3> SpherePositionsToFloat3Array()
    {
        List<float3> tempList = new List<float3>();
        foreach(var sphere in mSpheres)
        {
            float3 temp;
            temp.x = sphere.position.x;
            temp.y = sphere.position.y;
            temp.z = sphere.position.z;
            tempList.Add(temp);
        }
        return tempList;
    }
    private void DispatchCSSpaceFillingCurve()
    {
        EasyDispatch("CSSpaceFillingCurve", nStartingParticles, mPositionBufferName, mPositionBuffer,mSFCPointsName,mSFCBuffer);
    }
    private void DispatchCSInterleaveBits()
    {
        EasyDispatch("CSInterleaveBits", nStartingParticles, mSFCPointsName, mSFCBuffer,mMortonNumbersName,mMortonNumbersBuffer);
    }

    //Gotta be a better way to structure this than a TRIPLE for-loop
    private void CheckMortonNumbers()
    {
        uint[] outputMortons = new uint[nStartingParticles];
        mMortonNumbersBuffer.GetData(outputMortons);
        List<uint> mortonsList = new List<uint>(outputMortons);
        List<MortonIndexPair> pairs = new List<MortonIndexPair>();
        for(int i = 0; i < nStartingParticles; i++)
        {
            MortonIndexPair temp = new MortonIndexPair();
            temp.index = i;
            temp.morton = mortonsList[i];

            pairs.Add(temp);
        }

        pairs = pairs.OrderBy(pair => pair.morton).ToList();
        Debug.Log(pairs.Count);

        for (int i = 0; i < nStartingParticles-1;i++)
        {
            for(int j = i+1; j < nStartingParticles; j++)
            {
                var currentPair = pairs[i];
                var nextPair = pairs[j];

                if (Mathf.Abs(currentPair.morton - nextPair.morton) < closenessThreshold)
                {
                    Debug.Log("mortons too close!");
                    ApplyCollisionResolution(mSpheres[currentPair.index], mSpheres[nextPair.index]);
                }
            }
        }

        //foreach(uint morton in mortonsList)
        //{
        //    List<int> indexes = mortonsList
        //        .Select((value, index) => new { value, index })
        //        .Where(pair => pair.value == morton)
        //        .Select(pair => pair.index)
        //        .ToList();

        //    foreach(int index1 in indexes)
        //    {
        //        foreach(int index2 in indexes)
        //        {
        //            if (index1 != index2) ApplyCollisionResolution(mSpheres[index1], mSpheres[index2]); 
        //        }
        //    }
        //}

        
    }

    private void NewMortonCheck()
    {
        uint[] outputMortons = new uint[nStartingParticles];
        mMortonNumbersBuffer.GetData(outputMortons);
        List<uint> mortonsList = new List<uint>(outputMortons);
        List<uint> sortedMortons = new List<uint>(outputMortons);

        Dictionary<uint, List<int>> mortonIndexHash = new Dictionary<uint, List<int>>(); 
        for(int i = 0; i < nStartingParticles; i++)
        {
            if (!mortonIndexHash.ContainsKey(mortonsList[i]))
            {
                List<int> temp = new List<int>();
                temp.Add(i);
                mortonIndexHash.Add(mortonsList[i], temp);
            }
            else if (mortonIndexHash.ContainsKey(mortonsList[i]))
            {
                mortonIndexHash[mortonsList[i]].Add(i);
            }
        }
        foreach(var hash in mortonIndexHash)
        {
            foreach(int index1 in hash.Value)
            {
                foreach (int index2 in hash.Value)
                {
                    if (index1 != index2) ApplyCollisionResolution(mSpheres[index1], mSpheres[index2]);
                }
            }
        }
    }

    List<uint> GenerateMortonCheck()
    {
        List<uint> outputMortons = new List<uint>();
        for(int i = 0; i < Mathf.Pow(2,(3*K));i++)
        {
            outputMortons.Add((uint)i);
        }
        return outputMortons;
    }
    
    static public int GetStride<T>()
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }

    private ComputeBuffer CreateStructuredBuffer<T>(int count)
    {
        return new ComputeBuffer(count, GetStride<T>());
    }

    private void EasyDispatch(string kernel,int iterations,string inputBufferName,ComputeBuffer inputBuffer, string outputBufferName, ComputeBuffer outputBuffer)
    {
        if (mComputeShader.HasKernel(kernel))
        {
            var kernelIndex = mComputeShader.FindKernel(kernel);

            Vector3Int threadGroupSizes = GetThreadGroupSize(mComputeShader, kernelIndex);
            Vector3Int dispatchDimensions = DispatchDimensions(threadGroupSizes, iterations);

            EasyDispatch(kernel, dispatchDimensions.x, dispatchDimensions.y, dispatchDimensions.z, inputBufferName, inputBuffer, outputBufferName, outputBuffer);
        }
        else
        {
            Debug.Log(kernel + " was not found!");
        }
        
    }

    private void EasyDispatch(string kernel,int computeX,int ComputeY,int computeZ, string inputBufferName, ComputeBuffer inputBuffer, string outputBufferName, ComputeBuffer outputBuffer)
    {
        if(mComputeShader.HasKernel(kernel))
        {
            var kernelIndex = mComputeShader.FindKernel(kernel);
            //set buffer
            mComputeShader.SetBuffer(kernelIndex, inputBufferName,inputBuffer);
            mComputeShader.SetBuffer(kernelIndex, outputBufferName,outputBuffer);
            //dispatch kernel
            mComputeShader.Dispatch(kernelIndex, computeX, ComputeY, computeZ);
        }
        else
        {
            Debug.Log(kernel + " was not found!");
        }
    }

    Vector3Int GetThreadGroupSize(ComputeShader compute, int kernelIndex)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int) x, (int) y, (int) z);
    }

    Vector3Int DispatchDimensions(Vector3Int threadGroupDimensions, int iterationsX,int iterationsY = 1,int iterationsZ = 1)
    {
        Vector3Int output = new Vector3Int(0,0,0);
        output.x = Mathf.CeilToInt((float)iterationsX / (float)threadGroupDimensions.x);
        output.y = Mathf.CeilToInt((float)iterationsY / (float)threadGroupDimensions.y);
        output.z = Mathf.CeilToInt((float)iterationsZ / (float)threadGroupDimensions.z);
        return output;
    }
}
