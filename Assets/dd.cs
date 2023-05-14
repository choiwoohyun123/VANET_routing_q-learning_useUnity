using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dd : MonoBehaviour
{
    public float[][][] array;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        setArray();
        printArray();
    }
    void setArray()
    {
        array = new float[25][][];
        for(int i = 0; i<25; i++)
        {
            array[i] = new float[25][];
            for(int j=0; j<25;j++)
            {
                array[i][j] = new float[4];
                for(int k=0; k<4; k++)
                {
                    array[i][j][k] = 0.1f;
                }

            }
        }
    }

    void printArray()
    {
        for(int i = 0; i<25; i++)
        {
            for (int j = 0; j < 25; j++)
            {
                print(i + 1 + "|" + string.Format("{0:0.##}", array[i][j][0]) + " "
                    + string.Format("{0:0.##}", array[i][j][1]) + " "
                    + string.Format("{0:0.##}", array[i][j][2]) + " "
                    + string.Format("{0:0.##}", array[i][j][3]) + " ");
            }
        }
    }
}
