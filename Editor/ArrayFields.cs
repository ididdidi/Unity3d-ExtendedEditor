using UnityEditor;
using UnityEngine;

namespace UnityExtended
{
    /// <summary>
    /// Class that complements the capabilities of the editor GUI
    /// </summary>
    public static partial class ExtendedEditorGUI
    {
        /// <summary>
        /// Displays an array of objects as fields in the inspector
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Source array of elements</param>
        /// <param name="label">The label displayed above the array</param>
        /// <param name="open">Reference to a variable to control the collapse of an array</param>
        /// <param name="resizable">Is it possible to resize an array</param>
        /// <param name="requiredType">The type of the element to be returned</param>
        /// <returns>Modified array</returns>
        public static T[] ArrayFields<T>(T[] array, string label, ref bool open, bool resizable = true, params System.Type[] requiredTypes) where T : Object
        {
            var changed = GUI.changed;
            GUI.changed = false;
            open = EditorGUILayout.Foldout(open, label);
            GUI.changed = changed;

            if (array == null) { array = new T[0]; }
            int newSize = array.Length;

            if (open)
            {
                if (resizable)
                {
                    newSize = EditorGUILayout.IntField("Size", newSize);
                    newSize = newSize < 0 ? 0 : newSize;
                }

                if (newSize != array.Length)
                {
                    array = ResizeArray(array, newSize);
                }

                EditorGUI.indentLevel++;
                for (var i = 0; i < newSize; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    var @object = EditorGUILayout.ObjectField(typeof(T).Name, array[i], typeof(T), true) as T;
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (!@object) { array[i] = null; continue; }
                        foreach (var type in requiredTypes)
                        {
                            if (@object is Transform transform)
                            {
                                var cmp = transform.GetComponent(type);
                                if (cmp) { array[i] = cmp as T; continue; }
                            }
                            else
                            {
                                if (type.IsAssignableFrom(@object?.GetType())) { array[i] = @object; continue; }
                            }
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
            return array;
        }

        /// <summary>
        /// Method for resizing an array
        /// </summary>
        /// <typeparam name="T">Type of elements in array</typeparam>
        /// <param name="array">Source array of elements</param>
        /// <param name="size">New array size</param>
        /// <returns>An array of the specified size</returns>
        private static T[] ResizeArray<T>(T[] array, int size)
        {
            if (size < 0) { return array; }

            T[] newArray = new T[size];

            for (var i = 0; i < size; i++)
            {
                if (i < array.Length)
                {
                    newArray[i] = array[i];
                }
            }
            return newArray;
        }
    }
}