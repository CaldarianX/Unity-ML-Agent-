using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTranform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer FloorRenderer;
    public void Update() {
        targetTranform.transform.Rotate(0,1,0);
    }
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0,0,0);

        Vector3 newTargetPosition = new Vector3(Random.Range(2.2f,1.5f),0f,Random.Range(0.8f,-0.8f));

        targetTranform.transform.localPosition = newTargetPosition;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTranform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float moveSpeed = 1f;

        transform.localPosition += new Vector3(moveX,0,moveZ)*Time.deltaTime*moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousAction = actionsOut.ContinuousActions;
        continuousAction[0] = Input.GetAxisRaw("Horizontal");
        continuousAction[1] = Input.GetAxisRaw("Vertical");
    }
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Goal")) { 
            SetReward(+1f);
            FloorRenderer.material = winMaterial;
            EndEpisode();
        }
        else if(other.CompareTag("Wall")){
            SetReward(-1f);
            FloorRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
