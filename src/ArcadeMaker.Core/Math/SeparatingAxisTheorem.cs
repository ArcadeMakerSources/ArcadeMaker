using System;
using System.Collections.Generic;
using System.Drawing;
using ArcadeMaker.Core.Runtime;
using ArcadeMaker.Core.Math.Shapes;

namespace ArcadeMaker.Core.Math;

public static class SeparatingAxisTheorem
{
    /*
       This code was written by Claude. It's full message:
       ... code ...

       This uses the Separating Axis Theorem (SAT), which is the standard approach for rotated rectangle collision:

       Get rotated corners — Each rectangle's corners are computed by rotating around the origin point (OriginX, OriginY) and translating to world position (X, Y)

       Test separating axes — For two convex polygons, if you can find any axis where their projections don't overlap, they don't intersect. For rectangles, you only need to test the 4 edge normals (2 unique directions per rectangle)

       Project and compare — Project all corners onto each axis and check if the ranges overlap

       The method returns true if the rectangles overlap, false otherwise.
    */

    public static bool AreRectanglesIntersecting(Rect rect1, Rect rect2)
    {
        // Get the four corners of each rectangle in world space
        var corners1 = GetRotatedCorners(rect1);
        var corners2 = GetRotatedCorners(rect2);

        // Use Separating Axis Theorem (SAT)
        // Check all potential separating axes from both rectangles
        var axes = GetAxes(corners1).Concat(GetAxes(corners2));

        foreach (var axis in axes)
        {
            var projection1 = ProjectOntoAxis(corners1, axis);
            var projection2 = ProjectOntoAxis(corners2, axis);

            // If projections don't overlap, rectangles don't intersect
            if (projection1.Max <= projection2.Min || projection2.Max <= projection1.Min)
                return false;
        }

        return true;
    }

    public static bool IsPointInRectangle(double pointX, double pointY, Rect rect)
    {
        var corners = GetRotatedCorners(rect);

        var axes = GetAxes(corners);

        foreach (var axis in axes)
        {
            var projection = ProjectOntoAxis(corners, axis);
            double pointProjection = pointX * axis.X + pointY * axis.Y;

            if (pointProjection < projection.Min || pointProjection >= projection.Max)
                return false;
        }

        return true;
    }

    private static (double X, double Y)[] GetRotatedCorners(Rect rect)
    {
        double radians = rect.Angle * System.Math.PI / 180.0;
        double cos = System.Math.Cos(radians);
        double sin = System.Math.Sin(radians);

        // Local corners relative to the pivot (origin point)
        // OriginX/Y is how far the pivot is from the mask's top-left
        (double x, double y)[] localCorners =
        [
            (-rect.OriginX,                      -rect.OriginY),
            ( rect.Width - rect.OriginX - 1,         -rect.OriginY),
            ( rect.Width - rect.OriginX - 1,          rect.Height - rect.OriginY - 1),
            (-rect.OriginX,                       rect.Height - rect.OriginY)
        ];

        return localCorners
            .Select(c => (
                X: rect.X + c.x * cos - c.y * sin,
                Y: rect.Y + c.x * sin + c.y * cos
            ))
            .ToArray();
    }

    private static IEnumerable<(double X, double Y)> GetAxes((double X, double Y)[] corners)
    {
        // Get edge normals (perpendicular to each edge)
        for (int i = 0; i < corners.Length; i++)
        {
            var edge = (
                X: corners[(i + 1) % corners.Length].X - corners[i].X,
                Y: corners[(i + 1) % corners.Length].Y - corners[i].Y
            );
            // Perpendicular vector (normal)
            yield return (-edge.Y, edge.X);
        }
    }

    private static (double Min, double Max) ProjectOntoAxis(
        (double X, double Y)[] corners,
        (double X, double Y) axis)
    {
        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (var corner in corners)
        {
            double projection = corner.X * axis.X + corner.Y * axis.Y;
            min = System.Math.Min(min, projection);
            max = System.Math.Max(max, projection);
        }

        return (min, max);
    }
}


// much earlier code:

//using ArcadeMaker.Core.Math.Shapes;

