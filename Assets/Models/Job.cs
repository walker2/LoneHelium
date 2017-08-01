using System;

public class Job
{
    public Tile Tile { get; protected set; }

    private float m_jobTime;

    public event Action<Job> CbJobComplete;
    public event Action<Job> CbJobCancel;

    // TODO: FIX THIS
    public string JobObjectType { get; private set; }
    
    public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete,  float jobTime = 1f)
    {
        Tile = tile;
        JobObjectType = jobObjectType;
        CbJobComplete += cbJobComplete;
        m_jobTime = jobTime;
    }
    
    public void DoWork(float workTime)
    {
        m_jobTime -= workTime;

        if (m_jobTime <= 0)
        {
            if (CbJobComplete != null)
                CbJobComplete(this);
        }
    }
    
    public void CancelJob()
    {
        if (CbJobCancel != null)
            CbJobCancel(this);
    }
}