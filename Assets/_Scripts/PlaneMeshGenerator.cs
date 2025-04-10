﻿using UnityEngine;

public static class PlaneMeshGenerator {

    public struct MeshData {
        public int numVerticesX;
        public int numVerrticesZ;
        public float scale;

        public MeshData(int numVerticesX, int numVerticesZ, float scale) {
            this.numVerticesX = numVerticesX;
            this.numVerrticesZ = numVerticesZ;
            this.scale = scale;
        }
    }
    public static Mesh GeneratePlaneMesh(MeshData meshData) {
        // You need to create instantiate a meshData struct first and then pass it into
        // this function.

        int width = meshData.numVerticesX;
        int length = meshData.numVerrticesZ;
        float scale = meshData.scale;

        int verticesCountX = width + 1;
        int verticesCountZ = length + 1;
        int totalVertices = verticesCountX * verticesCountZ;
        int totalTriangles = width * length * 6;

        Vector3[] vertices = new Vector3[totalVertices];
        int[] triangles = new int[totalTriangles];
        Vector2[] uv = new Vector2[totalVertices];

        // Generate vertices and UVs
        for (int z = 0; z < verticesCountZ; z++) {
            for (int x = 0; x < verticesCountX; x++) {
                int index = z * verticesCountX + x;
                vertices[index] = new Vector3(x * scale, 0, z * scale);
                uv[index] = new Vector2((float)x / width, (float)z / length);
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int z = 0; z < length; z++) {
            for (int x = 0; x < width; x++) {
                int bottomLeft = z * verticesCountX + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + verticesCountX;
                int topRight = topLeft + 1;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = topRight;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomRight;
            }
        }

        // Assign mesh properties
        Mesh mesh = new() {
            //name = $"Plane Mesh: Dim: {width} x {length}. Scale: {scale}",
            vertices = vertices,
            triangles = triangles,
            uv = uv
        };

        mesh.RecalculateNormals();

        return mesh;
    }

}
