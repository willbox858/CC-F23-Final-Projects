using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderRunner : MonoBehaviour
{
    [SerializeField] Color mWallColor;
    [SerializeField] Color mEmptyColor;
    [SerializeField] ComputeShader mComputeShader;
    [SerializeField] int mSize;

    private float mSeed;

    RenderTexture mComputeOutput;

    // Start is called before the first frame update
    void Start()
    {
        mSeed = Random.value;

        mComputeOutput = new RenderTexture(mSize, mSize, 24);
        mComputeOutput.filterMode = FilterMode.Point;
        mComputeOutput.enableRandomWrite = true;
        mComputeOutput.Create();

        //Set values
        mComputeShader.SetFloat("_Seed", mSeed);
        mComputeShader.SetFloat("_Resolution",mComputeOutput.width);
        mComputeShader.SetVector("_WallColor", mWallColor);
        mComputeShader.SetVector("_EmptyColor", mEmptyColor);

        var main = mComputeShader.FindKernel("CSMain");
        mComputeShader.SetTexture(main,"Result",mComputeOutput);
        mComputeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
        mComputeShader.Dispatch(main, mComputeOutput.width / (int)xGroupSize, mComputeOutput.height / (int)yGroupSize, 1);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(mComputeOutput, destination);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
