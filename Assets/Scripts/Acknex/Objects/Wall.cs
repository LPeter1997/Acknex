﻿using LibTessDotNet;
using System.Collections.Generic;
using System.Linq;
using Acknex.Interfaces;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;

namespace Acknex
{
    public class Wall : MonoBehaviour, IAcknexObjectContainer
    {
        public IAcknexObject AcknexObject { get; set; } = new AcknexObject(GetTemplateCallback);

        private static IAcknexObject GetTemplateCallback(string name)
        {
            if (World.Instance.WallsByName.TryGetValue(name, out var wall))
            {
                return wall.AcknexObject;
            }
            return null;
        }
        public Region RightRegion;
        public Region LeftRegion;

        public bool DebugMarked;

        public bool Processed;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private GameObject _attached;

        //todo: can remove, debug info
        public Matrix4x4 BottomQuad;
        public Matrix4x4 BottomUV;

        private Vector3 XAxis;
        //public Vector3 BottomNormal;

        public Texture TextureObject => AcknexObject.GetString("TEXTURE") != null && World.Instance.TexturesByName.TryGetValue(AcknexObject.GetString("TEXTURE"), out var textureObject) ? textureObject : null;

        public Bitmap BitmapImage => TextureObject?.GetBitmapAt();

        public List<string> Flags
        {
            get
            {
                if (AcknexObject.TryGetObject("FLAGS", out List<string> flags))
                {
                    return flags;
                }
                flags = new List<string>();
                AcknexObject.SetObject("FLAGS", flags);
                return flags;
            }
        }

        private void Awake()
        {
            AcknexObject.Container = this;
        }

        private void Update()
        {
            UpdateObject();
        }

        private void OnDrawGizmos()
        {
            if (DebugMarked)
            {
                GizmosUtils.DrawString("0" + "(" + ((Vector2)BottomUV.GetColumn(0)) + ")", BottomQuad.GetColumn(0));
                GizmosUtils.DrawString("1" + "(" + ((Vector2)BottomUV.GetColumn(1)) + ")", BottomQuad.GetColumn(1));
                GizmosUtils.DrawString("2" + "(" + ((Vector2)BottomUV.GetColumn(2)) + ")", BottomQuad.GetColumn(2));
                GizmosUtils.DrawString("3" + "(" + ((Vector2)BottomUV.GetColumn(3)) + ")", BottomQuad.GetColumn(3));
            }
        }


        public void UpdateObject()
        {
            if (!AcknexObject.IsDirty)
            {
                return;
            }
            AcknexObject.IsDirty = false;
            var basePosition = new Vector3(BottomQuad.GetColumn(0).x, 0f, 0f);
            var baseRotation = Quaternion.LookRotation(Vector3.Cross(XAxis, Vector3.up));
            Attachment.HandleAttachment(ref _attached, gameObject, AcknexObject, TextureObject?.AcknexObject, basePosition, baseRotation);
            if (_meshRenderer != null)
            {
                BitmapImage?.UpdateMaterial(_meshRenderer.material, TextureObject, 0, false, AcknexObject);
                _meshRenderer.enabled = !Flags.Contains("INVISIBLE");
                _meshRenderer.shadowCastingMode = TextureObject != null && TextureObject.Flags.Contains("SKY") ? ShadowCastingMode.Off : ShadowCastingMode.TwoSided;
            }
            if (_meshCollider != null)
            {
                _meshCollider.enabled = !Flags.Contains("PASSABLE");
            }
            //todo: update <X1, <Y1, <Z1 <X2, <Y2, <Z2, DISTANCE, LENGTH, SIZE_X, LEFT, RIGHT skills
        }

