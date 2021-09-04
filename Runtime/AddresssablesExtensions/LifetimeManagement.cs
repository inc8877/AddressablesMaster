using System;
using System.Threading.Tasks;
using AddressablesMaster;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.AddressableAssets
{
    public static class LifetimeManagement
    {
        /// <summary>
        ///     Binds the release of the <see cref="UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle{T}" />
        ///     to the OnDestroy of the target GameObject.
        /// </summary>
        public static async Task<AsyncOperationHandle<T>> AddAutoRelease<T>(
            this AsyncOperationHandle<T> operationHandle, GameObject targetGO)
        {
            while (!operationHandle.IsDone) await Task.Yield();

            if (operationHandle.Status == AsyncOperationStatus.Failed) return default;

            AddAutoReleaseTrigger(operationHandle, targetGO);

            return operationHandle;
        }

        /// <summary>
        ///     Binds the release of the <see cref="UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle{T}" />
        ///     to the OnDestroy of the target GameObject and invokes Action when destroyed.
        /// </summary>
        public static async Task<AsyncOperationHandle<T>> AddAutoRelease<T>(
            this AsyncOperationHandle<T> operationHandle, GameObject targetGO, Action onCompletion)
        {
            while (!operationHandle.IsDone) await Task.Yield();

            if (operationHandle.Status == AsyncOperationStatus.Failed) return default;

            var operation = AddAutoReleaseTrigger(operationHandle, targetGO);

            operation.OnDestroyEvent += onCompletion;

            return operationHandle;
        }

        /// <summary>
        ///     Binds the release of the instantiated GameObject to its OnDestroy.
        /// </summary>
        public static async Task<AsyncOperationHandle<GameObject>> AddReleaseOnDestroy(
            this AsyncOperationHandle<GameObject> operationHandle)
        {
            while (!operationHandle.IsDone) await Task.Yield();

            if (operationHandle.Status == AsyncOperationStatus.Failed) return default;

            AddAutoReleaseTrigger(operationHandle, operationHandle.Result);

            return operationHandle;
        }

        private static ReleaseHandleOnDestroy AddAutoReleaseTrigger(AsyncOperationHandle operationHandle,
            GameObject targetGO)
        {
            _ = targetGO ?? throw new ArgumentNullException(nameof(targetGO));

            var trigger = targetGO.AddComponent<ReleaseHandleOnDestroy>();

            trigger.OnDestroyEvent += () => Addressables.Release(operationHandle);

            return trigger;
        }
    }
}