using System;
using UnityEngine;

namespace AddressablesMaster
{
    public class ReleaseHandleOnDestroy : MonoBehaviour
    {
        public void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }

        public event Action OnDestroyEvent;
    }
}