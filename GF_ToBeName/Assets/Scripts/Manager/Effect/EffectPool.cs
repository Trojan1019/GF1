using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using DG.Tweening;
using System;
using DG.Tweening.Core;
using UnityEngine.UI;
using TMPro;

namespace NewSideGame
{
    public class EffectPool : ActivatablePoolPrefabBase
    {
        [SerializeField] private ParticleSystem[] notPlayeff;
        ParticleSystem[] m_particleSystem;
        TrailRenderer[] trails;
        Sequence sequence;
        Sequence sequenceScale;
        Sequence sequenceRotate;
        Sequence sequenceFade;

        private bool hasAutodestory;

        public override void OnInit(PoolManager ppm)
        {
            base.OnInit(ppm);
            m_particleSystem = gameObject.GetComponentsInChildren<ParticleSystem>();
            trails = gameObject.GetComponentsInChildren<TrailRenderer>();

            hasAutodestory = gameObject.GetComponent<AutoDestroy>() != null;
        }


        public override void OnSpawn(PoolManager ppm)
        {
            base.OnSpawn(ppm);

            foreach (var item in trails)
            {
                item.enabled = false;

            }

            if (notPlayeff != null)
            {
                foreach (var item in notPlayeff)
                {
                    item.gameObject.SetActive(false);
                }
            }

            CachedTransform.position = Vector3.zero;
            CachedTransform.localScale = Vector3.one;
            CachedTransform.rotation = Quaternion.identity;
            CachedTransform.gameObject.SetActive(true);
            Image image = gameObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
            }

            foreach (var item in trails)
            {
                item.enabled = true;
                item.Clear();
            }
        }

        public override void OnDeSpawn(PoolManager ppm)
        {
            base.OnDeSpawn(ppm);

            foreach (var item in trails)
            {
                item.enabled = false;
                item.enabled = true;
                item.Clear();
            }

            if (!hasAutodestory)
            {
                var auto = gameObject.GetComponent<AutoDestroy>();
                if (auto != null)
                    DestroyImmediate(auto);
            }

            sequence = null;
            sequenceScale = null;
            sequenceRotate = null;
            sequenceFade = null;
        }

        public void Play(bool playHide = false)
        {
            for (int i = 0; i < m_particleSystem.Length; i++)
            {
                if (m_particleSystem[i] != null)
                {
                    m_particleSystem[i].Play();
                }
            }

            if (playHide)
            {
                if (notPlayeff != null)
                {
                    foreach (var item in notPlayeff)
                    {
                        item.gameObject.SetActive(true);
                        item.Play();
                    }
                }
            }
        }

        #region 附加功能
        /// <summary>
        /// 附加Ui特效属性
        /// </summary>
        public EffectPool AttachUIVfx(int order = 1, bool isUI = false, int layerId = -1)
        {
            //Ui 特效特殊的处理
            this.CachedTransform.SetParent(UITopCoverForm.Instance.transform);
            this.transform.localScale = Vector3.one;
            if (layerId < 0)
                this.gameObject.SetLayerRecursively(Constant.Layer.UILayerId);
            else
            {
                this.gameObject.SetLayerRecursively(layerId);
            }
            var depth = this.gameObject.GetOrAddComponent<UIDepth>();
            depth.isUI = isUI;
            depth.order = order;
            depth.Refresh();
            return this;
        }

        public EffectPool AttachUIVfxB(int order = 1, bool isUI = false, float scale = 1f)
        {
            //Ui 特效特殊的处理，自定义缩放
            this.CachedTransform.SetParent(UITopCoverForm.Instance.transform);
            this.transform.localScale = Vector3.one * scale;
            this.gameObject.SetLayerRecursively(Constant.Layer.UILayerId);
            var depth = this.gameObject.GetOrAddComponent<UIDepth>();
            depth.isUI = isUI;
            depth.order = order;
            depth.Refresh();
            return this;
        }

