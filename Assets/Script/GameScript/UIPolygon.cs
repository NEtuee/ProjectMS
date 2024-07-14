using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Extensions/Primitives/UI Polygon")]
    public class UIPolygon : MaskableGraphic
    {
        private Vector2 _size;

        private Vector2 _add1 = Vector2.zero;
        private Vector2 _add2 = Vector2.zero;
        private Vector2 _add3 = Vector2.zero;
        private Vector2 _add4 = Vector2.zero;

        public void InitRandomAdd(Vector2 add1, Vector2 add2, Vector2 add3, Vector2 add4)
        {
            _add1 = add1;
            _add2 = add2;
            _add3 = add3;
            _add4 = add4;
        }

        public void ForceUpdate()
        {
            _size = rectTransform.rect.size;
        }
        
        private void Update()
        {
            _size = rectTransform.rect.size;
        }
        
        private UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
        {
            UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
            return vbo;
        }
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Vector2 uv0 = new Vector2(0, 0);
            Vector2 uv1 = new Vector2(0, 1);
            Vector2 uv2 = new Vector2(1, 1);
            Vector2 uv3 = new Vector2(1, 0);
            Vector2 pos0 = new Vector2(_size.x * 0.5f, _size.y * 0.5f);
            Vector2 pos1 = new Vector2(-_size.x * 0.5f, _size.y * 0.5f);
            Vector2 pos2 = new Vector2(-_size.x * 0.5f, -_size.y * 0.5f);
            Vector2 pos3 = new Vector2(_size.x * 0.5f, -_size.y * 0.5f);

            pos0 += _add1;
            pos1 += _add2;
            pos2 += _add3;
            pos3 += _add4;

            int vertices = 5;

            for (int i = 0; i < vertices; i++)
            {
                uv0 = new Vector2(0, 1);
                uv1 = new Vector2(1, 1);
                uv2 = new Vector2(1, 0);
                uv3 = new Vector2(0, 0);

                vh.AddUIVertexQuad(SetVbo(new[] { pos0, pos1, pos2, pos3 }, new[] { uv0, uv1, uv2, uv3 }));
            }
        }
    }
}