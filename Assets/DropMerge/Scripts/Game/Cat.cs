using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

namespace DropMerge.Game
{
    public class Cat : MonoBehaviour
    {
        [HideInInspector]
        public int catId;

        [HideInInspector]
        public int score;

        public SpriteRenderer sprite;

        public UnityAction<Cat, Cat> OnCollider;

        [HideInInspector]
        public bool appearAnimation = true;

        private Rigidbody2D _body;
        public Rigidbody2D Body
        {
            get
            {
                if (!_body)
                    _body = GetComponent<Rigidbody2D>();
                return _body;
            }
        }

        private Collider2D _collider;

        public Collider2D Collider
        {
            get
            {
                if (!_collider)
                    _collider = GetComponent<Collider2D>();
                return _collider;
            }
        }

        private Vector3 spriteRenderScale;
        private Tween warningTween;

        public void SetData(int id, CatData data)
        {
            catId = id;
            score = data.score;
        }

        public void SetActive(bool value)
        {
            Body.simulated = value;
            Collider.enabled = value;
        }

        public bool IsActive
        {
            get { return Body.simulated && Collider.enabled; }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // check is cat
            if (!other.gameObject.CompareTag(Tags.Cat))
            {
                return;
            }

            // check is same cat
            var otherCat = other.gameObject.GetComponent<Cat>();
            if (otherCat.catId != catId)
            {
                return;
            }

            // invoke event
            OnCollider.Invoke(this, otherCat);
        }

        private void Awake()
        {
            spriteRenderScale = sprite.transform.localScale;
        }

        private void OnEnable()
        {
            if (appearAnimation)
            {
                Tween.Scale(
                    sprite.transform,
                    Vector3.zero,
                    spriteRenderScale,
                    0.15f,
                    ease: Ease.OutSine
                );
            }
            else
            {
                sprite.transform.localScale = spriteRenderScale;
            }
        }

        public void Warning(bool value)
        {
            if (value && !warningTween.isAlive)
            {
                warningTween = Tween.Color(
                    sprite,
                    Color.white,
                    Color.red,
                    duration: 0.5f,
                    cycleMode: CycleMode.Yoyo,
                    cycles: -1
                );
            }
            else if (!value)
            {
                Tween.StopAll(sprite);
                sprite.color = Color.white;
            }
        }

        public bool IsIdle()
        {
            return Mathf.Abs(Body.linearVelocity.y) < 0.5f;
        }
    }
}
