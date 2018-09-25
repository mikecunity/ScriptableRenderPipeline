using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{

    public class CullingGroupManager
    {
        static CullingGroupManager m_Instance;
        static public CullingGroupManager instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new CullingGroupManager();
                return m_Instance;
            }
        }

        private List<CullingGroup> m_FreeList = new List<CullingGroup>();

        public CullingGroup Alloc()
        {
            CullingGroup group;
            if(m_FreeList.Count > 0)
            {
                group = m_FreeList[0];
                m_FreeList.Remove(group);
            }
            else
            {
                group = new CullingGroup();
            }
            return group;
        }

        public void Free(CullingGroup group)
        {
            m_FreeList.Add(group);
        }

        public void Cleanup()
        {
            foreach( CullingGroup group in m_FreeList)
            {
                group.Dispose();
            }
            m_FreeList.Clear();
        }
    }
}
