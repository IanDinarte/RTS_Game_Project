using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;


/**
- direcao da formação ser a direção praonde eles andaram
- settings pro navmesh:
    - tile: 1024 (default: 256)
    - voxel: 0.01 (default: 0.1666667)
*/
public class GameRTSController : MonoBehaviour
{
    public static GameRTSController Instance { get; private set; }

    private Vector2 startPosition;
    private Vector2 endPosition;

    [SerializeField] private Transform selectionAreaTransform; //Selection Area Box
    [SerializeField] private int unitsPerRow; //Max units per row on formation (army)
    [SerializeField] public List<UnitController> selectedUnitList; //list of currently selected units
    [SerializeField] private float spacing; //gap between units in formation

    //navmesh agent stuff
    [SerializeField] private float AvoidancePredictionTime = 2;
    [SerializeField] private int PathfindingIterationsPerFrame = 100;

    private bool isShiftHeld;
    private bool isCtrlHeld;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        unitsPerRow = 3;
        spacing = 0.5f;
        selectedUnitList = new List<UnitController>();
        selectionAreaTransform.gameObject.SetActive(false);
        NavMesh.avoidancePredictionTime = AvoidancePredictionTime;
        NavMesh.pathfindingIterationsPerFrame = PathfindingIterationsPerFrame;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }
        else
        {
            isShiftHeld = Keyboard.current.shiftKey.isPressed;
            isCtrlHeld = Keyboard.current.ctrlKey.isPressed;

            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                if (selectedUnitList.Count > 0)
                {
                    foreach (UnitController unit in selectedUnitList)
                    {
                        // unit.SetAttackTarget(null);
                        unit.StopAction();
                    }
                }
            }

            if (Keyboard.current.deleteKey.wasPressedThisFrame || Keyboard.current.numpadPeriodKey.wasPressedThisFrame)
            {
                if (selectedUnitList.Count > 0)
                {
                    UnitController unitToDestroy = selectedUnitList.First();
                    if (unitToDestroy != null)
                    {
                        UnitHitController hitController = unitToDestroy.GetComponent<UnitHitController>();
                        if (hitController != null)
                        {
                            hitController.CurrentHealth = 0;
                        }
                    }
                }
            }
        }

        if (Mouse.current == null)
        {
            return;
        }
        else
        {
            //left click stuff
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                selectionAreaTransform.gameObject.SetActive(true);
                startPosition = Utils.GetWorldMousePosition();

                selectionAreaTransform.position = startPosition;
            }

            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 currentMousePos = Utils.GetWorldMousePosition();

                Vector3 lowerLeft = new Vector3(
                    Mathf.Min(startPosition.x, currentMousePos.x),
                    Mathf.Min(startPosition.y, currentMousePos.y)
                );

                Vector3 upperRight = new Vector3(
                    Mathf.Max(startPosition.x, currentMousePos.x),
                    Mathf.Max(startPosition.y, currentMousePos.y)
                );

                selectionAreaTransform.position = lowerLeft;
                selectionAreaTransform.localScale = (upperRight - lowerLeft) * 100;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (!isShiftHeld && !isCtrlHeld)
                {
                    foreach (UnitController unitController in selectedUnitList)
                    {
                        unitController.SetSelectedVisible(false);
                    }
                    selectedUnitList.Clear();
                }

                selectionAreaTransform.gameObject.SetActive(false);
                endPosition = Utils.GetWorldMousePosition();

                float dragDistance = Vector2.Distance(startPosition, endPosition);

                if (dragDistance < 0.15f)
                {
                    Collider2D[] collider2DArray = Physics2D.OverlapPointAll(endPosition);

                    UnitController targetUnit = GetFrontUnit(collider2DArray, "Player");

                    if (targetUnit != null)
                    {
                        targetUnit.SetSelectedVisible(true);
                        selectedUnitList.Add(targetUnit);
                    }
                }
                else
                {
                    Collider2D[] collider2DArray = Physics2D.OverlapAreaAll(startPosition, endPosition);

                    foreach (Collider2D collider2D in collider2DArray)
                    {
                        UnitController unitController = collider2D.GetComponent<UnitController>();
                        if (unitController != null && collider2D.gameObject.CompareTag("Player"))
                        {
                            if (!selectedUnitList.Contains(unitController))
                            {
                                unitController.SetSelectedVisible(true);
                                selectedUnitList.Add(unitController);
                            }
                        }
                    }
                }
            }

            //right click stuff
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Vector2 target = Utils.GetWorldMousePosition();

                Collider2D[] collider2DArray = Physics2D.OverlapPointAll(target);

                UnitController targetUnit = GetFrontUnit(collider2DArray, "Enemy");

                /*
                isso dentro da cena de mover se calhar, que dai eles se movem pro local
                e quando chegam la começam a atacar, e enquanto o inimigo for alvo eles
                atualizam o destino pro inimigo e continuam seguindo e atacando.
                */
                if (selectedUnitList.Count > 0)
                {
                    // List<Vector2> targetPositionList = GetPositionListRectangle(target, selectedUnitList.Count, unitPerRow);
                    List<Vector2> targetPositionList = GetOrientedFormationPositions(target);
                    int targetPositionListIndex = 0;

                    foreach (UnitController unit in selectedUnitList)
                    {
                        unit.StopAction();

                        if (targetUnit != null)
                        {
                            // Debug.Log("ataca");
                            unit.SetAttackTarget(targetUnit);
                        }
                        else
                        {
                            // Debug.Log("move");
                            unit.OrderMove(targetPositionList[targetPositionListIndex]);
                        }

                        targetPositionListIndex++;
                    }
                }
            }
        }
    }

    private UnitController GetFrontUnit(Collider2D[] collider2DArray, string targetTag)
    {
        UnitController frontUnit = null;
        float lowestY = float.MaxValue;

        foreach (Collider2D collider2D in collider2DArray)
        {
            UnitController unitController = collider2D.GetComponent<UnitController>();

            if (unitController != null && collider2D.gameObject.CompareTag(targetTag))
            {
                if (unitController.transform.position.y < lowestY)
                {
                    lowestY = unitController.transform.position.y;
                    frontUnit = unitController;
                }
            }
        }
        return frontUnit;
    }

    private List<Vector2> GetPositionListRectangle(Vector2 targetPosition, int unitCount, int unitPerRow)
    {
        List<Vector2> positionList = new List<Vector2>();
        int rowCount = DefineRowCount(unitCount, unitPerRow);

        float totalHeight = (rowCount - 1) * spacing;

        for (int i = 0; i < rowCount; i++)
        {
            int unitsInThisRow = Mathf.Min(unitPerRow, unitCount - (i * unitPerRow));
            float rowWidth = (unitsInThisRow - 1) * spacing;

            for (int j = 0; j < unitsInThisRow; j++)
            {
                float xOffset = (spacing * j) - (rowWidth / 2f);
                float yOffset = (spacing * i) - (totalHeight / 2f);

                positionList.Add(targetPosition + new Vector2(xOffset, yOffset));
            }
        }

        return positionList;
    }

    private List<Vector2> GetOrientedFormationPositions(Vector2 targetPosition)
    {
        List<Vector2> positionList = new List<Vector2>();
        int totalUnits = selectedUnitList.Count();

        if (totalUnits == 0) return positionList;

        Vector2 groupCenter = Vector2.zero;
        foreach (UnitController unit in selectedUnitList)
        {
            groupCenter += (Vector2)unit.transform.position;
        }
        groupCenter /= totalUnits;

        Vector2 direction = targetPosition - groupCenter;
        Vector2 forward = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.up;
        Vector2 right = new Vector2(forward.y, -forward.x);

        int totalRows = DefineRowCount(totalUnits, unitsPerRow);

        for (int i = 0; i < totalUnits; i++)
        {
            int row = i / unitsPerRow;
            int col = i % unitsPerRow;

            int unitsInCurrentRow = Mathf.Min(unitsPerRow, totalUnits - (row * unitsPerRow));

            float rowOffset = (row - (totalRows - 1) * 0.5f) * spacing;
            float colOffset = (col - (unitsInCurrentRow - 1) * 0.5f) * spacing;

            Vector2 finalPos = targetPosition + (right * colOffset) - (forward * rowOffset);
            positionList.Add(finalPos);
        }

        return positionList;
    }

    private int DefineRowCount(int unitCount, int unitPerRow)
    {
        int rowCount = (int)Math.Ceiling((double)unitCount / unitPerRow);

        return rowCount;
    }

    public void DeselectUnit(UnitController unit)
    {
        if (unit != null && selectedUnitList.Contains(unit))
        {
            unit.SetSelectedVisible(false);
            selectedUnitList.Remove(unit);
        }
    }
}
