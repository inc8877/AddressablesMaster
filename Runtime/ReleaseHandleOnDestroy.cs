using System;
using UnityEngine;

namespace AddressablesMaster
{
    public class ReleaseHandleOnDestroy : MonoBehaviour
    {
        public event Action OnDestroyEvent;
        
        public void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }
    }
}