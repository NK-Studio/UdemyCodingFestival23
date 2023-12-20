using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

namespace AutoSingleton
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonAttribute : Attribute
    {
        
    }
    
    //Todo 비주얼 스크립팅에 사용할 때는 자식 클래스에 다음과 같은 코드를 추가하여 사용하면 됩니다.
    //Todo public static --Manager GetManager()  => Get<--Manager>();
    public class Singleton : MonoBehaviour
    {
        private static readonly Dictionary<Type, MonoBehaviour> Managers = new();
        
        public static T Instance<T>() where T : MonoBehaviour
        {
            if (Managers.ContainsKey(typeof(T)))
                return (T) Managers[typeof(T)];
            
            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                Debug.LogError($"매니저 : '{typeof(T)}'가 액세스 되지 않았습니다. GameplayIngredientsSettings에서 해당 매니저가 제외됬는지 확인해주세요.");
            
            return null;
        }

        public static bool Has<T>() where T : MonoBehaviour
            => Managers.ContainsKey(typeof(T));
        
        private static readonly Type[] KAllManagerTypes = GetAllSingletonTypes();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreateAll()
        {
            Managers.Clear();

            string[] exclusionList = AutoSingletonSettings.CurrentSettings.ExcludedManagers;

            if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                Debug.Log("모든 매니저 초기화 중 ...");

            foreach (Type type in KAllManagerTypes)
            {
                if (exclusionList != null && exclusionList.ToList().Contains(type.Name))
                {
                    if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                        Debug.Log(
                            $"매니저 : {type.Name}가 GameplayIngredientSettings에 excludedManagers리스트에 있습니다. 생성을 무시합니다.");
                    continue;
                }

                var attribute = type.GetCustomAttribute<ManagerDefaultPrefabAttribute>();

                if (attribute != null)
                {
                    GameObject prefab = Resources.Load<GameObject>(attribute.Prefab);
                    
                    if (prefab != null)
                    {
                        GameObject gameObject = Object.Instantiate(prefab);
                        gameObject.name = type.Name;
                        
                        Object.DontDestroyOnLoad(gameObject);
                        MonoBehaviour comp = (MonoBehaviour) gameObject.GetComponent(type);
                        Managers.Add(type, comp);

                        if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                            Debug.Log($" -> {type.Name} 생성 완료");
                    }
                    else
                    {
                        if (AutoSingletonSettings.CurrentSettings.ShowDebugCustomManager)
                            Debug.LogError(
                                $"{type}에 대한 기본 프리 팹을 인스턴스화 할 수 없습니다. 리소스 폴더에 프리 팹 '{attribute.Prefab}'이 (가) 없습니다.");
                    }
                }
            }
        }

        private static Type[] GetAllSingletonTypes()
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes = null;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch
                {
                    Debug.LogError($"어셈블리에서 유형을 로드 할 수 없습니다 : {assembly.FullName}");
                }

                if (assemblyTypes != null)
                    types.AddRange(assemblyTypes.Where(t => 
                        (typeof(Singleton).IsAssignableFrom(t) || t.GetCustomAttribute<SingletonAttribute>() != null)
                        && !t.IsAbstract
                    ));
            }

            return types.ToArray();
        }
    }
}