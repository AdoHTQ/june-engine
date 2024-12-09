using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class RaymarchRenderer : Node
{
    public static RaymarchRenderer Instance {get; private set;}

    private List<RaymarchPrimitiveInstance3D> primitives;

    private const int WIDTH = 1152;
    private const int HEIGHT = 648;

    public override void _EnterTree()
    {
        Instance = this;
        primitives = new();
        SetupComputeShader();
    }

    public override void _ExitTree()
    {
        CleanupComputeShader();
    }

    public void RegisterPrimitive(RaymarchPrimitiveInstance3D primitive)
    {
        primitives.Add(primitive);
    }

    public void DeregisterPrimitive(RaymarchPrimitiveInstance3D primitive)
    {
        primitives.Remove(primitive);
    }



    private RenderingDevice device;
    private const int dataBufferObjectSize = 4;
    
    private Rid depthTexture = new();
    private Rid shader = new();
    private TextureRect rect;
    private RDUniform depthUniform;    

    private void SetupComputeShader()
    {
        device = RenderingServer.GetRenderingDevice();

        RDShaderFile shaderFile = GD.Load<RDShaderFile>("res://addons/june_renderer/shaders/raymarch.glsl");
        RDShaderSpirV shaderBytecode = shaderFile.GetSpirV();
        shader = device.ShaderCreateFromSpirV(shaderBytecode);

        RenderingDevice.DataFormat dataFormat = RenderingDevice.DataFormat.R32G32B32A32Sfloat;
        RenderingDevice.TextureType textureType = RenderingDevice.TextureType.Type2D;
        RenderingDevice.TextureUsageBits usageBits = RenderingDevice.TextureUsageBits.SamplingBit | 
                                                    RenderingDevice.TextureUsageBits.CanUpdateBit | 
                                                    RenderingDevice.TextureUsageBits.StorageBit | 
                                                    RenderingDevice.TextureUsageBits.CanCopyToBit | 
                                                    RenderingDevice.TextureUsageBits.ColorAttachmentBit;

        RDTextureFormat format = new() {
            Width = WIDTH,
            Height = HEIGHT,
            Format = dataFormat,
            TextureType = textureType,
            UsageBits = usageBits
        };

        depthTexture = device.TextureCreate(format, new(), new());
        device.TextureClear(depthTexture, Colors.Transparent, 0, 1, 0, 1);

        depthUniform = new() {UniformType = RenderingDevice.UniformType.Image, Binding = 0};
        depthUniform.AddId(depthTexture);


        //Create texture preview and set rid
        rect = new TextureRect
        {
            CustomMinimumSize = new Vector2(1152, 648),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest
        };
        AddChild(rect);
        var display = new Texture2Drd
        {
            TextureRdRid = depthTexture,
        };
        rect.Texture = display;
    }

    private void CleanupComputeShader()
    {
        device.FreeRid(depthTexture);
        device.FreeRid(shader);
        rect.QueueFree();
    }

    public override void _Process(double delta)
    {
        Rid pipeline = device.ComputePipelineCreate(shader);

        device.TextureClear(depthTexture, Colors.Transparent, 0, 1, 0, 1);

        //Create input arrays
        List<int> typeInput = new();
        List<float> dataInput = new();
        for (int k = 0; k < primitives.Count; k++)
        {
            RaymarchPrimitiveInstance3D primitive = primitives[k];

            typeInput.Add(primitive.GetID());
            typeInput.Add(dataInput.Count);
            dataInput.AddRange(primitive.GetData());
        }
        if (typeInput.Count == 0) typeInput.Add(-1);
        if (dataInput.Count == 0) dataInput.Add(-1);

        //Build push constant
        float[] pushConstant = 
        {
            0f,
            0f,
            5f,
            0.767326987979f,
            0f,
            0f,
            0f,
            0f
        };

        uint pushConstantSize = (uint)(pushConstant.Length * sizeof(float));
        byte[] pushConstantBytes = new byte[pushConstantSize];
        Buffer.BlockCopy(pushConstant.ToArray(), 0, pushConstantBytes, 0, pushConstantBytes.Length);

        //Format arrays
        byte[] typeInputBytes = new byte[typeInput.Count * sizeof(int)];
        Buffer.BlockCopy(typeInput.ToArray(), 0, typeInputBytes, 0, typeInputBytes.Length);
        Rid objectTypeBuffer = device.StorageBufferCreate((uint)typeInputBytes.Length, typeInputBytes);

        byte[] dataInputBytes = new byte[dataInput.Count * sizeof(float)];
        Buffer.BlockCopy(dataInput.ToArray(), 0, dataInputBytes, 0, dataInputBytes.Length);
        Rid objectDataBuffer = device.StorageBufferCreate((uint)dataInputBytes.Length, dataInputBytes);

        //Create dynamic uniforms
        RDUniform objectTypeUniform = new() {UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = 1};
        objectTypeUniform.AddId(objectTypeBuffer);

        RDUniform objectPositionUniform = new() {UniformType = RenderingDevice.UniformType.StorageBuffer, Binding = 2};
        objectPositionUniform.AddId(objectDataBuffer);

        Rid uniformSet = device.UniformSetCreate(new Array<RDUniform> { depthUniform, objectTypeUniform, objectPositionUniform }, shader, 0);


        long computeList = device.ComputeListBegin();
        device.ComputeListBindComputePipeline(computeList, pipeline);
        device.ComputeListBindUniformSet(computeList, uniformSet, 0);
        device.ComputeListSetPushConstant(computeList, pushConstantBytes, pushConstantSize);
        device.ComputeListDispatch(computeList, WIDTH / 8, HEIGHT / 8, 1);
        device.ComputeListEnd();

        
        // Submit to GPU
        device.Submit();
        
        
        device.FreeRid(pipeline);
        device.FreeRid(uniformSet);
        device.FreeRid(objectTypeBuffer);
        device.FreeRid(objectDataBuffer);

        //Sync at the end of the frame
        device.Sync();
    }
}