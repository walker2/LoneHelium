using System;
using System.Collections.Generic;

public class JobQueue
{
    private Queue<Job> m_jobQueue;

    public event Action<Job> CbJobCreated;
    public event Action<Job> CbJobEnded;

    public JobQueue()
    {
        m_jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        m_jobQueue.Enqueue(j);
        
        if (CbJobCreated != null)
        {
            CbJobCreated(j);
        }
    }

    public void Dequeue()
    {
        m_jobQueue.Dequeue();

        if (CbJobEnded != null)
        {
            //CbJobEnded;
        }
        
    }
}