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

    int[,,] DriveP = new int[25, 4, 3] // ������ box collider�� �ε����� �� ������ȯ(����,��ȸ��,��ȸ��) Ȯ�� ����
    {
         //     0            90            180              270
         //   �Ʒ���        ���ʺ�         ������          �����ʺ�
         //   {����, ��ȸ��, ��ȸ��}
        { {0, 0, 100}, {0, 0, 0}, {0, 0, 0}, {0, 100, 0} }, // RSU1 step ����
        { {0, 50, 50}, {50, 0, 50}, {0, 0, 0}, {50, 50, 0} }, // RSU2
        { {0, 50, 50}, {50, 0, 50}, {0, 0, 0}, {50, 50, 0} }, // RSU3
        { {0, 50, 50}, {50, 0, 50}, {0, 0, 0}, {50, 50, 0} }, // RSU4
        { {0, 100, 0}, {0, 0, 100}, {0, 0, 0}, {0, 0, 0} }, // RSU5 step ����
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

    private void Awake() //start���� ���� ȣ��, ��� ������ ���¸� �ʱ�ȭ�ϱ� ���� ȣ��
    {
        // ������Ʈ���� �浹�� �����ϵ��� �ϴ� ���
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

        transform.position += transform.forward * speed * Time.deltaTime; // ������ 
        rsuSelecter();
        rotation = this.transform.eulerAngles;

    }

    // ������ boxcollider�� �ε��������� ������ �ӵ��� 10���� �����ϵ��� ��
    // ������ �°� ������ȯ�� �ϱ� ���� ������ȯ�� �� ���� �ӵ��� �����س�����
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

    // ������ box collider�� �� ���� ���� �� �ӵ��� �ٲ���
    // Ư�� RSU�������� �ٸ��� ����
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


    // ������ box collider�� ��������� ��� �����
    // box collider�� �̿��� drive�Լ� ȣ��
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



    // ������ Ȯ���� ���� ������ȯ �� �� �ֵ��� ���ִ� �Լ�
    void drive(int SP, int LP, int RP) //���� ����̺� �Լ�
    {
        int randomX = Random.Range(1, SP + LP + RP);
        bool straight = (randomX <= SP); // Ȯ���� ���� ���� ���õ�
        bool left = (randomX > SP && randomX <= (SP + LP)); // Ȯ���� ���� ��ȸ�� ���õ�
                                                            // �������� ��ȸ��

        if (this.rotation.y == 0 || this.rotation.y == 360)  // ���� rotation�� 0�̸�(����Ƽ���� rotation 0�� 360�� �Ȱ����ŷ� ���������� if�������� �ٸ������� �Ǵ��ϱ� ������ �Ѵ� ����)
        {
            if (straight) // ���� Ȯ�� 50%
            {
                step_12(); 
            }
            else if (left) // ��ȸ�� Ȯ�� 30%         
            {
                if (collisionTarget.gameObject.layer == 11) // ����ȯ���� 4������ 8������ �Ѵ� �����ϴ� ���� ���� ��ȸ���� �ص� �����θ��� �� ���� step�� �����̰� ������ȯ�� �ؾ��ϴ� �κ��� ����� ������ layer������ ���
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
            else // ��ȸ�� Ȯ�� 20%
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
            if (straight) // ���� Ȯ�� 50%
            {
                step_12();
            }
            else if (left) // ��ȸ�� Ȯ�� 30%         
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
            else // ��ȸ�� Ȯ�� 20%
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
            if (straight) // ���� Ȯ�� 50%
            {
                step_12();
            }
            else if (left) // ��ȸ�� Ȯ�� 30%         
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
            else // ��ȸ�� Ȯ�� 20%
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
            if (straight) // ���� Ȯ�� 50%
            {
                step_12();
            }
            else if (left) // ��ȸ�� Ȯ�� 30%         
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
            else // ��ȸ�� Ȯ�� 20%
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



    // ������ box collider�� �ε����� �ٷ� ȸ���� �ϸ� ������ �ȸ±� ������  ��ȸ��,��ȸ�� ��Ȳ�� �°� ���� step �̵��� ȸ���� �ؾ���
    // �Ʒ� �Լ����� �������� ������ ���� ���ִ� �Լ���
    void step_36() //4�Ÿ� ��ȸ��, �ӵ� 10���� 36step��ŭ �ѹ��� ������
    {
        for (int i = 0; i < 36; i++)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_12() //4�Ÿ� ��ȸ��, �ӵ� 10���� 12step��ŭ �ѹ��� ������
    {
        for (int i = 0; i < 12; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_67() //3�Ÿ� ��ȸ��
    {
        for (int i = 0; i < 67; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_81() // �𼭸� ��ȸ��
    {
        for (int i = 0; i < 81; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void step_20() // �𼭸� ��ȸ��
    {
        for (int i = 0; i < 25; i++)
        {
            this.transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}