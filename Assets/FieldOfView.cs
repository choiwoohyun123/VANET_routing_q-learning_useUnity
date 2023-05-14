using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class FieldOfView : MonoBehaviour
{
    // field Of View 스크립트 - 변수
    public float viewRadius; 
    [Range(0, 360)]
    public float viewAngle = 360;
    public RaycastHit[] hits;
    public Transform k;
    public Transform target;
    public List<int> timer = new List<int>();
    public LayerMask targetMask, obstacleMask;
    public List<Transform> visibleTargets = new List<Transform>();
    public Collider[] targetsInViewRadius;

    private Vector3 rotation;

    public List<int> rewardCount = new List<int>();

    public static List<float> rotationValue = new List<float>();




    Vector3 tf;


    //Q-learning 스크립트 부분 - 변수

    public static float[][][] Q_table;
    public static int action;
    public static int currentAction;
    public static int state;

    // HyperParameter
    float alpha = 0.2f;
    float gamma = 0.9f;
    public static float e = 0.3f;
    float eMin = 0.1f;
    float e_decay = 0.01f;
    public static float episodes = 1f;
    public static bool done = false;

    public static int reward;
    public static int rAll;
    public static int nextState;
    public static int currentState;
    [Range(-1f, 1f)] float value;

    // 시작, 도착 State(RSU)변수
    public static int destinationState;
    public static int sourceState;

    // 처음 유니티를 실행 시켰을 때 랜덤으로 시작RSU와 도착RSU선택 후 시작RSU에서 traffic생성하고 traffic이 도착지점에 도달했을 때 다음 시작RSU와 도착 RSU를 다시 선택하게 해주는 변수
    int tempState;
    string startRSU;
    string destRSU;

    void Awake()
    {

        //field Of View 스크립트 부분 - 초기화
        timer.Clear();
        rewardCount.Clear();
        reward = 0;
        rAll = 0;
        //Q-learning 스크립트 부분 - 초기화
        SetQtable();
        set_sourceState();
        state = 0;
        
        tempState = sourceState + 1;
        startRSU = "RSU" + tempState;
        // 맨처음 유니티 실행 했을 때 traffic 생성
        if (this.gameObject.name == startRSU && this.gameObject.layer == 9)
        {
            GameObject.Find(startRSU).layer = 10;
        }
            
        currentState = sourceState;
        set_destinationState();
        
    }
    
    public void Start()
    {
        //Field Of View 부분 스크립트 - Start
        tf = this.transform.forward; // 처음 RSU에서 traffic전달 방향이 정해졌을때 그 방향으로만 전달할 수 있게 쓰이는 변수 초기값

    }

    // layer 9 = RSU, layer 10 = RSUwait/ 기본적으로 traffic을 받기 전,후 RSU layer는 9이고, traffic을 받아서 줘야할 때는 layer가 잠깐 10이 된다.
    // layer 7 = target, layer 8 = start/ 기본적으로 traffic을 받기 전,후 차량 layer는 7이고, traffic을 받아서 줘야할 때는 layer가 잠깐 8이 된다.
    private void FixedUpdate()
    {
        // Field Of View 스크립트 부분 - 업데이트
                                         
        if (this.gameObject.layer == 10) //  Q-learning을 하기 위해 RSU에서만 state,action 정해지도록 함
        {
            setcurrentState();
            learning_action();
            RSU_action();
        }

        if (this.gameObject.layer == 10 && this.gameObject.layer == 8)  // 위에 주석을 보면 traffic을 받았을 때 layer가 10, 8이기 때문에 이때만 FindVisibleTargets함수를 실행해 traffic을 전달해 준다.
        {
            FindVisibleTargets();
        }

        Time.timeScale = 0.2f; // 슬로우모션처럼 사용할 수 있다
    }

    //Field Of View 스크립트 부분 - 함수

    // 처음 RSU에서 traffic전달 방향이 정해졌을때 그 방향으로만 전달할 수 있게 쓰이는 함수
    void tfDecide()
    {
        if (rotationValue[0] == 0)
        {
            tf = Vector3.forward;
        }
        else if (rotationValue[0] == 90)
        {
            tf = Vector3.right;
        }
        else if (rotationValue[0] == 180)
        {
            tf = Vector3.back;
        }
        else if (rotationValue[0] == 270)
        {
            tf = Vector3.left;
        }


    }






    // Field Of View 스크립트 부분 - 함수
    // 설정한 timetick에 따라 RSU또는 Vehicle으로 traffic 전달하도록 하는 함수
    void timetick()
    {
        timer.Add(1);

        if (timer.Count > 20 && !(k == this.transform))  // timetick크기 20
        {
            if (k.gameObject.layer == 7) // target을 만났을 때 
            {
                if (gameObject.layer == 8)
                {
                    k.gameObject.layer = 8;
                    gameObject.layer = 7;
                }

                else if (gameObject.layer == 10)
                {
                    k.gameObject.layer = 8;
                    gameObject.layer = 9;
                }

                timer.Clear();
            }
            else if (k.gameObject.layer == 9) // RSU를 만났을 때 reward 값 저장하기, Q-tableUpdate
            {
                k.gameObject.layer = 10;
                gameObject.layer = 7;
                reward = rAll;
                rAll = 0;
                setState();
                UpdateState(state, reward, action);

            }
            if (gameObject.layer == 10)
            {
                k.gameObject.layer = 8;
                gameObject.layer = 9;
            }
        }


        else if (timer.Count > 400 && k == this.transform) // 20*20 = 400 / timetick이 20번 지날때까지 traffic을 전달 못하면 전달하던 traffic없애고 다시 시작RSU에서 traffic생성 하도록 함
        {

             GameObject.Find(startRSU).layer = 10;


            if (gameObject.layer == 8)
            {
                gameObject.layer = 7;
                timer.Clear();
            }
            else if (gameObject.layer == 10)
            {
                gameObject.layer = 9;
                timer.Clear();
            }

        }
    }






    //Field Of View 스크립트 부분 - 함수
    void FindVisibleTargets()
    {

        if (gameObject.layer == 8 || gameObject.layer == 10)
        {
            visibleTargets.Clear();
            // viewRadius를 반지름으로 한 원 영역 내 targetMask 레이어인 콜라이더를 모두 가져옴
            targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
            float[] dstToTarget = new float[targetsInViewRadius.Length];

            // traffic전달해줄 때 범위 가장 멀리있는 오브젝트에게 전달해주기 위한 변수
            float max = 0;         
            k = this.transform;    

            rotationValue.Add(this.transform.rotation.eulerAngles.y);

            // for문이 돌 때 범위 내 가장 가까운 target순으로 확인 
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform target = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                tfDecide();

                if (Vector3.Angle(tf, dirToTarget) < viewAngle / 2) // 플레이어와 forward와 target이 이루는 각이 설정한 각도 내라면
                {
                    dstToTarget[i] = Vector3.Distance(transform.position, target.transform.position);
                    if (max < dstToTarget[i])
                    {

                        if (target.gameObject.layer == 9)
                        {
                            max = 100;
                            k = target.transform;
                        }
                        if (target.gameObject.layer == 7)
                        {

                            if (Mathf.Round(target.transform.rotation.eulerAngles.y) == Mathf.Round(this.transform.rotation.eulerAngles.y)   /// 값이 조금씩 튈때가 있어서 Mathf.Round를 사용해 숫자 올림
                                 || Mathf.Round(target.transform.rotation.eulerAngles.y) == Mathf.Round(this.transform.rotation.eulerAngles.y) + 180
                                 || Mathf.Round(target.transform.rotation.eulerAngles.y) == Mathf.Round(this.transform.rotation.eulerAngles.y) - 180)
                            {
                                max = dstToTarget[i];
                                k = target.transform;

                            }
                        }
                    }

                }
            }

            visibleTargets.Add(k);
            timetick();
            rewardfunction();


        }

    }

    // RSU에서 해야하는 action을 선택하는 함수
    void RSU_action()
    {
        int RSUaction1 = Random.Range(0, 2);
        int RSUaction2 = Random.Range(0, 3);
        if (action == 0)
        {

            if (state % 5 == 4 && state < 25) // 오른쪽 변
            {

                if (state == 4)
                {
                    Q_table[state][destinationState][0] = -999f; // 외곽도로에서 도로 밖으로 나가는 action하면 안되므로 Q값을 매우 낮게(-999) 해 학습이 되었을때 그쪽 방향으로는 action못하게 함
                    Q_table[state][destinationState][3] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 1;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 2;
                    }
                }
                else if (state == 24)
                {
                    Q_table[state][destinationState][0] = -999f;
                    Q_table[state][destinationState][2] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 1;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 3;
                    }
                }
                else
                {
                    Q_table[state][destinationState][0] = -999f;
                    if (RSUaction2 == 0)
                    {

                        action = 1;
                    }
                    else if (RSUaction2 == 1)
                    {

                        action = 2;
                    }
                    else if (RSUaction2 == 2)
                    {

                        action = 3;
                    }
                }
            }
            else if (state % 5 != 4 && state < 25)
            {
                this.transform.eulerAngles = new Vector3(0, 90, 0); //오른쪽
                rotationValue[0] = 90;
            }
        }
        if (action == 1)
        {

            if (state % 5 == 0 && state < 25) // 왼쪽 변
            {

                if (state == 0)
                {
                    Q_table[state][destinationState][1] = -999f;
                    Q_table[state][destinationState][3] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 0;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 2;
                    }
                }
                else if (state == 20)
                {
                    Q_table[state][destinationState][1] = -999f;
                    Q_table[state][destinationState][2] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 0;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 3;
                    }
                }
                else
                {
                    Q_table[state][destinationState][1] = -999f;

                    if (RSUaction2 == 0)
                    {

                        action = 0;
                    }
                    else if (RSUaction2 == 1)
                    {

                        action = 2;
                    }
                    else if (RSUaction2 == 2)
                    {

                        action = 3;
                    }
                }
            }
            else if (state % 5 != 0 && state < 25)
            {
                this.transform.eulerAngles = new Vector3(0, 270, 0); //왼쪽
                rotationValue[0] = 270;
            }
        }
        if (action == 2)
        {

            if (state > 19 && state < 25) // 아랫 변
            {

                if (state == 20)
                {
                    Q_table[state][destinationState][1] = -999f;
                    Q_table[state][destinationState][2] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 0;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 3;
                    }
                }
                else if (state == 24)
                {
                    Q_table[state][destinationState][0] = -999f;
                    Q_table[state][destinationState][2] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 1;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 3;
                    }
                }
                else
                {
                    Q_table[state][destinationState][2] = -999f;

                    if (RSUaction2 == 0)
                    {

                        action = 0;
                    }
                    else if (RSUaction2 == 1)
                    {

                        action = 1;
                    }
                    else if (RSUaction2 == 2)
                    {

                        action = 3;
                    }
                }

            }
            else if (state < 20)
            {
                this.transform.eulerAngles = new Vector3(0, 180, 0); // 아래
                rotationValue[0] = 180;
            }
        }
        if (action == 3)
        {

            if (state < 5) // 윗 변
            {

                if (state == 0)
                {
                    Q_table[state][destinationState][1] = -999f;
                    Q_table[state][destinationState][3] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 0;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 2;
                    }
                }
                else if (state == 4)
                {
                    Q_table[state][destinationState][0] = -999f;
                    Q_table[state][destinationState][3] = -999f;
                    if (RSUaction1 == 0)
                    {

                        action = 1;
                    }
                    if (RSUaction1 == 1)
                    {

                        action = 2;
                    }
                }
                else
                {

                    Q_table[state][destinationState][3] = -999f;
                    if (RSUaction2 == 0)
                    {

                        action = 1;
                    }
                    else if (RSUaction2 == 1)
                    {

                        action = 2;
                    }
                    else if (RSUaction2 == 2)
                    {

                        action = 3;
                    }
                }
            }
            else if (state > 4 && state < 25)
            {
                this.transform.eulerAngles = new Vector3(0, 0, 0); //위
                rotationValue[0] = 0;
            }
        }
        print("RSU_action : " + action);

    }


    //Field Of View 스크립트 부분 - 함수
    void rewardfunction()
    {
        if (gameObject.layer == 8)
        {
            if (timer.Count > 19 && k != this.transform) // timetick마다 옮겨질때마다 reward를 -1씩 주어 한 hop마다 -1씩 reward를 더 해줌. 이런식으로 해서 reward를 hop count로 설정 가능
            {

                rAll += -1;

            }

            // 오브젝트의 연결성이 없을 때 Reward값이 time에 따라 증가하도록함
            else if (timer.Count % 20 == 0 && k == this.transform)
            {

                rAll += -1;

            }
        }



    }

    // Q-learning 스크립트 부분 - 함수
    // 도착지점 도달했을 때 Reset함수
    public void Reset()
    {
        currentState = 0;
        state = 0;
        nextState = 0;
        done = false;
        epsilonAction();
        episodes += 1f;
        
    }

    //Q-learning 스크립트 부분 - 함수
    public void setcurrentState()
    {
        int destination;
        destination = destinationState + 1;
        destRSU = "RSU" + destination;
        if (this.gameObject.name == destRSU && this.gameObject.layer == 10) // 도착RSU에 도달했을 때 시작RSU와 도착RSU를 랜덤으로 다시 생성
        {
            this.gameObject.layer = 9;
            set_sourceState();
            GameObject.Find("RSU"+(sourceState+1)).layer = 10;
            set_destinationState();
        }

        if (this.gameObject.name == "RSU1") currentState = 0;
        else if (this.gameObject.name == "RSU2") currentState = 1;
        else if (this.gameObject.name == "RSU3") currentState = 2;
        else if (this.gameObject.name == "RSU4") currentState = 3;
        else if (this.gameObject.name == "RSU5") currentState = 4;
        else if (this.gameObject.name == "RSU6") currentState = 5;
        else if (this.gameObject.name == "RSU7") currentState = 6;
        else if (this.gameObject.name == "RSU8") currentState = 7;
        else if (this.gameObject.name == "RSU9") currentState = 8;
        else if (this.gameObject.name == "RSU10") currentState = 9;
        else if (this.gameObject.name == "RSU11") currentState = 10;
        else if (this.gameObject.name == "RSU12") currentState = 11;
        else if (this.gameObject.name == "RSU13") currentState = 12;
        else if (this.gameObject.name == "RSU14") currentState = 13;
        else if (this.gameObject.name == "RSU15") currentState = 14;
        else if (this.gameObject.name == "RSU16") currentState = 15;
        else if (this.gameObject.name == "RSU17") currentState = 16;
        else if (this.gameObject.name == "RSU18") currentState = 17;
        else if (this.gameObject.name == "RSU19") currentState = 18;
        else if (this.gameObject.name == "RSU20") currentState = 19;
        else if (this.gameObject.name == "RSU21") currentState = 20;
        else if (this.gameObject.name == "RSU22") currentState = 21;
        else if (this.gameObject.name == "RSU23") currentState = 22;
        else if (this.gameObject.name == "RSU24") currentState = 23;
        else if (this.gameObject.name == "RSU25") currentState = 24;
    }

    // RSU에 따라 state정해주는 함수
    public void setState()
    {
        if (k.gameObject.name == ("RSU" + (destinationState + 1))) // 도착하면 done=true로 만들어 UpdateState함수에서 reward값을 많이 준다음 업데이트 하도록 함
        {
            done = true;
        }

        if (k.gameObject.name == "RSU1") state = 0;
        else if (k.gameObject.name == "RSU2") state = 1;
        else if (k.gameObject.name == "RSU3") state = 2;
        else if (k.gameObject.name == "RSU4") state = 3;
        else if (k.gameObject.name == "RSU5") state = 4;
        else if (k.gameObject.name == "RSU6") state = 5;
        else if (k.gameObject.name == "RSU7") state = 6;
        else if (k.gameObject.name == "RSU8") state = 7;
        else if (k.gameObject.name == "RSU9") state = 8;
        else if (k.gameObject.name == "RSU10") state = 9;
        else if (k.gameObject.name == "RSU11") state = 10;
        else if (k.gameObject.name == "RSU12") state = 11;
        else if (k.gameObject.name == "RSU13") state = 12;
        else if (k.gameObject.name == "RSU14") state = 13;
        else if (k.gameObject.name == "RSU15") state = 14;
        else if (k.gameObject.name == "RSU16") state = 15;
        else if (k.gameObject.name == "RSU17") state = 16;
        else if (k.gameObject.name == "RSU18") state = 17;
        else if (k.gameObject.name == "RSU19") state = 18;
        else if (k.gameObject.name == "RSU20") state = 19;
        else if (k.gameObject.name == "RSU21") state = 20;
        else if (k.gameObject.name == "RSU22") state = 21;
        else if (k.gameObject.name == "RSU23") state = 22;
        else if (k.gameObject.name == "RSU24") state = 23;
        else if (k.gameObject.name == "RSU25") state = 24;

    }


    // Q-learning 스크립트 부분 - 함수
    // Q-table 초기화
    public void SetQtable()
    {
        Q_table = new float[25][][];

        for (int i = 0; i < 25; i++)
        {
            Q_table[i] = new float[25][];
            for (int j = 0; j < 25; j++)
            {
                Q_table[i][j] = new float[4];
                for (int k = 0; k < 4; k++)
                {
                    Q_table[i][j][k] = 0;
                }
            }
        }
    }

    //Q-learning 스크립트 부분 - 함수
    // Q-table 출력 함수
    public void printQtable(int currentState) // Q-table 값 출력하기.
    {
        
        print("state " + (currentState + 1)+" Q_table");
        for (int j = 0; j < 25; j++)
        {
            print(j + 1 + "|" + string.Format("{0:0.##}", Q_table[currentState][j][0]) + " "
                + string.Format("{0:0.##}", Q_table[currentState][j][1]) + " "
                + string.Format("{0:0.##}", Q_table[currentState][j][2]) + " "
                + string.Format("{0:0.##}", Q_table[currentState][j][3]) + " ");
        }
    }

    //Q-learning 스크립트 부분 - 함수
    // epsilon greedy 함수
    public void learning_action()
    {

        if (Random.Range(0f, 1f) < e)
        {
            value = Random.Range(-1.0f, 1.0f);

            if (value <= -0.5f)//오른쪽
            {
                action = 0;

            }

            else if (value > -0.5f && value <= 0f)//왼쪽
            {
                action = 1;
            }
            else if (value > 0f && value <= 0.5f)//아래
            {
                action = 2;

            }
            else if (value > 0.5f && value <= 1.0f)//위
            {
                action = 3;
            }
            print("입실론 값 받기");
        }
        else
        {

            action = Q_table[state][destinationState].ToList().IndexOf(Q_table[state][destinationState].Max());


            print("MAX 값 ACTION = " + action);


        }


    }
    //Q-learning 스크립트 부분 - 함수


    // epsilon decay
    public float epsilonAction()
    {
        if (e > eMin)
        {
            e = e - e_decay;
            //e = e - ((1f - eMin) / (float)eStep);
        }

        return e;
    }


    //Q-learning 스크립트 부분 - 함수
    //Q-table 업데이트 해주는 함수
    public void UpdateState(int state, int reward, int action1)
    {
        print("Update전 CurrentState =" + currentState);
        print("Update전 action =" + action1);

        print(done);
        if (done == true)
        {
            reward += 100;
            Q_table[currentState][destinationState][action1] = (1 - alpha) * Q_table[currentState][destinationState][action1] + alpha * (reward + gamma * Q_table[state][destinationState].Max());
            printQtable(currentState);
            Reset();

        }

        else
        {
            Q_table[currentState][destinationState][action1] = (1 - alpha) * Q_table[currentState][destinationState][action1] + alpha * (reward + gamma * Q_table[state][destinationState].Max());
            print("Update 후 Max Q_table action 값  = " + Q_table[state][destinationState].ToList().IndexOf(Q_table[state][destinationState].Max()));
            printQtable(currentState);
        }
        currentState = nextState;

        print("Update 후 currentState = " + currentState);

    }

    // 도착RSU를 RSU23~25중 랜덤으로 선택
    public void set_destinationState()
    {
        int randomDestinationState = Random.Range(22, 25);
        destinationState = randomDestinationState;
        print("destinationState = " + destinationState);
    }

    // 도착RSU를 RSU1~3중 랜덤으로 선택
    public void set_sourceState()
    {
        int randomSource = Random.Range(0, 3);
        sourceState = randomSource;
        print("sourceState = " + sourceState);
    }

    //Field Of View 스크립트 부분 - 함수
    // FindVisibleTargets함수로 인한 범위, traffic전달 등을 시각적으로 보이게 해줌
    private void OnDrawGizmos()
    {
        if (gameObject.layer == 8 || gameObject.layer == 10)
        {
            FindVisibleTargets();
            Handles.DrawWireArc(transform.position, Vector3.down, Vector3.forward, 360, viewRadius); //3d공간에 circular 그리기 
            float angle = -viewAngle * 0.5f + transform.eulerAngles.y; //왼쪽
            float angle2 = viewAngle * 0.5f + transform.eulerAngles.y;//오른쪽
            Vector3 AngleLeft = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)); // 왼쪽
            Vector3 AngleRight = new Vector3(Mathf.Sin(angle2 * Mathf.Deg2Rad), 0, Mathf.Cos(angle2 * Mathf.Deg2Rad)); // 오른쪽
            Handles.color = Color.white;
            Handles.DrawLine(transform.position, transform.position + AngleLeft * viewRadius);
            Handles.DrawLine(transform.position, transform.position + AngleRight * viewRadius);
            Handles.color = Color.red;
            foreach (Transform visible in visibleTargets)
            {
                Handles.DrawLine(transform.position, visible.position);
            }
        }
    }
}

