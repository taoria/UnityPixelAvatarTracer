using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.PixelAvatarTracing{
    
    [Serializable]
    //The prefab to be attached on pixelart
    public class TracingPair{
        [SerializeField]
        private Color color ;
        [SerializeField] 
        private GameObject prefab;
        [SerializeField] 
        private Vector2 offset;

        public Color Color => color;
        public GameObject Prefab => prefab;
        public Vector2 Offset => offset;
    }
    
    public class PixelAvatarTracer: MonoBehaviour{
        public List<TracingPair> tracingPairs;
        private Dictionary<Color, List<GameObject>> _attachedPixelArts = new Dictionary<Color, List<GameObject>>();
        private PixelAvatarTracingSystem _pixelAvatarTracingSystem;
        private Animator _characterAnimator;
        private Sprite _currentSprite;
        public Sprite traceMap;
        private SpriteRenderer _spriteRenderer;
        
    
        public bool followAnimator;

        public bool flipWhenFlip;
        // Start is called before the first frame update
        public void SetAttachedAnimatorsParam<T>(Color c, string name, T args){
            List<GameObject> specifiedPixelArts = _attachedPixelArts[c];
            if (specifiedPixelArts==null){
                return;
            }
            foreach (var go in specifiedPixelArts){
                var animator = go.GetComponent<Animator>();
                if (animator){
                    if (typeof(float) == typeof(T)){
                        animator.SetFloat(name, (float)(object)args);
                    }
                    if (typeof(int) == typeof(T)){
                        animator.SetInteger(name, (int)(object)args);
                    }
                    if (typeof(bool) == typeof(T)){
                        animator.SetBool(name, (bool)(object)args);
                    }
                }
            }
        }
        void Start(){
            _characterAnimator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            GameObject go = GameObject.Find("PixelAvatarTracingSystem");
            if (go!=null){
                _pixelAvatarTracingSystem = go.GetComponent<PixelAvatarTracingSystem>();
            }
            else{
                go = new GameObject("PixelAvatarTracingSystem");
                DontDestroyOnLoad(go);
                go.AddComponent<PixelAvatarTracingSystem>();
                _pixelAvatarTracingSystem = go.GetComponent<PixelAvatarTracingSystem>();
            }
        }

        // Update is called once per frame
        void Update(){
            //if follow animator set to be true,the tracer will try to update the attached gameobject's animator state;
            Dictionary<Color,int> counter = new Dictionary<Color,int>();
  
            _currentSprite = _spriteRenderer.sprite;
            if (!_pixelAvatarTracingSystem.SpriteAttachedPosition.ContainsKey(_currentSprite)){
                Debug.Log("generate");
                List<TracingPixel> tracingPixels = new List<TracingPixel>();
                Rect rect = _currentSprite.textureRect;
                for (int x = (int)rect.xMin; x < rect.xMax; x++){
                    for (int y = (int)rect.yMin; y < rect.yMax; y++){
                        Color color = traceMap.texture.GetPixel(x, y);
                        var tracingPair = tracingPairs.FirstOrDefault(d => d.Color == color);
                        if (tracingPair!=null){
                            float midX = (rect.xMin + rect.xMax) * 0.5f;
                            float midY = (rect.yMin + rect.yMax) * 0.5f;
                            Vector2 vector2 = new Vector2(x-midX,y-midY);
                            Debug.Log(rect);
                            tracingPixels.Add(new TracingPixel(vector2,color));
                        }
                    }
                }
                _pixelAvatarTracingSystem.SpriteAttachedPosition.Add(_currentSprite,tracingPixels);
            }
            if (_pixelAvatarTracingSystem.SpriteAttachedPosition.ContainsKey(_currentSprite)){
                var listPos =_pixelAvatarTracingSystem.SpriteAttachedPosition[_currentSprite];
                foreach(var pixel in listPos){
                    var currentColor = pixel.PixelColor;
                    if (!counter.ContainsKey(pixel.PixelColor)){
                        int it=0;
                        if (_attachedPixelArts.ContainsKey(currentColor)){
                        }
                        else{
                            _attachedPixelArts.Add(currentColor,new List<GameObject>());
                        }
                        counter.Add(pixel.PixelColor, it);
                    }
                    
                    var listIterator = counter[pixel.PixelColor];
                    var tracingPair = tracingPairs.FirstOrDefault(x => x.Color == currentColor);
                    if (tracingPair != null){
                        if (listIterator>=_attachedPixelArts[currentColor].Count){
                            _attachedPixelArts[currentColor].Add(GameObject.Instantiate(tracingPair.Prefab,this.transform));
                        }

                        var go = _attachedPixelArts[currentColor][listIterator];
                        counter[pixel.PixelColor]++;
                        float pixelPerUnitRatio = 1 / _currentSprite.pixelsPerUnit;
                        if (go != null){
                            Vector2 pos = new Vector2(pixel.PixelPos.x,pixel.PixelPos.y);
                            Vector2 offset = tracingPair.Offset;
                            Debug.Log(pos);
                            if (flipWhenFlip){
                                if (go.GetComponent<SpriteRenderer>().flipX){
                                    pos.x =  -pos.x;
                                    offset.x = -offset.x;
                                }
                                if (go.GetComponent<SpriteRenderer>().flipY){
                                    pos.y = -pos.y;
                                    offset.y = -offset.y;
                                }
                            }

                            Debug.Log(pos);
                            go.transform.localPosition = pos * pixelPerUnitRatio + offset* pixelPerUnitRatio;
                            if (followAnimator){
                                foreach (var var in  _characterAnimator.parameters){
                                    var goAnimator = go.GetComponent<Animator>();
                                    if (goAnimator){
                                       goAnimator.SetInteger(var.name,_characterAnimator.GetInteger(var.name));
                                    }
                                }
                            }
                        
                        }

                        else{
                            Debug.LogError("can't find game object attached to this character.");
                            
                        }
                           
                    
                    }
                }
            }
        }

      
    }
}