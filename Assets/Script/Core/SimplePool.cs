using System.Collections.Generic;



public class SimplePool<T> where T : new()
{
    Queue<T> _queue = new Queue<T>();

    public SimplePool()
    {
        enqueue();
    }

    public void enqueue(T t)
    {
        _queue.Enqueue(t);
    }

    private void enqueue()
    {
        _queue.Enqueue(new T());
    }

    public T dequeue()
    {
        if(_queue.Count == 0)
            enqueue();

        return _queue.Dequeue();
    }
}