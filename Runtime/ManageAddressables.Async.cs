#if !USE_UNITASK
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AddressablesMaster
{
    public static partial class ManageAddressables
    {
        public static async Task<OperationResult<IResourceLocator>> InitializeAsync(Action<OperationResult<IResourceLocator>> onCompletion
 = null)
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                await operation.Task;

                OnInitializeCompleted(operation);
                
                OperationResult<IResourceLocator> operationResult = operation;
                
                onCompletion?.Invoke(operationResult);
                
                return operationResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<object>> LoadLocationsAsync(object key, Action<OperationResult<object>> onCompletion
 = null)
        {
            _ = key ?? throw new InvalidKeyException(key);
            
            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                await operation.Task;

                OnLoadLocationsCompleted(operation, key);
                
                var operationResult =
 new OperationResult<object>(operation.Status == AsyncOperationStatus.Succeeded, key);
                
                onCompletion?.Invoke(operationResult);
                
                return operationResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<T>> LoadAssetAsync<T>(string key, Action<T> onCompletion = null)
            where T : Object
        {
            RuntimeKeyIsValid(key, true);

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                    return new OperationResult<T>(true, asset);

                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                await operation.Task;

                OnLoadAssetCompleted(operation, key, false);
                onCompletion?.Invoke(operation.Result);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<T>> LoadAssetAsync<T>(AssetReferenceT<T> reference,
            Action<T> onCompletion = null) where T : Object
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T asset)
                    return new OperationResult<T>(true, asset);

                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                await operation.Task;

                OnLoadAssetCompleted(operation, key, true);
                onCompletion?.Invoke(operation.Result);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<SceneInstance>> LoadSceneAsync(string key,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, in scene);
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                await operation.Task;

                OnLoadSceneCompleted(operation, key);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<SceneInstance>> LoadSceneAsync(AssetReference reference, 
            LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, in scene);
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                await operation.Task;

                OnLoadSceneCompleted(operation, key);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<SceneInstance>> UnloadSceneAsync(string key,
            bool autoReleaseHandle = true)
        {
            RuntimeKeyIsValid(key, true);

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneKeyLoaded(key));

                return default;
            }

            _scenes.Remove(key);

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                await operation.Task;

                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<SceneInstance>> UnloadSceneAsync(AssetReference reference)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneReferenceLoaded(key));

                return default;
            }

            _scenes.Remove(key);

            try
            {
                var operation = reference.UnLoadScene();
                await operation.Task;

                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        public static async Task<OperationResult<GameObject>> InstantiateAsync(string key, Transform parent = null, 
            bool inWorldSpace = false, bool trackHandle = true, Action<GameObject> onCompletion = null)
        {
            RuntimeKeyIsValid(key, true);

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                await operation.Task;

                OnInstantiateCompleted(operation, key, false);
                onCompletion?.Invoke(operation.Result);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task<OperationResult<GameObject>> InstantiateAsync(AssetReference reference,
            Transform parent = null, bool inWorldSpace = false, Action<GameObject> onCompletion = null)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                await operation.Task;

                OnInstantiateCompleted(operation, key, true);
                onCompletion?.Invoke(operation.Result);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        /// <summary>
        /// Instantiates game object on the scene asynchronously and adds a trigger to the instance that
        /// releases <see cref="AsyncOperationHandle"/> when the instance is destroyed.
        /// </summary>
        /// <returns>Instantiated GameObject on the scene.</returns>
        public static async Task<GameObject> InstantiateAsyncWithAutoRelease(string key, Transform parent = null,
            bool inWorldSpace = false, Action<GameObject> onCompletion = null)
        {
            var operationResult = LoadAssetAsync<GameObject>(key);
            await operationResult;
            
            var instantiatedGO = Object.Instantiate(operationResult.Result.Value, parent, inWorldSpace);
            AddAutoReleaseAssetTrigger(key, instantiatedGO);
            
            onCompletion?.Invoke(instantiatedGO);
            
            return instantiatedGO;
        }
        
        /// <summary>
        /// Instantiates game object on the scene asynchronously and adds a trigger to the instance that
        /// releases <see cref="AsyncOperationHandle"/> when the instance is destroyed.
        /// </summary>
        /// <returns>Instantiated game object on the scene.</returns>
        public static async Task<GameObject> InstantiateAsyncWithAutoRelease(
            AssetReference assetReference, Transform parent = null, bool inWorldSpace = false,
            Action<GameObject> onCompletion = null)
        {
            var operationResult = LoadAssetAsync((AssetReferenceT<GameObject>)assetReference);
            await operationResult;
            
            var instantiatedGO = Object.Instantiate(operationResult.Result.Value, parent, inWorldSpace);
            AddAutoReleaseAssetTrigger(assetReference, instantiatedGO);
            
            onCompletion?.Invoke(instantiatedGO);
            
            return instantiatedGO;
        }
        
        private static async Task ActivateAsync(SceneInstance scene, int priority)
        {
            var operation = scene.ActivateAsync();
            operation.priority = priority;

            await operation;
        }
    }

    internal static partial class AsyncOperationExtensions
    {
        public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation operation)
        {
            return new AsyncOperationAwaiter(operation);
        }
    }

    internal readonly struct AsyncOperationAwaiter : INotifyCompletion
    {
        private readonly AsyncOperation operation;

        public AsyncOperationAwaiter(AsyncOperation operation)
        {
            this.operation = operation ?? throw new ArgumentNullException(nameof(operation));
        }

        public bool IsCompleted
            => this.operation.isDone;

        public void OnCompleted(Action continuation)
            => this.operation.completed += _ => continuation?.Invoke();

        public void GetResult()
        {
        }
    }
}

#endif