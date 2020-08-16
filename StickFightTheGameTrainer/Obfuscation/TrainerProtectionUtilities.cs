using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StickFightTheGameTrainer.Obfuscation
{
    internal static class TrainerProtectionUtilities
    {
        private static readonly List<string> _unityMethods = new List<string> {
            "Awake",
            "FixedUpdate",
            "LateUpdate",
            "OnAnimatorIK",
            "OnAnimatorMove",
            "OnApplicationFocus",
            "OnApplicationPause",
            "OnApplicationQuit",
            "OnAudioFilterRead",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnCollisionEnter",
            "OnCollisionEnter2D",
            "OnCollisionExit",
            "OnCollisionExit2D",
            "OnCollisionStay",
            "OnCollisionStay2D",
            "OnConnectedToServer",
            "OnControllerColliderHit",
            "OnDestroy",
            "OnDisable",
            "OnDisconnectedFromServer",
            "OnDrawGizmos",
            "OnDrawGizmosSelected",
            "OnEnable",
            "OnFailedToConnect",
            "OnFailedToConnectToMasterServer",
            "OnGUI",
            "OnJointBreak",
            "OnJointBreak2D",
            "OnMasterServerEvent",
            "OnMouseDown",
            "OnMouseDrag",
            "OnMouseEnter",
            "OnMouseExit",
            "OnMouseOver",
            "OnMouseUp",
            "OnMouseUpAsButton",
            "OnNetworkInstantiate",
            "OnParticleCollision",
            "OnParticleSystemStopped",
            "OnParticleTrigger",
            "OnParticleUpdateJobScheduled",
            "OnPlayerConnected",
            "OnPlayerDisconnected",
            "OnPostRender",
            "OnPreCull",
            "OnPreRender",
            "OnRenderImage",
            "OnRenderObject",
            "OnSerializeNetworkView",
            "OnServerInitialized",
            "OnTransformChildrenChanged",
            "OnTransformParentChanged",
            "OnTriggerEnter",
            "OnTriggerEnter2D",
            "OnTriggerExit",
            "OnTriggerExit2D",
            "OnTriggerStay",
            "OnTriggerStay2D",
            "OnValidate",
            "OnWillRenderObject",
            "Reset",
            "Start",
            "Update",
            "BroadcastMessage",
            "CompareTag",
            "GetComponent",
            "GetComponentInChildren",
            "GetComponentInParent",
            "GetComponents",
            "GetComponentsInChildren",
            "GetComponentsInParent",
            "SendMessage",
            "SendMessageUpwards",
            "TryGetComponent",
            "GetInstanceID",
            "ToString",
            "Destroy",
            "DestroyImmediate",
            "DontDestroyOnLoad",
            "FindObjectOfType",
            "FindObjectsOfType",
            "Instantiate",
            "CancelInvoke",
            "Invoke",
            "InvokeRepeating",
            "IsInvoking",
            "StartCoroutine",
            "StopAllCoroutines",
            "StopCoroutine",
            "print"
        };

        internal static async Task<int> ObfuscateTrainer(ModuleDefMD targetModule)
        {
            var trainerManagerModuleDef = targetModule.Find("TrainerManager", false);
            var trainerOptionsModuleDef = targetModule.Find("TrainerOptions", false);

            if(trainerManagerModuleDef == null)
            {
                return await Task.FromResult(1);
            }

            if (trainerOptionsModuleDef == null)
            {
                return await Task.FromResult(2);
            }

            // Rename fields
            var counter = 0;
            foreach (var field in trainerOptionsModuleDef.Fields)
            {
                field.Name = "f" + counter;
                counter++;
            }

            counter = 0;
            foreach (var field in trainerManagerModuleDef.Fields)
            {
                field.Name = "f" + counter;
                counter++;
            }

            // Rename properties
            counter = 0;
            foreach (var property in trainerOptionsModuleDef.Properties)
            {
                property.Name = "p" + counter;
                counter++;
            }

            counter = 0;
            foreach (var property in trainerManagerModuleDef.Properties)
            {
                property.Name = "p" + counter;
                counter++;
            }

            // Rename methods
            counter = 0;
            foreach (var method in trainerOptionsModuleDef.Methods)
            {
                if (method.IsConstructor)
                {
                    continue;
                }

                // Ignore inherited unity methods
                if (_unityMethods.Any(unityMethod => unityMethod.Equals(method.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Rename local variables
                var localCounter = 0;
                foreach (var variable in method.Body.Variables)
                {
                    variable.Name = "v" + localCounter;
                    localCounter++;
                }

                method.Name = "m" + counter;
                counter++;
            }

            counter = 0;
            foreach (var method in trainerManagerModuleDef.Methods)
            {
                if (method.IsConstructor)
                {
                    continue;
                }

                // Ignore inherited unity methods
                if (_unityMethods.Any(unityMethod => unityMethod.Equals(method.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Rename local variables
                var localCounter = 0;
                foreach (var variable in method.Body.Variables)
                {
                    variable.Name = "v" + localCounter;
                    localCounter++;
                }

                method.Name = "m" + counter;
                counter++;
            }

            // Rename modules
            trainerOptionsModuleDef.Name = "OptionsOne";
            trainerManagerModuleDef.Name = "OptionsTwo";

            return await Task.FromResult(0);
        }
    }
}