//namespace ArcadeMaker.Core.Math
//{
//    internal static class RectangleCollision
//    {
//        public static double[] WorkOutNewPoints(double cx, double cy, double vx, double vy, double rotatedAngle)
//        {   // From a rotated object
//            // cx,cy are the centre coordinates, vx,vy is the point to be measured against the center point

//            // Convert rotated angle into radians
//            rotatedAngle = (rotatedAngle * System.System.Math.PI) / 180;

//            double dx = vx - cx;
//            double dy = vy - cy;
//            double distance = System.System.Math.Sqrt(dx * dx + dy * dy);
//            double originalAngle = System.System.Math.Atan2(dy, dx);
//            double rotatedX = cx + distance * System.System.Math.Cos(originalAngle + rotatedAngle);
//            double rotatedY = cy + distance * System.System.Math.Sin(originalAngle + rotatedAngle);

//            return new double[] { rotatedX, rotatedY };
//        }

//        public static double[][] GetRotatedSquareCoordinates(Rect rect)
//        {
//            return GetRotatedSquareCoordinates(rect, rect.X, rect.Y);
//        }

//        // Get the rotated coordinates for the square
//        public static double[][] GetRotatedSquareCoordinates(Rect obj, double placeX, double placeY)
//        {
//            /*
//            double originX = placeX;
//            double originY = placeY;
//            placeX -= obj.sprite.originX;
//            placeY -= obj.sprite.originY;

//            // Work out the new locations
//            double[] topLeft = WorkOutNewPoints(originX, originY, placeX, placeY, obj.imageAngle);
//            double[] topRight = WorkOutNewPoints(originX, originY, placeX + obj.image.Width, placeY, obj.imageAngle);
//            double[] bottomLeft = WorkOutNewPoints(originX, originY, placeX, placeY + obj.image.Height, obj.imageAngle);
//            double[] bottomRight = WorkOutNewPoints(originX, originY, placeX + obj.image.Width, placeY + obj.image.Height, obj.imageAngle);

//            return new double[][] { topLeft, topRight, bottomLeft, bottomRight };
//            */
//            return GetRotatedSquareCoordinates(new Rectangle((int)obj.x, (int)obj.y, obj.image.Width, obj.image.Height), placeX, placeY, obj.imageAngle, obj.sprite.originX, obj.sprite.originY);
//        }

//        // Get the rotated coordinates for the square
//        public static double[][] GetRotatedSquareCoordinates(Rectangle rect, double placeX, double placeY, int angle, int ox, int oy)
//        {
//            double originX = placeX;
//            double originY = placeY;
//            placeX -= ox;
//            placeY -= oy;

//            // Work out the new locations
//            double[] topLeft = WorkOutNewPoints(originX, originY, placeX, placeY, angle);
//            double[] topRight = WorkOutNewPoints(originX, originY, placeX + rect.Width, placeY, angle);
//            double[] bottomLeft = WorkOutNewPoints(originX, originY, placeX, placeY + rect.Height, angle);
//            double[] bottomRight = WorkOutNewPoints(originX, originY, placeX + rect.Width, placeY + rect.Height, angle);

//            return new double[][] { topLeft, topRight, bottomLeft, bottomRight };
//        }

//        // Functional objects for the Seperate Axis Theorum (SAT)
//        // Single vertex
//        private static PointD GetPointD(double x, double y)
//        {
//            return new PointD(x, y);
//        }

//        // The polygon that is formed from vertices and edges.
//        private static PointD[][] Polygon(PointD[] vertices, PointD[] edges)
//        {
//            return new PointD[][] { vertices, edges };
//        }

//        // The actual Seperate Axis Theorum function
//        private static bool Sat(PointD[][] polygonA, PointD[][] polygonB)
//        {
//            PointD perpendicularLine = null;
//            double dot = 0;
//            var perpendicularStack = new Stack<PointD>();
//            double? amin = null;
//            double? amax = null;
//            double? bmin = null;
//            double? bmax = null;

