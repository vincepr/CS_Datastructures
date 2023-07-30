using System.Collections;

namespace RS_DATASTRUCTURES.Queue
{
    public static class Example
    {
        public static void Run() 
        {
            Console.WriteLine("--- Queue Example: ---");
            

        }
    }
    
    /// Singly linked list implementation.
    public sealed class Queue<T> //:IEnumerable<T> where T : IComparable<T>
    {
        private LinkedList<T> list = new LinkedList<T>();


    }

}