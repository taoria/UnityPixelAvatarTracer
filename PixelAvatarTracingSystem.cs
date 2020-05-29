using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.PixelAvatarTracing{
    public class TracingPixel{
        public Vector2 PixelPos{ get; set; }
        public Color PixelColor{ get; set; }

        public TracingPixel(Vector2 pixelPos, Color pixelColor){
            PixelPos = pixelPos;
            PixelColor = pixelColor;
            
        }
    }
    
    public class PixelAvatarTracingSystem : MonoBehaviour
    {
        public Dictionary<Sprite, List<TracingPixel>> SpriteAttachedPosition;

        private void Start(){
            SpriteAttachedPosition = new Dictionary<Sprite, List<TracingPixel>>();
        }
    }
}
