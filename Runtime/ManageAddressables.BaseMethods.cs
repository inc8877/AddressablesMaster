using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AddressablesMaster
{
    public static partial class ManageAddressables
    {
        public static void Initialize()
        {
            Clear();

            try
            {
                var operation = Addressables.InitializeAsync();
                operation.Completed += handle => OnInitializeCompleted(handle);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void LoadLocations(object key)
        {
            _ = key ?? throw new InvalidKeyException(key);

            try
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                operation.Completed += handle => OnLoadLocationsCompleted(handle, key);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void LoadAsset<T>(string key) where T : Object
        {
            RuntimeKeyIsValid(key, true);

            if (_assets.ContainsKey(key))
            {
                if (_assets[key] is T) return;
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                return;
            }

            try
            {
                var operation = Addressables.LoadAssetAsync<T>(key);
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, false);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void LoadAsset<T>(AssetReferenceT<T> reference) where T : Object
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_assets.ContainsKey(key))
            {
                if (!(_assets[key] is T))
                    if (!SuppressWarningLogs)
                        Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                return;
            }

            try
            {
                var operation = reference.LoadAssetAsync<T>();
                operation.Completed += handle => OnLoadAssetCompleted(handle, key, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void LoadScene(string key,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                {
                    var operation = scene.ActivateAsync();
                    operation.priority = priority;
                }

                return;
            }

            try
            {
                var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                operation.Completed += handle => OnLoadSceneCompleted(handle, key);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void LoadScene(AssetReference reference,
            LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_scenes.TryGetValue(key, out var scene))
            {
                if (activateOnLoad)
                {
                    var operation = scene.ActivateAsync();
                    operation.priority = priority;
                }

                return;
            }

            try
            {
                var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                operation.Completed += handle => OnLoadSceneCompleted(handle, key);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public static void UnloadScene(string key, bool autoReleaseHandle = true)
        {
            RuntimeKeyIsValid(key, true);

            if (!_scenes.TryGetValue(key, out var scene))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneKeyLoaded(key));

                return;
            }

            _scenes.Remove(key);

            try
            {
                Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void UnloadScene(AssetReference reference)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (!_scenes.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoSceneReferenceLoaded(key));

                return;
            }

            _scenes.Remove(key);

            try
            {
                reference.UnLoadScene();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void Instantiate(string key,
            Transform parent = null,
            bool inWorldSpace = false,
            bool trackHandle = true)
        {
            RuntimeKeyIsValid(key, true);

            try
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                operation.Completed += handle => OnInstantiateCompleted(handle, key, false);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void Instantiate(AssetReference reference,
            Transform parent = null,
            bool inWorldSpace = false)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            try
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                operation.Completed += handle => OnInstantiateCompleted(handle, key, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     USE ONLY IF TARGET LOADED VIA <see cref="ManageAddressables" />.
        ///     Adds a trigger to the object and releases the target addressable asset when the game object is destroyed.
        /// </summary>
        /// <param name="key">Asset provided by a key that will be released upon game object destryed.</param>
        /// <param name="targetGO">The object to which the trigger will be attached.</param>
        public static void AddAutoReleaseAssetTrigger(string key, GameObject targetGO)
        {
            targetGO.AddComponent<ReleaseHandleOnDestroy>().OnDestroyEvent +=
                () => ReleaseAsset(key);
        }

        /// <summary>
        ///     USE ONLY IF TARGET LOADED VIA <see cref="ManageAddressables" />.
        ///     Adds a trigger to the object and releases the target addressable asset when the game object is destroyed.
        /// </summary>
        /// <param name="assetReference">Asset that will be released upon game object destryed.</param>
        /// <param name="targetGO">The object to which the trigger will be attached.</param>
        public static void AddAutoReleaseAssetTrigger(AssetReference assetReference, GameObject targetGO)
        {
            targetGO.AddComponent<ReleaseHandleOnDestroy>().OnDestroyEvent +=
                () => ReleaseAsset(assetReference);
        }

        /// <summary>
        ///     USE ONLY IF TARGET INSTANTIATED VIA <see cref="ManageAddressables" />.
        ///     Adds a trigger to the object that is invoked when the object is destroyed
        ///     and releases the target asset instance.
        /// </summary>
        /// <param name="key">Asset provided by a key that will be released upon game object destryed.</param>
        /// <param name="targetGO">The object to which the trigger will be attached.</param>
        public static void AddAutoReleaseInstanceTrigger(string key, GameObject targetGO)
        {
            targetGO.AddComponent<ReleaseHandleOnDestroy>().OnDestroyEvent +=
                () => ReleaseInstance(key, targetGO);
        }

        /// <summary>
        ///     USE ONLY IF TARGET INSTANTIATED VIA <see cref="ManageAddressables" />.
        ///     Adds a trigger to the object that is invoked when the object is destroyed
        ///     and releases the target asset instance.
        /// </summary>
        /// <param name="assetReference">Asset that will be released upon game object destryed.</param>
        /// <param name="targetGO">The object to which the trigger will be attached.</param>
        public static void AddAutoReleaseInstanceTrigger(AssetReference assetReference, GameObject targetGO)
        {
            targetGO.AddComponent<ReleaseHandleOnDestroy>().OnDestroyEvent +=
                () => ReleaseInstance(assetReference, targetGO);
        }
    }
}