//            // Work out all perpendicular vectors on each edge for polygonA
//            for (var i = 0; i < polygonA[1].Length; i++)
//            {
//                perpendicularLine = GetPointD(-polygonA[1][i].Y, polygonA[1][i].X);
//                perpendicularStack.Push(perpendicularLine);
//            }
//            // Work out all perpendicular vectors on each edge for polygonB
//            for (var i = 0; i < polygonB[1].Length; i++)
//            {
//                perpendicularLine = GetPointD(-polygonB[1][i].Y, polygonB[1][i].X);
//                perpendicularStack.Push(perpendicularLine);
//            }
//            // Loop through each perpendicular vector for both polygons
//            for (var i = 0; i < perpendicularStack.Count; i++)
//            {
//                // These dot products will return different values each time
//                amin = null;
//                amax = null;
//                bmin = null;
//                bmax = null;

//                // Work out all of the dot products for all of the vertices in PolygonA against the perpendicular vector
//                // that is currently being looped through
//                for (var j = 0; j < polygonA[0].Length; j++)
//                {
//                    dot = polygonA[0][j].X *
//                          perpendicularStack.ElementAt(i).X +
//                          polygonA[0][j].Y *
//                          perpendicularStack.ElementAt(i).Y;

//                    // Then find the dot products with the highest and lowest values from polygonA.
//                    if (amax == null || dot > amax)
//                    {
//                        amax = dot;
//                    }
//                    if (amin == null || dot < amin)
//                    {
//                        amin = dot;
//                    }
//                }

//                // Work out all of the dot products for all of the vertices in PolygonB against the perpendicular vector
//                // that is currently being looped through
//                for (var j = 0; j < polygonB[0].Length; j++)
//                {
//                    dot = polygonB[0][j].X *
//                          perpendicularStack.ElementAt(i).X +
//                          polygonB[0][j].Y *
//                          perpendicularStack.ElementAt(i).Y;

//                    // Then find the dot products with the highest and lowest values from polygonB.
//                    if (bmax == null || dot > bmax)
//                    {
//                        bmax = dot;
//                    }
//                    if (bmin == null || dot < bmin)
//                    {
//                        bmin = dot;
//                    }
//                }

//                // If there is no gap between the dot products projection then we will continue onto evaluating the next perpendicular edge.
//                if ((amin < bmax && amin > bmin) || (bmin < amax && bmin > amin))
//                {
//                    continue;
//                }

//                // Otherwise, we know that there is no collision for definite.
//                else
//                {
//                    return false;
//                }
//            }

//            // If we have gotten this far. Where we have looped through all of the perpendicular edges and not a single one of there projections had
//            // a gap in them. Then we know that the 2 polygons are colliding for definite then.
//            return true;
//        }

//        // Detect a collision between the 2 rectangles
//        public static bool DetectRectangleCollision(ObjectModel self, double x, double y, ObjectModel other)
//        {
//            /*
//            // Because we are working with vertices and edges. self algorithm does not cover the normal un-rotated rectangle
//            // algorithm which just deals with sides
//            if (self.imageAngle == 0 && other.imageAngle == 0)
//            {
//                double ttlx = x - self.sprite.originX, ttly = y - self.sprite.originY, otlx = other.x - other.sprite.originX, otly = other.y - other.sprite.originY;

//                if (ttlx + self.image.Width >= otlx && ttlx <= otlx + other.image.Width && ttly + self.image.Height >= otly && ttly <= otly + other.image.Height)
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }

//            // Get rotated coordinates for both rectangles
//            var tRR = GetRotatedSquareCoordinates(self, x, y);
//            var oRR = GetRotatedSquareCoordinates(other);

//            // Vertices & Edges are listed in clockwise order. Starting from the top right
//            var thisTankVertices = new PointD[] {
//                GetPointD(tRR[1][0], tRR[1][1]),
//                GetPointD(tRR[3][0], tRR[3][1]),
//                GetPointD(tRR[2][0], tRR[2][1]),
//                GetPointD(tRR[0][0], tRR[0][1])
//            };
//            var thisTankEdges = new PointD[] {
//                GetPointD(tRR[3][0] - tRR[1][0], tRR[3][1] - tRR[1][1]),
//                GetPointD(tRR[2][0] - tRR[3][0], tRR[2][1] - tRR[3][1]),
//                GetPointD(tRR[0][0] - tRR[2][0], tRR[0][1] - tRR[2][1]),
//                GetPointD(tRR[1][0] - tRR[0][0], tRR[1][1] - tRR[0][1])
//            };
//            var otherTankVertices = new PointD[] {
//                GetPointD(oRR[1][0], oRR[1][1]),
//                GetPointD(oRR[3][0], oRR[3][1]),
//                GetPointD(oRR[2][0], oRR[2][1]),
//                GetPointD(oRR[0][0], oRR[0][1])
//            };
//            var otherTankEdges = new PointD[] {
//                GetPointD(oRR[3][0] - oRR[1][0], oRR[3][1] - oRR[1][1]),
//                GetPointD(oRR[2][0] - oRR[3][0], oRR[2][1] - oRR[3][1]),
//                GetPointD(oRR[0][0] - oRR[2][0], oRR[0][1] - oRR[2][1]),
//                GetPointD(oRR[1][0] - oRR[0][0], oRR[1][1] - oRR[0][1])
//            };

