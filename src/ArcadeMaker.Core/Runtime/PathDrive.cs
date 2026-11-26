using ArcadeMaker.Core.Resources;
using Exp;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Runtime;

internal class PathDrive(Instance inst, Resources.Path path, double speed, PathEndAction endAction, double startLocX, double startLocY)
{
    internal Resources.Path Path => path;
    internal Instance Instance => inst;
    internal int PathStepIndex { get; private set; }
    internal bool Reverse { get; private set; }
    internal PathEndAction EndAction => endAction;
    internal double Speed => speed;

    private double currentHspeed; 
    private double currentVspeed;

    /// <summary>
    /// How many frames until current path step is finished.
    /// </summary>
    private int stepLength = 1;

    internal bool Move(out double hsp, out double vsp, ref bool updated)
    {
        hsp = currentHspeed;
        vsp = currentVspeed;

        if (--stepLength <= 0)
        {
            int reverseSign = Reverse ? -1 : 1;
            PathStepIndex += reverseSign;
            if (PathStepIndex > Path.Steps.Length || PathStepIndex < -1) // if drive is over
            {
                if (endAction == PathEndAction.Stop)
                {
                    inst.Speed.Value = 0d.ToExp();
                    return false;
                }
                else if (endAction == PathEndAction.Restart)
                {
                    PathStepIndex = 0;
                    inst.X.Value = startLocX.ToExp();
                    inst.Y.Value = startLocY.ToExp();

                    // we don't want to start immidiatly - first we want a frame in the start location
                    hsp = 0;
                    vsp = 0;
                    updated = true;
                    return true;
                }
                else if (endAction == PathEndAction.Reverse)
                {
                    Reverse = !Reverse;
                    PathStepIndex = Reverse ? Path.Steps.Length - 1 : 0;

                    // same reason
                    hsp = 0;
                    vsp = 0;
                    updated = true;
                    return true;
                }
            }

            var newStep = Path.Steps[PathStepIndex - reverseSign];

            // calculate new direction and step length
            double dir = newStep.Direction + (Reverse ? 180 : 0);
            double sp = newStep.Speed * (Speed / 100);

            // calculate hspeed and vspeed
            hsp = Math.Formulas.LengthDirX(sp, dir);
            vsp = Math.Formulas.LengthDirY(sp, dir);

            // step length will be "how many {hspeed}s in {new step width}?" (or "how many {vspeed}s in {new step height}?" if it's greater)
            stepLength = System.Math.Max(System.Math.Abs((int)(newStep.Width / hsp)), System.Math.Abs((int)(newStep.Height / vsp)));

            currentHspeed = hsp;
            currentVspeed = vsp;
            updated = true;
        }

        return true;
    }
}