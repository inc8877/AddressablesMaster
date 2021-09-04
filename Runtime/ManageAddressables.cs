using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace AddressablesMaster
{
    public static partial class ManageAddressables
    {
        private static readonly Dictionary<string, List<IResourceLocation>> _locations;
        private static readonly List<IResourceLocation> _noLocation;
        private static readonly Dictionary<string, Object> _assets;
        private static readonly Dictionary<string, SceneInstance> _scenes;
        private static readonly Dictionary<string, List<GameObject>> _instances;
        private static readonly Queue<List<GameObject>> _instanceListPool;
        private static readonly List<GameObject> _noInstanceList;
        private static readonly List<object> _keys;

        static ManageAddressables()
        {
            _locations = new Dictionary<string, List<IResourceLocation>>();
            _noLocation = new List<IResourceLocation>(0);
            _assets = new Dictionary<string, Object>();
            _scenes = new Dictionary<string, SceneInstance>();
            _instances = new Dictionary<string, List<GameObject>>();
            _instanceListPool = new Queue<List<GameObject>>();
            _noInstanceList = new List<GameObject>(0);
            _keys = new List<object>();
        }

        public static IReadOnlyList<object> Keys => _keys;

        public static bool SuppressWarningLogs { get; set; }

        public static bool SuppressErrorLogs { get; set; }

        public static bool ContainsAsset(string key)
        {
            return _assets.ContainsKey(key) && _assets[key];
        }

        public static bool ContainsKey(object key)
        {
            return _keys.Contains(key);
        }

        public static bool TryGetScene(string key, out SceneInstance scene)
        {
            scene = default;

            RuntimeKeyIsValid(key, true);

            if (_scenes.TryGetValue(key, out var value))
            {
                scene = value;
                return true;
            }

            if (!SuppressWarningLogs)
                Debug.LogWarning($"No scene with key={key} has been loaded through {nameof(ManageAddressables)}.");

            return false;
        }

        public static IReadOnlyList<IResourceLocation> GetLocations(string key)
        {
            RuntimeKeyIsValid(key, true);

            if (!_locations.TryGetValue(key, out var list))
                return _noLocation;

            return list;
        }

        public static T GetAsset<T>(string key) where T : Object
        {
            RuntimeKeyIsValid(key, true);

            return GetAssetInternal<T>(key);
        }

        public static T GetAsset<T>(AssetReference reference) where T : Object
        {
            RuntimeKeyIsValid(reference, out var key, true);

            return GetAssetInternal<T>(key);
        }

        public static bool TryGetScene(AssetReference reference, out SceneInstance scene)
        {
            scene = default;

            RuntimeKeyIsValid(reference, out var key, true);

            if (_scenes.TryGetValue(key, out var value))
            {
                scene = value;
                return true;
            }

            return false;
        }

        public static bool TryGetAsset<T>(string key, out T asset) where T : Object
        {
            asset = default;

            RuntimeKeyIsValid(key, true);

            return TryGetAssetInternal(key, out asset);
        }

        public static bool TryGetAsset<T>(AssetReference reference, out T asset) where T : Object
        {
            asset = default;

            RuntimeKeyIsValid(reference, out var key, true);

            return TryGetAssetInternal(key, out asset);
        }

        private static void Clear()
        {
            _keys.Clear();
            _locations.Clear();
            _assets.Clear();
            _scenes.Clear();
        }

        private static List<GameObject> GetInstanceList()
        {
            if (_instanceListPool.Count > 0)
                return _instanceListPool.Dequeue();

            return new List<GameObject>();
        }

        private static void PoolInstanceList(List<GameObject> list)
        {
            list.Clear();
            _instanceListPool.Enqueue(list);
        }

        private static string ConvertRuntimeKey(IKeyEvaluator keyEvaluator)
        {
            return keyEvaluator.RuntimeKey.ToString();
        }

        private static bool RuntimeKeyIsValid(string key, bool throwExceptionOnCase = false)
        {
            if (!string.IsNullOrEmpty(key)) return true;
            if (throwExceptionOnCase) throw new InvalidKeyException(key);
            return false;
        }

        private static bool RuntimeKeyIsValid(AssetReference reference, out string result,
            bool throwExceptionOnCase = false)
        {
            if (throwExceptionOnCase) _ = reference ?? throw new ArgumentNullException(nameof(reference));

            return !string.IsNullOrEmpty(result = ConvertRuntimeKey(reference));
        }

        private static T GetAssetInternal<T>(string key) where T : Object
        {
            if (!_assets.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.CannotFindAssetByKey(key));

                return default;
            }

            if (_assets[key] is T asset)
                return asset;

            if (!SuppressWarningLogs)
                Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

            return default;
        }

        private static bool TryGetAssetInternal<T>(string key, out T asset) where T : Object
        {
            asset = default;

            if (!_assets.ContainsKey(key))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.CannotFindAssetByKey(key));

                return false;
            }

            if (_assets[key] is T assetT)
            {
                asset = assetT;
                return true;
            }

            if (!SuppressWarningLogs)
                Debug.LogWarning(Exceptions.AssetKeyNotInstanceOf<T>(key));

            return false;
        }

        public static void ReleaseAsset(string key)
        {
            RuntimeKeyIsValid(key, true);

            if (!_assets.TryGetValue(key, out var asset))
                return;

            _assets.Remove(key);
            Addressables.Release(asset);
        }

        public static void ReleaseAsset(AssetReference reference)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (!_assets.ContainsKey(key))
                return;

            _assets.Remove(key);
            reference.ReleaseAsset();
        }

        public static IReadOnlyList<GameObject> GetInstances(string key)
        {
            RuntimeKeyIsValid(key, true);

            if (_instances.TryGetValue(key, out var instanceList))
                return instanceList;

            return _noInstanceList;
        }

        public static IReadOnlyList<GameObject> GetInstances(AssetReference reference)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            if (_instances.TryGetValue(key, out var instanceList))
                return instanceList;

            return _noInstanceList;
        }

        public static void ReleaseInstances(string key)
        {
            RuntimeKeyIsValid(key, true);

            ReleaseInstanceInternal(key);
        }

        public static void ReleaseInstances(AssetReference reference)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            ReleaseInstanceInternal(key);
        }

        private static void ReleaseInstanceInternal(string key)
        {
            if (!_instances.TryGetValue(key, out var instanceList))
                return;

            _instances.Remove(key);

            foreach (var instance in instanceList) Addressables.ReleaseInstance(instance);

            PoolInstanceList(instanceList);
        }

        public static void ReleaseInstance(string key, GameObject instance)
        {
            RuntimeKeyIsValid(key, true);

            ReleaseInstanceInternal(key, instance);
        }

        public static void ReleaseInstance(AssetReference reference, GameObject instance)
        {
            RuntimeKeyIsValid(reference, out var key, true);

            ReleaseInstanceInternal(key, instance);
        }

        private static void ReleaseInstanceInternal(string key, GameObject instance)
        {
            if (!instance)
                return;

            if (!_instances.TryGetValue(key, out var instanceList))
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoInstanceKeyInitialized(key), instance);

                return;
            }

            var index = instanceList.FindIndex(x => x.GetInstanceID() == instance.GetInstanceID());

            if (index < 0)
            {
                if (!SuppressWarningLogs)
                    Debug.LogWarning(Exceptions.NoInstanceKeyInitialized(key), instance);

                return;
            }

            instanceList.RemoveAt(index);
            Addressables.ReleaseInstance(instance);

            if (instanceList.Count > 0)
                return;

            _instances.Remove(key);
            PoolInstanceList(instanceList);
        }
    }
}