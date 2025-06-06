using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.NoiseSystem.ScriptableObjects;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace _Scripts.NoiseSystem
{
    // responsible for adding noise layers together and producing 
    // a final height map

    
    
    public readonly struct HeightMap
    {
        private readonly int _gridLength;
        private readonly int _gridWidth;
        private readonly float[,] _heightMap;
        public HeightMap(int gridLength, int gridWidth)
        {
            _gridLength = gridLength;
            _gridWidth = gridWidth;
            _heightMap = new float[gridLength, gridWidth];
        }

        public void SetPoint(int x, int y, float value)
        {
            _heightMap[x, y] = value;
        }

        public float GetPoint(int x, int y)
        {
            return _heightMap[x, y];
        }
        
        public int GetGridWidth() => _gridWidth;
        public int GetGridLength() => _gridLength;

        public float[,] GetFloatArray() => _heightMap;
        
        
    }
    public class NoiseGenerator : MonoBehaviour
    {
        [SerializeField] private List<NoiseLayerSO> additiveNoiseLayers;
        [SerializeField] private List<NoiseLayerSO> multiplicativeNoiseLayers;
        [SerializeField] private List<NoiseLayerSO> compositionalNoiseLayers;
        
        private readonly List<NoiseLayerSO> _noiseLayers = new();
        private int _gridWidth = 0;
        private int _gridHeight = 0;
        private Vector3 _worldSpaceOrigin = new(0, 0, 0);
        private float _maxWorldHeight = 10f;
        private float _minWorldHeight = -10f;
        
        public HeightMap GenerateHeightMap(Vector2 botLeftPointInGlobalCoords, float distanceBetweenPoints, float globalHeightMultiplier)
        {
            var heightMap = new HeightMap(_gridHeight, _gridWidth);
            
            for(var y = 0; y < _gridWidth; y++)
            for (var x = 0; x < _gridHeight; x++)
                heightMap.SetPoint(x, y, SampleNoise(botLeftPointInGlobalCoords.x + x * distanceBetweenPoints, botLeftPointInGlobalCoords.y + y * distanceBetweenPoints));
            
            RenormalizeHeightMap(heightMap.GetFloatArray());
            
            for(var y = 0; y < _gridWidth; y++)
            for (var x = 0; x < _gridHeight; x++)
                heightMap.SetPoint(x, y, globalHeightMultiplier*heightMap.GetPoint(x,y));
            
            return heightMap;
        }
        private float SampleNoise(float x, float y)
        {
            var result = additiveNoiseLayers.Sum(layer => layer.Evaluate(new Vector2(x,y)));

            foreach (var layer in compositionalNoiseLayers)
            {
                result = layer.Compose(result, new Vector2(x, y));
            }

            return result;
        }
        private void RenormalizeHeightMap(float[,] heightMap)
        {
            
            foreach (var height in heightMap)
            {
                if (height < _minWorldHeight) _minWorldHeight = height;
                if (height > _maxWorldHeight) _maxWorldHeight = height;
            }

            float range = _maxWorldHeight - _minWorldHeight;
            if (range == 0f) return; // flat mesh, nothing to normalize

            // 2. Normalize Y values to [-1 1]
            for (int i = 0; i < heightMap.GetLength(0); i++)
                for(int j = 0; j < heightMap.GetLength(1); j++)
            {
                var normalizedY = 2f * (heightMap[i,j] - _minWorldHeight) / range - 1f;
                heightMap[i,j] = normalizedY;
            }

        }
        public void AddLayer(NoiseLayerSO layer)
        {
            _noiseLayers.Add(layer);
        }
        public void RemoveLayer<TLayerType>() where TLayerType : NoiseLayerSO
        {
            _noiseLayers.RemoveAll(layer => layer is TLayerType);
        }

        #region Setters & Getters
        public void SetGridWidth(int gridWidth)
        {
            if(gridWidth < 0) throw new ArgumentException("gridWidth must be greater than or equal to 0");
            _gridWidth = gridWidth;
        }

        public void SetGridHeight(int gridHeight)
        {
            if(gridHeight < 0) throw new ArgumentException("gridHeight must be greater than or equal to 0");
            _gridHeight = gridHeight;
        }

        public int GetGridWidth()
        {
            return _gridWidth;
        }

        public int GetGridHeight()
        {
            return _gridHeight;
        }
        
        public void SetGridDimensions(int gridWidth, int gridHeight)
        {
            SetGridWidth(gridWidth);
            SetGridHeight(gridHeight);
        }
        public List<NoiseLayerSO> GetLayers() { return _noiseLayers; }
        
        public Vector3 GetWorldSpaceOrigin() { return _worldSpaceOrigin; }
        public void SetWorldSpaceOrigin(Vector3 origin) { _worldSpaceOrigin = origin; }

        
        #endregion

        public void SetWorldHeightBounds(float minWorldHeight, float maxWorldHeight)
        {
            _minWorldHeight = minWorldHeight;
            _maxWorldHeight = maxWorldHeight;
        }
    }
}
