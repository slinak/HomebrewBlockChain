using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlockChainModel
{
    public class BlockChain
    {
        public List<Transaction> CurrentTransactions = new List<Transaction>();
        public List<Block> Chain = new List<Block>();
        public List<Node> Nodes = new List<Node>();

        public Block lastBlock => Chain.Last();

        public string nodeID { get; set; }

        public BlockChain()
        {
            nodeID = Guid.NewGuid().ToString().Replace("-", "");
            CreateNewBlock(proof: 100, previousHash: "1");
        }

        public void RegisterNode(string address)
        {
            Nodes.Add(new Node { Address = new Uri(address) });
        }

        public Block CreateNewBlock(int proof, string previousHash = null)
        {
            var block = new Block
            {
                Index = Chain.Count,
                CreationTime = DateTime.UtcNow,
                Transactions = CurrentTransactions.ToList(),
                Proof = proof,
                PreviousHash = previousHash ?? GetHash(Chain.Last())
            };

            CurrentTransactions.Clear();
            Chain.Add(block);
            return block;
        }

        public bool IsChainValid(List<Block> chain)
        {
            Block block;
            Block lastBlock = chain.First();
            int currentIndex = 1;

            while (currentIndex < chain.Count)
            {
                block = chain.ElementAt(currentIndex);

                if (block.PreviousHash != GetHash(lastBlock))
                    return false;

                if (!IsProofValid(lastBlock.Proof, block.Proof, lastBlock.PreviousHash))
                    return false;

                lastBlock = block;
                currentIndex++;
            }

            return true;
        }

        public bool ResolveConflicts()
        {
            List<Block> newChain = null;
            int maxLength = Chain.Count;

            foreach (Node node in Nodes)
            {
                var url = new Uri(node.Address, "/chain");
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var model = new
                    {
                        chain = new List<Block>(),
                        length = 0
                    };
                    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var data = JsonConvert.DeserializeAnonymousType(json, model);

                    if (data.chain.Count > Chain.Count && IsChainValid(data.chain))
                    {
                        maxLength = data.chain.Count;
                        newChain = data.chain;
                    }
                }
            }

            if (newChain != null)
            {
                Chain = newChain;
                return true;
            }

            return false;
        }

        public string GetHash(Block block)
        {
            string blockText = JsonConvert.SerializeObject(block);
            return GetSha256(blockText);
        }

        public string GetSha256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);

            foreach (byte x in hash)
                hashBuilder.Append($"{x:x2}");

            return hashBuilder.ToString();
        }

        private int CreateProofOfWork(int lastProof, string previousHash)
        {
            int proof = 0;
            while (!IsProofValid(lastProof, proof, previousHash))
                proof++;

            return proof;
        }

        public int CreateTransaction(string sender, string recipient, int amount)
        {
            var transaction = new Transaction
            {
                Sender = sender,
                Recipient = recipient,
                Amount = amount
            };

            CurrentTransactions.Add(transaction);

            return lastBlock != null ? lastBlock.Index + 1 : 0;
        }

        public bool IsProofValid(int lastProof, int proof, string previousHash)
        {
            string guess = $"{lastProof}{proof}{previousHash}";
            string result = GetSha256(guess);
            return result.StartsWith("0000");
        }

        public string GetFullChain()
        {
            return JsonConvert.SerializeObject(new
            {
                chain = Chain.ToArray(),
                length = Chain.Count
            });
        }

        public string MineBlock()
        {
            int proof = CreateProofOfWork(lastBlock.Proof, lastBlock.PreviousHash);

            CreateTransaction(sender: "0", recipient: nodeID, amount: 1);
            Block block = CreateNewBlock(proof);

            return JsonConvert.SerializeObject(new
            {
                Message = "New Block Created",
                Index = block.Index,
                Transactions = block.Transactions.ToArray(),
                Proof = block.Proof,
                PreviousHash = block.PreviousHash
            });
        }
    }
}