//            var thisRectPolygon = Polygon(thisTankVertices, thisTankEdges);
//            var otherRectPolygon = Polygon(otherTankVertices, otherTankEdges);

//            if (Sat(thisRectPolygon, otherRectPolygon))
//            {
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//            */
//            return DetectRectangleCollision(self, x, y, new Rectangle((int)other.x, (int)other.y, other.image.Width, other.image.Height), other.imageAngle, other.sprite.originX, other.sprite.originY);
//        }

//        public static bool DetectRectangleCollision(Rect self, Rect rect)
//        {
//            // Because we are working with vertices and edges. self algorithm does not cover the normal un-rotated rectangle
//            // algorithm which just deals with sides
//            if (self.Angle == 0 && rect.Angle == 0)
//            {
//                double ttlx = self.X - self.OriginX, ttly = self.Y - self.OriginY, otlx = rect.X - rect.OriginX, otly = rect.Y - rect.OriginY;

//                if (ttlx + self.Width >= otlx && ttlx <= otlx + rect.Width && ttly + self.Height >= otly && ttly <= otly + rect.Height)
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }

//            // Get rotated coordinates for both rectangles
//            var tRR = GetRotatedSquareCoordinates(self);
//            var oRR = GetRotatedSquareCoordinates(rect);

//            // Vertices & Edges are listed in clockwise order. Starting from the top right
//            var thisTankVertices = new PointD[] {
//                GetPointD(tRR[1][0], tRR[1][1]),
//                GetPointD(tRR[3][0], tRR[3][1]),
//                GetPointD(tRR[2][0], tRR[2][1]),
//                GetPointD(tRR[0][0], tRR[0][1])
//            };
//            var thisTankEdges = new PointD[] {
//                GetPointD(tRR[3][0] - tRR[1][0], tRR[3][1] - tRR[1][1]),
//                GetPointD(tRR[2][0] - tRR[3][0], tRR[2][1] - tRR[3][1]),
//                GetPointD(tRR[0][0] - tRR[2][0], tRR[0][1] - tRR[2][1]),
//                GetPointD(tRR[1][0] - tRR[0][0], tRR[1][1] - tRR[0][1])
//            };
//            var otherTankVertices = new PointD[] {
//                GetPointD(oRR[1][0], oRR[1][1]),
//                GetPointD(oRR[3][0], oRR[3][1]),
//                GetPointD(oRR[2][0], oRR[2][1]),
//                GetPointD(oRR[0][0], oRR[0][1])
//            };
//            var otherTankEdges = new PointD[] {
//                GetPointD(oRR[3][0] - oRR[1][0], oRR[3][1] - oRR[1][1]),
//                GetPointD(oRR[2][0] - oRR[3][0], oRR[2][1] - oRR[3][1]),
//                GetPointD(oRR[0][0] - oRR[2][0], oRR[0][1] - oRR[2][1]),
//                GetPointD(oRR[1][0] - oRR[0][0], oRR[1][1] - oRR[0][1])
//            };

//            var thisRectPolygon = Polygon(thisTankVertices, thisTankEdges);
//            var otherRectPolygon = Polygon(otherTankVertices, otherTankEdges);

//            if (Sat(thisRectPolygon, otherRectPolygon))
//            {
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }
//    }

//    class PointD
//    {
//        public double X { get; set; }
//        public double Y { get; set; }
//        public PointD(double x, double y)
//        {
//            X = x;
//            Y = y;
//        }
//    }
//}