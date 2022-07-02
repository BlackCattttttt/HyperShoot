using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using static HyperShoot.Manager.MissonData;

namespace HyperShoot.Manager
{
    public class GamePlayController : MonoBehaviour
    {
        [System.Serializable] 
        public class SpawnMissonData
        {
            public int index;
            public MissonSpawn missonSpawn;
        }
        [SerializeField] private MissonData missonData;
        [SerializeField] private List<SpawnMissonData> spawnMissons;

        private MissonAtribute currenMisson;

        private List<MissonAtribute> missonDatas;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public MissonAtribute CurrenMisson { get => currenMisson; set => currenMisson = value; }

        private void OnEnable()
        {
            EvenGlobalManager.Instance.OnFinishLoadScene.AddListener(OnLoadLevel);
            EvenGlobalManager.Instance.OnLoadLevel.AddListener(OnLoadLevel);
            EvenGlobalManager.Instance.OnStartPlay.AddListener(OnStartPlay);
           // EvenGlobalManager.Instance.OnEndPlay.AddListener(OnEnd);

        }
        public void OnLoadLevel()
        {
            MessageBroker.Default.Receive<BaseMessage.SpawnMissonMessage>()
                 .Subscribe(SpawnMisson)
                 .AddTo(_disposables);
            MessageBroker.Default.Receive<BaseMessage.MissonComplete>()
               .Subscribe(MissonComplete)
               .AddTo(_disposables);
            missonDatas = missonData.GetMissonInLevel(GameManager.Instance.Data.Level);
            for (int i = 0; i < spawnMissons.Count; i++)
            {
                if (spawnMissons[i].index == GameManager.Instance.Data.CurrentMissonIndex)
                    spawnMissons[i].missonSpawn.gameObject.SetActive(true);
                else
                    spawnMissons[i].missonSpawn.gameObject.SetActive(false);
            }
        }
        public void OnStartPlay()
        {

        }

        public void SpawnMisson(BaseMessage.SpawnMissonMessage missonMessage)
        {
            if (GameManager.Instance.Data.CurrentMissonIndex < missonDatas.Count)
            {
                BaseMisson misson = SimplePool.Spawn(missonDatas[GameManager.Instance.Data.CurrentMissonIndex].misson, transform, Vector3.zero, Quaternion.identity);
                currenMisson = missonDatas[GameManager.Instance.Data.CurrentMissonIndex];
                if (missonDatas[GameManager.Instance.Data.CurrentMissonIndex].skillType == MissonAtribute.MissonType.FIND)
                {
                    MessageBroker.Default.Publish(new BaseMessage.ActiveArtifact
                    {

                    });
                }
                PlayScreen.Instance.SetMisson(missonDatas[GameManager.Instance.Data.CurrentMissonIndex]);
            }
        }
        public void MissonComplete(BaseMessage.MissonComplete message)
        {
            if (GameManager.Instance.Data.CurrentMissonIndex < missonDatas.Count)
            {
                GameManager.Instance.Data.CurrentMissonIndex++;
                Database.SaveData();
                currenMisson = null;
                PlayScreen.Instance.NoMisson();
                if (GameManager.Instance.Data.CurrentMissonIndex == missonDatas.Count)
                {
                    LoadingManager.Instance.LoadScene(SCENE_INDEX.Lose, () => WinScreen.Show());
                }
                else
                {
                    for (int i = 0; i < spawnMissons.Count; i++)
                    {
                        if (spawnMissons[i].index == GameManager.Instance.Data.CurrentMissonIndex)
                            spawnMissons[i].missonSpawn.gameObject.SetActive(true);
                        else
                            spawnMissons[i].missonSpawn.gameObject.SetActive(false);
                    }
                }
            }
        }
        private void OnDisable()
        {
            _disposables.Dispose();
            spawnMissons.Clear();
        }
    }
}
