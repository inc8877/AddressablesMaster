using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressablesMaster
{
    public readonly struct OperationResult<T>
    {
        public readonly bool Succeeded;
        public readonly T Value;

        public OperationResult(bool succeeded, T value)
        {
            Succeeded = succeeded;
            Value = value;
        }

        public OperationResult(bool succeeded, in T value)
        {
            Succeeded = succeeded;
            Value = value;
        }

        public OperationResult(AsyncOperationStatus status, T value)
        {
            Succeeded = status == AsyncOperationStatus.Succeeded;
            Value = value;
        }

        public OperationResult(AsyncOperationStatus status, in T value)
        {
            Succeeded = status == AsyncOperationStatus.Succeeded;
            Value = value;
        }

        public void Deconstruct(out bool succeeded, out T value)
        {
            succeeded = Succeeded;
            value = Value;
        }

        public static implicit operator OperationResult<T>(in AsyncOperationHandle<T> handle)
        {
            return new OperationResult<T>(handle.Status, handle.Result);
        }

        public static implicit operator T(in OperationResult<T> result)
        {
            return result.Value;
        }
    }
}