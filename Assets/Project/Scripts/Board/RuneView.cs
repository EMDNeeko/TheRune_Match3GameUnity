using UnityEngine;
using UnityEngine.UI;

namespace Match3Game.Board
{
    public class RuneView : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        private Vector2 targetPosition;
        private bool isMoving = false;
        private float moveSpeed = 8f;

        [Header("Effect Visuals")]
        private GameObject currentEffectObj;
        public float effectAnimationSpeed = 1f;

        public Text stackText;

        public void Initialize(Sprite sprite, Vector2 startPos)
        {
            spriteRenderer.sprite = sprite;
            transform.position = startPos;
            targetPosition = startPos;

            UpdateStackText(0);
            ClearEffectVisual();
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

        public void ApplyEffectVisual(GameObject effectPrefab, int effectStack)
        {
            ClearEffectVisual();
            if (effectPrefab != null)
            {
                currentEffectObj = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
                currentEffectObj.transform.localPosition = new Vector3(0, 0, -1f);

                // ĐÃ SỬA: Gọi thẳng hàm UpdateStackText để nó tự lo việc bật/tắt UI thay vì chỉ gán chữ
                UpdateStackText(effectStack);

                //chinh toc do animate neu co animator
                Animator anim = currentEffectObj.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.speed = effectAnimationSpeed;
                }
            }
        }
        public void ClearEffectVisual()
        {
            if (currentEffectObj != null)
            {
                Destroy(currentEffectObj);
                currentEffectObj = null;
            }
        }

        public void UpdateStackText(int stacks)
        {
            if (stackText != null)
            {
                if (stacks > 0)
                {
                    stackText.gameObject.SetActive(true);
                    stackText.text = stacks.ToString();
                }
                else
                {
                    // Chắc chắn ẩn đi khi số tầng <= 0 hoặc khi đá mới được tạo ra
                    stackText.gameObject.SetActive(false);
                }
            }
            else
            {
                // In ra cảnh báo đỏ nếu quên kéo thả Text trong Unity
                Debug.LogWarning("Chưa gán biến 'Stack Text' cho Rune Prefab trong Inspector!");
            }
        }
    }
}