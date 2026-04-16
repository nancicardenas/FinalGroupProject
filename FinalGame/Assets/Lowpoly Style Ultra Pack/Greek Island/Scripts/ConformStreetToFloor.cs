using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CHAssets {
    public class ConformStreetToFloor : MonoBehaviour {

        public enum ProjectionMode { ObjectUp, WorldUp }
        public ProjectionMode Projection = ProjectionMode.ObjectUp;

        [Header("Settings")]
        public LayerMask AffectedLayers = Physics.AllLayers;

        [Tooltip("If true, the vertices will be projected to the surface and retain their internal offset within the model. If false, the vertices will be projected to the surface as a flat projection.")]
        public bool KeepInternalOffsets = true;

        [Tooltip("Vertices within this range will be projected to the surface, if KeepInternalOffsets is true. Vertices above this height will retain their internal offset within the model. Adjust this in a way that only the bottom most vertices will be confirmed to the underlying surface.")]
        public float VertexConformRange = 0.25f;

        [Tooltip("The maximum distance the vertices will be projected downwards.")]
        public float MaxProjectionDistance = 100f;

        [Tooltip("The distance the vertices will be submerged into the surface that they are projected on.")]
        public float SubmergeDistance = 0;
        public float YScaleBeforeProjection = 1;

        [Header("Debug")]
        public bool DrawDebugRays = false;

        private MeshFilter meshFilter;
        private List<int> verticesWithoutHit = new List<int>();
        private List<GameObject> debugSpheres = new List<GameObject>();

        private MeshFilter MeshFilter {
            get {
                if (meshFilter == null) {
                    meshFilter = GetComponent<MeshFilter>();
                }
                return meshFilter;
            }
        }

        private List<Vector3> VertexList => MeshFilter.mesh.vertices.ToList();

        private List<Vector3> WorldSpaceVertexList {
            get {
                return VertexList.Select(vertex => transform.TransformPoint(vertex)).ToList();
            }
        }

        void Start() {
            ConformDown();
            Destroy(this);
        }

        private List<float> CalculateVertexHeightsRelativeToDeepest(Vector3[] vertices, Vector3 upVector) {
            upVector.Normalize();
            float minHeight = float.MaxValue;
            List<float> heights = vertices.Select(vertex => {
                float height = Vector3.Dot(vertex, upVector);
                if (height < minHeight) minHeight = height;
                return height;
            }).ToList();

            return heights.Select(height => height - minHeight).ToList();
        }

        private void ConformDown(bool previewOnly = false) {
            transform.localScale = new Vector3(transform.localScale.x, YScaleBeforeProjection, transform.localScale.z);
            VertexConformRange *= YScaleBeforeProjection;
            SubmergeDistance *= YScaleBeforeProjection;

            if (Projection == ProjectionMode.WorldUp) {
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            }

            debugSpheres.Clear();
            Physics.queriesHitBackfaces = true;

            Vector3[] vertices = WorldSpaceVertexList.ToArray();
            List<float> heights = Projection == ProjectionMode.WorldUp
                ? CalculateVertexHeightsRelativeToDeepest(vertices, Vector3.up)
                : CalculateVertexHeightsRelativeToDeepest(vertices, transform.up);

            float distanceBetweenTransformAndCollider = GetHitDistance();

            for (int i = 0; i < vertices.Length; i++) {
                Vector3 v = vertices[i];
                float offset = heights[i];

                if (offset < MaxProjectionDistance) {
                    RaycastHit hit;
                    Vector3 castDir = GetCastDirection();

                    if (Physics.Raycast(v, castDir, out hit, MaxProjectionDistance, AffectedLayers)) {
                        if (hit.collider.transform != transform) {
                            Vector3 vertexOffsetDir = GetVertexOffsetDirection(hit);
                            if (DrawDebugRays) Debug.DrawRay(v, castDir * hit.distance, Color.red, 2, true);

                            v = hit.point + vertexOffsetDir * (offset < VertexConformRange ? 0.005f : (KeepInternalOffsets ? offset : 0.005f)) + vertexOffsetDir * -SubmergeDistance;
                            if (DrawDebugRays) Debug.DrawRay(hit.point, vertexOffsetDir * (offset < VertexConformRange ? 0.005f : (KeepInternalOffsets ? offset : 0.005f)) + vertexOffsetDir * -SubmergeDistance, Color.green, 2, true);
                        }
                    } else if (Physics.Raycast(v, -castDir, out hit, 1, AffectedLayers)) {
                        Vector3 vertexOffsetDir = GetVertexOffsetDirection(hit);
                        v = hit.point + vertexOffsetDir * (offset < VertexConformRange ? 0.005f : (KeepInternalOffsets ? offset : 0.005f)) + vertexOffsetDir * -SubmergeDistance;
                    } else {
                        verticesWithoutHit.Add(i);
                    }

                    vertices[i] = transform.InverseTransformPoint(v);
                }
            }

            float averageHeight = verticesWithoutHit.Count > 0 ? verticesWithoutHit.Average(i => vertices[i].y) : 0;
            foreach (int i in verticesWithoutHit) {
                vertices[i] = new Vector3(vertices[i].x, averageHeight, vertices[i].z);
            }

            ApplyNewVertexData(MeshFilter, vertices);
            UpdateColliders();
        }

        private void ApplyNewVertexData(MeshFilter filter, Vector3[] vertices) {
            filter.mesh.SetVertices(vertices);
            filter.mesh.RecalculateBounds();
            filter.mesh.RecalculateNormals();
        }

        private void UpdateColliders() {
            if (TryGetComponent(out Collider collider)) {
                collider.enabled = false;
            }
        }

        private Vector3 GetVertexOffsetDirection(RaycastHit hit) {
            return Projection == ProjectionMode.ObjectUp ? transform.up : Vector3.up;
        }

        private Vector3 GetCastDirection() {
            return Projection == ProjectionMode.ObjectUp ? -transform.up : Vector3.down;
        }

        private float GetHitDistance() {
            if (Physics.Raycast(transform.position, GetCastDirection(), out RaycastHit distHit, 1000)) {
                return distHit.distance;
            }
            return 0;
        }
    }
}

