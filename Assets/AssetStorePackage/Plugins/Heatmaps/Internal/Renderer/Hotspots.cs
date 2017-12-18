using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Video;
using UnityEngine.Assertions;
using System.IO;
using UnityAnalytics360VideoHeatmap;
using UnityEngine.Analytics;

[ExecuteInEditMode]
public class Hotspots : MonoBehaviour 
{
    // Try resource.loadAsset - if not, user has to assign
	public ComputeShader shader;

    // Default, but allow user to define
    // governs fidelity of heatmap render - needs to be same asepct ratio as video
	public int heatMapWidth = 1024;
	public int heatMapHeight = 512;

    //max number of points that can be displayed
	[Range(1, 2000)]
	public int numPoints = 500;

    int currPoints = 0;

    //can be private
	public Texture2D gradient;
    //number of colors in gradient - more is smoother, less you get more banding
	[Range(1, 256)]
	public int gradientSize = 16;

    //can be private
	public RenderTexture renderTexture;
    //dev needs to set to be same as one in video
	public Material videoMaterial;
    //hidden
	public RenderTexture composite;

    //hidden
    public Vector2[] points;
    public Vector2[] props;
	
	int kernelHeatmap;

    ComputeBuffer pointsBuffer;
	ComputeBuffer propsBuffer;
	ComputeBuffer argsBuffer;
	
    // already have controls from heatmap UI
	[Range(1, 50)]
	public float minRadius;
	[Range(1, 50)]
	public float maxRadius;

	[Range(0, 1)]
	public float minIntensity = 0;
	[Range(0, 1)]
	public float maxIntensity = 1;

    [Range(0, 1)]
    public float decay = 0.5f;

    // can be exposed a la heatmap UI
    public Gradient _gradient = new Gradient();

	bool inited = false;

	// Use this for initialization
	void Start () 
	{
		Init();
	}

    private void OnEnable()
    {
        Init();
    }

    public void Init(bool force = false)
	{
        var _player = HeatmapViewModel.videoPlayer;
        if (inited == false && _player != null && videoMaterial != null && _gradient != null)
		{
			inited = true;
				
			Assert.IsTrue(SystemInfo.supportsComputeShaders);

            if (_player != null)
			{
                renderTexture = new RenderTexture(heatMapWidth, heatMapHeight, 0, RenderTextureFormat.RFloat);
                renderTexture.enableRandomWrite = true;
                renderTexture.Create();

                composite = new RenderTexture(heatMapWidth, heatMapHeight, 0, RenderTextureFormat.ARGBFloat);
                composite.enableRandomWrite = true;
                composite.Create();

                //set up the default gradient
                SetUpDefaultGradient();

                gradient = new Texture2D(gradientSize, gradientSize, TextureFormat.RGBA32, false);
                gradient.wrapMode = TextureWrapMode.Clamp;
                gradient.filterMode = FilterMode.Point;
                for (var y = 0; y < gradientSize; ++y)
                    for (var x = 0; x < gradientSize; ++x)
                        gradient.SetPixel(x, y, _gradient.Evaluate((float)x / (float)gradientSize));
                gradient.Apply();

                points = new Vector2[numPoints];
                props = new Vector2[numPoints];

                shader = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath("ff969f478115f472980c928bc942fa56"));
                kernelHeatmap = shader.FindKernel("CSVRHeatMap");

                pointsBuffer = new ComputeBuffer(numPoints, 2 * 4);
                propsBuffer = new ComputeBuffer(numPoints, 2 * 4);
                argsBuffer = new ComputeBuffer(3, 4);
			}
		}
	}

    public void SetUpDefaultGradient ()
    {
        GradientColorKey[] gck = new GradientColorKey[4];
        gck[0].color = Color.white;
        gck[0].time = 0.0f;
        gck[1].color = new Color(NormalizeColorVal(56f), NormalizeColorVal(90f), NormalizeColorVal(255f));
        gck[1].time = 0.33f;
        gck[2].color = new Color(NormalizeColorVal(255f), NormalizeColorVal(255f), 0);
        gck[2].time = 0.66f;
        gck[3].color = new Color(NormalizeColorVal(255f), 0, 0);
        gck[3].time = 1.0f;
        GradientAlphaKey[] gak = new GradientAlphaKey[4];
        gak[0].alpha = 0.0f;
        gak[0].time = 0.0f;
        gak[1].alpha = 1.0f;
        gak[1].time = 0.33f;
        gak[2].alpha = 1.0f;
        gak[2].time = 0.66f;
        gak[3].alpha = 1.0f;
        gak[3].time = 1.0f;
        _gradient.SetKeys(gck, gak);
    }

    float NormalizeColorVal (float val)
    {
        return val / 255f;
    }
	
	// Update is called once per frame
	public void Update () 
	{
		Init();
	}

	public void SetPoints(HeatPoint[] heatPoints)
	{
        if (heatPoints != null && heatPoints.Length > 0 && renderTexture != null)
		{
            currPoints = heatPoints.Length;
            points = new Vector2[currPoints];
            props  = new Vector2[currPoints];

            for (var i = 0; i < currPoints; ++i)
			{
				var hp = heatPoints[i];
                points[i] = ProjectEquirectangular(hp.rotation);
				props[i]  = new Vector2(maxRadius, hp.density);
			}
		}
        RenderHeatmap();
	}

	void RenderHeatmap()
	{
        if (pointsBuffer == null || propsBuffer == null || renderTexture == null || composite == null || HeatmapViewModel.videoPlayer == null || HeatmapViewModel.videoPlayer.targetTexture == null || gradient == null || videoMaterial == null || gradientSize == 0)
            return;

        shader.SetBuffer(kernelHeatmap, "_points", pointsBuffer);
        shader.SetBuffer(kernelHeatmap, "_properties", propsBuffer);
        shader.SetInt("_numPoints", currPoints);
        shader.SetFloat("_decay", decay);
        shader.SetTexture(kernelHeatmap, "_result", renderTexture);
        shader.SetTexture(kernelHeatmap, "_composite", composite);
        shader.SetTexture(kernelHeatmap, "_video", HeatmapViewModel.videoPlayer.targetTexture);
        shader.SetTexture(kernelHeatmap, "_gradient", gradient);

        if (videoMaterial != null)
        {
            videoMaterial.SetTexture("_HeatmapTex", renderTexture);
            videoMaterial.SetTexture("_HeatmapGradient", gradient);
        }


        if (points != null && props != null && pointsBuffer != null && propsBuffer != null && argsBuffer != null)
        {
            pointsBuffer.SetData(points);
            propsBuffer.SetData(props);
            argsBuffer.SetData(new int[3] { heatMapWidth / 8, heatMapHeight / 8, 1 });
        }
		if (argsBuffer != null)
		{
        	shader.DispatchIndirect(kernelHeatmap, argsBuffer, 0);		
		}

        EditorUtility.SetDirty(this);
	}

	Vector2 ProjectEquirectangular(Vector3 coords)
	{
		Vector3 normalizedCoords = Vector3.Normalize(coords);
		float latitude = Mathf.Acos(normalizedCoords.y);
		float longitude = Mathf.Atan2(normalizedCoords.z, normalizedCoords.x);
		float spherex = longitude * (0.5f / Mathf.PI);
		float spherey = latitude  * (1.0f / Mathf.PI);
		Vector2 sphereCoords = new Vector2(0.5f - spherex, 1.0f - spherey);
		sphereCoords.x *= renderTexture.width;
		sphereCoords.y *= renderTexture.height;
		return sphereCoords;
	}
}


