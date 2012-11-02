namespace Nine.Navigation.Steering.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;

    [TestClass()]
    public class GraphTest
    {
        [TestMethod()]
        public void GraphSearchPerformanceTest()
        {
            int graphSize = 1024;
            int iteration = 100;
            float density = 0.5f;

            PathGrid graph = new PathGrid(0, 0, graphSize, graphSize, graphSize, graphSize);

            var picks = new int[graphSize * graphSize];
            for (int i = 0; i < picks.Length; ++i)
            {
                picks[i] = i;
            }

            Random random = new Random(0);
            int obstacleCount = (int)(density * graphSize * graphSize);
            while (obstacleCount > 0)
            {
                var i = random.Next(obstacleCount);
                graph.Mark(picks[i] % graphSize, picks[i] / graphSize);
                obstacleCount--;
                picks[i] = picks[obstacleCount];
            }
            int walkables = picks.Length - obstacleCount;


            GraphSearch search = new GraphSearch();
            List<int> result = new List<int>();

            int[] array = new int[graphSize * graphSize];

            GC.Collect();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < iteration; ++i)
            {
                result.Clear();
                search.Search(graph, picks[random.Next(walkables)], picks[random.Next(walkables)], result);
            }

            watch.Stop();

            Trace.WriteLine("Max searches per frame (60 FPS): " + iteration / watch.Elapsed.TotalSeconds / 60.0);
        }

        [TestMethod]
        public void ArrayClearTest()
        {
            Stopwatch watch;
            int iteration = 100;
            int[] array = new int[1024 * 1024];
            for (var i = 0; i < array.Length; ++i)
                array[i] = i;

            foreach (var blockSize in new int[] { 128, 256, 512, 1024, 2048, 4096, 8192, 8192 * 2, 8192 * 4 })
            {
                GC.Collect();
                watch = new Stopwatch();
                watch.Start();

                for (int i = 0; i < iteration; ++i)
                {
                    Clear(array, blockSize);
                }

                watch.Stop();

                Trace.WriteLine("Max clears per frame (60 FPS): " + iteration / watch.Elapsed.TotalSeconds / 60.0 + ", " + blockSize);
            }

            GC.Collect();
            watch = new Stopwatch();
            watch.Start();
            unsafe
            {
                for (int i = 0; i < iteration; ++i)
                {
                    fixed (int* pArray = array)
                        ZeroMemory((byte*)pArray, sizeof(int) * array.Length);
                }
            }
            watch.Stop();

            Trace.WriteLine("Max clears per frame (60 FPS): " + iteration / watch.Elapsed.TotalSeconds / 60.0 + ", ZeroMemory ");

            GC.Collect();
            watch = new Stopwatch();
            watch.Start();
            unsafe
            {
                for (int i = 0; i < iteration; ++i)
                {
                    Array.Clear(array, 0, array.Length);
                }
            }
            watch.Stop();

            Trace.WriteLine("Max clears per frame (60 FPS): " + iteration / watch.Elapsed.TotalSeconds / 60.0 + ", Clear ");
        }

        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public unsafe static extern bool ZeroMemory(byte* destination, int length);
                
        public static void Clear(int[] array, int blockSize)
        {
            int index = 0;

            int length = Math.Min(blockSize, array.Length);
            Array.Clear(array, 0, length);

            length = array.Length;
            while (index < length)
            {
                index += blockSize;
                Buffer.BlockCopy(array, 0, array, index * sizeof(int), 
                    Math.Min(blockSize, length - index) * sizeof(int));
            }
        }

        /// <summary>
        /// http://www.codeproject.com/KB/recipes/PathFinder.aspx
        /// </summary>
        [TestMethod]
        [DeploymentItem("Nine.Test/Navigation/HardToGet.astar")]
        public void GraphSearchTest()
        {
            FileStream fs = new FileStream("HardToGet.astar", FileMode.Open, FileAccess.Read);
            if (fs.Length != 1000020)
            {
                throw new InvalidOperationException();
            }

            var start = new Point(fs.ReadByte() << 8 | fs.ReadByte(), fs.ReadByte() << 8 | fs.ReadByte());
            var end = new Point(fs.ReadByte() << 8 | fs.ReadByte(), fs.ReadByte() << 8 | fs.ReadByte());
            
            int iStart = start.X + start.Y * 1000;
            int iEnd = end.X + end.Y * 1000;
        

            fs.ReadByte();
            fs.ReadByte();
            fs.ReadByte();
            fs.ReadByte();
            fs.ReadByte();
            fs.ReadByte();
            var a = fs.ReadByte() << 24 | fs.ReadByte() << 16 | fs.ReadByte() << 8 | fs.ReadByte();
            fs.ReadByte();
            fs.ReadByte();

            int iteration = 100;

            PathGrid grid = new PathGrid(0, 0, 1000, 1000, 1000, 1000);

            for (int y = 0; y < 1000; ++y)
                for (int x = 0; x < 1000; ++x)
                    if ((byte)fs.ReadByte() == 0)
                        grid.Mark(x, y);

            List<int> result = new List<int>();

            GraphSearch search = new GraphSearch();

            GC.Collect();
            Stopwatch watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < iteration; ++i)
                search.Search(grid, iStart, iEnd, result);

            watch.Stop();

            fs.Close();

            Trace.WriteLine("Time used in miliseconds: " + (double)watch.ElapsedMilliseconds / iteration);
        }
    }
}

