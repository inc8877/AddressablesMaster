#if ADDRESSABLES_1_17

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AddressablesMaster
{
    public static partial class ManageAddressables
    {
        public static IResourceLocator InitializeSync()
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                var result = operation.WaitForCompletion();
                OnInitializeCompleted(operation);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryInitializeSync(out IResourceLocator result)
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                result = operation.WaitForCompletion();
                OnInitializeCompleted(operation);

                return result != null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryInitializeSync()
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                operation.WaitForCompletion();
                OnInitializeCompleted(operation);

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static IList<IResourceLocation> LoadLocationsSync(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                var result = operation.WaitForCompletion();
                OnLoadLocationsCompleted(operation, key);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryLoadLocationsSync(object key, out IList<IResourceLocation> result)
        {
            _ = key ?? throw new InvalidKeyException(key);

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                result = operation.WaitForCompletion();
                OnLoadLocationsCompleted(operation, key);

                return result != null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static T LoadAssetSync<T>(string key) where T : Object
        {
            RuntimeKeyIsValid(key, true);

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T assetT)
                    return assetT;


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                var result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, false);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryLoadAssetSync<T>(string key, out T result) where T : Object
        {
            RuntimeKeyIsValid(key, true);

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T assetT)
                {
                    result = assetT;
                    return true;
                }


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                result = default;
                return false;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, false);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static T LoadAssetSync<T>(AssetReferenceT<T> reference) where T : Object
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T assetT)
                    return assetT;


                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                return default;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                var result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, true);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryLoadAssetSync<T>(AssetReferenceT<T> reference, out T result) where T : Object
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T assetT)
                {
                    result = assetT;
                    return true;
                }

                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                result = default;
                return false;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                result = operation.WaitForCompletion();
                OnLoadAssetCompleted(operation, key, true);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static SceneInstance LoadSceneSync(string key,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                return scene;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                var result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryLoadSceneSync(string key, out SceneInstance result,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static SceneInstance LoadSceneSync(AssetReference reference,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                return scene;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                var result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool LoadSceneSync(AssetReference reference, out SceneInstance result,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Obsolete]
        public static bool TryLoadSceneSync(string key, LoadSceneMode loadMode, out SceneInstance result,
            bool activateOnLoad = true, int priority = 100)
        {
            RuntimeKeyIsValid(key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Obsolete]
        public static bool LoadSceneSync(AssetReference reference, LoadSceneMode loadMode, out SceneInstance result,
            bool activateOnLoad = true, int priority = 100)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                    ActivateSceneSync(scene, priority);

                result = scene;
                return true;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                result = operation.WaitForCompletion();
                OnLoadSceneCompleted(operation, key);

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static SceneInstance UnloadSceneSync(string key, bool autoReleaseHandle = true)
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
                var result = operation.WaitForCompletion();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryUnloadSceneSync(string key, out SceneInstance result,
            bool autoReleaseHandle = true)
        {
            RuntimeKeyIsValid(key, true);

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneKeyLoaded(key));

                result = default;
                return false;
            }

            _scenes.Remove(key);

            try
            {
                var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                result = operation.WaitForCompletion();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static SceneInstance UnloadSceneSync(AssetReference reference)
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
                var result = operation.WaitForCompletion();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryUnloadSceneSync(AssetReference reference, out SceneInstance result)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneReferenceLoaded(key));

                result = default;
                return false;
            }

            _scenes.Remove(key);

            try
            {
                var operation = reference.UnLoadScene();
                result = operation.WaitForCompletion();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static GameObject InstantiateSync(string key, Transform parent = null, bool inWorldSpace = false,
            bool trackHandle = true)
        {
            RuntimeKeyIsValid(key, true);

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                var result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, false);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool TryInstantiateSync(string key, out GameObject result, Transform parent = null,
            bool inWorldSpace = false, bool trackHandle = true)
        {
            RuntimeKeyIsValid(key, true);

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, false);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static GameObject InstantiateSync(AssetReference reference, Transform parent = null,
            bool inWorldSpace = false)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                var result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, true);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     Instantiates game object on the scene synchronously and adds a trigger to the instance that
        ///     releases <see cref="UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle" /> when the instance is
        ///     destroyed.
        /// </summary>
        /// <returns>Instantiated game object on the scene.</returns>
        public static GameObject InstantiateSyncWithAutoRelease(string key, Transform parent = null,
            bool inWorldSpace = false)
        {
            var tempGO = Object.Instantiate(LoadAssetSync<GameObject>(key), parent, inWorldSpace);

            AddAutoReleaseAssetTrigger(key, tempGO);
            return tempGO;
        }

        /// <summary>
        ///     Instantiates game object on the scene synchronously and adds a trigger to the instance that
        ///     releases <see cref="UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle" /> when the instance is
        ///     destroyed.
        /// </summary>
        /// <returns>Instantiated game object on the scene.</returns>
        public static GameObject InstantiateSyncWithAutoRelease(AssetReference assetReference,
            Transform parent = null, bool inWorldSpace = false)
        {
            var tempGO = Object.Instantiate(LoadAssetSync((AssetReferenceT<GameObject>)assetReference), parent,
                inWorldSpace);

            AddAutoReleaseAssetTrigger(assetReference, tempGO);
            return tempGO;
        }

        public static bool TryInstantiateSync(AssetReference reference, out GameObject result,
            Transform parent = null,
            bool inWorldSpace = false)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                result = operation.WaitForCompletion();
                OnInstantiateCompleted(operation, key, true);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static void ActivateSceneSync(in SceneInstance instance, int priority)
        {
            var operation = instance.ActivateAsync();
            operation.priority = priority;
            operation.WaitForCompletion();
        }
    }

    internal static partial class AsyncOperationExtensions
    {
        public static void WaitForCompletion(this AsyncOperation operation)
        {
            new SyncOperationAwaiter(operation).WaitForCompletion();
        }
    }

    internal readonly struct SyncOperationAwaiter
    {
        private readonly AsyncOperation operation;

        public SyncOperationAwaiter(AsyncOperation operation)
        {
            this.operation = operation;
        }

        public bool IsCompleted
        {
            get
            {
                if (operation == null)
                    return true;

                return operation.isDone;
            }
        }

        public void WaitForCompletion()
        {
            while (!IsCompleted)
            {
            }
        }
    }
}

#endif