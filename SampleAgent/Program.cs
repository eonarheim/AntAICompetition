using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var agent = new Agent("Sample Agent");
                agent.Start().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ooops! Something went wrong! {0}", e.Message);
            }
        }
    }
}
