using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NKStudio
{
    [InitializeOnLoad]
    public class CreateImageOnDragAndDrop : Editor
    {
        static CreateImageOnDragAndDrop()
        {
            ObjectChangeEvents.changesPublished += ChangesPublished;
        }
        
        static void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; ++i)
            {
                ObjectChangeKind type = stream.GetEventType(i);
                switch (type)
                {
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                        stream.GetCreateGameObjectHierarchyEvent(i,
                            out CreateGameObjectHierarchyEventArgs createGameObjectHierarchyEvent);
    
                        GameObject newGameObject =
                            EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;
    
                        if (newGameObject != null)
                            if (newGameObject.transform.parent)
                            {
                                var parentCanvas = newGameObject.transform.parent.GetComponentInParent<Canvas>();

                                if (parentCanvas)
                                {
                                    var cachedSprite = newGameObject.TryGetComponent(out SpriteRenderer spriteRenderer);

                                    if (cachedSprite)
                                    {
                                        // 프리팹이 아니라면처리
                                        // * 프리팹은 Add Component시 원본을 훼손해야하기 때문에 보안 처리
                                        if (PrefabUtility.GetPrefabInstanceStatus(newGameObject) ==
                                            PrefabInstanceStatus.NotAPrefab)
                                        {
                                            var image = newGameObject.AddComponent<Image>();

                                            image.sprite = spriteRenderer.sprite;
                                            image.SetNativeSize();

                                            var rectTransform = newGameObject.GetComponent<RectTransform>();
                                            rectTransform.localPosition = Vector3.zero;
                                            rectTransform.localScale = Vector3.one;

                                            DestroyImmediate(spriteRenderer);
                                        }
                                    }
                                }
                            }
                        break;
                }
            }
        }
    }
}
