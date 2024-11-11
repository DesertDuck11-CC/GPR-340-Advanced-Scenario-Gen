using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;

public class IslandTerrain : MonoBehaviour
{
    private Terrain terrain;
    [SerializeField] GameObject water;

    public int width = 256;
    public int height = 256;

    //Noise Settings
    public float scale = 16f;
    public float islandRadius = 0.5f;
    public float exponent = 1.2f;

    //Seed for randomization
    private float seed;

    //Texture Heights
    public float grassHeight = 0f;
    public float rockHeight = 0.25f;
    public float snowHeight = 0.3f;
    
    //Water Height
    public float waterHeight = 0f;

    [SerializeField] Slider frequencySlider;
    [SerializeField] Slider depthSlider;
    [SerializeField] Slider waterHeightSlider;
    [SerializeField] Slider radiusSlider;

    [SerializeField] Slider grassSlider;
    [SerializeField] Slider rockSlider;
    [SerializeField] Slider snowSlider;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        seed = Random.Range(0f, 1000f);
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        PaintTerrain();

        frequencySlider.value = scale;
        depthSlider.value = exponent;
        waterHeightSlider.value = waterHeight;
        radiusSlider.value = islandRadius;

        grassSlider.value = grassHeight;
        rockSlider.value = rockHeight;
        snowSlider.value = snowHeight;
    }

    private void Update()
    {
        waterHeight = waterHeightSlider.value;
        water.GetComponent<Transform>().transform.position = 
            new Vector3(water.GetComponent<Transform>().transform.position.x, waterHeight, water.GetComponent<Transform>().transform.position.z);

        if (scale != frequencySlider.value)
        {
            scale = frequencySlider.value;
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            PaintTerrain();
        }
        if (exponent != depthSlider.value)
        {
            exponent = depthSlider.value;
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            PaintTerrain();
        }
        if (islandRadius != radiusSlider.value)
        {
            islandRadius = radiusSlider.value;
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            PaintTerrain();
        }

        if (grassHeight != grassSlider.value)
        {
            grassHeight = grassSlider.value;
            PaintTerrain();
        }
        if (rockHeight != rockSlider.value)
        {
            rockHeight = rockSlider.value;
            PaintTerrain();
        }
        if (snowHeight != snowSlider.value)
        {
            snowHeight = snowSlider.value;
            PaintTerrain();
        }
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.size = new Vector3(width, 50, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        TerrainData data = terrain.terrainData;
        int size = data.heightmapResolution;
        float[,] heights = new float[size, size];

        //Center calculation
        float centerX = size / 2f;
        float centerY = size / 2f;

        //Calculate max distance with center and island radius
        float maxDistance = Mathf.Min(centerX, centerY) * islandRadius;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                //Offset coords by the seed value for randomness
                float xCoord = (x + seed) / size * scale;
                float yCoord = (y + seed) / size * scale;

                //Noise calculation
                float noise = 1f * Mathf.PerlinNoise(1 * xCoord, 1f * yCoord)
                + 0.5f * Mathf.PerlinNoise(2f * xCoord, 2f * yCoord)
                + 0.25f * Mathf.PerlinNoise(4f * xCoord, 4f * yCoord);
                noise /= (1f + 0.5f + 0.25f);

                //Calculate distance from center
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                //Apply falloff to make it look like an island
                float falloff = Mathf.Clamp01((maxDistance - distance) / maxDistance);

                //Final height calculation
                heights[x, y] = Mathf.Pow(noise, exponent) * falloff;
            }
        }
        return heights;
    }

    private void PaintTerrain()
    {
        TerrainData data = terrain.terrainData;
        int width = data.alphamapWidth;
        int height = data.alphamapHeight;

        float[,] heights = data.GetHeights(0, 0, width, height);
        float[,,] splatmapData = new float[width, height, data.alphamapLayers];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float terrainHeight = heights[x, y];

                if (terrainHeight <= grassHeight)
                {
                    splatmapData[x, y, 0] = 1;
                    splatmapData[x, y, 1] = 0;
                    splatmapData[x, y, 2] = 0;
                }
                else if (terrainHeight <= rockHeight)
                {
                    float blend = Mathf.InverseLerp(grassHeight, rockHeight, terrainHeight);
                    splatmapData[x, y, 0] = 1 - blend;
                    splatmapData[x, y, 1] = blend;
                    splatmapData[x, y, 2] = 0;
                }
                else if (terrainHeight >= snowHeight)
                {
                    splatmapData[x, y, 0] = 0;
                    splatmapData[x, y, 1] = 0;
                    splatmapData[x, y, 2] = 1;
                }
                else
                {
                    float blend = Mathf.InverseLerp(rockHeight, snowHeight, terrainHeight);
                    splatmapData[x, y, 0] = 0;
                    splatmapData[x, y, 1] = 1 - blend;
                    splatmapData[x, y, 2] = blend;
                }
            }
        }

        data.SetAlphamaps(0, 0, splatmapData);
    }

    public void GenHud()
    {
        if(Input.GetMouseButtonUp(0))
        {
            seed = Random.Range(0f, 1000f);
            terrain.terrainData = GenerateTerrain(terrain.terrainData);
            PaintTerrain();
        }
    }
}
