#nullable enable
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityEditor
{
    [CustomPropertyDrawer(typeof(SerializeReferenceSelectorAttribute))]
    public sealed class SerializeReferenceSelectorDrawer : PropertyDrawer
    {
        #region fields
        static readonly System.Type k_MonoBehaviourType = typeof(MonoBehaviour);

        int m_CurrentTypeIndex;
        string[]? m_DisplayNameArray;
        System.Type?[]? m_InheritedTypes;
        bool m_IsInitialized = false;
        string[]? m_TypeFullNameArray;
        #endregion

        #region methods
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference) return;

            if (!m_IsInitialized)
            {
                FindAllInheritedTypes(GetType(property));
                m_CurrentTypeIndex = System.Array.IndexOf(m_TypeFullNameArray, property.managedReferenceFullTypename);
                m_IsInitialized = true;
            }

            var selectedTypeIndex = EditorGUI.Popup(CalcDisplayNamePosition(position), m_CurrentTypeIndex, m_DisplayNameArray);
            UpdatePropertyToSelectedTypeIndex(property, selectedTypeIndex);
            EditorGUI.PropertyField(position, property, label, true);
        }

        static Rect CalcDisplayNamePosition(Rect currentPosition)
        {
            var popupPosition = new Rect(currentPosition);
            popupPosition.width -= EditorGUIUtility.labelWidth;
            popupPosition.x += EditorGUIUtility.labelWidth;
            popupPosition.height = EditorGUIUtility.singleLineHeight;
            return popupPosition;
        }

        static System.Type GetType(SerializedProperty property)
        {
            const BindingFlags k_BindingAttr =
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.FlattenHierarchy |
                BindingFlags.Instance;

            var propertyPaths = property.propertyPath.Split('.');
            var parentType = property.serializedObject.targetObject.GetType();
            var fieldInfo = parentType.GetField(propertyPaths[0], k_BindingAttr);
            var fieldType = fieldInfo.FieldType;

            if (propertyPaths.Contains("Array"))
            {
                return fieldType.IsArray
                    ? fieldType.GetElementType()
                    : fieldType.GetGenericArguments()[0];
            }
            return fieldType;
        }

        SerializeReferenceSelectorAttribute Attr => (SerializeReferenceSelectorAttribute)attribute;

        void FindAllInheritedTypes(System.Type baseType)
        {
            m_InheritedTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract && (Attr.IsIncludeMono || !k_MonoBehaviourType.IsAssignableFrom(p)))
                .Prepend(null)
                .ToArray();

            m_DisplayNameArray = m_InheritedTypes.Select(type => type == null ? "<null>" : type.ToString()).ToArray();
            m_TypeFullNameArray = m_InheritedTypes.Select(type => type == null ? "" : $"{type.Assembly.ToString().Split(',')[0]} {type.FullName}").ToArray();
        }

        void UpdatePropertyToSelectedTypeIndex(SerializedProperty property, int selectedTypeIndex)
        {
            if (m_CurrentTypeIndex == selectedTypeIndex) return;
            m_CurrentTypeIndex = selectedTypeIndex;

            var selectedType = m_InheritedTypes![selectedTypeIndex];
            property.managedReferenceValue = (selectedType == null)
                 ? null
                 : System.Activator.CreateInstance(selectedType);
        }
        #endregion
    }
}
