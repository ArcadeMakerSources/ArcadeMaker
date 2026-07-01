using ArcadeMaker.Core.Models;
using ArcadeMaker.IDE.Items;
using System;
using System.Collections.Generic;

namespace ArcadeMaker.IDE.Debugging.Solutions
{
    internal class RemoveCollisionWithDeletedObjectsSolution(string deletedObjectName, GameObject? onlyFromObj = null) :
        ProjectError.Solution($"Remove 'Collision with {deletedObjectName}' event{(onlyFromObj == null ? "s" : "")} from {(onlyFromObj == null ? "all objects" : $"object {onlyFromObj.name}")}")
    {
        internal override void Apply()
        {
            var result = MessageBox.Show
            (
                $"Are you sure you want to remove 'Collision with {deletedObjectName}' event{(onlyFromObj == null ? "s" : "")} from {(onlyFromObj == null ? "all objects" : $"object {onlyFromObj.name}")}?\nAny script associated with {(onlyFromObj == null ? "them" : "it")} will be deleted too.",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                // remove all collision events
                foreach (var obj in onlyFromObj == null ? Environment.project.items.OfType<GameObject>() : [onlyFromObj])
                {
                    obj.Events.RemoveAll(ev => ev is CollisionEvent colEv && colEv.Param == deletedObjectName);
                }

                Debugging.Debug.TryBuild();
            }
        }
    }
}