using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
#if ENABLE_RAYTRACING
    public class HDRaytracingManager
    {
        public class HDRayTracingSubScene
        {
            public LayerMask mask = -1;
            public RaytracingAccelerationStructure accelerationStructure = null;
            public List<Renderer> targetRenderers = null;
            public List<HDRayTracingGraph> referenceGraphs = new List<HDRayTracingGraph>();
        }

        Dictionary<int, HDRayTracingSubScene> m_subScenes = null;
        List<int> m_layerMasks = null;

        public HDRaytracingManager()
        {
        }

        public void InitAccelerationStructures()
        {
            // Create the sub-scene structure
            m_subScenes = new Dictionary<int, HDRayTracingSubScene>();

            // Grab all the ray tracing graphs in the scene
            HDRayTracingGraph[] tracingGraphs = UnityEngine.Object.FindObjectsOfType<HDRayTracingGraph>();

            m_layerMasks = new List<int>();
            // Build an array with all the layer combinations that are requested
            for (var graphIndex = 0; graphIndex < tracingGraphs.Length; graphIndex++)
            {
                // Grab the current graph
                HDRayTracingGraph currentGraph = tracingGraphs[graphIndex];

                // Fetch or create the sub-scene
                HDRayTracingSubScene currentSubScene = null;
                if (!m_subScenes.TryGetValue(currentGraph.layermask.value, out currentSubScene))
                {
                    currentSubScene = new HDRayTracingSubScene();
                    currentSubScene.mask = currentGraph.layermask.value;
                    m_subScenes.Add(currentGraph.layermask.value, currentSubScene);
                    m_layerMasks.Add(currentGraph.layermask.value);
                }

                // Mark the current graph for a reference
                currentSubScene.referenceGraphs.Add(currentGraph);
            }

            // Create all the ray tracing
            for (var subSceneIndex = 0; subSceneIndex < m_layerMasks.Count; subSceneIndex++)
            {
                HDRayTracingSubScene currentSubScene = m_subScenes[m_layerMasks[subSceneIndex]];
                BuildSubSceneStructure(ref currentSubScene);
            }
        }

        public void ReleaseAccelerationStructures()
        {
            for (var subSceneIndex = 0; subSceneIndex < m_layerMasks.Count; subSceneIndex++)
            {
                HDRayTracingSubScene currentSubScene = m_subScenes[m_layerMasks[subSceneIndex]];
                DestroySubSceneStructure(ref currentSubScene);
            }
        }

        public void DestroySubSceneStructure(ref HDRayTracingSubScene subScene)
        {
            if (subScene.accelerationStructure != null)
            {
                for (var i = 0; i < subScene.targetRenderers.Count; i++)
                {
                    subScene.accelerationStructure.RemoveInstance(subScene.targetRenderers[i]);
                }
                subScene.accelerationStructure.Dispose();
                subScene.targetRenderers = null;
                subScene.accelerationStructure = null;
            }
        }

        public void BuildSubSceneStructure(ref HDRayTracingSubScene subScene)
        {
            // Destroy the acceleration structure
            subScene.targetRenderers = new List<Renderer>();

            // Create the acceleration structure
            subScene.accelerationStructure = new RaytracingAccelerationStructure();

            // Grab all the renderers from the scene
            var rendererArray = UnityEngine.GameObject.FindObjectsOfType<Renderer>();
            for (var i = 0; i < rendererArray.Length; i++)
            {
                // Convert the object's layer to an int
                int objectLayerValue = 1 << rendererArray[i].gameObject.layer;

                // Is this object in one of the allowed layers ?
                if ((objectLayerValue & subScene.mask.value) != 0)
                {
                    // Add this fella to the renderer list
                    subScene.targetRenderers.Add(rendererArray[i]);
                }
            }

            // If any object build the acceleration structure
            if (subScene.targetRenderers.Count != 0)
            {
                for (var i = 0; i < subScene.targetRenderers.Count; i++)
                {
                    // Add it to the acceleration structure
                    subScene.accelerationStructure.AddInstance(subScene.targetRenderers[i]);
                }
            }

            // build the acceleration structure
            subScene.accelerationStructure.Build();
        }

        public RaytracingAccelerationStructure RequestAccelerationStructure(LayerMask layerMask)
        {
            HDRayTracingSubScene currentSubScene = null;
            if (m_subScenes.TryGetValue(layerMask.value, out currentSubScene))
            {
                return currentSubScene.accelerationStructure;
            }
            return null;
        }
    }
#endif
}
