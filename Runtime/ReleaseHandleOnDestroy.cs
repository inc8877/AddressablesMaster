using System;
using UnityEngine;

namespace AddressablesMaster
{
    [DisallowMultipleComponent]
    public class ReleaseHandleOnDestroy : MonoBehaviour
    {
        public void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }

        public event Action OnDestroyEvent;
    }
}