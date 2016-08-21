using System;

namespace SampleLogger
{
    class Program
    {

        static void Main(string[] args)
        {
            var writer = new LogDataWriter();
            using (writer.ProduceLogMessages().Subscribe())
            {
                Console.WriteLine("press enter to terminate...");
                Console.ReadLine();
            }
        }
    }
}
