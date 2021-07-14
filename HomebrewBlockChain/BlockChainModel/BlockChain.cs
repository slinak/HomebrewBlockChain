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
        public List<Transaction> currentTransactions = new List<Transaction>();
        public List<Block> blockChain = new List<Block>();
        public List<Node> nodes = new List<Node>();

        public Block lastBlock => blockChain.Last();

        public string nodeID { get; set; }

        public BlockChain()
        {
            nodeID = Guid.NewGuid().ToString().Replace("-", "");
            CreateNewBlock(proof: 100, previousHash: "1");
        }

        public void RegisterNode(string address)
        {
            nodes.Add(new Node { address = new Uri(address) });
        }

        public Block CreateNewBlock(int proof, string previousHash = null)
        {
            var block = new Block
            {
                index = blockChain.Count,
                timeStamp = DateTime.UtcNow,
                transactions = currentTransactions.ToList(),
                proof = proof,
                previousHash = previousHash ?? GetHash(blockChain.Last())
            };

            currentTransactions.Clear();
            blockChain.Add(block);
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

                if (block.previousHash != GetHash(lastBlock))
                    return false;

                if (!IsProofValid(lastBlock.proof, block.proof, lastBlock.previousHash))
                    return false;

                lastBlock = block;
                currentIndex++;
            }

            return true;
        }

        public bool ResolveConflicts()
        {
            List<Block> newChain = null;
            int maxLength = blockChain.Count;

            foreach (Node node in nodes)
            {
                var url = new Uri(node.address, "/chain");
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

                    if (data.chain.Count > blockChain.Count && IsChainValid(data.chain))
                    {
                        maxLength = data.chain.Count;
                        newChain = data.chain;
                    }
                }
            }

            if (newChain != null)
            {
                blockChain = newChain;
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
                sender = sender,
                recipient = recipient,
                amount = amount
            };

            currentTransactions.Add(transaction);

            return lastBlock != null ? lastBlock.index + 1 : 0;
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
                chain = blockChain.ToArray(),
                length = blockChain.Count
            });
        }

        public string MineBlock()
        {
            int proof = CreateProofOfWork(lastBlock.proof, lastBlock.previousHash);

            CreateTransaction(sender: "0", recipient: nodeID, amount: 1);
            Block block = CreateNewBlock(proof);

            return JsonConvert.SerializeObject(new
            {
                Message = "New Block Created",
                Index = block.index,
                Transactions = block.transactions.ToArray(),
                Proof = block.proof,
                PreviousHash = block.previousHash
            });
        }
    }
}