        public EffectPool AttachUIVfxC(Transform parent, int order = 1, bool isUI = false, float scale = 1f)
        {
            //Ui 特效特殊的处理，自定义缩放
            this.CachedTransform.SetParent(parent);
            this.transform.localScale = Vector3.one * scale;
            this.gameObject.SetLayerRecursively(Constant.Layer.UILayerId);
            var depth = this.gameObject.GetOrAddComponent<UIDepth>();
            depth.isUI = isUI;
            depth.order = order;
            depth.Refresh();
            return this;
        }

        /// <summary>
        /// 附加自动回收
        /// </summary>
        /// <returns></returns>
        public EffectPool AttachAutoDestory(float time)
        {
            var autoDestory = gameObject.GetOrAddComponent<AutoDestroy>();
            autoDestory.delayTime = time;
            autoDestory.Reset();
            return this;
        }

        public void Recyle()
        {
            GameEntry.PoolManager.DeSpawnSync(this);
        }

        /// <summary>
        /// 一直不销毁
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public EffectPool AttachNeverDestory(float time)
        {
            var autoDestory = gameObject.GetComponent<AutoDestroy>();
            if (autoDestory != null)
            {
                UnityEngine.Object.Destroy(autoDestory);
            }
            return this;
        }

        /// <summary>
        /// 附加飞行动画
        /// </summary>
        /// <param name="starPos"></param>
        /// <param name="endPos"></param>
        /// <param name="flyduration"></param>
        /// <returns></returns>
        public EffectPool AttachFly(Vector3 starPos, Vector3 endPos, float flyduration, Action finish = null, Ease ease = Ease.Linear)
        {
            CachedTransform.position = starPos;
            gameObject.SetActive(true);
            Play();
            if (sequence != null)
                sequence.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(CachedTransform.DOMove(endPos, flyduration).SetEase(ease));
            sequence.OnComplete(() =>
            {
                finish?.Invoke();
            });
            sequence.Play();
            return this;
        }


