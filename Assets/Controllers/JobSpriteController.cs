using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour
{
    private FurnitureSpriteController m_fsc;
    private Dictionary<Job, GameObject> m_jobGameObjectMap;

    // Use this for initialization
    void Start()
    {
        m_jobGameObjectMap = new Dictionary<Job, GameObject>();
        m_fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();

        // TODO: Should be something like that
        WorldController.Instance.World.JobQueue.CbJobCreated += OnJobCreated;
    }

    private void OnJobCreated(Job job)
    {
        // TODO: We can only do furniture-building

        // TODO: Does not consider multitiled object nor object rotation

        var jobGameObject =
            new GameObject("JOB_" + job.JobObjectType + "_" + job.Tile.Position.x + "_" + job.Tile.Position.y);

        m_jobGameObjectMap.Add(job, jobGameObject);

        jobGameObject.transform.position = new Vector3(job.Tile.Position.x, job.Tile.Position.y, 0);
        jobGameObject.transform.SetParent(this.transform, true);

        var sr = jobGameObject.AddComponent<SpriteRenderer>();
        sr.sprite = m_fsc.GetSpriteForFurniture(job.JobObjectType);
        sr.color = new Color(0.5f, 1.0f, 0.5f, 0.3f);
        sr.sortingLayerName = "Jobs";

        job.CbJobComplete += OnJobEnded;
        job.CbJobCancel += OnJobEnded;
    }


    private void OnJobEnded(Job job)
    {
        // TODO: We can only do furniture-building 

        // TODO: Delete sprite

        GameObject jobGameObject = m_jobGameObjectMap[job];
        
        job.CbJobComplete -= OnJobEnded;
        job.CbJobCancel -= OnJobEnded;
        Destroy(jobGameObject);
    }
}