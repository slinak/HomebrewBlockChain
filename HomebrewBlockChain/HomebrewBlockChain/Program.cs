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
            Console.ReadLine();

        }
    }
}
