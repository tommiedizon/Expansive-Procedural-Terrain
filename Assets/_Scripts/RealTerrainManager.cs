﻿using UnityEngine;
using System.Collections.Generic;
using static QuadTree;
using static PlaneMeshGenerator;
using System.Linq;

public class RealTerrainManager : MonoBehaviour {

    public static RealTerrainManager Instance { get; private set; }
    /* It is assumed here that all vertices lie on integers only.
     */
    [SerializeField] QTViewer viewer;
    [SerializeField] NoiseSettings noiseSettings;
    [SerializeField] int rootNodeLengthMultiplier = 1;

    // PRE-CALCULATED
    public const int MAX_NUM_VERTICES_PER_SIDE = 120;
    public static readonly int[] FACTORS_OF_MAX_NUM_VERTICES_PER_SIDE = { 1, 2, 3, 4, 6, 8, 10, 12 };
    int rootNodeLength;
    QuadNode rootNode;
    /* For any Mesh generated, the number of vertices per side is required to be a factor of 
     * MAX_NUM_VERTICES_PER_SIDE
     */

    QuadTree quadTree; // Generalise this to a collection in the future
    Dictionary<uint, QuadChunk> chunks = new();

    // For Debugging
    List<Bounds> boundsToDraw = new();

    #region Unity Functions
    // Unity Functions
    private void Awake() {
        rootNodeLength = MAX_NUM_VERTICES_PER_SIDE * rootNodeLengthMultiplier;

        if (Instance == null) Instance = this;
        ValidateNoiseSettings();

        // Create the rootNode
        rootNode = new QuadNode(new Vector2(-0.5f * rootNodeLength, -0.5f * rootNodeLength), rootNodeLength);
        rootNode.SetLevel(0);
    }

    private void OnDrawGizmos() {
        if (null == viewer) return;
        GizmosDrawViewTriangleAndTriBounds();
        GizmosDrawNodeSquares();
    }
    private void Update() {
        // Generate the quad tree
        quadTree = new QuadTree(rootNode, viewer.GetViewTriangle(), viewer.GetTriBounds(), MAX_NUM_VERTICES_PER_SIDE);
        quadTree.SaveTree(ref boundsToDraw);

        // Retrieve all leaf nodes
        List<QuadNode> leafNodes = quadTree.GetAllLeafNodes();

        // Hash leaf nodes
        foreach(QuadNode leafNode in leafNodes) {
            uint hash = leafNode.ComputeHash();

            if (chunks.TryGetValue(hash, out QuadChunk value)) {
                // Chunk exists
            } else {
                // Chunk does not exist

                // Generate new chunk
                int leafNodeLevel = leafNode.GetLevel();
                int chunkLODIndex = quadTree.GetTreeHeight() - leafNodeLevel;
                int chunkScaleFactor = FACTORS_OF_MAX_NUM_VERTICES_PER_SIDE[chunkLODIndex];

                float requiredMeshLength = leafNode.GetSideLength();

                int numVertsPerSide = MAX_NUM_VERTICES_PER_SIDE / chunkScaleFactor;

                Debug.Log($"chunkLOD:{chunkLODIndex}, chunkScaleFactor:{chunkScaleFactor}, numVerticesPerSide:{numVertsPerSide}");

                MeshData newMeshData = new MeshData(numVertsPerSide, numVertsPerSide, requiredMeshLength);
                Mesh newMesh = GeneratePlaneMesh(newMeshData);

                GameObject chunkObject;
                string chunkName = $"BotLeftPoint:{leafNode.GetBotLeftPoint()},chunkLOD:{chunkLODIndex}, chunkScaleFactor:{chunkScaleFactor}, numVerticesPerSide:{numVertsPerSide} ";
                chunkObject = new GameObject(chunkName, typeof(MeshFilter), typeof(MeshRenderer));
                chunkObject.GetComponent<MeshFilter>().mesh = newMesh;
                chunkObject.GetComponent<MeshRenderer>().material = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultMaterial;
                chunkObject.transform.position = new Vector3(leafNode.GetBotLeftPoint().x, 0f, leafNode.GetBotLeftPoint().y);
                // Add it to the dictionary

            }
        }

        // Debugging
    }
    private void OnValidate() {
        // I should really be configuring the noise separately then 
        // running it on here.
        ValidateNoiseSettings();
        //UpdateNoise();
    }
    #endregion

    #region Helper Functions
    // HELPERS
    private void GizmosDrawNodeSquares() {
        Gizmos.color = Color.green;
        foreach (Bounds bounds in boundsToDraw) {
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
    private void GizmosDrawViewTriangleAndTriBounds() {
        Vector3[] viewTriangle = viewer.GetViewTriangle();
        if (viewTriangle == null || viewTriangle.Length == 0) return;
        Gizmos.DrawLine(viewTriangle[0], viewTriangle[1]);
        Gizmos.DrawLine(viewTriangle[1], viewTriangle[2]);
        Gizmos.DrawLine(viewTriangle[0], viewTriangle[2]);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(viewer.GetTriBounds().center, viewer.GetTriBounds().size);
    }
    private void ValidateNoiseSettings() {
        noiseSettings.width = MAX_NUM_VERTICES_PER_SIDE;
        noiseSettings.length = MAX_NUM_VERTICES_PER_SIDE;
        if (noiseSettings.persistance > 1) noiseSettings.persistance = 0.99f;
        if (noiseSettings.persistance < 0) noiseSettings.persistance = 0.01f;
        if (noiseSettings.octaves < 0) noiseSettings.octaves = 0;
        if (noiseSettings.octaves > 6) noiseSettings.octaves = 6;
        if (noiseSettings.lacunarity < 0) noiseSettings.lacunarity = 0.01f;
    }
    #endregion

    #region Getter and Setter Functions
    #endregion
}
