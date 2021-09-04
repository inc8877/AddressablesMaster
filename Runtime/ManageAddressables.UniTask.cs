#if ADDRESSABLES_UNITASK && USE_UNITASK

using System;
using Cysharp.Threading.Tasks;
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
        public static async UniTask<OperationResult<IResourceLocator>> InitializeAsync()
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                await operation;

                OnInitializeCompleted(operation);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<object>> LoadLocationsAsync(object key)
        {
            _ = key ?? throw new InvalidKeyException(key);

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                await operation;

                OnLoadLocationsCompleted(operation, key);
                return new OperationResult<object>(operation.Status == AsyncOperationStatus.Succeeded, key);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<T>> LoadAssetAsync<T>(string key) where T : Object
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
                await operation;

                OnLoadAssetCompleted(operation, key, false);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<T>> LoadAssetAsync<T>(AssetReferenceT<T> reference) where T : Object
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
                await operation;

                OnLoadAssetCompleted(operation, key, true);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask ActivateSceneAsync(SceneInstance scene, int priority)
        {
            var operation = scene.ActivateAsync();
            operation.priority = priority;

            await operation;
        }

        public static async UniTask<OperationResult<SceneInstance>> LoadSceneAsync(string key,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateSceneAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, in scene);
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                await operation;

                OnLoadSceneCompleted(operation, key);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<SceneInstance>> LoadSceneAsync(AssetReference reference,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    await ActivateSceneAsync(scene, priority);

                return new OperationResult<SceneInstance>(true, in scene);
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                await operation;

                OnLoadSceneCompleted(operation, key);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<SceneInstance>> UnloadSceneAsync(string key,
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
                await operation;

                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<SceneInstance>> UnloadSceneAsync(AssetReference reference)
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
                await operation;

                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(string key,
            Transform parent = null,
            bool inWorldSpace = false,
            bool trackHandle = true)
        {
            RuntimeKeyIsValid(key, true);

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                await operation;

                OnInstantiateCompleted(operation, key, false);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async UniTask<OperationResult<GameObject>> InstantiateAsync(AssetReference reference,
            Transform parent = null,
            bool inWorldSpace = false)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                await operation;

                OnInstantiateCompleted(operation, key, true);
                return operation;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     Instantiates game object on the scene asynchronously and adds a trigger to the instance that
        ///     releases <see cref="AsyncOperationHandle" /> when the instance is destroyed.
        /// </summary>
        /// <returns>Instantiated game object on the scene.</returns>
        public static async UniTask<GameObject> InstantiateAsyncWithAutoRelease(string key, Transform parent = null,
            bool inWorldSpace = false, Action<GameObject> onCompletion = null)
        {
            var operationResult = await LoadAssetAsync<GameObject>(key);

            var instantiatedGO = Object.Instantiate(operationResult.Value, parent, inWorldSpace);

            AddAutoReleaseAssetTrigger(key, instantiatedGO);

            onCompletion?.Invoke(instantiatedGO);

            return instantiatedGO;
        }

        /// <summary>
        ///     Instantiates game object on the scene asynchronously and adds a trigger to the instance that
        ///     releases <see cref="AsyncOperationHandle" /> when the instance is destroyed.
        /// </summary>
        /// <returns>Instantiated game object on the scene.</returns>
        public static async UniTask<GameObject> InstantiateAsyncWithAutoRelease(AssetReference assetReference,
            Transform parent = null, bool inWorldSpace = false, Action<GameObject> onCompletion = null)
        {
            var operationResult = await LoadAssetAsync((AssetReferenceT<GameObject>)assetReference);

            var instantiatedGO = Object.Instantiate(operationResult.Value, parent, inWorldSpace);

            AddAutoReleaseAssetTrigger(assetReference, instantiatedGO);

            onCompletion?.Invoke(instantiatedGO);

            return instantiatedGO;
        }
    }
}

#endif