        public void Enable()
        {

        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void ProcessWall(
            List<ContourVertex> allContourVertices,
            List<ContourVertex> contourVertices,
            Wall wallA,
            KeyValuePair<int, RegionWall> kvp,
            ref int vertexCount,
            bool inverted = false)
        {
            vertexCount++;
            var initialVertex = contourVertices[inverted ? wallA.AcknexObject.GetInteger("VERTEX2") : wallA.AcknexObject.GetInteger("VERTEX1")];
            initialVertex = new ContourVertex(initialVertex.Position, new WallWithSide(wallA, inverted));
            allContourVertices.Add(initialVertex);
            wallA.Processed = true;
            var foundPair = false;
            foreach (var wallB in kvp.Value)
            {
                if (wallB.Processed)
                {
                    continue;
                }

                if (inverted)
                {
                    if (wallB.AcknexObject.GetInteger("VERTEX2") == wallA.AcknexObject.GetInteger("VERTEX1"))
                    {
                        ProcessWall(allContourVertices, contourVertices, wallB, kvp, ref vertexCount, true);
                        foundPair = true;
                        break;
                    }

                    if (wallB.AcknexObject.GetInteger("VERTEX1") == wallA.AcknexObject.GetInteger("VERTEX1"))
                    {
                        ProcessWall(allContourVertices, contourVertices, wallB, kvp, ref vertexCount);
                        foundPair = true;
                        break;
                    }
                }
                else
                {
                    if (wallB.AcknexObject.GetInteger("VERTEX1") == wallA.AcknexObject.GetInteger("VERTEX2"))
                    {
                        ProcessWall(allContourVertices, contourVertices, wallB, kvp, ref vertexCount);
                        foundPair = true;
                        break;
                    }

                    if (wallB.AcknexObject.GetInteger("VERTEX2") == wallA.AcknexObject.GetInteger("VERTEX2"))
                    {
                        ProcessWall(allContourVertices, contourVertices, wallB, kvp, ref vertexCount, true);
                        foundPair = true;
                        break;
                    }
                }

            }

            if (!foundPair)
            {
                var endingVertex = contourVertices[inverted ? wallA.AcknexObject.GetInteger("VERTEX1") : wallA.AcknexObject.GetInteger("VERTEX2")];
                endingVertex = new ContourVertex(endingVertex.Position, new WallWithSide(wallA, inverted));
                allContourVertices.Add(endingVertex);
                vertexCount++;
            }
        }

        private static void BuildWallMesh(List<Vector3> allVertices, List<Vector2> allUVs, Dictionary<string, List<int>> allTriangles, Wall wall)
        {
            var mesh = new Mesh();
            mesh.SetVertices(allVertices);
            mesh.SetUVs(0, allUVs);

            mesh.subMeshCount = allTriangles.Count;

            var trianglesList = allTriangles.Values.ToList();
            for (var i = 0; i < trianglesList.Count; i++)
            {
                mesh.SetIndices(trianglesList[i], MeshTopology.Quads, i);
            }
            mesh.RecalculateNormals();
            mesh.UploadMeshData(true);

            var texturesList = allTriangles.Keys.ToList();
            var materials = new Material[texturesList.Count];
            for (var i = 0; i < texturesList.Count; i++)
            {
                var textureName = texturesList[i];
                var material = World.Instance.BuildMaterial(textureName);
                materials[i] = material;
            }

            wall._meshFilter = wall.gameObject.AddComponent<MeshFilter>();
            wall._meshFilter.sharedMesh = mesh;

            wall._meshRenderer = wall.gameObject.AddComponent<MeshRenderer>();
            wall._meshRenderer.sharedMaterials = materials;

            wall._meshCollider = wall.gameObject.AddComponent<MeshCollider>();
            wall._meshCollider.sharedMesh = mesh;
        }

        private static void BuildWall(Wall wall, Region rightRegion, Region leftRegion, List<ContourVertex> vertices, List<Vector3> allVertices, List<Vector2> allUVs, Dictionary<string, List<int>> allTriangles, Region rightRegionAbove = null, Region leftRegionAbove = null)
        {
            while (true)
            {
                var vertexA = vertices[wall.AcknexObject.GetInteger("VERTEX1")];
                var vertexB = vertices[wall.AcknexObject.GetInteger("VERTEX2")];

                var xAxis = (new Vector3(vertexB.Position.X, 0f, vertexB.Position.Y) - new Vector3(vertexA.Position.X, 0f, vertexA.Position.Y)).normalized;
                wall.XAxis= xAxis;

                wall.AcknexObject.SetFloat("X1", vertexA.Position.X);
                wall.AcknexObject.SetFloat("Y1", vertexA.Position.Y);
                wall.AcknexObject.SetFloat("Z1", vertexA.Position.Z);
                                  
                wall.AcknexObject.SetFloat("X2", vertexB.Position.X);
                wall.AcknexObject.SetFloat("Y2", vertexB.Position.Y);
                wall.AcknexObject.SetFloat("Z2", vertexB.Position.Z);

                if (leftRegion == null)
                {
                    leftRegion = leftRegionAbove;
                }

                if (rightRegion == null)
                {
                    rightRegion = rightRegionAbove;
                }

                wall.RightRegion = rightRegion;
                wall.LeftRegion = leftRegion;

                //todo: checking by the flag here is wrong
                if (wall.Flags.Contains("FENCE"))
                    //leftRegion.AcknexObject.GetNumber("FLOOR_HGT") == rightRegion.AcknexObject.GetNumber("FLOOR_HGT") && 
                    //leftRegion.AcknexObject.GetNumber("CEIL_HGT") == rightRegion.AcknexObject.GetNumber("CEIL_HGT"))
                {
                    var floor = leftRegion.AcknexObject.GetFloat("FLOOR_HGT", false);
                    var ceil = leftRegion.AcknexObject.GetFloat("CEIL_HGT", false);
                    if (leftRegion.Below != null)
                    {
                        floor = Mathf.Max(floor, leftRegion.Below.AcknexObject.GetFloat("CEIL_HGT"));
                    }
                    if (rightRegion.Below != null)
                    {
                        floor = Mathf.Max(floor, rightRegion.Below.AcknexObject.GetFloat("CEIL_HGT"));
                    }
                    if (leftRegionAbove != null)
                    {
                        ceil = Mathf.Min(ceil, leftRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                    }
                    if (rightRegionAbove != null)
                    {
                        ceil = Mathf.Min(ceil, rightRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                    }
                    var liftedLeft = /*leftRegionAbove != null && leftRegionAbove.Flags.Contains("FLOOR_LIFTED") ||*/ leftRegionAbove == null && leftRegion.Flags.Contains("FLOOR_LIFTED");
                    var liftedRight = /*rightRegionAbove != null && rightRegionAbove.Flags.Contains("FLOOR_LIFTED") ||*/ rightRegionAbove == null && rightRegion.Flags.Contains("FLOOR_LIFTED");
                    var v0 = new Vector3(vertexA.Position.X, ceil + (liftedLeft ? vertexA.Position.Z : 0), vertexA.Position.Y);
                    var v1 = new Vector3(vertexB.Position.X, ceil + (liftedLeft ? vertexB.Position.Z : 0), vertexB.Position.Y);
                    var v2 = new Vector3(vertexB.Position.X, floor + (liftedRight ? vertexB.Position.Z : 0), vertexB.Position.Y);
                    var v3 = new Vector3(vertexA.Position.X, floor + (liftedRight ? vertexA.Position.Z : 0), vertexA.Position.Y);
                    MeshUtils.AddQuad(0, 1, 2, 3, allTriangles, wall.AcknexObject.GetString("TEXTURE"), allVertices.Count);
                    //todo: double-sided walls glitch Unity shadows
                    //MeshUtils.AddQuad(3, 2, 1, 0, allTriangles, wall.AcknexObject.GetString("TEXTURE"), allVertices.Count);
                    allVertices.Add(v0); //a
                    allVertices.Add(v1); //b
                    allVertices.Add(v2); //c
                    allVertices.Add(v3); //d
                    //var normal = MeshUtils.GetNormal(v0, v1, v2, v3);
                    var uv0 = CalculateUV(v0 - v0, xAxis, v0);
                    var uv1 = CalculateUV(v1 - v0, xAxis, v1);
                    var uv2 = CalculateUV(v2 - v0, xAxis, v2);
                    var uv3 = CalculateUV(v3 - v0, xAxis, v3);
                    allUVs.Add(uv0);
                    allUVs.Add(uv1);
                    allUVs.Add(uv2);
                    allUVs.Add(uv3);
                    wall.BottomQuad = new Matrix4x4(v0, v1, v2, v3);
                    wall.BottomUV = new Matrix4x4(uv0, uv1, uv2, uv3);
                }
                else
                {
                    {
                        var heightLeft = leftRegion.AcknexObject.GetFloat("FLOOR_HGT", false);
                        var heightRight = rightRegion.AcknexObject.GetFloat("FLOOR_HGT", false);
                        if (leftRegion.Below != null)
                        {
                            heightLeft = Mathf.Max(heightLeft, leftRegion.Below.AcknexObject.GetFloat("CEIL_HGT"));
                            heightRight = Mathf.Max(heightRight, leftRegion.Below.AcknexObject.GetFloat("CEIL_HGT"));
                        }
                        if (rightRegion.Below != null)
                        {
                            heightRight = Mathf.Max(heightRight, rightRegion.Below.AcknexObject.GetFloat("CEIL_HGT"));
                            heightLeft = Mathf.Max(heightLeft, rightRegion.Below.AcknexObject.GetFloat("CEIL_HGT"));
                        }
                        if (leftRegionAbove != null)
                        {
                            heightLeft = Mathf.Min(heightLeft, leftRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                            heightRight = Mathf.Min(heightRight, leftRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                        }
                        if (rightRegionAbove != null)
                        {
                            heightRight = Mathf.Min(heightRight, rightRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                            heightLeft = Mathf.Min(heightLeft, rightRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                        }
                        var liftedLeft = /*leftRegionAbove != null && leftRegionAbove.Flags.Contains("FLOOR_LIFTED") ||*/ leftRegionAbove == null && leftRegion.Flags.Contains("FLOOR_LIFTED");
                        var liftedRight = /*rightRegionAbove != null && rightRegionAbove.Flags.Contains("FLOOR_LIFTED") ||*/ rightRegionAbove == null && rightRegion.Flags.Contains("FLOOR_LIFTED");
                        var v0 = new Vector3(vertexA.Position.X, heightLeft + (liftedLeft ? vertexA.Position.Z : 0), vertexA.Position.Y);
                        var v1 = new Vector3(vertexB.Position.X, heightLeft + (liftedLeft ? vertexB.Position.Z : 0), vertexB.Position.Y);
                        var v2 = new Vector3(vertexB.Position.X, heightRight + (liftedRight ? vertexB.Position.Z : 0), vertexB.Position.Y);
                        var v3 = new Vector3(vertexA.Position.X, heightRight + (liftedRight ? vertexA.Position.Z : 0), vertexA.Position.Y);
                        MeshUtils.AddQuad(0, 1, 2, 3, allTriangles, wall.AcknexObject.GetString("TEXTURE"), allVertices.Count);
                        allVertices.Add(v0); //a
                        allVertices.Add(v1); //b
                        allVertices.Add(v2); //c
                        allVertices.Add(v3); //d
                        //var normal = MeshUtils.GetNormal(v0, v1, v2, v3);
                        //var xAxis = Vector3.Normalize(v1 - v0);
                        var uv0 = CalculateUV(v0 - v0, xAxis, v0);
                        var uv1 = CalculateUV(v1 - v0, xAxis, v1);
                        var uv2 = CalculateUV(v2 - v0, xAxis, v2);
                        var uv3 = CalculateUV(v3 - v0, xAxis, v3);
                        allUVs.Add(uv0);
                        allUVs.Add(uv1);
                        allUVs.Add(uv2);
                        allUVs.Add(uv3);
                        wall.BottomQuad = new Matrix4x4(v0, v1, v2, v3);
                        wall.BottomUV = new Matrix4x4(uv0, uv1, uv2, uv3);
                    }
                    {
                        var heightLeft = leftRegion.AcknexObject.GetFloat("CEIL_HGT");
                        var heightRight = rightRegion.AcknexObject.GetFloat("CEIL_HGT");
                        if (leftRegionAbove != null)
                        {
                            heightLeft = Mathf.Min(heightLeft, leftRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                            heightRight = Mathf.Min(heightRight, leftRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                        }

                        if (rightRegionAbove != null)
                        {
                            heightRight = Mathf.Min(heightRight, rightRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                            heightLeft = Mathf.Min(heightLeft, rightRegionAbove.AcknexObject.GetFloat("FLOOR_HGT"));
                        }

                        var liftedLeft = leftRegion.Flags.Contains("CEIL_LIFTED") && leftRegionAbove == null;
                        var liftedRight = rightRegion.Flags.Contains("CEIL_LIFTED") && rightRegionAbove == null;
                        var v0 = new Vector3(vertexA.Position.X, heightLeft + (liftedLeft ? vertexA.Position.Z : 0), vertexA.Position.Y);
                        var v1 = new Vector3(vertexB.Position.X, heightLeft + (liftedLeft ? vertexB.Position.Z : 0), vertexB.Position.Y);
                        var v2 = new Vector3(vertexB.Position.X, heightRight + (liftedRight ? vertexB.Position.Z : 0), vertexB.Position.Y);
                        var v3 = new Vector3(vertexA.Position.X, heightRight + (liftedRight ? vertexA.Position.Z : 0), vertexA.Position.Y);
                        MeshUtils.AddQuad(3, 2, 1, 0, allTriangles, wall.AcknexObject.GetString("TEXTURE"), allVertices.Count);
                        allVertices.Add(v0);
                        allVertices.Add(v1);
                        allVertices.Add(v2);
                        allVertices.Add(v3);
                        //var xAxis = Vector3.Normalize(v1 - v0);
                        var uv0 = CalculateUV(v0 - v0, xAxis, v0);
                        var uv1 = CalculateUV(v1 - v0, xAxis, v1);
                        var uv2 = CalculateUV(v2 - v0, xAxis, v2);
                        var uv3 = CalculateUV(v3 - v0, xAxis, v3);
                        allUVs.Add(uv0);
                        allUVs.Add(uv1);
                        allUVs.Add(uv2);
                        allUVs.Add(uv3);
                    }
                }
                if (leftRegion.Below != null && rightRegion.Below != null)
                {
                    var rightRegion1 = rightRegion;
                    var leftRegion1 = leftRegion;
                    rightRegion = rightRegion.Below;
                    leftRegion = leftRegion.Below;
                    rightRegionAbove = rightRegion1;
                    leftRegionAbove = leftRegion1;
                    continue;
                }
                if (leftRegion.Below != null)
                {
                    var rightRegion1 = rightRegion;
                    var leftRegion1 = leftRegion;
                    rightRegion = null;
                    leftRegion = leftRegion.Below;
                    rightRegionAbove = rightRegion1;
                    leftRegionAbove = leftRegion1;
                    continue;
                }
                if (rightRegion.Below != null)
                {
                    var rightRegion1 = rightRegion;
                    var leftRegion1 = leftRegion;
                    rightRegion = rightRegion.Below;
                    leftRegion = null;
                    rightRegionAbove = rightRegion1;
                    leftRegionAbove = leftRegion1;
                    continue;
                }
                break;
            }
        }

        private static Vector2 CalculateUV(Vector3 input, Vector3 xAxis, Vector3 yAxis)
        {
            var uv = new Vector2
            {
                x = Vector3.Project(input, xAxis).magnitude,
                y = yAxis.y
            };
            return uv;
        }

        public static void BuildWallAndMesh(Wall wall, List<ContourVertex> contourVertices)
        {
            var allVertices = new List<Vector3>();
            var allUVs = new List<Vector2>();
            var allTriangles = new Dictionary<string, List<int>>();
            var rightRegion = World.Instance.RegionsByIndex[wall.AcknexObject.GetInteger("REGION1")];
            var leftRegion = World.Instance.RegionsByIndex[wall.AcknexObject.GetInteger("REGION2")];
            BuildWall(wall, rightRegion, leftRegion, contourVertices, allVertices, allUVs, allTriangles);
            BuildWallMesh(allVertices, allUVs, allTriangles, wall);
        }
    }
}