        public EffectPool AttachFlyAnchorPos(Vector2 starPos, Vector2 endPos, float flyduration, Action finish = null, Ease ease = Ease.Linear)
        {
            var rectTransform = CachedTransform.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = starPos;
            gameObject.SetActive(true);
            Play();
            if (sequence != null)
                sequence.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOAnchorPos(endPos, flyduration));
            sequence.OnComplete(() =>
            {
                finish?.Invoke();
            });
            sequence.SetEase(ease);
            sequence.Play();
            return this;
        }

        /// <summary>
        /// 附加飞行动画  
        /// </summary>
        /// <param name="starPos"></param>
        /// <param name="endPos"></param>
        /// <param name="flyduration"></param>
        /// <returns></returns>
        public EffectPool AttachFlyPath(Vector3 starPos, Vector3[] path, float flyduration, Action finish = null, Ease ease = Ease.Linear)
        {
            CachedTransform.position = starPos;
            gameObject.SetActive(true);
            Play();
            if (sequence != null)
                sequence.Kill();
            sequence = DOTween.Sequence();
            sequence.Append(CachedTransform.DOPath(path, flyduration, PathType.CatmullRom));
            sequence.OnComplete(() =>
            {
                finish?.Invoke();
            });
            sequence.SetEase(ease);
            sequence.Play();
            return this;
        }

        /// <summary>
        /// 附加 生成到默认地点
        /// </summary>
        /// <param name="starPos"></param>
        /// <returns></returns>
        public EffectPool AttachSpawnPosition(Vector3 starPos)
        {
            CachedTransform.position = starPos;
            gameObject.SetActive(true);
            Play();
            return this;
        }

        public EffectPool AttachFade(float fade, float fadeDuration, Action finish = null)
        {
            Image image = gameObject.GetComponent<Image>();
            if (image == null) return this;

            image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
            if (sequenceFade != null)
                sequenceFade.Kill();
            sequenceFade = DOTween.Sequence();
            sequenceFade.Append(image.DOFade(fade, fadeDuration));
            sequenceFade.OnComplete(() =>
            {
                finish?.Invoke();
            });
            return this;
        }

        /// <summary>
        /// 附加缩放动画
        /// </summary>
        /// <param name="startScale"></param>
        /// <param name="endScale"></param>
        /// <param name="scaleDuration"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        public EffectPool AttachScale(Vector3 startScale, Vector3 endScale, float scaleDuration, Action finish = null, Ease ease = Ease.Linear)
        {
            CachedTransform.localScale = startScale;
            gameObject.SetActive(true);
            Play();
            if (sequenceScale != null)
                sequenceScale.Kill();
            sequenceScale = DOTween.Sequence();
            sequenceScale.Append(CachedTransform.DOScale(endScale, scaleDuration));
            sequenceScale.OnComplete(() =>
            {
                finish?.Invoke();
            });
            sequenceScale.SetEase(ease);
            sequenceScale.Play();
            return this;
        }

        public EffectPool AttachRotate(Vector3 endValue, float scaleDuration, RotateMode mode = RotateMode.FastBeyond360, Action finish = null, Ease ease = Ease.Linear)
        {
            gameObject.SetActive(true);
            Play();

            if (sequenceRotate != null)
                sequenceRotate.Kill();
            sequenceRotate = DOTween.Sequence();
            sequenceRotate.Append(CachedTransform.DORotate(endValue, scaleDuration, mode));
            sequenceRotate.OnComplete(() =>
            {
                finish?.Invoke();
            });
            sequenceRotate.SetEase(ease);
            sequenceRotate.Play();
            return this;
        }

        public EffectPool AttachImageInfo(Vector2 sizeDelta, int ItemId, string atlasId = null, string spriteName = null)
        {
            gameObject.SetActive(true);
            Image image = gameObject.GetComponent<Image>();
            if (image == null) return this;
            ItemData itemData = new ItemData(ItemId, 1);
            image.rectTransform.sizeDelta = sizeDelta;

            if (!string.IsNullOrEmpty(atlasId) && !string.IsNullOrEmpty(spriteName))
                SpriteAtlasManager.Instance.SetSprite(image, atlasId, spriteName);

            itemData.SetSprite(image);
            return this;
        }

        public EffectPool AttachTextMeshProAlpha(float starAlpha, float endAlpha, float duraing, Ease ease = Ease.Linear)
        {
            gameObject.SetActive(true);
            var txts = gameObject.GetComponentsInChildren<TextMeshPro>();

            var sequence = DOTween.Sequence();
            for (int i = 0; i < txts.Length; i++)
            {
                Color color = txts[i].color;
                color.a = starAlpha;
                txts[i].color = color;
                color.a = endAlpha;
                sequence.Join(txts[i].DOColor(color, duraing));
            }
            sequence.SetEase(ease);
            sequenceRotate.Play();
            return this;
        }

        public float AttachTextMeshProText(string tips)
        {
            gameObject.SetActive(true);
            var txts = gameObject.GetComponentsInChildren<TextMeshPro>();
            if (txts.Length > 0)
            {
                txts[0].text = tips;
                return txts[0].preferredWidth;
            }
            return 0f;
        }

        public EffectPool AttachSpriteRendererAlpha(float starAlpha, float endAlpha, float duraing, Ease ease = Ease.Linear)
        {
            gameObject.SetActive(true);
            var txts = gameObject.GetComponentsInChildren<SpriteRenderer>();

            var sequence = DOTween.Sequence();
            for (int i = 0; i < txts.Length; i++)
            {
                Color color = txts[i].color;
                color.a = starAlpha;
                txts[i].color = color;
                color.a = endAlpha;
                sequence.Join(txts[i].DOColor(color, duraing));
            }
            sequence.SetEase(ease);
            sequenceRotate.Play();
            return this;
        }

        #endregion

    }

}
