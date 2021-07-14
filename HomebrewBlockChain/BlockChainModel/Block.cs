using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainModel
{
    public class Block
    {
        public int index { get; set; }
        public DateTime timeStamp { get; set; }
        public List<Transaction> transactions { get; set; }
        public int proof { get; set; }
        public string previousHash { get; set; }

        public override string ToString()
        {
            return $"{index} [{timeStamp.ToString("yyyy-MM-dd HH:mm:ss")}] Proof: {proof} | PrevHash: {previousHash} | Trx: {transactions.Count}";
        }
    }
}
