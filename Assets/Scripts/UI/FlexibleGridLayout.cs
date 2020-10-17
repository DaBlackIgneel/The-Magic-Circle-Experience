using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

public class FlexibleGridLayout : LayoutGroup
{
    [ReadOnly]
    public int rows;
    [ReadOnly]
    public int columns;
    [ReadOnly]
    public Vector2 cellSize;

    public Vector2 spacing;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float sqrt = Mathf.Sqrt( transform.childCount );
        rows = Mathf.CeilToInt( sqrt );
        columns = Mathf.CeilToInt( sqrt );

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = (parentWidth - (spacing.x * columns - spacing.x) - padding.left - padding.right ) / (float) columns;
        float cellHeight = (parentHeight - (spacing.y * rows - spacing.y) - padding.top - padding.bottom ) / (float) rows;

        cellSize.x = cellWidth;
        cellSize.y = cellHeight;

        int rowIndex = 0;
        int columnIndex = 0;

        for( int i = 0; i < transform.childCount; i ++ )
        {
            rowIndex = i / columns;
            columnIndex = i % columns;

            var item = rectChildren[i];

            var xPos = (cellSize.x + spacing.x) * columnIndex + padding.left;
            var yPos = (cellSize.y + spacing.y) * rowIndex + padding.top;

            SetChildAlongAxis( item, 0, xPos, cellSize.x );
            SetChildAlongAxis( item, 1, yPos, cellSize.y );
        }
    }

    public override void CalculateLayoutInputVertical()
    {

    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {

    }
}
