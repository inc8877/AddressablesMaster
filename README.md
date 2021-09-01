<p align="center"><img src="https://user-images.githubusercontent.com/29813954/131343434-0b4c9271-7c13-49b6-a753-b084f1cd78cc.png" width="600" height="350" /> </p>

## About

Manage Addressables using Sync, Async(Built-in/UniTask), Coroutine, Lifetime Managing systems.

This solution will provide you with simple and convenient operations for Addressables assets management. You have a synchronous, asynchronous, coroutine use-case at your disposal. Also, if you are concerned about preventing memory leaks, you can use `lifetime` managment tools to release an unused asset in time.

If you find this project useful, star it, I will be grateful!

## Table of Contents
- [About](#about)
- [Table of Contents](#table-of-contents)
- [Roadmap](#roadmap)
- [Installation](#installation)
  - [Install via OpenUPM](#install-via-openupm)
  - [Install via Git URL](#install-via-git-url)
- [How to use](#how-to-use)
  - [Sync](#sync)
    - [Examples](#examples)
  - [Async](#async)
    - [Examples](#examples-1)
  - [Lifetime managment](#lifetime-managment)
    - [Examples](#examples-2)
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

Plugin nampespace first.

```c#
using AddressablesMaster;
```

All basic features for asset management are available along the following path:

```c#
ManageAddressables.[SOME_COMMAND];
```

The `AddressablesMaster` implements the ability to manage assets in several ways, synchronously, asynchronously and coroutines.

> Important! The use of an asynchronous model depends on the presence of `UniTask` in the project, if it does not exist in the project, then the `.NET` async system is used by default, if the project has `UniTask`, then it is used. When using asynchronous operations with or without `UniTask`, the code will remain the same.

No matter which management model you use, each has basic control operations with its own unique implementation. For example this is how initialization looks like on each model:

```c#
ManageAddressables.InitializeSync();
ManageAddressables.InitializeAsync();
StartCoroutine(ManageAddressables.InitializeCoroutine());
```

For all examples we will use the following data:

```c#
public string keyOfAddressablesFigure;
public AssetReferenceGameObject figureAssetRefGO;
public AssetReferenceMaterial assetReferenceMaterial;

[Serializable]
public class AssetReferenceMaterial : AssetReferenceT<Material>
{
  public AssetReferenceMaterial(string guid) : base(guid) { }
}
```

### Sync

When using a synchronous operation script, in response to a command output that has a return value, you will receive a value of the type specified in the signature of the operation being called, this seems obvious, but for example, by calling the same operation using the asynchronous model, you will get a custom type similar to an `AsyncOperationHandle`.

#### Examples

```c#
ManageAddressables.InstantiateSync(keyOfAddressablesFigure).transform.position = Vector3.back;
ManageAddressables.InstantiateSync(figureAssetRefGO);
Debug.Log(ManageAddressables.LoadAssetSync(assetReferenceMaterial).color.a);
```

### Async

When using a synchronous operation script, in response to a command output that has a return value, you will get a `Task` that contains as a result a custom class named `OperationResult` with several fields, `Succeeded` and `Value`. You can process this data in accordance with the logic of your project.

#### Examples

```c#
ManageAddressables.InstantiateAsync(figureAssetRefGO, onCompletion: (go =>
{
  ManageAddressables.LoadAssetAsync(assetReferenceMaterial, material => go.GetComponent<MeshRenderer>().material = material);
}));
```

### Lifetime managment

When you load an addressable asset, you should release it as soon as you don't need it anymore, forgetting to do this can lead to many bad processes at runtime. Using the `Addressables Master` you can bind a release to the `GameoObject` that will do it for you automatically as soon as it is destroyed.

Below are methods for working with assets with lifetime management:

`Base methods`

> Important! Use base methods below only for assets that instanced via `Addressables Master`.

```c#
public static void AddAutoReleaseAssetTrigger(AssetReference assetReference, GameObject targetGO)
```

```c#
public static void AddAutoReleaseInstanceTrigger(AssetReference assetReference, GameObject targetGO)
```

`Sync`

```c#
public static GameObject InstantiateSyncWithAutoRelease(AssetReference assetReference, Transform parent = null, bool inWorldSpace = false)
```

`Async`

```c#
public static async Task<GameObject> InstantiateAsyncWithAutoRelease(AssetReference assetReference, Transform parent = null, bool inWorldSpace = false)
```

#### Examples

```c#
ManageAddressables.InstantiateSyncWithAutoRelease(figureAssetRefGO);
ManageAddressables.InstantiateAsyncWithAutoRelease(figureAssetRefGO).Result.transform.position = Vector3.up;

// or

var material = ManageAddressables.LoadAssetAsync(assetReferenceMaterial).Result.Value;
GameObject tempGO = new GameObject("Temp");
ManageAddressables.AddAutoReleaseAssetTrigger(assetReferenceMaterial, tempGO); // assetReferenceMaterial will be released as soon as tempGO is destroyed
```

## Credits

The project is based on a [Laicasaane](https://github.com/laicasaane) solution named [Unity Addressables Manager](https://github.com/laicasaane/unity-addressables-manager).

Logo background by founder of [Kvistholt Photography](https://unsplash.com/@freeche).
