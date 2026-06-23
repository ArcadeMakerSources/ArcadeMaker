using ArcadeMaker.Core.Models;
using ArcadeMaker.IDE.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.IDE.Debugging.Solutions
{
    internal class RemoveCollisionWithDeletedObjectsSolution(string deletedObjectName) :
        ProjectError.Solution($"Remove all 'Collision with {deletedObjectName}' events")
    {
        internal override void Apply()
        {
            if (MessageBox.Show(
                    $"Are you sure you want to remove all 'Collision with {deletedObjectName}' events?\nAny script associated with them will be deleted too.",
                    "Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
               ) == DialogResult.Yes)
            {
                // remove all collision events
                foreach (var obj in Environment.project.items.OfType<GameObject>())
                {
                    obj.Events.RemoveAll(ev => ev is CollisionEvent colEv && colEv.Param == deletedObjectName);
                }
            }
        }
    }
}