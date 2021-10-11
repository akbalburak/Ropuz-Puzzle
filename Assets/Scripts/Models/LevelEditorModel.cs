using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Models
{
    [Serializable]
    public class LevelEditorModel
    {
        [Header("We order with index.")]
        public int OrderIndex;
        [Header("Unique id for model.")]
        public string UniqueID;

        [Header("Total row count.")]
        public int RowCount;

        [Header("Total column count.")]
        public int ColCount;

        [Header("Size of per cell.")]
        public int Size;

        [Header("Is always going to be randomized when level initializing.")]
        public bool AlwaysRandom;

        [Header("Level name that given by user to remember it.")]
        public string LevelName;

        [Header("Level image that will be used in puzzle.")]
        public string ImageUrl;

        [Header("ConfigurationFilePath")]
        public string ConfigFileName;

        [Header("Seed value for randomizer.")]
        public int SeedValue;

        [Header("Is this level system defined.")]
        public bool IsSystemDefined;

        [Header("Score point when user win the playground.")]
        public int ScoreOnWin;
        
        /// <summary>
        /// Downloaded texture.
        /// </summary>
        public Texture2D RemoteTexture { get; set; }

        public Texture2D LoadTextureFromFile()
        {
            // if remote texture exists just return it.
            if (RemoteTexture != null)
                return RemoteTexture;

            // if system defined? then we will load from the resources folder.
            if (IsSystemDefined)
                return Resources.Load<Texture2D>($"SystemLevels/{ImageUrl}");

            // if image url not exists just return null
            if (string.IsNullOrEmpty(ImageUrl))
                return null;

            // We read all the bytes.
            byte[] textureBytes = FileBrowserController.Instance.LoadFile(ImageUrl);

            // We create the texture.
            Texture2D texture = new Texture2D(ColCount * Size, RowCount * Size, TextureFormat.RGB24, true);

            // We load the texture data.
            texture.LoadImage(textureBytes, false);

            // We have to apply changes.
            texture.Apply();

            // We return the texture.
            return texture;
        }

        public LevelEditorModel Clone()
        {
            return new LevelEditorModel
            {
                AlwaysRandom = this.AlwaysRandom,
                ColCount = this.ColCount,
                LevelName = this.LevelName,
                RowCount = this.RowCount,
                Size = this.Size,
                SeedValue = this.SeedValue,
                UniqueID = Guid.NewGuid().ToString()
            };
        }

        public System.Random GetRandom()
        {
            if (this.AlwaysRandom)
                return new System.Random();
            else
                return new System.Random(SeedValue);
        }
    }
}
