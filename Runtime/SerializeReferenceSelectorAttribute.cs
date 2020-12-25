#nullable enable
namespace UnityEngine
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SerializeReferenceSelectorAttribute : PropertyAttribute
    {
        public readonly bool IsIncludeMono;

        public SerializeReferenceSelectorAttribute(bool isIncludeMono = false)
        {
            IsIncludeMono = isIncludeMono;
        }
    }
}
