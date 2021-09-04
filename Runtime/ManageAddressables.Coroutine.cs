using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AddressablesMaster
{
    public static partial class ManageAddressables
    {
        public static IEnumerator InitializeCoroutine(Action onSucceeded = null, Action onFailed = null)
        {
            Clear();

            var operation = Addressables.InitializeAsync();
            yield return operation;

            OnInitializeCompleted(operation, onSucceeded, onFailed);
        }

        public static IEnumerator LoadLocationsCoroutine(object key, Action<object> onSucceeded = null,
            Action<object> onFailed = null)
        {
            if (key == null)
            {
                onFailed?.Invoke(key);
            }
            else
            {
                var operation = Addressables.LoadResourceLocationsAsync(key);
                yield return operation;

                OnLoadLocationsCompleted(operation, key, onSucceeded, onFailed);
            }
        }

        public static IEnumerator LoadAssetCoroutine<T>(string key, Action<string, T> onSucceeded = null,
            Action<string> onFailed = null) where T : Object
        {
            if (!RuntimeKeyIsValid(key))
            {
                onFailed?.Invoke(key);
            }
            else
            {
                if (!_assets.ContainsKey(key))
                {
                    var operation = Addressables.LoadAssetAsync<T>(key);
                    yield return operation;

                    OnLoadAssetCompleted(operation, key, false, onSucceeded, onFailed);
                }
                else if (_assets[key] is T asset)
                {
                    onSucceeded?.Invoke(key, asset);
                }
                else
                {
                    if (!SuppressWarningLogs)
                        Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

                    onFailed?.Invoke(key);
                }
            }
        }

        public static IEnumerator LoadAssetCoroutine<T>(AssetReferenceT<T> reference,
            Action<string, T> onSucceeded = null, Action<string> onFailed = null) where T : Object
        {
            if (!RuntimeKeyIsValid(reference, out var key))
            {
                onFailed?.Invoke(key);
            }
            else
            {
                if (!_assets.ContainsKey(key))
                {
                    var operation = reference.LoadAssetAsync<T>();
                    yield return operation;

                    OnLoadAssetCompleted(operation, key, true, onSucceeded, onFailed);
                }
                else if (_assets[key] is T asset)
                {
                    onSucceeded?.Invoke(key, asset);
                }
                else
                {
                    if (!SuppressWarningLogs)
                        Debug.LogWarning(Exceptions.AssetReferenceNotInstanceOf<T>(key));

                    onFailed?.Invoke(key);
                }
            }
        }

        private static IEnumerator ActivateSceneCoroutine(SceneInstance instance, int priority)
        {
            var operation = instance.ActivateAsync();
            operation.priority = priority;

            yield return operation;
        }

        public static IEnumerator LoadSceneCoroutine(string key, LoadSceneMode loadMode = LoadSceneMode.Single,
            bool activateOnLoad = true, int priority = 100, Action<SceneInstance> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(key))
            {
                onFailed?.Invoke(key);
            }
            else
            {
                if (_scenes.TryGetValue(key, out var scene))
                {
                    if (activateOnLoad) yield return ActivateSceneCoroutine(scene, priority);

                    onSucceeded?.Invoke(scene);
                }
                else
                {
                    var operation = Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
                    yield return operation;

                    OnLoadSceneCompleted(operation, key, onSucceeded, onFailed);
                }
            }
        }

        public static IEnumerator LoadSceneCoroutine(AssetReference reference,
            LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100,
            Action<SceneInstance> onSucceeded = null, Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(reference, out var key))
            {
                onFailed?.Invoke(string.Empty);
            }
            else
            {
                if (_scenes.TryGetValue(key, out var scene))
                {
                    if (activateOnLoad) yield return ActivateSceneCoroutine(scene, priority);

                    onSucceeded?.Invoke(scene);
                }
                else
                {
                    var operation = reference.LoadSceneAsync(loadMode, activateOnLoad, priority);
                    yield return operation;

                    OnLoadSceneCompleted(operation, key, onSucceeded, onFailed);
                }
            }
        }

        public static IEnumerator UnloadSceneCoroutine(string key, bool autoReleaseHandle = true,
            Action<string> onSucceeded = null, Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(key))
            {
                onFailed?.Invoke(key);
            }
            else
            {
                if (!_scenes.TryGetValue(key, out var scene))
                {
                    onFailed?.Invoke(key);
                }
                else
                {
                    _scenes.Remove(key);

                    var operation = Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
                    yield return operation;

                    OnUnloadSceneCompleted(operation, key, onSucceeded, onFailed);
                }
            }
        }

        public static IEnumerator UnloadSceneCoroutine(AssetReference reference, Action<string> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(reference, out var key))
            {
                onFailed?.Invoke(string.Empty);
            }
            else
            {
                if (!_scenes.ContainsKey(key))
                {
                    onFailed?.Invoke(key);
                }
                else
                {
                    _scenes.Remove(key);

                    var operation = reference.UnLoadScene();
                    yield return operation;

                    OnUnloadSceneCompleted(operation, key, onSucceeded, onFailed);
                }
            }
        }

        public static IEnumerator InstantiateCoroutine(string key, Transform parent = null, bool inWorldSpace = false,
            bool trackHandle = true, Action<string, GameObject> onSucceeded = null, Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(key))
            {
                onFailed?.Invoke(key);
            }
            else
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                yield return operation;

                OnInstantiateCompleted(operation, key, false, onSucceeded, onFailed);
            }
        }

        public static IEnumerator InstantiateCoroutine(AssetReference reference, Transform parent = null,
            bool inWorldSpace = false, Action<string, GameObject> onSucceeded = null, Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(reference, out var key))
            {
                onFailed?.Invoke(string.Empty);
            }
            else
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                yield return operation;

                OnInstantiateCompleted(operation, key, true, onSucceeded, onFailed);
            }
        }

        public static IEnumerator InstantiateWithAutoReleaseCoroutine(string key, Transform parent = null,
            bool inWorldSpace = false, bool trackHandle = true, Action<string, GameObject> onSucceeded = null,
            Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(key))
            {
                onFailed?.Invoke(key);
            }
            else
            {
                var operation = Addressables.InstantiateAsync(key, parent, inWorldSpace, trackHandle);
                yield return operation;

                AddAutoReleaseInstanceTrigger(key, operation.Result);

                OnInstantiateCompleted(operation, key, false, onSucceeded, onFailed);
            }
        }

        public static IEnumerator InstantiateWithAutoReleaseCoroutine(AssetReference reference, Transform parent = null,
            bool inWorldSpace = false, Action<string, GameObject> onSucceeded = null, Action<string> onFailed = null)
        {
            if (!RuntimeKeyIsValid(reference, out var key))
            {
                onFailed?.Invoke(string.Empty);
            }
            else
            {
                var operation = reference.InstantiateAsync(parent, inWorldSpace);
                yield return operation;

                AddAutoReleaseInstanceTrigger(reference, operation.Result);

                OnInstantiateCompleted(operation, key, true, onSucceeded, onFailed);
            }
        }
    }
}