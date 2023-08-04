using src.datastructures.PriorityQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace src.algorithms.HuffmanCoding
{
    internal static class Example
    {
        public static void Run()
        {
            Console.WriteLine("--- Example: HuffmanCoding - Building a Huffman Tree ---");
            
            string input = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            var frequencies = HuffmanCoding.Step1CountCharFrequencies(input);
            foreach(var frequency in frequencies)
                Console.WriteLine(frequency);
            var tree = HuffmanCoding.Step2BuildMinTree(frequencies);
            for (int i = 0; i < frequencies.Count; i++)
            {

                HuffmanCoding.Step3IterateHuffmanTree(ref tree);
            }
            HuffmanCoding.printOptimalPrefixCodes(tree.Peek());
        }
    }

    internal class HuffmanCoding
    {

        internal record Node(char? data, Node? left=null, Node? right=null)
        {
            public char? data = data;
            //protected uint freq = frequency;
            public Node? left=left, right=right;
        }

        public static List<KeyValuePair<char, uint>> Step1CountCharFrequencies(in string input)
        {
            Dictionary<char, uint> map = new Dictionary<char, uint>();
            foreach (char ch in input)
            {
                if (map.ContainsKey(ch)) map[ch] = map[ch] + 1;
                else map.Add(ch,1);
            }
            return map.ToList();
        }

        public static MinPriorityQueue<uint, Node> Step2BuildMinTree(in List<KeyValuePair<char, uint>> nodes)
        {
            MinPriorityQueue<uint, Node> heap = new();
            foreach (var node in nodes)
                heap.Add(node.Value, new Node(node.Key));
            return heap;
        }

        public static void Step3IterateHuffmanTree(ref MinPriorityQueue<uint, Node> heap)
        {
            while (heap.Count > 1)
            {
                var (left, lPrio) = heap.PopPair();
                var (right, rPrio) = heap.PopPair();
                Node root = new Node(null, left, right);
                heap.Add(lPrio + rPrio, root);
            }
        }


        public static void printOptimalPrefixCodes(Node? root, string str = "")
        {
            if (root == null) return;
            if (root.data is not null) Console.WriteLine(root.data + ": " + str);

            printOptimalPrefixCodes(root.left, str + "0");
            printOptimalPrefixCodes(root.right, str + "1");

        }
    }
}
