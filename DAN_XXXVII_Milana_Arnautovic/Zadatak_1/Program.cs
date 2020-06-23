using System;
using System.Collections.Generic;
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
        public static Random random = new Random();
        static object l = new object();
        static int[] arrayRnd;
        static List<int> best = new List<int>();
        

        public static void NameTrucks()

        {

            for (int i = 1; i <= 10; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(Loading));
                t.Start(i);
            }
        }

        

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

                best.Sort();
                List<int> best10 = best.GetRange(0, 10); //top 10 routes
                foreach (var item in best10)
                {
                    Console.WriteLine(item);
                }

            }

        }

       

        public static void Loading(object o)
        {
            Console.WriteLine("The truck {0} is waiting to be loaded", o);
            Manager.WaitOne();
            Console.WriteLine("The truck {0} is loading", o);
            Thread.Sleep(random.Next(500, 5000));
            Console.WriteLine("The truck {0} finished loading", o);
            Manager.Release(1);

        }


        static void Main(string[] args)
        {
            Thread route = new Thread(Route);//Creatig thread
            route.Start();//thread start
            route.Join();
            Console.WriteLine("The best routes are: ");
            Thread best = new Thread(BestRoute);//Creatig thread
            best.Start();//thread start
            best.Join();
            Manager = new Semaphore(2, 2);
            NameTrucks();
            

            Console.ReadLine();


        }

    }
}

