using HarmonyLib;
using MelonLoader;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne;  // Verify namespace!
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
#else
using ScheduleOne;  // Verify namespace!
using System.Collections.Generic;
#endif

namespace ScheduleIMod.Patches
{
    /// <summary>
    /// [EXAMPLE] IL2CPP-specific patterns and techniques
    /// </summary>
    public static class IL2CPPExamples
    {
        #if IL2CPP

        // PATTERN 1: Type Casting
        public static void ExampleTypeCasting(Object unityObject)
        {
            // Cast Il2Cpp objects using TryCast or Cast
            var camera = unityObject.TryCast<Camera>();
            if (camera != null)
            {
                MelonLogger.Msg($"Found camera: {camera.name}");
            }
        }

        // PATTERN 2: Finding Objects by Type
        public static void ExampleFindObjectsOfType()
        {
            // Use Il2CppType.Of<T>() for FindObjectsOfTypeAll
            var cameras = Resources.FindObjectsOfTypeAll(Il2CppType.Of<Camera>());
            MelonLogger.Msg($"Found {cameras.Length} cameras");
        }

        // PATTERN 3: Collections - Modern Il2CppInterop (MelonLoader 0.7.1)
        public static void ExampleCollectionHandling(List<int> il2cppList)
        {
            // Modern Il2CppInterop provides extension methods
            // You can use LINQ directly on Il2Cpp collections
            var count = il2cppList.Count;

            // Iterate using foreach
            foreach (var item in il2cppList)
            {
                MelonLogger.Msg($"Item: {item}");
            }

            // Note: Older guides mention ._items - this is OBSOLETE for MelonLoader 0.7.1
            // Use the collection directly with extension methods
        }

        // PATTERN 4: Custom MonoBehaviour Classes
        // When creating custom Unity components in IL2CPP, you MUST register them
        public class CustomBehaviour : MonoBehaviour
        {
            // Constructor required for IL2CPP
            public CustomBehaviour(System.IntPtr ptr) : base(ptr) { }

            // Static constructor to register the type
            static CustomBehaviour()
            {
                ClassInjector.RegisterTypeInIl2Cpp<CustomBehaviour>();
                MelonLogger.Msg("CustomBehaviour registered in Il2Cpp");
            }

            void Start()
            {
                MelonLogger.Msg("CustomBehaviour started!");
            }
        }

        // PATTERN 5: Adding Custom Component to GameObject
        public static void ExampleAddCustomComponent(GameObject targetObject)
        {
            // Add your custom component (automatically uses ClassInjector)
            var customComp = targetObject.AddComponent<CustomBehaviour>();
            MelonLogger.Msg("Added custom component to GameObject");
        }

        #endif
    }
}
