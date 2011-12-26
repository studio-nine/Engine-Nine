#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics;
using Nine.Graphics.ObjectModel;
using Nine;
using Nine.Navigation;
#endregion

namespace Nine.Tools.PathGraphBuilder
{
    static class Builder
    {
        static object SyncRoot = new object();

        public static PathGrid Build(Scene scene, float step, float maxSlope, float maxActorHeight)
        {
            Heightmap heightmap;
            BoundingBox bounds;
            CreateHeightmapFromScene(scene, step, out heightmap, out bounds);

            var collisionMap = CreateCollisionMapFromHeightmap(heightmap, maxSlope);
            ObstacleCollisionTest(scene, heightmap, bounds, collisionMap, maxActorHeight);

            var pathGrid = new PathGrid(bounds.Min.X, bounds.Min.Y, heightmap.Size.X, heightmap.Size.Y, heightmap.Width, heightmap.Height);
            for (int i = 0; i < collisionMap.Length; i++)
                if (collisionMap[i])
                    pathGrid.Mark(i % pathGrid.SegmentCountX, i / pathGrid.SegmentCountX);
            return pathGrid;
        }

        private static void CreateHeightmapFromScene(Scene scene, float step, out Heightmap heightmap, out BoundingBox boundingBox)
        {
            // TODO: Use "IsPath" and "IsObstacle" attached properties.
            var surfaces = new List<DrawableSurfacePatch>();
            var obstacles = new List<DrawableModelPart>();

            scene.FindAll(surfaces);
            scene.FindAll(obstacles);

            var bounds = BoundingBoxExtensions.CreateMerged(
                surfaces.Select(patch => patch.BoundingBox).Concat(obstacles.Select(obstacle => obstacle.Model.BoundingBox)));

            var width = (int)Math.Round((bounds.Max.X - bounds.Min.X) / step) + 1;
            var height = (int)Math.Round((bounds.Max.Y - bounds.Min.Y) / step) + 1;
            
            var heightField = new float[width * height];

            Parallel.For(0, height, y =>
            //for (int y = 0; y < height; y++)
            {
                var rayPicks = new List<FindResult>();

                for (int x = 0; x < width; x++)
                {
                    var pickRay = new Ray();
                    pickRay.Position.X = bounds.Min.X + x * step;
                    pickRay.Position.Y = bounds.Min.Y + y * step;
                    pickRay.Position.Z = bounds.Max.Z;
                    pickRay.Direction.Z = -1;

                    lock (SyncRoot)
                    {
                        scene.FindAll(ref pickRay, rayPicks);
                    }

                    float min = float.MaxValue;
                    foreach (var pick in rayPicks)
                    {
                        if (!(pick.OriginalTarget is DrawableSurfacePatch))
                            continue;

                        var geometry = pick.OriginalTarget as IGeometry;
                        if (geometry != null)
                        {
                            int i = 0;
                            lock (SyncRoot)
                            {
                                // Force lazy initialization;
                                i += geometry.Indices.Length;
                                i += geometry.Positions.Length;

                                var pickResult = pickRay.Intersects(geometry);
                                if (pickResult.HasValue && pickResult.Value < min)
                                {
                                    min = pickResult.Value;
                                    heightField[x + y * width] = pickRay.Position.Z - min;
                                }
                            }
                        }
                    }
                    rayPicks.Clear();
                }
            //}
            });

            boundingBox = bounds;
            heightmap = new Heightmap(heightField, step, width - 1, height - 1);
        }
        
        private static bool[] CreateCollisionMapFromHeightmap(Heightmap heightmap, float maxSlope)
        {
            var sqrtTwo = (float)Math.Sqrt(2);
            var maxHeightDif = heightmap.Step * Math.Tan(maxSlope);
            var collisions = new bool[heightmap.Width * heightmap.Height];
            var width = heightmap.Width;
            var heights = heightmap.Heights;

            Parallel.For(0, heightmap.Height, y =>
            //for (int y = 0; y < heightmap.Height; y++)
            {
                for (int x = 0; x < heightmap.Width; x++)
                {
                    var p1 = heights[x + y * width];
                    var p2 = heights[x + 1 + y * width];
                    var p3 = heights[x + (y + 1) * width];
                    var p4 = heights[x + 1 + (y + 1) * width];

                    collisions[x + y * width] =
                       (Math.Abs(p1 - p2) > maxHeightDif || Math.Abs(p1 - p3) > maxHeightDif ||
                        Math.Abs(p2 - p4) > maxHeightDif || Math.Abs(p3 - p4) > maxHeightDif ||
                        Math.Abs(p1 - p4) > maxHeightDif * sqrtTwo || Math.Abs(p2 - p3) > maxHeightDif * sqrtTwo);
                }
            //}
            });
            return collisions;
        }

        private static void ObstacleCollisionTest(Scene scene, Heightmap heightmap, BoundingBox bounds, bool[] collisionMap, float maxActorHeight)
        {
            var step = heightmap.Step;
            var width = heightmap.Width;
            var heights = heightmap.Heights;
            
            Parallel.For(0, heightmap.Height, y =>
            //for (int y = 0; y < heightmap.Height; y++)
            {
                var obstacles = new List<DrawableModel>();

                for (int x = 0; x < heightmap.Width; x++)
                {
                    var index = x + y * width;
                    if (collisionMap[index])
                        continue;

                    var p1 = heights[x + y * width];
                    var p2 = heights[x + 1 + y * width];
                    var p3 = heights[x + (y + 1) * width];
                    var p4 = heights[x + 1 + (y + 1) * width];

                    var boundingBox = new BoundingBox();
                    boundingBox.Min.Z = (p1 + p2 + p3 + p4) * 0.25f;
                    boundingBox.Max.Z = boundingBox.Min.Z + maxActorHeight;
                    boundingBox.Min.X = bounds.Min.X + x * step;
                    boundingBox.Min.Y = bounds.Min.Y + y * step;
                    boundingBox.Max.X = boundingBox.Min.X + step;
                    boundingBox.Max.Y = boundingBox.Min.Y + step;

                    lock (SyncRoot)
                    {
                        scene.FindAll(ref boundingBox, obstacles);
                    }

                    foreach (IGeometry geometry in obstacles)
                    {
                        int i = 0;
                        lock (SyncRoot)
                        {
                            // Force lazy initialization;
                            i += geometry.Indices.Length;
                            i += geometry.Positions.Length;

                            var containment = boundingBox.Contains(geometry);
                            if (containment != ContainmentType.Disjoint)
                            {
                                collisionMap[index] = true;
                                break;
                            }
                        }
                    }
                    obstacles.Clear();
                }
            //}
            });
        }
    }
}
