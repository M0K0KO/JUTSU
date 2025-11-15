using System.Collections.Generic;
using System.Linq;

public static class QueueExtensions
{
    public static int GetCount<T>(this Queue<T> queue, T itemToCount)
    {
        var comparer = EqualityComparer<T>.Default;
        return queue.Count(item => comparer.Equals(item, itemToCount));
    }
    
    
    // if queue is full, pop one element and enqueue the target value;
    public static void CapacitySafeEnqueue<T>(this Queue<T> queue, T value, int capacity)
    {
        if (queue.Count == capacity) queue.Dequeue();
            
        queue.Enqueue(value);
    }
    
}