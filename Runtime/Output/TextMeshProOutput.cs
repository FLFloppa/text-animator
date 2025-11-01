using System;
using TMPro;
using UnityEngine;

namespace FLFloppa.TextAnimator.Output
{
    /// <summary>
    /// Concrete <see cref="ITextOutput"/> implementation targeting TextMeshPro components.
    /// </summary>
    public sealed class TextMeshProOutput : ITextOutput
    {
        private readonly TMP_Text _textComponent;
        private TMP_MeshInfo[] _baseMeshInfo = Array.Empty<TMP_MeshInfo>();
        private int _visibleCharacterCount;
        private Renderer _renderer;
        private readonly MaterialPropertyBlock _sharedPropertyBlock = new MaterialPropertyBlock();
        private MaterialPropertyBlock[] _materialPropertyBlocks = Array.Empty<MaterialPropertyBlock>();

        public TextMeshProOutput(TMP_Text textComponent)
        {
            _textComponent = textComponent != null
                ? textComponent
                : throw new ArgumentNullException(nameof(textComponent));
        }

        public TMP_Text TextComponent => _textComponent;

        internal Color32 GetBaseVertexColor(int materialIndex, int vertexIndex)
        {
            if (materialIndex < 0 || materialIndex >= _baseMeshInfo.Length)
            {
                return new Color32(255, 255, 255, 0);
            }

            var baseColors = _baseMeshInfo[materialIndex].colors32;
            if (baseColors == null || vertexIndex < 0 || vertexIndex >= baseColors.Length)
            {
                return new Color32(255, 255, 255, 0);
            }

            return baseColors[vertexIndex];
        }

        internal Renderer GetRenderer()
        {
            _renderer ??= _textComponent.GetComponent<Renderer>();
            return _renderer;
        }

        internal MaterialPropertyBlock GetPropertyBlock(int materialIndex)
        {
            EnsurePropertyBlockCapacity(materialIndex + 1);
            return _materialPropertyBlocks[materialIndex];
        }

        public void SetText(string plainText)
        {
            _textComponent.text = plainText;
            _textComponent.ForceMeshUpdate();

            // Copy mesh data after ForceMeshUpdate
            _baseMeshInfo = _textComponent.textInfo.CopyMeshInfoVertexData();
            
            // Initialize all base mesh colors to transparent so characters start invisible
            var textInfo = _textComponent.textInfo;
            for (var i = 0; i < _baseMeshInfo.Length; i++)
            {
                var meshInfo = _baseMeshInfo[i];
                var baseColors = meshInfo.colors32;
                var currentColors = textInfo.meshInfo[i].colors32;
                
                if (baseColors != null)
                {
                    for (var j = 0; j < baseColors.Length; j++)
                    {
                        var color = baseColors[j];
                        color.a = 0;
                        baseColors[j] = color;
                        
                        // Also apply to current mesh immediately
                        if (currentColors != null && j < currentColors.Length)
                        {
                            currentColors[j] = color;
                        }
                    }
                }
                
                // Write back the modified mesh info (struct assignment)
                _baseMeshInfo[i] = meshInfo;
            }
            
            // Push the transparent colors to the mesh
            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            // Ensure all characters are allowed to render; visibility now controlled via alpha
            _textComponent.maxVisibleCharacters = _textComponent.textInfo.characterCount;
        }

        public void SetVisibleCharacterCount(int count)
        {
            _visibleCharacterCount = Mathf.Max(0, count);
        }

        public void BeginFrame()
        {
            var textInfo = _textComponent.textInfo;
            var meshCount = Math.Min(_baseMeshInfo.Length, textInfo.meshInfo.Length);

            for (var meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                ref var srcMesh = ref _baseMeshInfo[meshIndex];
                var dstMesh = textInfo.meshInfo[meshIndex];

                var srcVertices = srcMesh.vertices;
                var dstVertices = dstMesh.vertices;
                if (srcVertices != null && dstVertices != null)
                {
                    Array.Copy(srcVertices, dstVertices, Math.Min(srcVertices.Length, dstVertices.Length));
                }

                var srcColors = srcMesh.colors32;
                var dstColors = dstMesh.colors32;
                if (srcColors != null && dstColors != null)
                {
                    var colorCount = Math.Min(srcColors.Length, dstColors.Length);
                    Array.Copy(srcColors, dstColors, colorCount);

                    var colorSpan = dstColors.AsSpan(0, colorCount);
                    for (var colorIndex = 0; colorIndex < colorCount; colorIndex++)
                    {
                        ref var color = ref colorSpan[colorIndex];
                        color.a = 0;
                    }
                }

                textInfo.meshInfo[meshIndex] = dstMesh;
            }
        }

        public void FinalizeUpdate()
        {
            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32 | TMP_VertexDataUpdateFlags.Vertices);
        }

        private void EnsureCharacterCache(int characterCount, bool resetValues)
        {
            // Intentionally left in place for future extensibility; currently no-op.
        }

        private void EnsurePropertyBlockCapacity(int desiredLength)
        {
            if (desiredLength <= 0)
            {
                return;
            }

            var currentLength = _materialPropertyBlocks.Length;
            if (currentLength >= desiredLength)
            {
                return;
            }

            Array.Resize(ref _materialPropertyBlocks, desiredLength);
            for (var i = currentLength; i < desiredLength; i++)
            {
                _materialPropertyBlocks[i] = new MaterialPropertyBlock();
            }
        }
    }
}
