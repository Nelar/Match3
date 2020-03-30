using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Match3
{
    public class Setting : MonoBehaviour, IConvertGameObjectToEntity
    {
        private static Setting instance;

        public static Setting Instance
        {
            get {
                return instance;
            }
        }

        [System.Serializable]
        public struct ViewSetting
        {
            public Color color;
            public Type type;
            public Material material;
        }

        [SerializeField]
        public Camera camera;

        [SerializeField]
        public GameObject prefab;
        [SerializeField]
        public Material material;
        [SerializeField]
        public ViewSetting[] views;
        [SerializeField]
        public int columnCount;
        [SerializeField]
        public int rowCount;
        [SerializeField]
        public float selectedScale;
        [SerializeField]
        public float normalScale;
        [SerializeField]
        public float speed;
        [SerializeField]
        public Gravity gravity;

        private void Awake()
        {
            instance = this;
        }

        public ViewSetting[] Views
        {
            get {
                return views;
            }
        }

        public Entity PrefabEntity
        {
            get;
            private set;
        }

        public Camera Camera
        {
            get {
                return camera;
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            using (BlobAssetStore blobAssetStore = new BlobAssetStore())
            {
                PrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab,
                    GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            }
        }
    }
}
