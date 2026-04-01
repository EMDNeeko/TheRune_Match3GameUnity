using UnityEngine;
using UnityEngine.UI;

namespace Match3Game.Mechanics
{
    public class FloatingText : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public Text text;
        public Outline outline;
        public float moveSpeed = 2f;
        public float destroyTime = 0.8f;

        public void Setup(string value, Color color)
        {
            // Cách 1: Ưu tiên lấy từ ô đã kéo thả trong Inspector (vì bạn đã kéo sẵn như trong ảnh)
            // Cách 2: Nếu chưa kéo, nó mới tự đi tìm ở con
            text = GetComponentInChildren<Text>();
            outline = GetComponentInChildren<Outline>();

            if (text != null)
            {
                text.text = value;      // Lúc này nó sẽ gán vào đúng object con
                text.color = Color.white;
                if (outline != null)
                {
                    outline.effectColor = color;
                }
                else
                {
                    text.color = color;
                }

            }
            else
            {
                Debug.LogError("Không tìm thấy Component Text ở bất kỳ object con nào!");
            }


            Destroy(gameObject, destroyTime);
        }

        void Update()
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
        }
    }

}
