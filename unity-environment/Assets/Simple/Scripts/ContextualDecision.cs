using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

public class ContextualDecision : MonoBehaviour, Decision
{
    private int action;
    private int lastAction, lastState;
    public float[][] q;
    public float learningRate;
    public float explorationEpsilon;

    public void Awake()
    {
        q = new float[4][];
        for(int i = 0; i < 4; i++)
        {
            q[i] = new float[4];
        }
    }

    public float[] Decide(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        lastAction = action - 1;
        if(++action > 4) action = 1;
        if(lastAction > -1)
        {
            q[lastState][lastAction] = q[lastState][lastAction] + learningRate * (reward - q[lastState][lastAction]);
        }
        lastState = (int)vectorObs[0];
        return DecideAction(q[lastState].ToList());
    }

    public float[] DecideAction(List<float> states)
    {
        var r = Random.Range(0.0f, 1.0f);
        explorationEpsilon = Mathf.Min(explorationEpsilon - 0.01f, .1f);
        if(r < explorationEpsilon)
        {
            action = RandomAction(states) + 1;
        }
        else
        {
            action = GetAction(states) + 1;
        }

        return new float[] {action};
    }

    private int RandomAction(List<float> states)
    {
        return Random.Range(0, states.Count);
    }

    private int GetAction(List<float> values)
    {
        float maxValue = values.Max();
        return values.ToList().IndexOf(maxValue);
    }

    public List<float> MakeMemory(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        return new List<float>();
    }
}
