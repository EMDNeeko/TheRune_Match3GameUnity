using UnityEngine;

namespace Match3Game.Board
{
    public class RuneView : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        private Vector2 targetPosition;
        private bool isMoving = false;
        private float moveSpeed = 8f;

        public void Initialize(Sprite sprite, Vector2 startPos)
        {
            spriteRenderer.sprite = sprite;
            transform.position = startPos;
            targetPosition = startPos;
        }

        //khi rune thay doi vi tri
        public void MoveToPosition(Vector2 newTarget)
        {
            targetPosition = newTarget;
            isMoving = true;
        }

        void Update()
        {
            if (isMoving)
            {
                transform.position = Vector2.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetPosition) < 0.05f)
                {
                    transform.position = targetPosition;
                    isMoving = false;
                }
            }
        }
    }
}