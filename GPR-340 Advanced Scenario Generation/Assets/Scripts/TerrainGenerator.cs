using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    Terrain terrain;
    public Gradient gradient;

    public int width = 256;
    public int height = 256;

    public float amplitude = 1.0f;
    public float frequency = 4.0f;
    public float exponent = 2.0f;
    public float mix = 0.5f;

    public float grassHeight = 0.4f;
    public float rockHeight = 0.8f;
    public float snowHeight = 1.0f;

    [SerializeField] Transform water;
    public float waterHeight = 2f;

    float seed;

    void Start()
    {
        seed = Random.Range(0f, 9999f);

        terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        PaintTerrain();
    }

    private void Update()
    {

        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        PaintTerrain();

        if (water.transform.position.y != waterHeight)
        {
            water.transform.position = new Vector3(water.transform.position.x, waterHeight, water.transform.position.z);
        }
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        //terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, 50, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float nx = ((float)x) / width;
                float ny = ((float)y) / height;

                //Using Perlin Noise and applying amplitude and frequency values
                float e =
                    ((amplitude * 1.0f) * Mathf.PerlinNoise((frequency * 1f) * nx, (frequency * 1f) * ny))
                    + ((amplitude * 0.5f) * Mathf.PerlinNoise((frequency * 2f) * nx, (frequency * 2f) * ny))
                    + ((amplitude * 0.25f) * Mathf.PerlinNoise((frequency * 4f) * nx, (frequency * 4f) * ny));
                e /= (1f + 0.5f + 0.25f);

                //Island
                float d = Mathf.Min(1f, (Mathf.Pow(nx - 0.5f, 2f) + Mathf.Pow(ny - 0.5f, 2f)) / Mathf.Sqrt(2f));

                e = Mathf.Lerp(e, 1f - d, mix);


                //Final Height Calculation
                //Using Mathf.Round to make terraces
                heights[x, y] = Mathf.Pow(e, exponent);
            }
        }
        return heights;
    }

    private void PaintTerrain()
    {
        TerrainData data = terrain.terrainData;

        int heightmapWidth = data.heightmapResolution;  // Width of the heightmap
        int heightmapHeight = data.heightmapResolution;  // Height of the heightmap

        int alphamapWidth = data.alphamapWidth;
        int alphamapHeight = data.alphamapHeight;

        // Get the heights of the terrain
        float[,] heights = data.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        // Ensure there are enough texture layers for painting
        int numLayers = data.alphamapLayers;
        if (numLayers < 3) // Check if there are at least 3 texture layers
        {
            Debug.LogError("Terrain does not have enough texture layers. Please assign textures to the terrain.");
            return;
        }

        float[,,] splatmapData = new float[heightmapWidth, heightmapHeight, numLayers];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate corresponding position in the heightmap
                int heightmapX = Mathf.FloorToInt((float)x / alphamapWidth * heightmapWidth);
                int heightmapY = Mathf.FloorToInt((float)y / alphamapHeight * heightmapHeight);

                // Get the height at this position
                float terrainHeight = heights[heightmapX, heightmapY];

                if (terrainHeight <= grassHeight)
                {
                    splatmapData[x, y, 0] = 1;
                    splatmapData[x, y, 1] = 0;
                    splatmapData[x, y, 2] = 0;
                }
                else if (terrainHeight <= rockHeight)
                {
                    float blend = Mathf.InverseLerp(0.25f, 0.4f, terrainHeight);
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
                    float blend = Mathf.InverseLerp(0.4f, 0.75f, terrainHeight);
                    splatmapData[x, y, 0] = 0;
                    splatmapData[x, y, 1] = 1 - blend;
                    splatmapData[x, y, 2] = blend;
                }
            }
        }

        data.SetAlphamaps(0, 0, splatmapData);
    }
}
