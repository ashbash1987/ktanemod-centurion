using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoSpacing))]
public class AutoSpacingEditor : Editor
{
    private AutoSpacing _spacing = null;

    private void OnEnable()
    {
        _spacing = (AutoSpacing)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Transform transform = _spacing.transform;
        int childCount = transform.childCount;
        Vector3 maxSpaceBetweenElements = (childCount - 1) * _spacing.spaceBetweenElements;
        Vector3 edge = maxSpaceBetweenElements * -0.5f;
        for (int childIndex = 0; childIndex < childCount; ++childIndex)
        {
            Transform child = transform.GetChild(childIndex);
            child.localPosition = Vector3.Lerp(edge, -edge, childIndex / (childCount - 1.0f));
        }        
    }
}
