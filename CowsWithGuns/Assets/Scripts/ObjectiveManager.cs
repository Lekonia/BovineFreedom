using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager instance;

    public Text objectiveText;
    public float displayTime = 1;
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;

    private Coroutine displayTextCoroutine;


    public enum ObjectiveType
	{
        TIMED,
        COUNTED
	}
    [System.Serializable]
    public struct Objective
    {
        public string name;
        public bool shouldDisplayObjective;
        public ObjectiveType type;
        [Space]

        public float timed_time;
        [Space]
        public int counted_count;

        [Space]
        public UnityEvent OnObjectiveCompleated;
    }

    public Objective[] objectives;
    private int currentObjective = 0;


    private float timer = 0;
    private int count = 0;



    void Awake()
    {
        instance = this;
        displayTextCoroutine = StartCoroutine(DisplayObjective());
    }


    void Update()
    {
        if (objectives[currentObjective].type == ObjectiveType.TIMED)
		{
            timer += Time.deltaTime;
            if (timer >= objectives[currentObjective].timed_time)
            {
                ObjectiveCompleated();
            }
        }
    }

    public void CountedTaskComplete(int objectiveIndex)
	{
        if (objectives[currentObjective].type == ObjectiveType.COUNTED && currentObjective == objectiveIndex)
		{
            count++;
            if (count >= objectives[currentObjective].counted_count)
			{
                ObjectiveCompleated();
			}
		}
	}


    private void ObjectiveCompleated()
	{
        objectives[currentObjective].OnObjectiveCompleated.Invoke();

        currentObjective++;
        if (currentObjective == objectives.Length)
		{
            //all objectives compleated
            this.enabled = false;
            return;
		}

        
        if (objectives[currentObjective].shouldDisplayObjective)
		{
            // Display objective
            if (displayTextCoroutine != null)
            {
                StopCoroutine(displayTextCoroutine);
            }
            displayTextCoroutine = StartCoroutine(DisplayObjective());
        }


        //reset
        timer = 0;
        count = 0;
    }

    private IEnumerator DisplayObjective()
	{
        objectiveText.text = objectives[currentObjective].name;
        Color color = objectiveText.color;
        color.a = 0;

        // Fade text in
        float t = 0;
        while (t < fadeInTime)
		{
            color.a = Mathf.Lerp(0, 1, t / fadeInTime);
            objectiveText.color = color;

            t += Time.deltaTime;
            yield return null;
		}

        // Display for time
        color.a = 1;
        objectiveText.color = color;
        yield return new WaitForSeconds(displayTime);

        // Fade text out
        t = 0;
        while (t < fadeOutTime)
        {
            color.a = Mathf.Lerp(1, 0, t / fadeOutTime);
            objectiveText.color = color;

            t += Time.deltaTime;
            yield return null;
        }

        color.a = 0;
        objectiveText.color = color;

        displayTextCoroutine = null;
    }
}
