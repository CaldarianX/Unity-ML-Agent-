using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
public enum GridType
{
    Empty,
    Obstacle,
    Coin,
}

public class MoveToGoalWithObs : Agent
{
    public int rows = 12;
    public int cols = 12;
    public int obstacleDensity = 20;
    public int coinNumber = 2;

    public GameObject obstaclePrefab;
    public GameObject coinPrefab;

    public Transform env; // Environment parent transform

    private Grid grid;
    private PositionVector coinPosition;
    private GameObject gridParent;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer FloorRenderer;
    // void Start()
    // {
    //     NewBoard();
    // }

    public override void OnEpisodeBegin()
    {

        ClearOldGrid();
        NewBoard();
        transform.localPosition = new Vector3(4.2f,0.4f,4.2f); 
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 bestCoinPosition = coinPosition.ClosestPosition(transform.localPosition);
        sensor.AddObservation(SwitchXZVector3(bestCoinPosition));
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.transform.rotation.y);

    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float moveSpeed = 3f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
        AddReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousAction = actionsOut.ContinuousActions;
        continuousAction[0] = Input.GetAxisRaw("Horizontal");
        continuousAction[1] = Input.GetAxisRaw("Vertical");

        continuousAction[2] = Input.GetKey(KeyCode.LeftArrow) ? -1f : Input.GetKey(KeyCode.RightArrow) ? 1f : 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            Debug.Log("Hit Goal");
            AddReward(+200f);
            string coinName = other.transform.name;
            int coinIndex = int.Parse(coinName[coinName.Length - 1].ToString());
            coinPosition.RemovePosition(coinIndex);
            Destroy(other);
            // FloorRenderer.material = winMaterial;
            if (coinPosition.allComplete())
            {
                Debug.Log("Collected all Coins");
                FloorRenderer.material = winMaterial;
                EndEpisode();
            }
        }
        else if (other.CompareTag("Wall"))
        {
            Debug.Log("Hit The Wall ");
            AddReward(-300f);
            FloorRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

    void NewBoard()
    {
        // Debug.Log("New Board");
        gridParent = new GameObject("GridParent");

        // Set gridParent as a child of env
        gridParent.transform.parent = env;

        grid = new Grid(rows, cols);
        grid.FillWithObstacle(obstacleDensity);
        coinPosition = grid.FillWithCoin(coinNumber);
        grid.RenderTheGrid(obstaclePrefab, coinPrefab, gridParent.transform);
        gridParent.transform.localPosition = new Vector3(0,0,0);
        // Debug.Log("Total Coins : " + coinPosition.positions.Count.ToString());
        // for(int i =0;i<coinPosition.positions.Count;i++){
        //     Vector3 pos = coinPosition.positions[i];
        //     pos = SwitchXZVector3(pos);
        //     // Debug.Log(pos.x.ToString() + "|" + pos.z.ToString());
        // }
    }

    void ClearOldGrid()
    {
        if (gridParent != null)
        {
            Destroy(gridParent);
        }
    }

    Vector3 SwitchXZVector3(Vector3 vec){
        return new Vector3(vec.z,vec.y,vec.x);
    }
}

public class Grid
{
    public int rows;
    public int cols;

    private GridType[,] grids;

    public Grid(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;
        grids = new GridType[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                grids[i, j] = GridType.Empty;
            }
        }
    }

    public float GetPositionGridX(int index)
    {
        return index - (cols - 1) / 2f;
    }

    public float GetPositionGridY(int index)
    {
        return index - (rows - 1) / 2f;
    }

    public void FillWithObstacle(int density)
    {
        for (int i = 0; i < density; i++)
        {
            int indexX = Random.Range(0, rows);
            int indexY = Random.Range(0, cols);

            if (indexX <= 8 && indexY <= 8)
            {
                grids[indexX, indexY] = GridType.Obstacle;
            }
        }
        for (int i = 1; i < rows - 1; i++)
        {
            for (int j = 1; j < cols - 1; j++)
            {
                if (grids[i, j + 1] == GridType.Obstacle && grids[i, j - 1] == GridType.Obstacle)
                {
                    grids[i, j] = GridType.Obstacle;
                }
                if (grids[i + 1, j] == GridType.Obstacle && grids[i - 1, j] == GridType.Obstacle)
                {
                    grids[i, j] = GridType.Obstacle;
                }
            }
        }
    }

    public PositionVector FillWithCoin(int number)
    {
        PositionVector coinPosition = new PositionVector();
        while (number != 0)
        {
            int indexX = Random.Range(0, rows);
            int indexY = Random.Range(0, cols);
            if (indexX <= 9 && indexY <= 9 && grids[indexX, indexY] == GridType.Empty)
            {
                grids[indexX, indexY] = GridType.Coin;
                coinPosition.AddPosition(new Vector3(GetPositionGridY(indexX), 0, GetPositionGridX(indexY)));
                number--;
            }
        }
        return coinPosition;
    }

    public void RenderTheGrid(GameObject obstaclePrefab, GameObject coinPrefab, Transform parent)
    {
        int coinIndex = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Vector3 position = new Vector3(GetPositionGridY(j), 0, GetPositionGridX(i));
                GameObject instance = null;

                switch (grids[i, j])
                {
                    case GridType.Obstacle:
                        instance = GameObject.Instantiate(obstaclePrefab, position, Quaternion.identity, parent);
                        break;
                    case GridType.Coin:
                        position.y = 0.5f;
                        instance = GameObject.Instantiate(coinPrefab, position, Quaternion.identity, parent);
                        instance.name = "Coin" + coinIndex.ToString();
                        coinIndex+=1;
                        break;
                }
            }
        }
    }
}

public class PositionVector
{
    public List<Vector3> positions;
    public List<bool> avaliable;
    public PositionVector()
    {
        positions = new List<Vector3>();
        avaliable = new List<bool>();
    }

    public void AddPosition(Vector3 position)
    {
        positions.Add(position);
        avaliable.Add(true);
    }

    public void RemovePosition(Vector3 position)
    {
        Debug.Log("Nuh ad----------------");
        for (int i = 0; i < positions.Count; i++)
        {
            if (!IsSamePosition(position, positions[i]))
            {
                // positions.RemoveAt(i);
                avaliable[i] = false;
                // Debug.Log("------------");
                // Debug.Log(position.x.ToString() +  "=" +positions[i].x.ToString());
                // Debug.Log(position.z.ToString() +  "=" +positions[i].z.ToString());
                // return positions;
            }
        }
        // return positions;
    }
    public void RemovePosition(int index)
    {
        avaliable[index] = false;
    }
    public bool IsSamePosition(Vector3 pos1, Vector3 pos2)
    {
        return pos1.x == pos2.x && pos1.z == pos2.z;
    }

    public Vector3 ClosestPosition(Vector3 position){
        float lowestDis = 2000000;
        int bestIndex = 0;
        for(int i =0;i<positions.Count;i+=1){
            if(avaliable[i]){
                float dis = CalculateDistance(positions[i],position);
                if(dis < lowestDis){
                    lowestDis =dis;
                    bestIndex = i;
                }
            }
        }
        return positions[bestIndex];
    }
    public bool allComplete(){
        bool x = false;
        for(int i =0;i<avaliable.Count;i++){
            x = x || avaliable[i];
        }
        return x==false;
    }
    public float CalculateDistance(Vector3 A,Vector3 B){
        return (A.x-B.x)*(A.x-B.x) + (A.y-B.y)*(A.y-B.y) +(A.z-B.z)*(A.z-B.z);
    }
}
