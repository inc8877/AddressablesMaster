<p align="center"><img src="https://user-images.githubusercontent.com/29813954/131343434-0b4c9271-7c13-49b6-a753-b084f1cd78cc.png" width="600" height="350" /> </p>

## About

Manage Addressables using Sync, Async(Built-in/UniTask), Coroutine, Lifetime Managing systems.

This solution will provide you with simple and convenient operations for Addressables assets management with results caching.
You have a synchronous, asynchronous, coroutine use-case at your disposal.
Also, if you are concerned about preventing memory leaks, you can use `lifetime` management tools to release an unused asset in time.

If you find this project useful, star it, I will be grateful!

## Table of Contents
- [About](#about)
- [Table of Contents](#table-of-contents)
- [Roadmap](#roadmap)
- [Installation](#installation)
  - [Install via OpenUPM](#install-via-openupm)
  - [Install via Git URL](#install-via-git-url)
- [How to use](#how-to-use)
  - [Intro](#intro)
  - [Short examples](#short-examples)
  - [Lifetime managment](#lifetime-managment)
    - [Addressables Extensions](#addressables-extensions)
    - [Management operations](#management-operations)
    - [Short examples](#short-examples-1)
- [Credits](#credits)

## Roadmap

|  Status   | Milestone                |
| :-------: | :----------------------- |
| :rocket:  | Comment out all code     |
| :rocket:  | Reduce memory allocation |
| :pushpin: | Completely docs          |

## Installation

### Install via OpenUPM

The package is available on the [openupm](https://openupm.com) registry. It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```bash
openupm add com.inc8877.addressables-master
```

### Install via Git URL

Open `Packages/manifest.json` with your favorite text editor. Add the following line to the dependencies block.

```json
{
  "dependencies": {
    "com.inc8877.addressables-master": "https://github.com/inc8877/AddressablesMaster.git",
   }
}
```

## How to use

### Intro

Plug in namespace first.

```c#
using AddressablesMaster;
```

All basic features for asset management are available along the following path:

```c#
ManageAddressables.[SOME_COMMAND];
```

Below is a list of operations that are available in each control model:

- Initialize
- LoadLocations
- LoadAsset
- LoadScene
- UnloadScene
- Instantiate
- InstantiateWithAutoRelease

If you are using the `UniTask` in a project and want to use it in an asynchronous control model, then connect it by following the path `Tools > Addressables Master > UniTask > On`.
`.NET` asynchronous operating model is used by default.
> Carefully! If you switch the asynchronous model, all existing code will be invalidated as each model has its own implementation.

### Short examples

For examples we will take some data:

```c#
// Some data for examples
public string startupSound;
public AssetReferenceGameObject props;
public AssetReferenceMaterial material;
public AudioSource audioSource;

[Serializable]
public class AssetReferenceMaterial : AssetReferenceT<Material>
{
  public AssetReferenceMaterial(string guid) : base(guid) { }
}
```

`Sync`

```c#
audioSource.PlayOneShot(ManageAddressables.LoadAssetSync<AudioClip>(startupSound));
            
var _material = ManageAddressables.LoadAssetSync(material);
            
ManageAddressables.InstantiateSync(props).GetComponent<MeshRenderer>().material = _material;
```

`Async .NET`

```c#
ManageAddressables.LoadAssetAsync(material, _material => ManageAddressables.InstantiateAsync(props,
                onCompletion: _go => _go.GetComponent<MeshRenderer>().material = _material));
```

`Async UniTask`

```c#
ManageAddressables.LoadAssetAsync(material).ContinueWith(result =>
            ManageAddressables.InstantiateAsync(props).ContinueWith(x =>
            x.Value.GetComponent<MeshRenderer>().material = result));
```

`Coroutine`

```c#
StartCoroutine(ManageAddressables.LoadAssetCoroutine(material, (key1, result1) =>
                StartCoroutine(ManageAddressables.InstantiateCoroutine(props,
                    onSucceeded: (key2, result2) => result2.GetComponent<MeshRenderer>().material = result1))));
```

### Lifetime managment

When you load an addressable asset, you should release it as soon as you don't need it anymore, forgetting to do this can lead to many bad processes at runtime. Using the `Addressables Master` you can bind a release to the `GameoObject` that will do it for you automatically as soon as it is destroyed.

The `Addressables Master` has two ways to manage the release of objects, the first is to use [methods of extending the basic operations](#addressables-extensions) of the Addressables, the second is to use separate methods of life management.

#### Addressables Extensions

If you need to use standard operations for working with addressables assets, then you can use the extensions.

```c#
public static async Task<AsyncOperationHandle<T>> AddAutoRelease<T>(this AsyncOperationHandle<T> operationHandle, GameObject targetGO)
```

```c#
public static async Task<AsyncOperationHandle<T>> AddAutoRelease<T>(this AsyncOperationHandle<T> operationHandle, GameObject targetGO, Action onCompletion)
```

```c#
public static async Task<AsyncOperationHandle<GameObject>> AddReleaseOnDestroy(this AsyncOperationHandle<GameObject> operationHandle)
```

Examples:

```c#
GameObject go = new GameObject("Temp");

assetReferenceMaterial.LoadAssetAsync().AddAutoRelease(go);

figureAssetRefGO.InstantiateAsync().AddReleaseOnDestroy();
```

#### Management operations

`Sync`

```c#
public static GameObject InstantiateSyncWithAutoRelease(string key, Transform parent = null,
            bool inWorldSpace = false)
```

```c#
public static GameObject InstantiateSyncWithAutoRelease(AssetReference assetReference, Transform parent = null,
            bool inWorldSpace = false)
```

`Async .NET`

```c#
public static async Task<GameObject> InstantiateAsyncWithAutoRelease(string key, Transform parent = null,
            bool inWorldSpace = false, Action<GameObject> onCompletion = null)
```

```c#
public static async Task<GameObject> InstantiateAsyncWithAutoRelease(AssetReference assetReference, Transform parent = null, bool inWorldSpace = false)
```

`Async UniTask`

```c#
public static async UniTask<GameObject> InstantiateAsyncWithAutoRelease(string key, Transform parent = null,
            bool inWorldSpace = false, Action<GameObject> onCompletion = null)
```

```c#
public static async UniTask<GameObject> InstantiateAsyncWithAutoRelease(AssetReference assetReference,
            Transform parent = null, bool inWorldSpace = false, Action<GameObject> onCompletion = null)
```

`Coroutine`

```c#
public static IEnumerator InstantiateWithAutoReleaseCoroutine(string key, Transform parent = null,
            bool inWorldSpace = false, bool trackHandle = true, Action<string, GameObject> onSucceeded = null,
            Action<string> onFailed = null)
```

```c#
public static IEnumerator InstantiateWithAutoReleaseCoroutine(AssetReference reference, Transform parent = null,
            bool inWorldSpace = false, Action<string, GameObject> onSucceeded = null, Action<string> onFailed = null)
```

`Methods not tied to a specific management model`

```c#
public static void AddAutoReleaseAssetTrigger(string key, GameObject targetGO)
```

```c#
public static void AddAutoReleaseAssetTrigger(AssetReference assetReference, GameObject targetGO)
```

```c#
public static void AddAutoReleaseInstanceTrigger(string key, GameObject targetGO)
```

```c#
public static void AddAutoReleaseInstanceTrigger(AssetReference assetReference, GameObject targetGO)
```

#### Short examples

```c#
ManageAddressables.InstantiateSyncWithAutoRelease(figureAssetRefGO);
ManageAddressables.InstantiateAsyncWithAutoRelease(figureAssetRefGO, onCompletion: x => x.transform.position = Vector3.up);

// or

var material = ManageAddressables.LoadAssetAsync(assetReferenceMaterial).Result.Value;
GameObject tempGO = new GameObject("Temp");
ManageAddressables.AddAutoReleaseAssetTrigger(assetReferenceMaterial, tempGO); // assetReferenceMaterial will be released as soon as tempGO is destroyed
```

## Credits

The project is based on a [Laicasaane](https://github.com/laicasaane) solution named [Unity Addressables Manager](https://github.com/laicasaane/unity-addressables-manager).

Logo background by founder of [Kvistholt Photography](https://unsplash.com/@freeche).
