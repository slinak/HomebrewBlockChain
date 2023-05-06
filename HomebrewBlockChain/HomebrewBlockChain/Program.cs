using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomebrewBlockChain
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing blockchain and creating genesis block");
            var blockChain = new BlockChainModel.BlockChain();
            Console.WriteLine(blockChain.GetFullChain());
            Console.WriteLine("---------------------------");
            blockChain.MineBlock();
            Console.WriteLine(blockChain.GetFullChain());

            ProcessChainInputAction(blockChain);
            Console.ReadLine();

        }

        static void ProcessChainInputAction(BlockChainModel.BlockChain blockChain)
        {
            string command = "C";
            bool stop = false;
            while (!stop)
            {
                Console.WriteLine("Enter command:");
                Console.WriteLine("(M)ine | e(X)it | (P)rint Chain | (R)esolve Conflicts");
                command = Console.ReadLine();

                switch (command)
                {
                    case "m":
                    case "M":
                        Console.WriteLine("Mining block...");
                        Console.WriteLine(blockChain.MineBlock());
                        break;
                    case "p":
                    case "P":
                        Console.WriteLine("Printing full chain...");
                        Console.WriteLine(blockChain.GetFullChain());
                        break;
                    case "x":
                    case "X":
                        stop = true;
                        Console.WriteLine("Stopping input loop...");
                        break;
                }
            }
        }
    }
}
