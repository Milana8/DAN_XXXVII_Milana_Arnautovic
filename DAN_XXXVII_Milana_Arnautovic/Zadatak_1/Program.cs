using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zadatak_1
{
    class Program
    {
        public static Semaphore Manager { get; set; }
        public static Semaphore Driver { get; set; }
        public static Random random = new Random();
        static object l = new object();
        static int[] arrayRnd;
        static List<int> best = new List<int>();
        static List<int> best10 = new List<int>();


        public static void LoadingTrucks()

        {

            for (int i = 1; i <= 10; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(Loading)); //creating threads
                t.Start(i); //starting threads
            }

        }

        public static void RoadTrucks()
        {
            lock (l)
            {
                Monitor.Wait(l);
                Thread.Sleep(500);
                for (int i = 1; i <= 10; i++)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(Road)); //creating threads
                    t.Name = best10[i - 1].ToString();
                    t.Start(i); //starting threads
                }
            }

        }

        /// <summary>
        /// Random routes that are written to the file
        /// </summary>
        public static void Route()
        {

            arrayRnd = new int[1000];

            for (int i = 0; i < arrayRnd.Length; i++)
            {
                arrayRnd[i] = random.Next(1, 5000);
            }

            StreamWriter sw = new StreamWriter(@"../../Routes.txt");
            foreach (int number in arrayRnd)
            {
                sw.WriteLine(number);
            }
            sw.Close();
        }

        /// <summary>
        /// Method for filtering the best routes
        /// </summary>
        public static void BestRoute()
        {
            lock (l)
            {
                Thread.Sleep(random.Next(0, 3000));
                List<int> best = new List<int>();
                while (arrayRnd == null)
                {
                    Monitor.Wait(l); //Waiting for array to be created
                }

                string[] red = File.ReadAllLines(@"../../Routes.txt");

                foreach (var item in red)
                {
                    if (Int32.Parse(item) % 3 == 0)
                    {

                        best.Add(Int32.Parse(item));
                    }
                }

                best = best.OrderBy(x => x).Distinct().ToList();
                for (int i = 0; i < 10; i++)
                {
                    best10.Add(best.ElementAt(i));
                }
                Console.WriteLine("The best routes are chosen.\nThe best routes are:");
                foreach (var item in best10)
                {
                    Console.WriteLine(item);
                }

                Monitor.Pulse(l);
            }
        }

        /// <summary>
        /// Truck loading method
        /// </summary>
        /// <param name="o"></param>
        public static void Loading(object o)
        {

            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("The truck {0} is waiting to be loaded", o);
            Manager.WaitOne();
            stopwatch.Start();
            Console.WriteLine("The truck {0} is loading", o);
            Thread.Sleep(random.Next(500, 5000));
            Console.WriteLine("The truck {0} finished loading. Loading time: {1:N3} seconds", o, stopwatch.Elapsed.TotalSeconds);
            stopwatch.Reset();
            Manager.Release(1);

            if ((int)o == 10)
            {
                lock (l)
                {
                    Console.WriteLine("Loading is complete.");
                    Monitor.Pulse(l);

                }
            }

        }

        /// <summary>
        /// Method for a truck to travel to a specific route
        /// </summary>
        /// <param name="n"></param>
        public static void Road(object n)
        {

            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Truck {0} going to the route: {1}.", n, Thread.CurrentThread.Name);

            Driver.WaitOne();
            int rnd = random.Next(500, 5001);
            Driver.Release(1);

            if (rnd > 3000)
            {
                Thread.Sleep(rnd);
                Console.WriteLine("Truck {0} did not cross the route: {1} on time. Order canceled.",
                    n, Thread.CurrentThread.Name);
                stopwatch.Start();
                Thread.Sleep(rnd);
                Console.WriteLine("Truck {0} returning from failed road route: {2}. Road time: {1:N3} seconds.",
                    n, stopwatch.Elapsed.TotalSeconds, Thread.CurrentThread.Name);
                stopwatch.Stop();
            }
            else
            {
                stopwatch.Start();
                Thread.Sleep(rnd);
                Console.WriteLine("Truck {0} finishes road to route: {1}. Road time: {2:N3} seconds.",
                    n, Thread.CurrentThread.Name, stopwatch.Elapsed.TotalSeconds);

                stopwatch.Stop();
            }

        }


        static void Main(string[] args)
        {
            Thread route = new Thread(Route);//Creatig thread
            route.Start();//thread start
            route.Join();


            Thread best = new Thread(BestRoute);//Creatig thread
            best.Start();//thread start
            best.Join();

            Manager = new Semaphore(2, 2);
            LoadingTrucks();

            Driver = new Semaphore(10, 10);
            RoadTrucks();

            Console.ReadLine();


        }

    }
}

