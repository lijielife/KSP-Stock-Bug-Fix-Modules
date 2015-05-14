﻿/*
 * This module written by Claw. For more details, please visit
 * http://forum.kerbalspaceprogram.com/threads/97285
 * 
 * This mod is covered under the CC-BY-NC-SA license. See the readme.txt for more details.
 * (https://creativecommons.org/licenses/by-nc-sa/4.0/)
 * 
 *
 * ModuleProceduralFairingFix - Written for KSP v1.0
 * 
 * - Fixes some bugs with pulling and replacing fairings.
 * - (Plus) Activates a tweakable slider for the user to select the number of panels on the fairing.
 * - (Plus) Activates a tweakable slider to control ejection forces on the panels.
 * 
 * Change Log:
 * - v01.03  (13 May 15)  Updates and minor adjustments, incorporates StockPlus
 * - v01.02  (3 May 15)   Moved ejection force out to Module Manager
 * - v01.01  (2 May 15)   Updated and recompiled for KSP 1.0.2
 * - v01.00  (27 Apr 15)  Initial Release
 * 
 */

using UnityEngine;
using KSP;
using System.Reflection;

namespace ClawKSP
{
    public class MPFFix : PartModule
    {
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Panels")]
        [UI_FloatRange(minValue = 1f, maxValue = 8f, stepIncrement = 1f)]
        public float nArcs = 0f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Ejection Force")]
        [UI_FloatRange(minValue = 0f, maxValue = 200f, stepIncrement = 10f)]
        public float ejectionForce = -1f;

        ModuleProceduralFairing FairingModule;

        [KSPField(isPersistant = false)]
        public bool plusEnabled = false;

        private PartModule GetModule(string moduleName)
        {
            for (int indexModules = 0; indexModules < part.Modules.Count; indexModules++)
            {
                if (moduleName == part.Modules[indexModules].moduleName)
                {
                    return (part.Modules[indexModules]);
                }
            }

            return (null);

        }  // GetModule

        private void SetupStockPlus()
        {
            if (StockPlusController.plusActive == false || plusEnabled == false)
            {
                plusEnabled = false;
                return;
            }

            Fields["nArcs"].guiActiveEditor = true;
            Fields["ejectionForce"].guiActiveEditor = true;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Debug.Log("MPFPlus.OnStart(): v00.01");

            FairingModule = (ModuleProceduralFairing) GetModule("ModuleProceduralFairing");

            if (null == FairingModule)
            {
                Debug.LogWarning("ModuleProceduralFairingFix.Start(): Did not find Fairing Module.");
                return;
            }

            if (plusEnabled == true)
            {
                if (ejectionForce == -1)
                {
                    ejectionForce = FairingModule.ejectionForce;
                }

                if (nArcs == 0)
                {
                    nArcs = FairingModule.nArcs;
                }
            }

            GameEvents.onPartRemove.Add(RemovePart);
        }

        public void RemovePart(GameEvents.HostTargetAction<Part, Part> RemovedPart)
        {
            if (null == FairingModule) { return; }

            if (RemovedPart.target == part)
            {
                if (FairingModule.xSections.Count > 0)
                {
                    Debug.LogWarning("Deleting Fairing");
                    FairingModule.DeleteFairing();
                }
                MethodInfo MPFMethod = FairingModule.GetType().GetMethod("DumpInterstage", BindingFlags.NonPublic | BindingFlags.Instance);

                if (MPFMethod != null)
                {
                    MPFMethod.Invoke(FairingModule, new object[] { });
                }
            }
        }

        public void FixedUpdate()
        {
            if (FairingModule == null) { return; }

            if (plusEnabled == true)
            {

                if (FairingModule.nArcs != nArcs)
                {
                    FairingModule.nArcs = (int)nArcs;

                    MethodInfo MPFMethod = FairingModule.GetType().GetMethod("WipeMesh", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (MPFMethod != null)
                    {
                        MPFMethod.Invoke(FairingModule, new object[] { });
                    }

                    MPFMethod = FairingModule.GetType().GetMethod("SpawnMeshes", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (MPFMethod != null)
                    {
                        MPFMethod.Invoke(FairingModule, new object[] { true });
                    }
                }

                FairingModule.ejectionForce = ejectionForce;
            }
        }

        public void OnDestroy()
        {
            GameEvents.onPartRemove.Remove(RemovePart);
        }
    }
}