using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeComputeShaderRunner : MonoBehaviour
{
    [SerializeField] ComputeShader mComputeShader;

    [SerializeField] RenderTexture mRenderTexture;
    [SerializeField] int mSize;
    int mCachedSize;
    [SerializeField] Color mWallColor = Color.cyan;
    [SerializeField] Color mEmptyColor = Color.black;
    Color mCachedWallColor;
    Color mCachedEmptyColor;
    [SerializeField, Range(0, 1000)] int mSeed;
    int mCachedSeed;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Preparing mRenderTexture");

        mRenderTexture = new RenderTexture(mSize, mSize, 24);
        mRenderTexture.filterMode = FilterMode.Point;
        mRenderTexture.enableRandomWrite = true;
        if (mRenderTexture.Create()) Debug.Log("Texture created successfully");

        GenerateMaze();
    }

    // Update is called once per frame
    void Update()
    {
        if(mCachedSize != mSize || mCachedWallColor != mWallColor || mCachedSeed != mSeed || mCachedEmptyColor != mEmptyColor)
        {
            LoadTexture();
            GenerateMaze();
        }
    }

    void LoadTexture()
    {
        mRenderTexture = new RenderTexture(mSize, mSize, 24);
        mRenderTexture.filterMode = FilterMode.Point;
        mRenderTexture.enableRandomWrite = true;
        if (mRenderTexture.Create()) Debug.Log("Texture created successfully");
    }

    void GenerateMaze()
    {
        Debug.Log("Beginning Maze generation");

        Debug.Log("Setting values");
        mComputeShader.SetInt("_Resolution", mRenderTexture.width);
        mComputeShader.SetInt("_Seed", mSeed);
        mComputeShader.SetVector("_WallColor", mWallColor);
        mComputeShader.SetVector("_EmptyColor", mEmptyColor);

        bool prepassFound = mComputeShader.HasKernel("Prepass");
        bool cSMainFound = mComputeShader.HasKernel("CSMain");

        if (prepassFound && cSMainFound)
        {
            Debug.Log("Finding prepass");
            var prepass = mComputeShader.FindKernel("Prepass");
            mComputeShader.SetTexture(prepass, "Result", mRenderTexture);
            mComputeShader.Dispatch(prepass, mRenderTexture.width / 8, mRenderTexture.height / 8, 1);

            Debug.Log("Finding CSMain");
            var main = mComputeShader.FindKernel("CSMain");
            mComputeShader.SetTexture(main, "Result", mRenderTexture);
            mComputeShader.Dispatch(main, mRenderTexture.width / 8, mRenderTexture.height / 8, 1);
        }
        else
        {
            if (!prepassFound) Debug.Log("Prepass wasn't found");
            if (!cSMainFound) Debug.Log("CSMain wasn't found");
        }

        Debug.Log("Setting seed, wall, and empty colors");
        mCachedSize = mSize;
        mCachedSeed = mSeed;
        mCachedWallColor = mWallColor;
        mCachedEmptyColor = mEmptyColor;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(mRenderTexture, destination);
    }
}
