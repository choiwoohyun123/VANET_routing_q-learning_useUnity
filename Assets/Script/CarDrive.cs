using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDrive : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rigid;
    [Range(0, 360)]
    public int speed = 10;
    private Vector3 rotation;
    public int way = 4;
    public int turn = 3;
    public int rsu_row = 5;
    public int rsu_col = 5;
    public string[][] rsuSelect;
    public string[] a;
    Collision collisionTarget;

    int[,,] DriveP = new int[25, 4, 3] // 차량이 box collider에 부딪혔을 때 방향전환(직진,좌회전,우회전) 확률 변수
    {
         //     0            90            180              270
         //   아래벽        왼쪽벽         위에벽          오른쪽벽
         //   {직진, 좌회전, 우회전}
        { {0, 0, 100}, {0, 0, 0}, {0, 0, 0}, {0, 100, 0} }, // RSU1 step 수정
        { {0, 50, 50}, {50, 0, 50}, {0, 0, 0}, {50, 50, 0} }, // RSU2
        { {0, 50, 50}, {50, 0, 50}, {0, 0, 0}, {50, 50, 0} }, // RSU3
        { {0, 50, 50}, {50, 0, 50}, {0, 0, 0}, {50, 50, 0} }, // RSU4
        { {0, 100, 0}, {0, 0, 100}, {0, 0, 0}, {0, 0, 0} }, // RSU5 step 수정
        { {50, 0, 50}, {0, 0, 0}, {50, 50, 0}, {0, 50, 50} }, // RSU6
        { {50, 20, 30}, {40, 40, 20}, {50, 20, 30}, {80, 10, 10} }, // RSU7
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU8
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU9
        { {50, 50, 0}, {0, 50, 50}, {50, 0, 50}, {0, 0, 0} }, // RSU10
        { {50, 0, 50}, {0, 0, 0}, {50, 50, 0}, {0, 50, 50} }, // RSU11
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU12
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU13
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU14
        { {50, 50, 0}, {0, 50, 50}, {50, 0, 50}, {0, 0, 0} }, // RSU15
        { {50, 0, 50}, {0, 0, 0}, {50, 50, 0}, {0, 50, 50} }, // RSU16
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU17
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU18
        { {50, 20, 30}, {50, 20, 30}, {50, 20, 30}, {50, 20, 30} }, // RSU19
        { {50, 50, 0}, {0, 50, 50}, {50, 0, 50}, {0, 0, 0} }, // RSU20
        { {0, 0, 0}, {0, 0, 0}, {0, 100, 0}, {0, 0, 100} }, // RSU21
        { {0, 0, 0}, {50, 50, 0}, {0, 50, 50}, {50, 0, 50} }, // RSU22
        { {0, 0, 0}, {50, 50, 0}, {0, 50, 50}, {50, 0, 50} }, // RSU23
        { {0, 0, 0}, {50, 50, 0}, {0, 50, 50}, {50, 0, 50} }, // RSU24
        { {0, 0, 0}, {0, 100, 0}, {0, 0, 100}, {0, 0, 0} }, // RSU25
    };

    private void Awake() //start보다 먼저 호출, 모든 변수와 상태를 초기화하기 위해 호출
    {
        // 오브젝트끼리 충돌을 무시하도록 하는 기능
        rigid = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("target"), LayerMask.NameToLayer("target"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("target"), LayerMask.NameToLayer("start"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("target"), LayerMask.NameToLayer("RSU"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("target"), LayerMask.NameToLayer("RSUwait"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("start"), LayerMask.NameToLayer("start"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("start"), LayerMask.NameToLayer("RSU"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("start"), LayerMask.NameToLayer("RSUwait"), true);
    }

    public void Start()
    {

    }

    // Update is called once per frame
    public void FixedUpdate()
    {

        transform.position += transform.forward * speed * Time.deltaTime; // 차량이 
        rsuSelecter();
        rotation = this.transform.eulerAngles;

    }

    // 차량이 boxcollider에 부딪혔을때는 무조건 속도를 10으로 고정하도록 함
    // 차선에 맞게 방향전환을 하기 위해 방향전환을 할 때는 속도를 고정해놔야함
    public void OnCollisionEnter(Collision collision)
    {
        collisionTarget = collision;
        for (int i = 0; i < DriveP.GetLength(0); i++)
        {
            if (collision.collider.CompareTag("RSU" + (i + 1)))
            {
                speed = 10;
            }
        }
    }

    // 차량이 box collider를 다 지나 갔을 때 속도를 바꿔줌
    // 특정 RSU지점마다 다르게 설정
    public void OnCollisionExit(Collision collision)
    {
        for (int i = 0; i < DriveP.GetLength(0); i++)
        {
            if (collision.collider.CompareTag("RSU" + (i + 1)))
            {
                if ((i + 1) <= 5 || (i + 1) > 20) speed = 30;
                else if ((i + 1) % 5 == 0 || (i + 1) % 5 == 1) speed = 40;
                else speed = 50;
            }
        }
    }


    // 차량이 box collider에 닿아있으면 계속 실행됨
    // box collider를 이용해 drive함수 호출
    public void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < DriveP.GetLength(0); i++)
        {
            if (collision.collider.CompareTag("RSU" + (i + 1)))
            {
                for (int j = 0; j < DriveP.GetLength(1); j++)
                {
                    if (rotation.y == j * 90)
                    {
                        drive(DriveP[i, j, 0], DriveP[i, j, 1], DriveP[i, j, 2]);
                    }
                }
            }
        }
    }



    // 차량을 확률에 따라 방향전환 할 수 있도록 해주는 함수
    void drive(int SP, int LP, int RP) //차량 드라이브 함수
    {
        int randomX = Random.Range(1, SP + LP + RP);
        bool straight = (randomX <= SP); // 확률에 따라 직진 선택됨
        bool left = (randomX > SP && randomX <= (SP + LP)); // 확률에 따라 좌회전 선택됨
                                                            // 나머지는 우회전

        if (this.rotation.y == 0 || this.rotation.y == 360)  // 차량 rotation이 0이면(유니티에서 rotation 0과 360이 똑같은거로 존재하지만 if문에서는 다른것으로 판단하기 때문에 둘다 적용)
        {
            if (straight) // 직진 확률 50%
            {
                step_12(); 
            }
            else if (left) // 좌회전 확률 30%         
            {
                if (collisionTarget.gameObject.layer == 11) // 도로환경이 4차선과 8차선이 둘다 존재하다 보니 같은 좌회전을 해도 교차로마다 더 많은 step을 움직이고 방향전환을 해야하는 부분이 생기기 때문에 layer조건을 사용
                {
                    step_67();
                }
                else if (collisionTarget.gameObject.layer == 12)
                {
                    step_81();

                }
                else if (collisionTarget.gameObject.layer == 0)
                {
                    step_36();
                }
                this.transform.eulerAngles = new Vector3(0, 270, 0);
            }
            else // 우회전 확률 20%
            {
                if (collisionTarget.gameObject.layer == 12)
                {
                    step_20();

                }
                else
                {
                    step_12();
                }
                this.transform.eulerAngles = new Vector3(0, 90, 0);
            }
        }
        else if (this.rotation.y == 270 || this.rotation.y == -90)
        {
            if (straight) // 직진 확률 50%
            {
                step_12();
            }
            else if (left) // 좌회전 확률 30%         
            {
                if (collisionTarget.gameObject.layer == 11)
                {
                    step_67();
                }
                else if (collisionTarget.gameObject.layer == 12)
                {
                    step_81();

                }
                else if (collisionTarget.gameObject.layer == 0)
                {
                    step_36();
                }
                this.transform.eulerAngles = new Vector3(0, 180, 0);
            }
            else // 우회전 확률 20%
            {
                if (collisionTarget.gameObject.layer == 12)
                {
                    step_20();

                }
                else
                {
                    step_12();
                }
                this.transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        else if (this.rotation.y == 180 || this.rotation.y == -180)
        {
            if (straight) // 직진 확률 50%
            {
                step_12();
            }
            else if (left) // 좌회전 확률 30%         
            {
                if (collisionTarget.gameObject.layer == 11)
                {
                    step_36();
                }
                else if (collisionTarget.gameObject.layer == 12)
                {
                    step_81();

                }
                else if (collisionTarget.gameObject.layer == 0)
                {
                    step_36();
                }
                this.transform.eulerAngles = new Vector3(0, 90, 0);
            }
            else // 우회전 확률 20%
            {
                if (collisionTarget.gameObject.layer == 12)
                {
                    step_20();

                }
                else
                {
                    step_12();
                }
                this.transform.eulerAngles = new Vector3(0, 270, 0);
            }
        }
        else if (this.rotation.y == 90 || this.rotation.y == -270)
        {
            if (straight) // 직진 확률 50%
            {
                step_12();
            }
            else if (left) // 좌회전 확률 30%         
            {
                if (collisionTarget.gameObject.layer == 11)
                {
                    step_36();
                }
                else if (collisionTarget.gameObject.layer == 12)
                {
                    step_81();

                }
                else if (collisionTarget.gameObject.layer == 0)
                {
                    step_36();
                }

                this.transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else // 우회전 확률 20%
            {

                if (collisionTarget.gameObject.layer == 12)
                {
                    step_20();

                }
                else
                {
                    step_12();
                }
                this.transform.eulerAngles = new Vector3(0, 180, 0);
            }
        }
    }

    void rsuSelecter() //RSU
    {
        rsuSelect = new string[rsu_row][];
        for (int i = 0; i < rsu_row; i++)
        {
            rsuSelect[i] = new string[rsu_col];
            for (int j = 0; j < rsu_col; j++)
            {
                rsuSelect[i][j] = "RSU" + (i * 5 + j + 1);
            }
        }

    }



    // 차량이 box collider에 부딪히고 바로 회전을 하면 차선이 안맞기 때문에  우회전,좌회전 상황에 맞게 일정 step 이동후 회전을 해야함
    // 아래 함수들은 일정스텝 앞으로 가게 해주는 함수들
    void step_36() //4거리 좌회전, 속도 10으로 36step만큼 한번에 움직임
    {
        for (int i = 0; i < 36; i++)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_12() //4거리 우회전, 속도 10으로 12step만큼 한번에 움직임
    {
        for (int i = 0; i < 12; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_67() //3거리 좌회전
    {
        for (int i = 0; i < 67; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_81() // 모서리 좌회전
    {
        for (int i = 0; i < 81; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_20() // 모서리 우회전
    {
        for (int i = 0; i < 25; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}