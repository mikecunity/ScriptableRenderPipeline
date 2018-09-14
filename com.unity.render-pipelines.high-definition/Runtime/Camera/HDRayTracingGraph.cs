using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class HDRayTracingGraph : MonoBehaviour
    {
        [HideInInspector]
        const int currentVersion = 1;

#if ENABLE_RAYTRACING
        // Culling mask that defines the layers that this acceleration structure
        public LayerMask layermask = -1;
    #endif
    }
}
