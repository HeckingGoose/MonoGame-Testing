using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    internal class _3D_Test
    {
        // Variables
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Shaders
        private BasicEffect _shader;

        // Vertex storage
        private VertexPositionColor[] _verts;
        private VertexBuffer _vertexBuffer;

        // Constructors
        public _3D_Test(
            ref GraphicsDeviceManager graphics,
            ref SpriteBatch spriteBatch,
            BasicEffect shader
            )
        {
            // Set references to graphics device and spritebatch
            _graphics = graphics;
            _spriteBatch = spriteBatch;

            _verts = new VertexPositionColor[3];

            _verts[0] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0), Color.Red);
            _verts[1] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0), Color.Blue);
            _verts[2] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0), Color.Green);

            _shader = shader;

            _vertexBuffer = new VertexBuffer(_graphics.GraphicsDevice, VertexPositionColor.VertexDeclaration, _verts.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_verts);
        }

        // Methods
        public void RunLogic()
        {

        }
        public void RunGraphics()
        {
            foreach (EffectPass pass in _shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphics.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                _graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _verts, 0, 1);
            }
        }
    }
}
