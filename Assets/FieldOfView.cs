using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class FieldOfView : MonoBehaviour
{
    // field Of View ��ũ��Ʈ - ����
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


    //Q-learning ��ũ��Ʈ �κ� - ����

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

    // ����, ���� State(RSU)����
    public static int destinationState;
    public static int sourceState;

    // ó�� ����Ƽ�� ���� ������ �� �������� ����RSU�� ����RSU���� �� ����RSU���� traffic�����ϰ� traffic�� ���������� �������� �� ���� ����RSU�� ���� RSU�� �ٽ� �����ϰ� ���ִ� ����
    int tempState;
    string startRSU;
    string destRSU;

    void Awake()
    {

        //field Of View ��ũ��Ʈ �κ� - �ʱ�ȭ
        timer.Clear();
        rewardCount.Clear();
        reward = 0;
        rAll = 0;
        //Q-learning ��ũ��Ʈ �κ� - �ʱ�ȭ
        SetQtable();
        set_sourceState();
        state = 0;
        
        tempState = sourceState + 1;
        startRSU = "RSU" + tempState;
        // ��ó�� ����Ƽ ���� ���� �� traffic ����
        if (this.gameObject.name == startRSU && this.gameObject.layer == 9)
        {
            GameObject.Find(startRSU).layer = 10;
        }
            
        currentState = sourceState;
        set_destinationState();
        
    }
    
    public void Start()
    {
        //Field Of View �κ� ��ũ��Ʈ - Start
        tf = this.transform.forward; // ó�� RSU���� traffic���� ������ ���������� �� �������θ� ������ �� �ְ� ���̴� ���� �ʱⰪ

    }

    // layer 9 = RSU, layer 10 = RSUwait/ �⺻������ traffic�� �ޱ� ��,�� RSU layer�� 9�̰�, traffic�� �޾Ƽ� ����� ���� layer�� ��� 10�� �ȴ�.
    // layer 7 = target, layer 8 = start/ �⺻������ traffic�� �ޱ� ��,�� ���� layer�� 7�̰�, traffic�� �޾Ƽ� ����� ���� layer�� ��� 8�� �ȴ�.
    private void FixedUpdate()
    {
        // Field Of View ��ũ��Ʈ �κ� - ������Ʈ
                                         
        if (this.gameObject.layer == 10) //  Q-learning�� �ϱ� ���� RSU������ state,action ���������� ��
        {
            setcurrentState();
            learning_action();
            RSU_action();
        }

        if (this.gameObject.layer == 10 && this.gameObject.layer == 8)  // ���� �ּ��� ���� traffic�� �޾��� �� layer�� 10, 8�̱� ������ �̶��� FindVisibleTargets�Լ��� ������ traffic�� ������ �ش�.
        {
            FindVisibleTargets();
        }

        Time.timeScale = 0.2f; // ���ο���ó�� ����� �� �ִ�
    }

    //Field Of View ��ũ��Ʈ �κ� - �Լ�

    // ó�� RSU���� traffic���� ������ ���������� �� �������θ� ������ �� �ְ� ���̴� �Լ�
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






    // Field Of View ��ũ��Ʈ �κ� - �Լ�
    // ������ timetick�� ���� RSU�Ǵ� Vehicle���� traffic �����ϵ��� �ϴ� �Լ�
    void timetick()
    {
        timer.Add(1);

        if (timer.Count > 20 && !(k == this.transform))  // timetickũ�� 20
        {
            if (k.gameObject.layer == 7) // target�� ������ �� 
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
            else if (k.gameObject.layer == 9) // RSU�� ������ �� reward �� �����ϱ�, Q-tableUpdate
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


        else if (timer.Count > 400 && k == this.transform) // 20*20 = 400 / timetick�� 20�� ���������� traffic�� ���� ���ϸ� �����ϴ� traffic���ְ� �ٽ� ����RSU���� traffic���� �ϵ��� ��
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






    //Field Of View ��ũ��Ʈ �κ� - �Լ�
    void FindVisibleTargets()
    {

        if (gameObject.layer == 8 || gameObject.layer == 10)
        {
            visibleTargets.Clear();
            // viewRadius�� ���������� �� �� ���� �� targetMask ���̾��� �ݶ��̴��� ��� ������
            targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
            float[] dstToTarget = new float[targetsInViewRadius.Length];

            // traffic�������� �� ���� ���� �ָ��ִ� ������Ʈ���� �������ֱ� ���� ����
            float max = 0;         
            k = this.transform;    

            rotationValue.Add(this.transform.rotation.eulerAngles.y);

            // for���� �� �� ���� �� ���� ����� target������ Ȯ�� 
            for (int i = 0; i < targetsInViewRadius.Length; i++)
            {
                Transform target = targetsInViewRadius[i].transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                tfDecide();

                if (Vector3.Angle(tf, dirToTarget) < viewAngle / 2) // �÷��̾�� forward�� target�� �̷�� ���� ������ ���� �����
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

                            if (Mathf.Round(target.transform.rotation.eulerAngles.y) == Mathf.Round(this.transform.rotation.eulerAngles.y)   /// ���� ���ݾ� ƥ���� �־ Mathf.Round�� ����� ���� �ø�
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

    // RSU���� �ؾ��ϴ� action�� �����ϴ� �Լ�
    void RSU_action()
    {
        int RSUaction1 = Random.Range(0, 2);
        int RSUaction2 = Random.Range(0, 3);
        if (action == 0)
        {

            if (state % 5 == 4 && state < 25) // ������ ��
            {

                if (state == 4)
                {
                    Q_table[state][destinationState][0] = -999f; // �ܰ����ο��� ���� ������ ������ action�ϸ� �ȵǹǷ� Q���� �ſ� ����(-999) �� �н��� �Ǿ����� ���� �������δ� action���ϰ� ��
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
                this.transform.eulerAngles = new Vector3(0, 90, 0); //������
                rotationValue[0] = 90;
            }
        }
        if (action == 1)
        {

            if (state % 5 == 0 && state < 25) // ���� ��
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
                this.transform.eulerAngles = new Vector3(0, 270, 0); //����
                rotationValue[0] = 270;
            }
        }
        if (action == 2)
        {

            if (state > 19 && state < 25) // �Ʒ� ��
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
                this.transform.eulerAngles = new Vector3(0, 180, 0); // �Ʒ�
                rotationValue[0] = 180;
            }
        }
        if (action == 3)
        {

            if (state < 5) // �� ��
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
                this.transform.eulerAngles = new Vector3(0, 0, 0); //��
                rotationValue[0] = 0;
            }
        }
        print("RSU_action : " + action);

    }


    //Field Of View ��ũ��Ʈ �κ� - �Լ�
    void rewardfunction()
    {
        if (gameObject.layer == 8)
        {
            if (timer.Count > 19 && k != this.transform) // timetick���� �Ű��������� reward�� -1�� �־� �� hop���� -1�� reward�� �� ����. �̷������� �ؼ� reward�� hop count�� ���� ����
            {

                rAll += -1;

            }

            // ������Ʈ�� ���Ἲ�� ���� �� Reward���� time�� ���� �����ϵ�����
            else if (timer.Count % 20 == 0 && k == this.transform)
            {

                rAll += -1;

            }
        }



    }

    // Q-learning ��ũ��Ʈ �κ� - �Լ�
    // �������� �������� �� Reset�Լ�
    public void Reset()
    {
        currentState = 0;
        state = 0;
        nextState = 0;
        done = false;
        epsilonAction();
        episodes += 1f;
        
    }

    //Q-learning ��ũ��Ʈ �κ� - �Լ�
    public void setcurrentState()
    {
        int destination;
        destination = destinationState + 1;
        destRSU = "RSU" + destination;
        if (this.gameObject.name == destRSU && this.gameObject.layer == 10) // ����RSU�� �������� �� ����RSU�� ����RSU�� �������� �ٽ� ����
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

    // RSU�� ���� state�����ִ� �Լ�
    public void setState()
    {
        if (k.gameObject.name == ("RSU" + (destinationState + 1))) // �����ϸ� done=true�� ����� UpdateState�Լ����� reward���� ���� �ش��� ������Ʈ �ϵ��� ��
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


    // Q-learning ��ũ��Ʈ �κ� - �Լ�
    // Q-table �ʱ�ȭ
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

    //Q-learning ��ũ��Ʈ �κ� - �Լ�
    // Q-table ��� �Լ�
    public void printQtable(int currentState) // Q-table �� ����ϱ�.
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

    //Q-learning ��ũ��Ʈ �κ� - �Լ�
    // epsilon greedy �Լ�
    public void learning_action()
    {

        if (Random.Range(0f, 1f) < e)
        {
            value = Random.Range(-1.0f, 1.0f);

            if (value <= -0.5f)//������
            {
                action = 0;

            }

            else if (value > -0.5f && value <= 0f)//����
            {
                action = 1;
            }
            else if (value > 0f && value <= 0.5f)//�Ʒ�
            {
                action = 2;

            }
            else if (value > 0.5f && value <= 1.0f)//��
            {
                action = 3;
            }
            print("�ԽǷ� �� �ޱ�");
        }
        else
        {

            action = Q_table[state][destinationState].ToList().IndexOf(Q_table[state][destinationState].Max());


            print("MAX �� ACTION = " + action);


        }


    }
    //Q-learning ��ũ��Ʈ �κ� - �Լ�


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


    //Q-learning ��ũ��Ʈ �κ� - �Լ�
    //Q-table ������Ʈ ���ִ� �Լ�
    public void UpdateState(int state, int reward, int action1)
    {
        print("Update�� CurrentState =" + currentState);
        print("Update�� action =" + action1);

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
            print("Update �� Max Q_table action ��  = " + Q_table[state][destinationState].ToList().IndexOf(Q_table[state][destinationState].Max()));
            printQtable(currentState);
        }
        currentState = nextState;

        print("Update �� currentState = " + currentState);

    }

    // ����RSU�� RSU23~25�� �������� ����
    public void set_destinationState()
    {
        int randomDestinationState = Random.Range(22, 25);
        destinationState = randomDestinationState;
        print("destinationState = " + destinationState);
    }

    // ����RSU�� RSU1~3�� �������� ����
    public void set_sourceState()
    {
        int randomSource = Random.Range(0, 3);
        sourceState = randomSource;
        print("sourceState = " + sourceState);
    }

    //Field Of View ��ũ��Ʈ �κ� - �Լ�
    // FindVisibleTargets�Լ��� ���� ����, traffic���� ���� �ð������� ���̰� ����
    private void OnDrawGizmos()
    {
        if (gameObject.layer == 8 || gameObject.layer == 10)
        {
            FindVisibleTargets();
            Handles.DrawWireArc(transform.position, Vector3.down, Vector3.forward, 360, viewRadius); //3d������ circular �׸��� 
            float angle = -viewAngle * 0.5f + transform.eulerAngles.y; //����
            float angle2 = viewAngle * 0.5f + transform.eulerAngles.y;//������
            Vector3 AngleLeft = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)); // ����
            Vector3 AngleRight = new Vector3(Mathf.Sin(angle2 * Mathf.Deg2Rad), 0, Mathf.Cos(angle2 * Mathf.Deg2Rad)); // ������
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

