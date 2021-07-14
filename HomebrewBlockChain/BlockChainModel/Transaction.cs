using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainModel
{
    public class Transaction
    {
        public int amount { get; set; }
        public string recipient { get; set; }
        public string sender { get; set; }

    }
}
