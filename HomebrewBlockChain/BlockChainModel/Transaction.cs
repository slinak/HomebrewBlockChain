﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainModel
{
    public class Transaction
    {
        public int Amount { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }

    }
}
