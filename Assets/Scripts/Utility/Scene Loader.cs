using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NKStudio
{
    public class SceneLoader : MonoBehaviour
    {
        public enum State
        {
            /// <summary> Idle mode </summary>
            Idle,
            /// <summary> Starting to load a scene </summary>
            LoadScene,
            /// <summary> Is in the process of loading a scene </summary>
            Loading,
            /// <summary> Has finished loading a scene, but has not activated it yet </summary>
            SceneLoaded,
            /// <summary> Is activating the loaded scene </summary>
            ActivatingScene
        }

        /// <summary>
        /// 로드할 씬의 이름 또는 경로
        /// </summary>
        public SceneReference TargetScene;
        
        /// <summary> SceneLoader의 현재 상태는 SceneLoader에 있습니다. </summary>
        public State CurrentState { get; private set; } = State.Idle;

        /// <summary> 씬 로더가 장면 로드를 시작할 때 시작된 asyncOperation을 추적하고 관리합니다. </summary>
        private AsyncOperation _currentAsyncOperation;

        /// <summary> 비동기 작업이 실행 중인 경우 현재 로드 진행률을 반환합니다(0과 1 사이의 부동 소수점). </summary>
        public float Progress
        {
            get => _progress;
            private set
            {
                _progress = value;
                OnProgressChanged?.Invoke(value);
            }
        }

        [Tooltip("장면 로드가 시작될 때 호출됩니다.")]
        public UnityEvent OnLoadScene;

        /// <summary>
        /// 아직 활성화되지 않았습니다(재설정 0.1(10%)).
        /// <para/>
        /// 장면을 로드할 때 Unity는 먼저 장면을 로드합니다(로드 진행률은 0%에서 90%까지).
        /// 그런 다음 활성화합니다(로드 진행률 90%에서 100%로). 이는 두 가지 상태 프로세스입니다.
        /// <para/>
        /// 이 작업은 장면이 로드된 후에 트리거됩니다.
        /// 활성화 전(로드 진행률 90%)
        /// </summary>
        [Tooltip("씬이 로드되면 호출됩니다 (진행률 0.9(90%)) 때 1회 호출.")]
        public UnityEvent OnSceneLoaded;

        /// <summary>
        /// <para/>
        /// 씬을 로드할 때 Unity는 먼저 씬을 로드합니다(로드 진행률은 0%에서 90%까지).
        /// 그런 다음 활성화합니다(로드 진행률 90%에서 100%로). 이는 두 가지 상태 프로세스입니다.
        /// <para/>
        /// 이 작업은 씬이 로드되고 활성화된 후에 트리거됩니다.
        /// </summary>
        [Tooltip("씬이 로드된 후 활성화된 후 호출됩니다.")]
        public UnityEvent OnSceneActivated;

        /// <summary> 비동기 작업이 실행 중이고 진행 상황이 업데이트되면 이벤트가 트리거됩니다. </summary>
        public UnityAction<float> OnProgressChanged;

        [Tooltip("디버그 메시지가 콘솔에 인쇄되도록 활성화")]
        public bool DebugMode;
        
        private bool _sceneLoadedAndReady; // 씬이 로드되지 않았음을 표시합니다(로드 진행률이 90%에 도달하지 않았습니다).
        private float _sceneLoadedAndReadyTime;
        private float _progress; // 비동기 작업이 실행 중일 때 업데이트됩니다(0과 1 사이에서 부동).
        
        private void Start()
        {
            if (TargetScene == null) return;
            LoadSceneAsync(TargetScene.ScenePath);
        }

        /// <summary> 장면 로드 진행률을 0으로 설정합니다. </summary>
        private void ResetProgress()
        {
            Progress = 0;
        }

        /// <summary> 빌드 설정의 이름을 기준으로 백그라운드에서 장면을 비동기적으로 로드합니다. </summary>
        /// <param name="sceneName"> 로드할 Scene의 이름 또는 경로 </param>
        /// <param name="mode"> LoadSceneMode.Single인 경우 새로 로드된 장면을 활성화하기 전에 모든 현재 장면이 언로드됩니다. </param>
        private void LoadSceneAsync(string sceneName)
        {
            if (IsSceneLoaded(sceneName)) return;
            StartCoroutine(AsynchronousLoad(sceneName));
        }
        
        private IEnumerator AsynchronousLoad(string sceneName)
        {
            ResetProgress();

            OnLoadScene?.Invoke();
            CurrentState = State.LoadScene;

            _currentAsyncOperation = SceneManager.LoadSceneAsync(sceneName);

            if (_currentAsyncOperation == null) yield break;

            _currentAsyncOperation.allowSceneActivation = false; // 씬 활성화 모드 업데이트
            bool sceneLoadedAndReady = false; // 씬이 로드되지 않았음을 표시합니다(로드 진행률이 90%에 도달하지 않았습니다).
            bool activatingScene = false;

            while (!_currentAsyncOperation.isDone)
                //while (m_LoadInProgress)
            {
                // [0, 0.9] > [0, 1]
                Progress = Mathf.Clamp01(_currentAsyncOperation.progress/0.9f); // 업데이트 로드 진행률

                if (DebugMode && !activatingScene) Debug.Log($"Load progress: {Mathf.Round(Progress*100)}%");

                // 로딩 완료
                if (!sceneLoadedAndReady && _currentAsyncOperation.progress == 0.9f)
                {
                    // progress = 1f;

                    if (DebugMode) Debug.Log($"씬을 활성화할 준비가 되었습니다.");

                    OnSceneLoaded.Invoke();

                    sceneLoadedAndReady = true; // 씬이 로드되었고 이제 활성화할 준비가 되었음을 표시합니다. (LoadBehavior.OnSceneLoaded.Invoke(게임 오브젝트)가 두 번 이상 실행되는 것을 중지하는 데 필요한 bool) 
                }
                
                yield return null;
            }
            
            StartCoroutine(SelfDestruct());
        }

        /// <summary>
        /// 인자로 넘어온 씬 이름과 현재 씬이 동일한지 확인합니다.
        /// </summary>
        /// <param name="sceneName"> 씬 이름 </param>
        public static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName) return true;
            }

            return false;
        }

        /// <summary>
        /// 현재 로드된 씬을 활성화합니다.
        /// <para />
        /// SceneLoader가 장면을 로드했고 해당 AllowSceneActivation 옵션이 false로 설정된 경우에만 작동합니다.
        /// <para />
        /// 이 메서드는 90%에서 일시 중지된 CurrentAsyncOperation에 대해 'allowSceneActivation'을 활성화합니다.
        /// <para />
        /// 장면을 로드할 때 Unity는 먼저 장면을 로드한 다음(로드 진행률 0%에서 90%) 이를 활성화합니다(로드 진행률 90%에서 100%). 이는 두 가지 상태 프로세스입니다.
        /// <para />
        /// 이 방법은 장면이 로드된 후 활성화되기 전(로드 진행률 90%)에 사용됩니다.
        /// </summary>
        public void ActivateLoadedScene()
        {
            if (_currentAsyncOperation == null) return; // 로드 프로세스가 실행되고 있지 않습니다.
            if (DebugMode) Debug.Log("Activating Scene...");
            OnSceneActivated?.Invoke();
            CurrentState = State.ActivatingScene;
            _currentAsyncOperation.allowSceneActivation = true;
        }

        private IEnumerator SelfDestruct()
        {
            yield return null;
            Destroy(gameObject);
        }
    }
}
