using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceReference1.CalculatorClient client = new ServiceReference1.CalculatorClient();

            double res = client.Add(2.5, 2.8);

            Console.WriteLine(res);

            Console.ReadLine();
        }
    }
}
