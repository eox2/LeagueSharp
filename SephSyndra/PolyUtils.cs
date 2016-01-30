using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipperLib;
using Color = System.Drawing.Color;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using SharpDX;
using LeagueSharp.Common;
using LeagueSharp;

namespace SephSyndra
{
    class PolyUtils
    {
        //Credits to Esk0r
        public class Polygon
        {
            public List<Vector2> Points = new List<Vector2>();

            public void Add(Vector2 point)
            {
                Points.Add(point);
            }

            public Path ToClipperPath()
            {
                var result = new Path(Points.Count);

                foreach (var point in Points)
                {
                    result.Add(new IntPoint(point.X, point.Y));
                }

                return result;
            }

            public bool IsOutside(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
            }
            public int PointInPolygon(Vector2 point)
            {
                var p = new IntPoint(point.X, point.Y);
                return Clipper.PointInPolygon(p, ToClipperPath());
            }

            public void Draw(Color color, int width = 1)
            {
                for (var i = 0; i <= Points.Count - 1; i++)
                {
                    var nextIndex = (Points.Count - 1 == i) ? 0 : (i + 1);
                    DrawLineInWorld(Points[i].To3D(), Points[nextIndex].To3D(), width, color);
                }
            }
        }

        public static void DrawLineInWorld(Vector3 start, Vector3 end, int width, Color color)
        {
            var from = Drawing.WorldToScreen(start);
            var to = Drawing.WorldToScreen(end);
            Drawing.DrawLine(from[0], from[1], to[0], to[1], width, color);
            //Drawing.DrawLine(from.X, from.Y, to.X, to.Y, width, color);
        }

        public class Rectangle
        {
            public Vector2 Direction;
            public Vector2 Perpendicular;
            public Vector2 REnd;
            public Vector2 RStart;
            public float Width;

            public Rectangle(Vector2 start, Vector2 end, float width)
            {
                RStart = start;
                REnd = end;
                Width = width;
                Direction = (end - start).Normalized();
                Perpendicular = Direction.Perpendicular();
            }

            public Polygon ToPolygon(int offset = 0, float overrideWidth = -1)
            {
                var result = new Polygon();

                result.Add(
                    RStart + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                result.Add(
                    RStart - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular - offset * Direction);
                result.Add(
                    REnd - (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);
                result.Add(
                    REnd + (overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular + offset * Direction);

                return result;
            }
        }
    }
}
