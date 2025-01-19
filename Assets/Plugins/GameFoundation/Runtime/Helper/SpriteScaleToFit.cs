using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteScaleToFit : MonoBehaviour
    {
        private void Start()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            float width = spriteRenderer.sprite.bounds.size.x;
            float height = spriteRenderer.sprite.bounds.size.y;

            float worldScreenHeight = Camera.main.orthographicSize * 2.0f;
            float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            var maxScale = Mathf.Max(worldScreenWidth / width, worldScreenHeight / height);
            transform.localScale = new Vector3(maxScale, maxScale, 1);
        }
    }
}
