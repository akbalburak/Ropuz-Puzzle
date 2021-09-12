using UnityEngine;

public class PlayGroundItemController : MonoBehaviour
{
    public Vector3? TargetPosition { get; private set; }

    /// <summary>
    /// When the rotation is completed just set true.
    /// </summary>
    public bool IsRotationCompleted { get; private set; }

    private RectTransform Rect;
    // Start is called before the first frame update
    void Start()
    {
        Rect = GetComponent<RectTransform>();    
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if there is no target position you dont have to go further.
        if (!TargetPosition.HasValue)
            return;

        // If rotation completed return.
        if (IsRotationCompleted)
            return;

        // We are moving through the position.
        Rect.anchoredPosition = Vector3.Lerp(Rect.anchoredPosition, TargetPosition.Value, Time.fixedDeltaTime * GameController.Instance.RotationSpeed);

        // if we move enough we will stop moving.
        if (Vector3.Distance(Rect.anchoredPosition, TargetPosition.Value) <= GameController.Instance.RotationSpeed * Time.fixedDeltaTime)
        {
            // Position must be the same.
            Rect.anchoredPosition = TargetPosition.Value;

            // We are telling rotation is completed.
            IsRotationCompleted = true;
        }
    }

    public void StartRotate(Vector3 position)
    {
        // Destination position.
        this.TargetPosition = position;
    }

    public void RevertItemProps()
    {
        // We are resetting rotation completed state.
        this.IsRotationCompleted = false;

        // We reset the rotation position.
        this.TargetPosition = null;
    }
}
