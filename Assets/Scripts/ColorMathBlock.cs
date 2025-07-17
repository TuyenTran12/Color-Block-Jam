using System.Collections.Generic;
using UnityEngine;

public class ColorMathBlock : MonoBehaviour
{
    private GameObject[] blockChildren;
    List<GameObject> childrenList = new List<GameObject>();

    public CubeController.ColorType blockColor;
    public int blockFaceCount;
    // Start is called before the first frame update
    void Start()
    {
        BlockChildren(childrenList);
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void BlockChildren(List<GameObject> children)
    {
        foreach (GameObject child in children)
        {
            if (child.GetComponent<BoxCollider2D>() != null)
            {
                children.Add(child.gameObject);
                blockFaceCount++;
            }
        }
        blockChildren = children.ToArray();
    }
    
}
