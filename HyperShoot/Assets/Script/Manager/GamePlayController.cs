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

        private BaseMisson currenMisson;
        private int currentLevel = 1;
        private int currentMissonIndex = 2;
        private List<MissonAtribute> missonDatas;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
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
            missonDatas = missonData.GetMissonInLevel(currentLevel);
            for (int i = 0; i < spawnMissons.Count; i++)
            {
                if (spawnMissons[i].index == currentMissonIndex)
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
            if (currentMissonIndex < missonDatas.Count)
            {
                BaseMisson misson = SimplePool.Spawn(missonDatas[currentMissonIndex].misson, transform, Vector3.zero, Quaternion.identity);
                currenMisson = misson;
                if (missonDatas[currentMissonIndex].skillType == MissonAtribute.MissonType.FIND)
                {
                    MessageBroker.Default.Publish(new BaseMessage.ActiveArtifact
                    {

                    });
                }
                PlayScreen.Instance.SetMisson(missonDatas[currentMissonIndex]);
            }
        }
        public void MissonComplete(BaseMessage.MissonComplete message)
        {
            if (currentMissonIndex < missonDatas.Count)
            {
                currentMissonIndex++;
                if (currentMissonIndex == missonDatas.Count)
                {
                    LoadingManager.Instance.LoadScene(SCENE_INDEX.Lose, () => WinScreen.Show());
                }
                else
                {
                    for (int i = 0; i < spawnMissons.Count; i++)
                    {
                        if (spawnMissons[i].index == currentMissonIndex)
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
