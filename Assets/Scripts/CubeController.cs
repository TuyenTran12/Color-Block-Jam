using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CubeController : MonoBehaviour
{
    #region Components
    private Camera mainCamera;
    private Rigidbody2D rgbd2D;
    private Vector3 offset;
    private GameObject[] cubeChildren;
    List<GameObject> childrenList = new List<GameObject>();
    private Tilemap floorGrid;
    private Tilemap blockGrid;
    private Vector3Int gridPosition;
    private ColorMathBlock block;
    [SerializeField] private ParticleSystem destroyEffectPrefab;
    #endregion

    private bool isDragging = false;
    private bool isClickCube = false;
    public int isTouching = 0;
    public enum ColorType { Red, Green, Yellow, Purple, Blue }
    [SerializeField] private ColorType cubeColor;

    [SerializeField] private int cubeFaceCount = 1;

    private void Start()
    {
        rgbd2D = GetComponent<Rigidbody2D>();
        rgbd2D.bodyType = RigidbodyType2D.Static;
        rgbd2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Ngăn tunneling
        mainCamera = Camera.main;
        CubeChildren(childrenList);

        floorGrid = GameObject.Find("FloorTile").GetComponent<Tilemap>();
        blockGrid = GameObject.Find("BlockTile").GetComponent<Tilemap>();
        block = FindAnyObjectByType<ColorMathBlock>();

        gridPosition = floorGrid.WorldToCell(transform.position);
        transform.position = floorGrid.CellToWorld(gridPosition) + new Vector3(0.225f, 0.225f, 0);
    }

    private void CubeChildren(List<GameObject> childrenList)
    {
        foreach (GameObject child in childrenList)
        {
            if (child.GetComponent<BoxCollider2D>() != null)
            {
                childrenList.Add(child.gameObject);
            }
        }
        cubeChildren = childrenList.ToArray();
    }

    #region Move Cube
    private void OnMouseDown()
    {
        isClickCube = true;

        if (!isDragging)
        {
            offset = transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            rgbd2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition) + offset;
        Vector3Int newGridPosition = floorGrid.WorldToCell(newPosition);

        gridPosition = newGridPosition;
        rgbd2D.MovePosition(floorGrid.CellToWorld(gridPosition) + new Vector3(0.225f, 0.225f, 0));
    }

    private void OnMouseUp()
    {
        isDragging = false;
        rgbd2D.bodyType = RigidbodyType2D.Static;
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateCollisionState(collision, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        UpdateCollisionState(collision, false);
    }

    private void UpdateCollisionState(Collision2D collision, bool isEntering)
    {
        if (collision.gameObject.CompareTag("BlockHaveColor"))
        {
            ColorMathBlock currentBlock = collision.gameObject.GetComponent<ColorMathBlock>();
            if (currentBlock != null && cubeColor == currentBlock.blockColor)
            {
                isTouching += isEntering ? 1 : -1;
                isTouching = Mathf.Max(0, isTouching); // Đảm bảo không âm
                if (isTouching == cubeFaceCount * 2 && cubeFaceCount <= currentBlock.blockFaceCount)
                {
                    StartCoroutine(MoveAndDestroy(collision.gameObject));
                }
            }
        }
    }

    public IEnumerator DestroyCube(GameObject cube)
    {
        ParticleSystem destroyEffect = Instantiate(destroyEffectPrefab, cube.transform.position, Quaternion.identity);

        Material particleMaterial = null;
        switch (cubeColor)
        {
            case ColorType.Red:
                particleMaterial = Resources.Load<Material>("ParticleMaterial/Red");
                break;
            case ColorType.Green:
                particleMaterial = Resources.Load<Material>("ParticleMaterial/Green");
                break;
            case ColorType.Yellow:
                particleMaterial = Resources.Load<Material>("ParticleMaterial/Yellow");
                break;
            case ColorType.Purple:
                particleMaterial = Resources.Load<Material>("ParticleMaterial/Purple");
                break;
            case ColorType.Blue:
                particleMaterial = Resources.Load<Material>("ParticleMaterial/Blue");
                break;
        }
        if (particleMaterial != null)
        {
            ParticleSystemRenderer renderer = destroyEffect.GetComponent<ParticleSystemRenderer>();
            renderer.material = particleMaterial;
        }

        destroyEffect.Play();

        cube.SetActive(false);

        yield return new WaitForSeconds(1f);

        Destroy(cube);

        yield return new WaitForSeconds(destroyEffect.main.startLifetime.constantMax);

        Destroy(destroyEffect.gameObject);
    }

    private IEnumerator MoveAndDestroy(GameObject block)
    {
        rgbd2D.bodyType = RigidbodyType2D.Kinematic;

        float speed = 3f; // Tăng tốc độ để nhanh hơn
        float moveDuration = 0.5f; // Giảm thời gian di chuyển
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = block.transform.position;
        Vector3 direction = (targetPosition - startPosition).normalized;

        while (elapsedTime < moveDuration)
        {
            float distance = speed * Time.deltaTime;
            Vector3 newPosition = transform.position + direction * distance;

            // Kiểm tra va chạm để tránh kẹt
            if (!Physics2D.OverlapCircle(newPosition, 0.1f, LayerMask.GetMask("Block")))
            {
                rgbd2D.MovePosition(newPosition);
            }
            else
            {
                // Nếu chạm block, dừng lại và điều chỉnh vị trí
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(DestroyCube(gameObject));
    }
    #region Components
    public bool IsClickCube() => isClickCube;
    #endregion
}